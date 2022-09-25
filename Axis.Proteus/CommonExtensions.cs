using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
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
            context = registrationInfo.BindContexts
                .Where(context =>
                    context is IBindContext.NamedContext namedContext
                    && namedContext.Name == name)
                .FirstOrDefault()
                .As<IBindContext.NamedContext>();

            return context != null;
        }
    }
}
