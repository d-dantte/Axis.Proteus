using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Axis.Proteus.Interception;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Facilitates the registration of services against implementations.
    /// Registration model adhered to here is the many-to-many style, meaning any service can have any number of implementations, and each concreate type can implement any number of services.
    /// Implication of the above is that any "register" method can be called multiple times to append a unique registration of services and implementations
    /// </summary>
    public class ServiceRegistrar
    {
        private static readonly MethodInfo _registerFactoryMethod = GetRegisterFactoryMethod();

        private readonly IRegistrarContract _registrar;
        private readonly IProxyGenerator _proxyGenerator;
        private readonly Dictionary<Type, List<RegistrationInfo>> _registrationManifest = new Dictionary<Type, List<RegistrationInfo>>();
        private ServiceResolver _serviceResolver;

        public ServiceRegistrar(IRegistrarContract registrar)
            : this(registrar, new ProxyGenerator())
        {
        }

        public ServiceRegistrar(IRegistrarContract registrar, IProxyGenerator proxyGenerator)
        {
            _registrar = registrar ?? throw new ArgumentNullException(nameof(registrar));
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
        }

        #region Registration Methods

        /// <summary>
        /// Register a concrete type. Concrete types cannot be registered more than once.
        /// </summary>
        /// <param name="serviceType">The concrete type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register(
            Type serviceType,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            => Register(serviceType, serviceType, scope, interceptorProfile);

        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="concreteType">The concrete service type to resolve to</params>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
        {
            if (!CanRegister())
                throw new InvalidOperationException("Registry is locked");

            _registrationManifest
                .GetOrAdd(serviceType, type => new List<RegistrationInfo>())
                .Add(new RegistrationInfo(
                    serviceType,
                    IBoundImplementation.Of(concreteType),
                    scope ?? default,
                    interceptorProfile ?? default));

            return this;
        }

        /// <summary>
        /// Register a factory method to be used in resolving instances of the service interface/type
        /// Note that the interception logic is baked into the resolution logic because there's no way to distinguish between
        /// the result of any two or more factory generated services.
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="factory">A factory method used to create the service type. Note that this delegate MUST safely be castable to: <c>Func&lt;IRegistrarContract, TServiceType&gt;</c></param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register(
            Type serviceType,
            Delegate factory,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            => _registerFactoryMethod
                .MakeGenericMethod(serviceType)
                .ApplyTo(method => this.InvokeFunc(method, factory, scope, interceptorProfile))
                .As<ServiceRegistrar>();

        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <typeparam name="Impl">The concrete type to be registered and resolved</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register<Impl>(
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Impl : class
            => Register<Impl, Impl>(scope, interceptorProfile);

        /// <summary>
        /// Register a concrete implementation for a service type
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <typeparam name="Impl">The service implementation to be resolved for the service type</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register<Service, Impl>(
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Service : class
            where Impl : class, Service
        {
            if (!CanRegister())
                throw new InvalidOperationException("Registry is locked");

            var serviceType = typeof(Service);
            _registrationManifest
                .GetOrAdd(serviceType, type => new List<RegistrationInfo>())
                .Add(new RegistrationInfo(
                    serviceType,
                    IBoundImplementation.Of(typeof(Impl)),
                    scope ?? default,
                    interceptorProfile ?? default));

            return this;
        }

        /// <summary>
        /// Register a factory method that should be used in resolving instances of a service interface/type
        /// </summary>
        /// <typeparam name="Impl">The service type to be registered and resolved</typeparam>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register<Service>(
            Func<IResolverContract, Service> factory,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Service : class
        {
            if (!CanRegister())
                throw new InvalidOperationException("Registry is locked");

            var serviceType = typeof(Service);
            _registrationManifest
                .GetOrAdd(serviceType, type => new List<RegistrationInfo>())
                .Add(new RegistrationInfo(
                    serviceType,
                    IBoundImplementation.Of(typeof(Service), factory),
                    scope ?? default,
                    interceptorProfile ?? default));

            return this;
        }

        /// <summary>
        /// Register a list of concrete types for the given service type
        /// </summary>
        /// <param name="serviceType">the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="concreteTypes">Concrete types to be registered to the service type</param>
        public virtual ServiceRegistrar RegisterAll(
            Type serviceType,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null,
            params Type[] concreteTypes)
        {
            if (concreteTypes == null)
                throw new ArgumentNullException(nameof(concreteTypes));

            concreteTypes.ForAll(type => Register(serviceType, type, scope, interceptorProfile));

            return this;
        }

        /// <summary>
        /// Register a list of concrete types for the given service type
        /// </summary>
        /// <param name="serviceType">the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="concreteTypes">Concrete types to be registered to the service type</param>
        public virtual ServiceRegistrar RegisterAll<Service>(
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null,
            params Type[] concreteTypes)
            where Service : class
            => RegisterAll(typeof(Service), scope, interceptorProfile, concreteTypes);
        #endregion

        /// <summary>
        /// Effectively locks the registrar (no longer accepts registration requests), and returns a Resolver that resovles all of the registered types.
        /// </summary>
        public virtual ServiceResolver BuildResolver()
        {
            if (!CanRegister())
                return _serviceResolver ?? throw new InvalidOperationException("The resolver is not yet initialized");

            // register the types in the manifest on the IRegistrarContract
            foreach(var infoMap in _registrationManifest)
            {
                foreach(var info in infoMap.Value)
                {
                    _ = info.Implementation switch
                    {
                        IBoundImplementation.ImplType implType => _registrar.Register(
                            infoMap.Key,
                            implType.Type,
                            info.Scope),

                        IBoundImplementation.ImplFactory factory => _registrar.Register(
                            infoMap.Key,
                            factory.Factory,
                            info.Scope),

                        _ => throw new InvalidOperationException($"Invalid implementation type: {info.Implementation}")
                    };
                }
            }

            return _serviceResolver = new ServiceResolver(
                _registrar.BuildResolver(),
                _proxyGenerator,
                _registrationManifest);
        }

        private bool CanRegister() => !_registrar.IsRegistrationClosed();

        /// <summary>
        /// For testing purposes
        /// </summary>
        public IEnumerable<RegistrationInfo> RegistrationsFor(Type serviceType)
            => _registrationManifest
                .GetOrDefault(serviceType)?
                .AsEnumerable();

        /// <summary>
        /// For testing purposes
        /// </summary>
        public IEnumerable<RegistrationInfo> RegistrationsFor<ServiceType>()
            => _registrationManifest
                .GetOrDefault(typeof(ServiceType))?
                .AsEnumerable();

        #region Method Accessor

        private static MethodInfo GetRegisterFactoryMethod()
        {
            ServiceRegistrar c = null;
            Expression<Func<Func<IResolverContract, IFake>, RegistryScope?, InterceptorProfile?, ServiceRegistrar>> expression = (f, a, b) => c.Register(f, a, b);
            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }
        #endregion

        #region inner types
        internal interface IFake { }

        internal class FakeImpl : IFake { }
        #endregion
    }
}
