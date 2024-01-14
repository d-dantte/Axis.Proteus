using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using Axis.Proteus.SimpleInjector.NamedContext;
using Castle.DynamicProxy;
using SimpleInjector;
using System;
using System.Linq;
using System.Reflection;
using static Axis.Proteus.IoC.IBindContext;

namespace Axis.Proteus.SimpleInjector
{
    /// <summary>
    /// SimpleInjector implementation of the <see cref="IRegistrarContract"/> contract.
    /// <para>
    /// Of special note is fact that using the <see cref="IBindContext.NamedContext"/> conditional binding attempts to use <see cref="object"/> as a base registration
    /// for all the dynamically generated proxies that represent all the named-contexts in use. This also means that externally, <see cref="object"/> can no longer
    /// be registered with the same container unless it is another conditional registration - a restriction imposed by SimpleInjector.
    /// </para>
    /// </summary>
    public sealed class SimpleInjectorRegistrar : AbstractRegistrar
    {
        private readonly Container _container;
        private readonly IProxyGenerator _proxyGenerator;
        private IResolverContract? _resolverContract;

        #region constructor
        public SimpleInjectorRegistrar(Container container, IProxyGenerator proxyGenerator)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
            _resolverContract = null;
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
        public override IResolverContract BuildResolver()
        {
            if (IsRegistrationClosed())
                return _resolverContract ?? throw new InvalidOperationException("The resolver is not yet initialized");

            // register root types
            foreach (var serviceType in Manifest.RootServices())
            {
                var rootRegistration =
                    Manifest.RootRegistrationFor(serviceType)
                    ?? throw new InvalidOperationException(
                        $"The registration info for the given service type ({serviceType}) cannot be found.");

                var contexts = rootRegistration.BindContexts;
                if (contexts.Length == 1)
                    BindDefaultContext(
                        rootRegistration.ServiceType,
                        contexts[0].As<IBindContext.DefaultContext>());

                else
                {
                    foreach (var context in rootRegistration.BindContexts.Reverse())
                    {
                        // the default context should always come last, hence the "Reverse()" action applied above.
                        if (context is IBindContext.DefaultContext defaultContext)
                            BindDefaultContextConditionally(
                                rootRegistration.ServiceType,
                                defaultContext);

                        else if (context is IBindContext.ParameterContext paramContext)
                            BindParameterContext(
                                rootRegistration.ServiceType,
                                paramContext);

                        else if (context is IBindContext.PropertyContext propContext)
                            BindPropertyContext(
                                rootRegistration.ServiceType,
                                propContext);

                        else if (context is IBindContext.NamedContext namedContext)
                            BindNamedContext(
                                rootRegistration.ServiceType,
                                namedContext);

                        else
                            throw new InvalidOperationException($"Invalid context type: {context?.GetType()}");
                    }
                }
            }

            // register collection types
            foreach(var serviceType in Manifest.CollectionServices())
            {
                var collectionRegistrations =
                    Manifest.CollectionRegistrationsFor(serviceType)
                    ?? throw new InvalidOperationException(
                        $"The registration info for the given service type ({serviceType}) cannot be found.");

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
                    Manifest));

            _container.Verify();

            // resolve the IResolverContract, effectively locking the container.
            return _resolverContract = _container.GetInstance<IResolverContract>();
        }

        public override bool IsRegistrationClosed() => _container.IsLocked;

        #endregion

