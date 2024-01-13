using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    internal static class Extensions
    {
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
    }
}
