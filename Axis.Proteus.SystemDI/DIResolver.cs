using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Axis.Proteus.SystemDI
{
    public sealed class DIResolver : IResolverContract
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceDescriptors;

        public DIResolver(
            IServiceCollection serviceDescriptors,
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(serviceDescriptors);

            _serviceProvider = serviceProvider;
            _serviceDescriptors = serviceDescriptors;
        }

        public void Dispose() { }

        public Service? Resolve<Service>(
            ResolutionContextName contextName = default)
            where Service : class
            => contextName.IsDefault switch
            {
                true => _serviceProvider.GetService<Service>(),
                false => _serviceProvider.GetKeyedService<Service>(contextName.Name)
            };

        public object? Resolve(
            Type serviceType,
            ResolutionContextName contextName = default)
            => contextName.IsDefault switch
            {
                true => _serviceProvider.GetService(serviceType),
                false => Result
                    .Of(() => _serviceProvider.GetRequiredKeyedService(
                        serviceType,
                        contextName.Name))
                    .MapError(_ => null!)
                    .Resolve()
            };

        public IEnumerable<Service?> ResolveAll<Service>()
            where Service : class
            => _serviceProvider.GetServices<Service>();

        public IEnumerable<object?> ResolveAll(
            Type serviceType)
            => _serviceProvider.GetServices(serviceType);

        public ReadOnlyDictionary<Type, RegistrationInfo> RootManifest()
        {
            return _serviceDescriptors
                .ToDictionary(
                    d => d.ServiceType,
                    ToRegistrationInfo)
                .ApplyTo(map => new ReadOnlyDictionary<Type, RegistrationInfo>(map));
        }

        public ReadOnlyDictionary<Type, RegistrationInfo[]> CollectionManifest()
        {
            return _serviceDescriptors
                .GroupBy(d => d.ServiceType)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(ToRegistrationInfo).ToArray())
                .ApplyTo(map => new ReadOnlyDictionary<Type, RegistrationInfo[]>(map));
        }

        private RegistrationInfo ToRegistrationInfo(
            ServiceDescriptor serviceDescriptor)
        {
            return new RegistrationInfo(
                serviceType: serviceDescriptor.ServiceType,
                target: ToBindTarget(serviceDescriptor),
                scope: ToRegistryScope(serviceDescriptor.Lifetime));
        }

        private static IBindTarget ToBindTarget(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationType is not null)
                return IBindTarget.Of(serviceDescriptor.ImplementationType);

            if (serviceDescriptor.KeyedImplementationType is not null)
                return IBindTarget.Of(serviceDescriptor.KeyedImplementationType);

            if (serviceDescriptor.ImplementationInstance is not null)
                return IBindTarget.Of(_ => serviceDescriptor.ImplementationInstance);

            if (serviceDescriptor.KeyedImplementationInstance is not null)
                return IBindTarget.Of(_ => serviceDescriptor.KeyedImplementationInstance);

            if (serviceDescriptor.ImplementationFactory is not null)
                return IBindTarget.Of(contract => contract
                    .As<DIResolver>()._serviceProvider
                    .ApplyTo(serviceDescriptor.ImplementationFactory));

            if (serviceDescriptor.KeyedImplementationFactory is not null)
                return IBindTarget.Of(contract =>
                {
                    return serviceDescriptor.KeyedImplementationFactory.Invoke(
                        contract.As<DIResolver>()._serviceProvider,
                        serviceDescriptor.ServiceKey);
                });

            throw new InvalidOperationException(
                $"Invalid {nameof(serviceDescriptor)}: no suitable bind target");
        }

        private static ResolutionScope ToRegistryScope(
            ServiceLifetime lifetime)
            => lifetime switch
            {
                ServiceLifetime.Singleton => ResolutionScope.Singleton,
                ServiceLifetime.Transient => ResolutionScope.Transient,
                ServiceLifetime.Scoped
                or _ => ResolutionScope.DefaultScope
            };
    }
}
