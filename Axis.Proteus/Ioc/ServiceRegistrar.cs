using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Facilitates the registration of services against implementations.
    /// Registration model adhered to here is the many-to-many style, meaning any service can have any number of implementations, and each concreate type can implement any number of services.
    /// Implication of the above is that any "register" method can be called multiple times to append a unique registration of services and implementations
    /// </summary>
    public class ServiceRegistrar
    {
        private readonly IRegistrarContract _registrar;
        private readonly IProxyGenerator _proxyGenerator;
        private readonly HashSet<RegistrationMap> _trackedRegistrations = new HashSet<RegistrationMap>();
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
        {
            if (!CanRegister())
                throw new InvalidOperationException("Registry is locked");

            return Register(serviceType, serviceType, scope, interceptorProfile);
        }

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

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (!serviceType.IsAssignableFrom(concreteType))
                throw new IncompatibleTypesException(serviceType, concreteType);

            // if we can't add it, it's been registered already
            if (!_trackedRegistrations.Add(new RegistrationMap(serviceType, concreteType, interceptorProfile)))
                throw new DuplicateRegistrationException(serviceType);

            _ = _registrar.Register(serviceType, concreteType, scope);

            return this;
        }

        /// <summary>
        /// Register a factory method to be used in resolving instances of the service interface/type
        /// Note that the interception logic is baked into the resolution logic because there's no way to distinguish between
        /// the result of any two or more factory generated services.
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register(
            Type serviceType,
            Func<IResolverContract, object> factory,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
        {
            if (!CanRegister())
                throw new InvalidOperationException("Registry is locked");

            if (interceptorProfile == null)
                _ = _registrar.Register(serviceType, factory, scope);

            else
            {
                _ = _registrar.Register(
                    serviceType: serviceType,
                    scope: scope,
                    factory: resolver =>
                    {
                        var instance = factory.Invoke(resolver);
                        var proxy = serviceType.IsClass
                            ? _proxyGenerator.CreateClassProxyWithTarget(
                                serviceType,
                                new[] {typeof(IProxyMarker)},
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray())
                            : _proxyGenerator.CreateInterfaceProxyWithTarget(
                                serviceType,
                                new[] { typeof(IProxyMarker) },
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray());

                        return proxy;
                    });
            }

            return this;
        }

        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <typeparam name="Impl">The concrete type to be registered and resolved</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        public virtual ServiceRegistrar Register<Impl>(
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Impl : class => Register(typeof(Impl), scope, interceptorProfile);

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
            => Register(typeof(Service), typeof(Impl), scope, interceptorProfile);

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
            => Register(typeof(Service), factory, scope, interceptorProfile);

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
            if (!CanRegister())
                throw new InvalidOperationException("Registry is locked");

            concreteTypes?.ForAll(type => Register(serviceType, type, scope, interceptorProfile));

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
            if (CanRegister())
            {
                _serviceResolver = new ServiceResolver(
                    _registrar.BuildResolver(),
                    _proxyGenerator,
                    _trackedRegistrations);
                //clear the trackedRegistrations??
            }

            return _serviceResolver;
        }

        private bool CanRegister() => _serviceResolver == null;

        /// <summary>
        /// 
        /// </summary>
        public readonly struct RegistrationMap
        {
            /// <summary>
            /// 
            /// </summary>
            internal Type ServiceType { get; }

            /// <summary>
            /// 
            /// </summary>
            internal Type ImplementationType { get; }

            /// <summary>
            /// 
            /// </summary>
            internal InterceptorProfile? InterceptorProfile { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="serviceType"></param>
            /// <param name="implementationType"></param>
            /// <param name="interceptorProfile"></param>
            public RegistrationMap(Type serviceType, Type implementationType, InterceptorProfile? interceptorProfile = null)
            {
                ServiceType = serviceType
                    .ThrowIfNull(new ArgumentNullException(nameof(serviceType)))
                    .ThrowIf(t => !(t.IsPlainClass() || t.IsInterface), new ArgumentException($"{serviceType} must be a class or interface (not a delegate)"));

                ImplementationType = implementationType
                    .ThrowIfNull(new ArgumentNullException(nameof(implementationType)))
                    .ThrowIf(t => !t.IsPlainClass(), new ArgumentException());

                InterceptorProfile = interceptorProfile;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="concreteType"></param>
            /// <param name="interceptorProfile"></param>
            public RegistrationMap(Type concreteType, InterceptorProfile? interceptorProfile = null)
            : this(concreteType, concreteType, interceptorProfile)
            {
            }

            public override int GetHashCode() => HashCode.Combine(ServiceType, ImplementationType);

            public override bool Equals(object obj)
            {
                return obj is RegistrationMap other
                    && other.ServiceType == ServiceType
                    && other.ImplementationType == ImplementationType;
            }

            public override string ToString()
            => $"[{ServiceType?.FullName ?? null}::{ImplementationType?.FullName ?? null}]{(InterceptorProfile == null ? string.Empty : '*'.ToString())}";

            public static bool operator ==(RegistrationMap first, RegistrationMap second) => first.Equals(second);

            public static bool operator !=(RegistrationMap first, RegistrationMap second) => !(first == second);
        }
    }
}