        #region Context Binders
        private void BindNamedContext(Type serviceType, IBindContext.NamedContext namedContext)
        {
            // register the dynamic type
            var lifestyle = namedContext.Scope.ToSimpleInjectorLifeStyle();

            if (namedContext.Target is IBindTarget.TypeTarget namedTypeTarget)
            {
                var replacementType = GetReplacementTypeForNamedContext(
                        namedContext.Name,
                       serviceType,
                        namedTypeTarget.Type);

                _container.Register(
                    serviceType: replacementType,
                    implementationType: replacementType,
                    lifestyle: lifestyle);
            }

            else if (namedContext.Target is IBindTarget.FactoryTarget namedFactoryTarget)
            {
                (var containerType, var containerInstanceProducer) = GetContainerTypeForNamedContext(
                    namedContext.Name,
                    serviceType,
                    namedFactoryTarget.Type);

                _container.Register(
                    serviceType: containerType,
                    lifestyle: lifestyle,
                    instanceCreator: () =>
                    {
                        // resolve the target instance.
                        var instance = namedFactoryTarget.Factory
                            .ApplyTo(CreateResolverInjectedInstanceProducer)
                            .Invoke();

                        // resolve and return the target injected container instance.
                        return containerInstanceProducer.Invoke(instance);
                    });
            }

            else
                throw new InvalidOperationException($"Invalid target type: {namedContext.Target?.GetType()}");
        }

        private void BindPropertyContext(Type serviceType, IBindContext.PropertyContext propContext)
        {
            var lifeStyle = propContext.Scope.ToSimpleInjectorLifeStyle();
            if (propContext.Target is IBindTarget.TypeTarget propTypeTarget)
                _container.RegisterConditional(
                    serviceType: serviceType,
                    implementationType: propTypeTarget.Type,
                    lifestyle: lifeStyle,
                    predicate: CreatePropertyContextPredicate(propContext));

            else if (propContext.Target is IBindTarget.FactoryTarget propFactoryTarget)
                _container.RegisterConditional(
                    serviceType: serviceType,
                    predicate: CreatePropertyContextPredicate(propContext),
                    registration: lifeStyle
                        .CreateRegistration(
                            serviceType: serviceType,
                            container: _container,
                            instanceCreator: propFactoryTarget.Factory.ApplyTo(
                                CreateResolverInjectedInstanceProducer)));

            else
                throw new InvalidOperationException("Invalid target type: " + propContext.Target?.GetType());
        }

        private void BindParameterContext(Type serviceType, IBindContext.ParameterContext paramContext)
        {
            var lifeStyle = paramContext.Scope.ToSimpleInjectorLifeStyle();
            if (paramContext.Target is IBindTarget.TypeTarget paramTypeTarget)
                _container.RegisterConditional(
                    serviceType: serviceType,
                    implementationType: paramTypeTarget.Type,
                    lifestyle: lifeStyle,
                    predicate: CreateParamContextPredicate(paramContext));

            else if (paramContext.Target is IBindTarget.FactoryTarget paramFactoryTarget)
                _container.RegisterConditional(
                    serviceType: serviceType,
                    predicate: CreateParamContextPredicate(paramContext),
                    registration: lifeStyle
                        .CreateRegistration(
                            serviceType: serviceType,
                            container: _container,
                            instanceCreator: paramFactoryTarget.Factory.ApplyTo(
                                CreateResolverInjectedInstanceProducer)));

            else
                throw new InvalidOperationException("Invalid target type: " + paramContext.Target?.GetType());
        }

        private void BindDefaultContext(Type serviceType, IBindContext.DefaultContext context)
        {
            var lifeStyle = context.Scope.ToSimpleInjectorLifeStyle();
            if (context.Target is IBindTarget.TypeTarget typeTarget)
                _container.Register(
                    serviceType,
                    typeTarget.Type,
                    lifeStyle);

            else if (context.Target is IBindTarget.FactoryTarget factoryTarget)
                _container.Register(
                    serviceType: serviceType,
                    lifestyle: lifeStyle,
                    instanceCreator: factoryTarget.Factory.ApplyTo(CreateResolverInjectedInstanceProducer));

            else
                throw new InvalidOperationException("Invalid target type: " + context.Target?.GetType());
        }

