using Axis.Luna.Extensions;
using Axis.Proteus;
using Axis.Proteus.IoC;
using Axis.Proteus.SimpleInjector.NamedContext;
using Castle.DynamicProxy;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Axis.Proteus.SimpleInjector
{
    public class SimpleInjectorResolver : IResolverContract
    {
        private readonly Container _container;
        private readonly IProxyGenerator _proxyGenerator;
        private readonly RegistryManifest _manifest;

        public SimpleInjectorResolver(
            Container container,
            IProxyGenerator proxyGenerator,
            RegistryManifest manifest)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }

        public void Dispose() => _container.Dispose();

        public Service Resolve<Service>(ResolutionContextName contextName = default)
            where Service : class
            => Resolve(typeof(Service), contextName).As<Service>();

        public object Resolve(Type serviceType, ResolutionContextName contextName = default)
        {
            var registration = _manifest.RootRegistrationFor(serviceType);

            if (registration == null)
                return null;

            var instance =
                contextName != default
                && registration.Value.TryGetNamedContext(contextName, out var namedContext)
                ? _container
                    .GetInstance(
                        contextName.ToNamedContextType(
                            serviceType,
                            namedContext.Target.Type))
                    .As<NamedContextContainer>().Instance
                : _container.GetInstance(serviceType);


            if (registration.Value.Profile == default)
                return instance;

            if (registration.Value.DefaultContext.Target is IBindTarget.TypeTarget typeTarget
                && typeTarget.Type != instance.GetType())
                throw new InvalidOperationException($"Registration mis-match: registration type: {typeTarget.Type}, resolved type: {instance.GetType()}");

            else if (registration.Value.DefaultContext.Target is IBindTarget.FactoryTarget factoryTarget
                && !factoryTarget.Type.IsAssignableFrom(instance.GetType()))
                throw new InvalidOperationException($"Registration mis-match: registration type: {factoryTarget.Type}, resolved type: {instance.GetType()}");

            return serviceType.IsClass
                ? _proxyGenerator
                    .CreateClassProxyWithTarget(
                        serviceType,
                        instance,
                        registration.Value.Profile.Interceptors.ToArray())
                : _proxyGenerator
                    .CreateInterfaceProxyWithTarget(
                        serviceType,
                        instance,
                        registration.Value.Profile.Interceptors.ToArray());
        }

        public IEnumerable<Service> ResolveAll<Service>()
            where Service : class
            => ResolveAll(typeof(Service)).Select(Common.As<Service>);

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _container
                .GetAllInstances(serviceType)
                .PairWith(_manifest.CollectionRegistrationsFor(serviceType))
                .Select((pair, index) =>
                {
                    var instance = pair.Key;

                    if (instance == null)
                        return null;

                    var info = pair.Value.ThrowIfDefault(
                        new InvalidOperationException($"Invalid registration found at index: {index}, for service {serviceType}"));

                    if (info.Profile == default)
                        return instance;

                    if (info.DefaultContext.Target is IBindTarget.TypeTarget implType
                        && implType.Type != instance.GetType())
                        throw new InvalidOperationException($"Registration mis-match: registration type: {info.DefaultContext.Target.Type}, resolved type: {instance.GetType()}");

                    else if (info.DefaultContext.Target is IBindTarget.FactoryTarget implFactory
                        && !implFactory.Type.IsAssignableFrom(instance.GetType()))
                        throw new InvalidOperationException($"Registration mis-match: registration type: {info.DefaultContext.Target.Type}, resolved type: {instance.GetType()}");

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
    }
}
