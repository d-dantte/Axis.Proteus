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
        private readonly Dictionary<Type, List<RegistrationInfo>> _manifest = new Dictionary<Type, List<RegistrationInfo>>();

        public IEnumerable<RegistrationInfo> Registrations => _manifest.Values.SelectMany();

        public ServiceResolver(
            IResolverContract resolverContract,
            IProxyGenerator proxyGenerator,
            IEnumerable<RegistrationInfo> flatManifest)
            :this(resolverContract, proxyGenerator, ToDictionary(flatManifest))
        {
        }

        public ServiceResolver(
            IResolverContract resolverContract,
            IProxyGenerator proxyGenerator,
            Dictionary<Type, List<RegistrationInfo>> registrationManifest)
        {
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
            _resolverContract = resolverContract ?? throw new ArgumentNullException(nameof(resolverContract));
            _manifest = registrationManifest ?? throw new ArgumentNullException(nameof(registrationManifest));
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

            if (instance == null)
                return null;

            var info = _manifest[typeof(Service)]
                .FirstOrDefault()
                .ThrowIfDefault(new InvalidOperationException($"No registration found in the manifest for Service: {typeof(Service)}"));

            if (info.Profile == default)
                return instance;

            return typeof(Service).IsClass
                ? _proxyGenerator
                    .CreateClassProxyWithTarget(
                        typeof(Service),
                        new[] { typeof(IProxyMarker) },
                        instance,
                        info.Profile.Interceptors.ToArray())
                    .As<Service>()
                : _proxyGenerator
                    .CreateInterfaceProxyWithTarget(
                        typeof(Service),
                        new[] { typeof(IProxyMarker) },
                        instance,
                        info.Profile.Interceptors.ToArray())
                    .As<Service>();
        }

        /// <summary>
        /// Resolve one instance of the specified service
        /// </summary>
        /// <param name="serviceType">The typeof the service to be resolved</param>
        /// <returns>The resolved service, or null if it was not registered</returns>
        public virtual object Resolve(Type serviceType)
        {
            var instance = _resolverContract.Resolve(serviceType);

            if (instance == null)
                return null;

            var info = _manifest[serviceType]
                .FirstOrDefault()
                .ThrowIfDefault(new InvalidOperationException($"No registration found in the manifest for Service: {serviceType}"));

            if (info.Profile == default)
                return instance;

            return serviceType.IsClass
                ? _proxyGenerator
                    .CreateClassProxyWithTarget(
                        serviceType,
                        new[] { typeof(IProxyMarker) },
                        instance,
                        info.Profile.Interceptors.ToArray())
                : _proxyGenerator
                    .CreateInterfaceProxyWithTarget(
                        serviceType,
                        new[] { typeof(IProxyMarker) },
                        instance,
                        info.Profile.Interceptors.ToArray());
        }

        /// <summary>
        /// Resolves a collection of instances registered for a service
        /// </summary>
        /// <typeparam name="Service">The service to be resolved</typeparam>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        public virtual IEnumerable<Service> ResolveAll<Service>() where Service : class
        {
            return _resolverContract
                .ResolveAll<Service>()
                .PairWith(_manifest[typeof(Service)])
                .Select((pair, index) =>
                {
                    var instance = pair.first;

                    if (instance == null)
                        return null;

                    var info = pair.second.ThrowIfDefault(
                        new InvalidOperationException($"Invalid registration found at index: {index}, for service {typeof(Service)}"));

                    if (info.Profile == default)
                        return instance;

                    return typeof(Service).IsClass
                        ? _proxyGenerator
                            .CreateClassProxyWithTarget(
                                typeof(Service),
                                new[] { typeof(IProxyMarker) },
                                instance,
                                info.Profile.Interceptors.ToArray())
                            .As<Service>()
                        : _proxyGenerator
                            .CreateInterfaceProxyWithTarget(
                                typeof(Service),
                                new[] { typeof(IProxyMarker) },
                                instance,
                                info.Profile.Interceptors.ToArray())
                            .As<Service>();
                });
        }

        /// <summary>
        /// Resolves a collection of instances registered for a service
        /// </summary>
        /// <param name="serviceType">The service to be resolved</param>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        public virtual IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _resolverContract
                .ResolveAll(serviceType)
                .PairWith(_manifest[serviceType])
                .Select((pair, index) =>
                {
                    var instance = pair.first;

                    if (instance == null)
                        return null;

                    var info = pair.second.ThrowIfDefault(
                        new InvalidOperationException($"Invalid registration found at index: {index}, for service {serviceType}"));

                    if (info.Profile == default)
                        return instance;

                    return serviceType.IsClass
                        ? _proxyGenerator
                            .CreateClassProxyWithTarget(
                                serviceType,
                                new[] { typeof(IProxyMarker) },
                                instance,
                                info.Profile.Interceptors.ToArray())
                        : _proxyGenerator
                            .CreateInterfaceProxyWithTarget(
                                serviceType,
                                new[] { typeof(IProxyMarker) },
                                instance,
                                info.Profile.Interceptors.ToArray());
                });
        }

        private static Dictionary<Type, List<RegistrationInfo>> ToDictionary(IEnumerable<RegistrationInfo> flatManifest)
        {
            return flatManifest
                .GroupBy(info => info.ServiceType)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
