using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus
{
    public static class CommonExtensions
    {
        public static bool TryGetNamedContext(this
            RegistrationInfo registrationInfo,
            ResolutionContextName name,
            out IBindContext.NamedContext context)
        {
            var _context = registrationInfo.BindContexts
                .Where(context =>
                    context is IBindContext.NamedContext namedContext
                    && namedContext.Name == name)
                .FirstOrDefault();

            if (_context is null)
            {
                context = default;
                return false;
            }

            context = _context.As<IBindContext.NamedContext>();
            return true;
        }

        internal static bool IsNullOrEquals<TValue>(this
            TValue first,
            TValue second)
            where TValue : class
            => EqualityComparer<TValue>.Default.Equals(first, second);

        internal static bool IsNullOrTrue<TValue>(this
            TValue first,
            TValue second,
            Func<TValue, TValue, bool> predicate)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(predicate);

            if (first is null && second is null)
                return true;

            if (first is not null && second is not null)
                return predicate.Invoke(first, second);

            return false;
        }
    }
}
