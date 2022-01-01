using System;

namespace Axis.Proteus
{
    public static class TypeExtensions
    {
        public static bool IsPlainClass(this Type type) => type.IsClass && !typeof(Delegate).IsAssignableFrom(type);
    }
}
