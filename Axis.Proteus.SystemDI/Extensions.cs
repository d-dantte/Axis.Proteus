using Axis.Proteus.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Proteus.SystemDI
{
    public static class Extensions
    {
        public static IServiceCollection AddProteusResolverContract(this IServiceCollection services)
        {
            services.Add(ServiceDescriptor.Describe(
                serviceType: typeof(IResolverContract),
                lifetime: ServiceLifetime.Singleton,
                implementationFactory: serviceProvider => new DIResolver(services, serviceProvider)));

            return services;
        }
    }
}