        private void BindDefaultContextConditionally(
            Type serviceType,
            IBindContext.DefaultContext defaultContext)
        {
            var lifeStyle = defaultContext.Scope.ToSimpleInjectorLifeStyle();
            if (defaultContext.Target is IBindTarget.TypeTarget typeTarget)
                _container.RegisterConditional(
                    serviceType: serviceType,
                    predicate: predicateContext => !predicateContext.Handled, // <-- meaning, fall back to this condition
                    implementationType: typeTarget.Type,
                    lifestyle: lifeStyle);

            else if (defaultContext.Target is IBindTarget.FactoryTarget factoryTarget)
                _container.RegisterConditional(
                    serviceType: serviceType,
                    predicate: predicateContext => !predicateContext.Handled, // <-- meaning, fall back to this condition
                    registration: lifeStyle
                        .CreateRegistration(
                            serviceType: serviceType,
                            container: _container,
                            instanceCreator: factoryTarget.Factory.ApplyTo(
                                CreateResolverInjectedInstanceProducer)));
        }

        private void BindCollectionDefaultContext(RegistrationInfo registration)
        {
            var defaultContext = registration.DefaultContext;
            if (defaultContext.Target is IBindTarget.TypeTarget typeTarget)
                _container.Collection.Append(
                    registration.ServiceType,
                    typeTarget.Type,
                    defaultContext.Scope.ToSimpleInjectorLifeStyle());

            else if (defaultContext.Target is IBindTarget.FactoryTarget factoryTarget)
                _container.Collection.Append(
                    registration.ServiceType,
                    defaultContext.Scope
                        .ToSimpleInjectorLifeStyle()
                        .CreateRegistration(
                            serviceType: registration.ServiceType,
                            container: _container,
                            instanceCreator: factoryTarget.Factory.ApplyTo(
                                CreateResolverInjectedInstanceProducer)));

            else
                throw new InvalidOperationException($"Invalid default binding context: {defaultContext.Target}");
        }
        #endregion

        private static Type GetReplacementTypeForNamedContext(
            ResolutionContextName contextName,
            Type serviceType,
            Type implType)
            => DynamicTypeUtil.ToReplacementTypeForNamedContext(contextName, serviceType, implType);

        private static (Type containerType, Func<object, NamedContextContainerBase> instanceProducer) GetContainerTypeForNamedContext(
            ResolutionContextName contextName,
            Type serviceType,
            Type implType)
        {
            // create (and cache) the container type
            var containerType = DynamicTypeUtil.ToContainerTypeForNamedContext(
                contextName,
                serviceType,
                implType);

            // get the static 'NewInstance' method.
            MethodInfo newInstanceMethod = containerType.GetMethod(
                DynamicTypeUtil.NamedContextContainerTypeNewInstanceMethodName)!;

            // create delegate for NewInstance method
            var contextContainerProducer = newInstanceMethod
                .CreateDelegate(typeof(Func<object, NamedContextContainerBase>))
                .As<Func<object, NamedContextContainerBase>>();

            return (containerType, contextContainerProducer);
        }

        private Func<object> CreateResolverInjectedInstanceProducer(Func<IResolverContract, object> externalInstanceProducer)
        {
            if (externalInstanceProducer == null)
                throw new ArgumentNullException(nameof(externalInstanceProducer));

            return () => externalInstanceProducer.Invoke(_container.GetInstance<IResolverContract>());
        }

        private static Predicate<PredicateContext> CreateParamContextPredicate(IBindContext.ParameterContext paramContext)
        {
            return predicateContext =>
            {
                if (predicateContext.HasConsumer && predicateContext.Consumer.Target.Parameter != null)
                    return paramContext.Predicate(predicateContext.Consumer.Target.Parameter);

                return false;
            };
        }

        private static Predicate<PredicateContext> CreatePropertyContextPredicate(IBindContext.PropertyContext propContext)
        {
            return predicateContext =>
            {
                if (predicateContext.HasConsumer && predicateContext.Consumer.Target.Property != null)
                    return propContext.Predicate(predicateContext.Consumer.Target.Property);

                return false;
            };
        }

    }
}
