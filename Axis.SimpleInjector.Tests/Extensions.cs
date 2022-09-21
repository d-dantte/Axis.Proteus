using SimpleInjector;

namespace Axis.SimpleInjector.Tests
{
    public static class Extensions
    {
        public static bool ContainsUnverifiedRegistration(this
            Container container,
            Type serviceType,
            Type implementationType)
        {
            return container
                .GetCurrentRegistrations()
                .Where(registration => registration.ServiceType == serviceType)
                .Where(registation => registation.ImplementationType == implementationType)
                .Any();
        }

        public static bool ContainsUnverifiedRegistration(this
            Container container,
            Type serviceType)
            => container.ContainsUnverifiedRegistration(serviceType, serviceType);
    }
}
