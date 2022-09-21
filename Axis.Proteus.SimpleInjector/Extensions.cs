using Axis.Proteus.IoC;
using SimpleInjector;
using System;
using System.Linq;

namespace Axis.Proteus.SimpleInjector
{
    internal static class Extensions
    {
        public static Lifestyle ToSimpleInjectorLifeStyle(this RegistryScope? scope)
        {
            if (scope == null)
                return Lifestyle.Transient;

            else return scope.Value.ToSimpleInjectorLifeStyle();
        }

        public static Lifestyle ToSimpleInjectorLifeStyle(this RegistryScope scope)
        {
            if (scope.Name == RegistryScope.Transient.Name)
                return Lifestyle.Transient;

            else if (scope.Name == RegistryScope.DefaultScope.Name)
                return Lifestyle.Scoped;

            else if (scope.Name == RegistryScope.Singleton.Name)
                return Lifestyle.Singleton;

            else return Lifestyle.Scoped; //<-- till custom scopes are supported
        }

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
