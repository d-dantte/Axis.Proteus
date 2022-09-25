using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Axis.Proteus.SimpleInjector.NamedContext;
using Castle.DynamicProxy;
using SimpleInjector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Axis.Proteus.SimpleInjector
{
    public class SimpleInjectorRegistrar : IRegistrarContract
    {
        private static readonly MethodInfo _registerFactoryMethod = GetRegisterFactoryMethod();
        private static readonly MethodInfo _containerAppendFactoryMethod = GetContainerAppendFactoryMethod();
        private static readonly MethodInfo _createLifestyleProducerMethod = GetCreateLifestyleProducerMethod();

        private readonly RegistryManifest _manifest = new RegistryManifest();
        private readonly Container _container;
        private readonly IProxyGenerator _proxyGenerator;
        private IResolverContract _resolverContract;

        #region constructor
        public SimpleInjectorRegistrar(Container container, IProxyGenerator proxyGenerator)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
        }

        public SimpleInjectorRegistrar(Container container)
            : this(container, new ProxyGenerator())
        {
        }

        public SimpleInjectorRegistrar()
            : this(new Container(), new ProxyGenerator())
        {
        }
        #endregion


        #region IRegistrarContract
        public IResolverContract BuildResolver()
        {
            if (IsRegistrationClosed())
                return _resolverContract ?? throw new InvalidOperationException("The resolver is not yet initialized");

            // register root types
            foreach (var serviceType in _manifest.RootServices())
            {
                var rootRegistration =
                    _manifest.RootRegistrationFor(serviceType)
                    ?? throw new InvalidManifestRegistrationMappingException(serviceType);

                foreach (var context in rootRegistration.BindContexts)
                {
                    if(context is IBindContext.DefaultContext defaultContext)
                        BindDefaultContext(rootRegistration, defaultContext);

                    else if(context is IBindContext.ParameterContext paramContext)
                        BindParameterContext(rootRegistration, paramContext);

                    else if (context is IBindContext.PropertyContext propContext)
                        BindPropertyContext(rootRegistration, propContext);

                    else if(context is IBindContext.NamedContext namedContext)
                        BindNamedContext(rootRegistration, namedContext);

                    else
                        throw new InvalidOperationException($"Invalid context type: {context?.GetType()}");
                }
            }

            // register collection types
            foreach(var serviceType in _manifest.CollectionServices())
            {
                var collectionRegistrations =
                    _manifest.CollectionRegistrationsFor(serviceType)
                    ?? throw new InvalidManifestRegistrationMappingException(serviceType);

                foreach (var registration in collectionRegistrations)
                {
                    BindCollectionDefaultContext(registration);
                }
            }

            // register the IResolverContract on the container. Note that duplicate registrations should fail
            // as there should be only one such registration on this container.
            _container.RegisterInstance<IResolverContract>(
                new SimpleInjectorResolver(
                    _container,
                    _proxyGenerator,
                    _manifest));

            _container.Verify();

            // resolve the IResolverContract, effectively locking the container.
            return _resolverContract = _container.GetInstance<IResolverContract>();
        }

        public bool IsRegistrationClosed() => _container.IsLocked;

        public ReadOnlyDictionary<Type, RegistrationInfo[]> CollectionManifest()
            => _manifest
                .CollectionServices()
                .SelectMany(type => _manifest.CollectionRegistrationsFor(type))
                .GroupBy(info => info.ServiceType)
                .ToDictionary(group => group.Key, group => group.ToArray())
                .ApplyTo(dict => new ReadOnlyDictionary<Type, RegistrationInfo[]>(dict));

        public ReadOnlyDictionary<Type, RegistrationInfo> RootManifest()
            => _manifest
                .RootServices()
                .ToDictionary(type => type, type => _manifest.RootRegistrationFor(type) ?? throw new Exception($"no registration was found for the given type: {type}"))
                .ApplyTo(dict => new ReadOnlyDictionary<Type, RegistrationInfo>(dict));

        public IRegistrarContract Register<Impl>(
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Impl : class
            => Register<Impl, Impl>(scope, profile, conditionalBindingContexts);

        public IRegistrarContract Register<Service, Impl>(
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Service : class
            where Impl : class, Service
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            var serviceType = typeof(Service);
            var implType = typeof(Impl);
            _manifest.AddRootRegistration(
                new RegistrationInfo(
                    serviceType,
                    IBindTarget.Of(implType),
                    scope,
                    profile,
                    conditionalBindingContexts));

            return this;
        }

        public IRegistrarContract Register<Service>(
            Func<IResolverContract, Service> factory,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Service : class
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            Func<Service> resolvedFactory = () => 
                factory.Invoke(_container.GetInstance<IResolverContract>());
            var serviceType = typeof(Service);
            _manifest.AddRootRegistration(
                new RegistrationInfo(
                    serviceType,
                    IBindTarget.Of(serviceType, resolvedFactory),
                    scope,
                    profile,
                    conditionalBindingContexts));

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            => Register(serviceType, serviceType, scope, profile, conditionalBindingContexts);

        public IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
        {
            _manifest.AddRootRegistration(
                new RegistrationInfo(
                    serviceType,
                    IBindTarget.Of(concreteType),
                    scope,
                    profile,
                    conditionalBindingContexts));

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Delegate factory,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            => _registerFactoryMethod
                .MakeGenericMethod(serviceType)
                .ApplyTo(method => this.InvokeFunc(method, factory, scope, profile, conditionalBindingContexts))
                .As<IRegistrarContract>();

        public IRegistrarContract RegisterAll(
            Type serviceType,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets)
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            targets?.ForAll(target =>
            {
                _ = _manifest.AddCollectionRegistrations(
                    new RegistrationInfo(
                        serviceType,
                        target,
                        scope,
                        profile));
            });

            return this;
        }

        public IRegistrarContract RegisterAll<Service>(
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets)
            => RegisterAll(typeof(Service), scope, profile);
        #endregion

        #region Method Access
        private static MethodInfo GetRegisterFactoryMethod()
        {
            SimpleInjectorRegistrar contract = null;
            Expression<Func<
                Func<IResolverContract, IEnumerable<int>>,
                RegistryScope,
                InterceptorProfile,
                IBindContext[],
                IRegistrarContract>> expression = (f, a, b, c) => contract.Register(f, a, b, c);
            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        private static MethodInfo GetContainerAppendFactoryMethod()
        {
            ContainerCollectionRegistrator collectionContainer = null;
            Expression<Action<Func<IEnumerable<int>>, Lifestyle>> expression = (f, l) => collectionContainer.Append(f, l);
            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        private static MethodInfo GetCreateLifestyleProducerMethod()
        {
            Lifestyle lifestyle = null;
            Expression<Func<
                Func<IEnumerable>,
                Container,
                InstanceProducer<IEnumerable>>> expression = (f, c) => lifestyle.CreateProducer(f, c);
            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }
        #endregion


        private void BindCollectionDefaultContext(RegistrationInfo registration)
        {
            if (registration.DefaultContext.Target is IBindTarget.TypeTarget typeTarget)
                _container.Collection.Append(
                    registration.ServiceType,
                    typeTarget.Type,
                    registration.Scope.ToSimpleInjectorLifeStyle());

            else if (registration.DefaultContext.Target is IBindTarget.FactoryTarget factory)
                _containerAppendFactoryMethod
                    .MakeGenericMethod(registration.ServiceType)
                    .Consume(method => _container.Collection.InvokeAction(
                        method,
                        factory.Factory,
                        registration.Scope.ToSimpleInjectorLifeStyle()));

            else
                throw new InvalidOperationException($"Invalid default binding context: {registration.DefaultContext.Target}");
        }

        private void BindNamedContext(RegistrationInfo rootRegistration, IBindContext.NamedContext namedContext)
        {
            // register the dynamic type
            var lifestyle = rootRegistration.Scope.ToSimpleInjectorLifeStyle();
            var dynamicType = CreateNamedContextDynamicType(
                namedContext.Name,
                rootRegistration.ServiceType,
                namedContext.Target.Type);
            _container.Register(dynamicType, dynamicType, lifestyle);

            // conditionally register 'object'
            if (namedContext.Target is IBindTarget.TypeTarget namedTypeTarget)
                _container.RegisterConditional(
                    typeof(object),
                    namedTypeTarget.Type,
                    predicateContext =>
                        predicateContext.HasConsumer
                        && dynamicType.Equals(predicateContext.Consumer.Target.Parameter.Member.DeclaringType));

            else if (namedContext.Target is IBindTarget.FactoryTarget namedFactoryTarget)
                _container.RegisterConditional(
                    serviceType: typeof(object),
                    predicate: predicateContext =>
                        predicateContext.HasConsumer
                        && dynamicType.Equals(predicateContext.Consumer.Target.Parameter.Member.DeclaringType),
                    registration: CreateLifestyleRegistration(
                        typeof(object),
                        namedFactoryTarget,
                        rootRegistration.Scope,
                        _container));

            else
                throw new InvalidOperationException("Invalid target type: " + namedContext.Target?.GetType());
        }

        private void BindPropertyContext(RegistrationInfo rootRegistration, IBindContext.PropertyContext propContext)
        {
            if (propContext.Target is IBindTarget.TypeTarget propTypeTarget)
                _container.RegisterConditional(
                    serviceType: rootRegistration.ServiceType,
                    implementationType: propTypeTarget.Type,
                    lifestyle: rootRegistration.Scope.ToSimpleInjectorLifeStyle(),
                    predicate: predicateContext =>
                    {
                        if (predicateContext.HasConsumer && predicateContext.Consumer.Target.Property != null)
                            return propContext.Predicate(predicateContext.Consumer.Target.Property);

                        return false;
                    });

            else if (propContext.Target is IBindTarget.FactoryTarget propFactoryTarget)
                _container.RegisterConditional(
                    serviceType: rootRegistration.ServiceType,
                    predicate: predicateContext =>
                    {
                        if (predicateContext.HasConsumer && predicateContext.Consumer.Target.Property != null)
                            return propContext.Predicate(predicateContext.Consumer.Target.Property);

                        return false;
                    },
                    registration: CreateLifestyleRegistration(
                        rootRegistration.ServiceType,
                        propFactoryTarget,
                        rootRegistration.Scope,
                        _container));

            else
                throw new InvalidOperationException("Invalid target type: " + propContext.Target?.GetType());
        }

        private void BindParameterContext(RegistrationInfo rootRegistration, IBindContext.ParameterContext paramContext)
        {
            if (paramContext.Target is IBindTarget.TypeTarget paramTypeTarget)
                _container.RegisterConditional(
                    serviceType: rootRegistration.ServiceType,
                    implementationType: paramTypeTarget.Type,
                    lifestyle: rootRegistration.Scope.ToSimpleInjectorLifeStyle(),
                    predicate: predicateContext =>
                    {
                        if (predicateContext.HasConsumer && predicateContext.Consumer.Target.Parameter != null)
                            return paramContext.Predicate(predicateContext.Consumer.Target.Parameter);

                        return false;
                    });

            else if (paramContext.Target is IBindTarget.FactoryTarget paramFactoryTarget)
                _container.RegisterConditional(
                    serviceType: rootRegistration.ServiceType,
                    predicate: predicateContext =>
                    {
                        if (predicateContext.HasConsumer && predicateContext.Consumer.Target.Parameter != null)
                            return paramContext.Predicate(predicateContext.Consumer.Target.Parameter);

                        return false;
                    },
                    registration: CreateLifestyleRegistration(
                        rootRegistration.ServiceType,
                        paramFactoryTarget,
                        rootRegistration.Scope,
                        _container));

            else
                throw new InvalidOperationException("Invalid target type: " + paramContext.Target?.GetType());
        }

        private void BindDefaultContext(RegistrationInfo rootRegistration, IBindContext.DefaultContext context)
        {
            if (context.Target is IBindTarget.TypeTarget typeTarget)
                _container.Register(
                    rootRegistration.ServiceType,
                    typeTarget.Type,
                    rootRegistration.Scope.ToSimpleInjectorLifeStyle());

            else if (context.Target is IBindTarget.FactoryTarget factoryTarget)
                _container.Register(
                    rootRegistration.ServiceType,
                    (Func<object>)factoryTarget.Factory,
                    rootRegistration.Scope.ToSimpleInjectorLifeStyle());

            else
                throw new InvalidOperationException("Invalid target type: " + context.Target?.GetType());
        }

        private Type CreateNamedContextDynamicType(
            ResolutionContextName contextName,
            Type serviceType,
            Type implType)
            => DynamicTypeUtil.ToNamedContextType(contextName, serviceType, implType);


        private static Registration CreateLifestyleRegistration(
            Type serviceType,
            IBindTarget.FactoryTarget factoryTarget,
            RegistryScope scope,
            Container container)
        {
            return scope
                .ToSimpleInjectorLifeStyle()
                .InvokeFunc(
                    _createLifestyleProducerMethod.MakeGenericMethod(serviceType),
                    factoryTarget.Factory,
                    container)
                .As<Registration>();
        }
    }
}
