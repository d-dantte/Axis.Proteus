using Axis.Luna.Extensions;
using Axis.Proteus.Interception;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.IoC
{
    public class ServiceResolver: IDisposable
    {
        private readonly IProxyGenerator _proxyGenerator;
        private readonly IResolverContract _resolverContract;
        private readonly Dictionary<Type, HashSet<ServiceRegistrar.RegistrationMap>> _interceptedRegistrations = new Dictionary<Type, HashSet<ServiceRegistrar.RegistrationMap>>();

        internal IEnumerable<ServiceRegistrar.RegistrationMap> Registrations => _interceptedRegistrations.Values.SelectMany();

        public ServiceResolver(
            IResolverContract resolverContract,
            IProxyGenerator proxyGenerator,
            IEnumerable<ServiceRegistrar.RegistrationMap> trackedRegistrations)
        {
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
            _resolverContract = resolverContract ?? throw new ArgumentNullException(nameof(resolverContract));

            // gather only registrations that are intercepted
            _ = trackedRegistrations
                .ThrowIfNull(new ArgumentNullException(nameof(trackedRegistrations)))
                .Where(registration => registration.InterceptorProfile != null)
                .Where(registration => !registration.IsDefault())
                .Aggregate(_interceptedRegistrations, (registrations, next) =>
                {
                    registrations
                        .GetOrAdd(next.ServiceType, t => new HashSet<ServiceRegistrar.RegistrationMap>())
                        .Add(next);

                    return registrations;
                });
        }

        public virtual void Dispose()
        {
            _resolverContract.Dispose();
        }


        /// <summary>
        /// Resolve one instance of the specified service
        /// </summary>
        /// <typeparam name="Service">The type of the service to be resovled</typeparam>
        /// <returns>The resolved service, or null if it was not registered</returns>
        public virtual Service Resolve<Service>() where Service : class
        {
            var instance = _resolverContract.Resolve<Service>();
            var instanceType = instance?.GetType();

            if (instance == null)
                return null;

            if (!instanceType.Implements(typeof(IProxyMarker))
                && TryFindServiceRegistration(typeof(Service), instanceType, out var registration))
            {
                return typeof(Service).IsClass
                    ? _proxyGenerator
                        .CreateClassProxyWithTarget(
                            typeof(Service),
                            new[] { typeof(IProxyMarker) },
                            instance,
                            registration.InterceptorProfile.Value.Interceptors.ToArray())
                        .As<Service>()
                    : _proxyGenerator
                        .CreateInterfaceProxyWithTarget(
                            typeof(Service),
                            new[] { typeof(IProxyMarker) },
                            instance,
                            registration.InterceptorProfile.Value.Interceptors.ToArray())
                        .As<Service>();
            }

            else
                return instance;
        }

        /// <summary>
        /// Resolve one instance of the specified service
        /// </summary>
        /// <param name="serviceType">The typeof the service to be resolved</param>
        /// <returns>The resolved service, or null if it was not registered</returns>
        public virtual object Resolve(Type serviceType)
        {
            var instance = _resolverContract.Resolve(serviceType ?? throw new ArgumentNullException(nameof(serviceType)));
            var instanceType = instance?.GetType();

            if (instance == null)
                return null;

            if (!instanceType.Implements(typeof(IProxyMarker))
                && TryFindServiceRegistration(serviceType, instanceType, out var registration))
            {
                return serviceType.IsClass
                    ? _proxyGenerator
                        .CreateClassProxyWithTarget(
                            serviceType,
                            new[] { typeof(IProxyMarker) },
                            instance,
                            registration.InterceptorProfile.Value.Interceptors.ToArray())
                    : _proxyGenerator
                        .CreateInterfaceProxyWithTarget(
                            serviceType,
                            new[] { typeof(IProxyMarker) },
                            instance,
                            registration.InterceptorProfile.Value.Interceptors.ToArray());
            }

            else
                return instance;
        }

        /// <summary>
        /// Resolves a collection of instances registered for a service
        /// </summary>
        /// <typeparam name="Service">The service to be resolved</typeparam>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        public virtual IEnumerable<Service> ResolveAll<Service>() where Service : class
        {
            var instances = _resolverContract.ResolveAll<Service>().ToArray();
            if (instances.Length == 0)
                return Array.Empty<Service>();

            return instances.Select((instance, index) =>
            {
                var instanceType = instance.GetType();

                if (!instanceType.Implements(typeof(IProxyMarker))
                    && TryFindServiceRegistration(typeof(Service), instanceType, out var registration))
                {
                    return typeof(Service).IsClass
                        ? _proxyGenerator
                            .CreateClassProxyWithTarget(
                                typeof(Service),
                                new[] { typeof(IProxyMarker) },
                                instance,
                                registration.InterceptorProfile.Value.Interceptors.ToArray())
                            .As<Service>()
                        : _proxyGenerator
                            .CreateInterfaceProxyWithTarget(
                                typeof(Service),
                                new[] { typeof(IProxyMarker) },
                                instance,
                                registration.InterceptorProfile.Value.Interceptors.ToArray())
                            .As<Service>();
                }

                else
                    return instance;
            });
        }

        /// <summary>
        /// Resolves a collection of instances registered for a service
        /// </summary>
        /// <param name="serviceType">The service to be resolved</param>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        public virtual IEnumerable<object> ResolveAll(Type serviceType)
        {
            var instances = _resolverContract.ResolveAll(serviceType).ToArray();
            if (instances.Length == 0)
                return Array.Empty<object>();

            return instances.Select((instance, index) =>
            {
                var instanceType = instance.GetType();

                if (!instanceType.Implements(typeof(IProxyMarker))
                    && TryFindServiceRegistration(serviceType, instanceType, out var registration))
                {
                    return serviceType.IsClass
                        ? _proxyGenerator
                            .CreateClassProxyWithTarget(
                                serviceType,
                                new[] { typeof(IProxyMarker) },
                                instance,
                                registration.InterceptorProfile.Value.Interceptors.ToArray())
                        : _proxyGenerator
                            .CreateInterfaceProxyWithTarget(
                                serviceType,
                                new[] { typeof(IProxyMarker) },
                                instance,
                                registration.InterceptorProfile.Value.Interceptors.ToArray());
                }

                else
                    return instance;
            });
        }


        private bool TryFindServiceRegistration(Type serviceType, Type instanceType, out ServiceRegistrar.RegistrationMap registration)
        {
            registration = default;

            if (!_interceptedRegistrations.ContainsKey(serviceType))
                return false;

            var registrations = _interceptedRegistrations[serviceType];
            var map = new ServiceRegistrar.RegistrationMap(serviceType, instanceType);

            if (!registrations.Contains(map))
                return false;

            registration = registrations.First(m => m.Equals(map));
            return true;
        }
    }
}
