using Axis.Luna.Extensions;
using Axis.Proteus.IoC2;
using Castle.DynamicProxy;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Axis.Proteus.SimpleInjector.IoC2
{
    public class SimpleInjectorResolver : IResolverContract
    {
        private readonly Container _container;
        private readonly IProxyGenerator _proxyGenerator;
        private readonly IoC.RegistryManifest _manifest;

        public SimpleInjectorResolver(
            Container container,
            IProxyGenerator proxyGenerator,
            IoC.RegistryManifest manifest)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }

        public void Dispose() => _container.Dispose();

        public ReadOnlyDictionary<Type, IoC.RegistrationInfo[]> Manifest()
            => _manifest
                .Registrations()
                .GroupBy(info => info.ServiceType)
                .ToDictionary(group => group.Key, group => group.ToArray())
                .ApplyTo(dict => new ReadOnlyDictionary<Type, IoC.RegistrationInfo[]>(dict));

        public Service Resolve<Service>()
            where Service : class
            => ResolveAll<Service>().First();

        public object Resolve(Type serviceType)
            => ResolveAll(serviceType).First();

        public IEnumerable<Service> ResolveAll<Service>()
            where Service : class
        {
            return _container
                .GetAllInstances<Service>()
                .PairWith(_manifest.RegistrationsFor<Service>())
                .Select((pair, index) =>
                {
                    var instance = pair.Key;

                    if (instance == null)
                        return null;

                    var info = pair.Value.ThrowIfDefault(
                        new InvalidOperationException($"Invalid registration found at index: {index}, for service {typeof(Service)}"));

                    if (info.Profile == default)
                        return instance;

                    return typeof(Service).IsClass
                        ? _proxyGenerator
                            .CreateClassProxyWithTarget(
                                typeof(Service),
                                instance,
                                info.Profile.Interceptors.ToArray())
                            .As<Service>()
                        : _proxyGenerator
                            .CreateInterfaceProxyWithTarget(
                                typeof(Service),
                                instance,
                                info.Profile.Interceptors.ToArray())
                            .As<Service>();
                });
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _container
                .GetAllInstances(serviceType)
                .PairWith(_manifest.RegistrationsFor(serviceType))
                .Select((pair, index) =>
                {
                    var instance = pair.Key;

                    if (instance == null)
                        return null;

                    var info = pair.Value.ThrowIfDefault(
                        new InvalidOperationException($"Invalid registration found at index: {index}, for service {serviceType}"));

                    if (info.Profile == default)
                        return instance;

                    return serviceType.IsClass
                        ? _proxyGenerator
                            .CreateClassProxyWithTarget(
                                serviceType,
                                instance,
                                info.Profile.Interceptors.ToArray())
                        : _proxyGenerator
                            .CreateInterfaceProxyWithTarget(
                                serviceType,
                                instance,
                                info.Profile.Interceptors.ToArray());
                });
        }
    }
}
