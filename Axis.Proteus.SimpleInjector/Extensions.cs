using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    internal static class Extensions
    {
        public static Lifestyle ToSimpleInjectorLifeStyle(this ResolutionScope scope)
        {
            if (scope.Name == ResolutionScope.Transient.Name)
                return Lifestyle.Transient;

            else if (scope.Name == ResolutionScope.DefaultScope.Name)
                return Lifestyle.Scoped;

            else if (scope.Name == ResolutionScope.Singleton.Name)
                return Lifestyle.Singleton;

            else return Lifestyle.Scoped; //<-- till custom scopes are supported
        }
    }
}
