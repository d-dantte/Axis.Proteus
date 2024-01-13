using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.IoC
{
    public readonly struct RegistrationInfo
    {
        private readonly HashSet<IBindContext> _bindContexts;

        /// <summary>
        /// The service type
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// The default context. 
        /// Note that the following will always throw an <c>InvalidOperationException</c>
        /// <code>
        /// defualt(<see cref="RegistrationInfo"/>).DefaultContext
        /// </code>
        /// </summary>
        public IBindContext.DefaultContext DefaultContext => 
            _bindContexts?.FirstOrDefault().As<IBindContext.DefaultContext>()
            ?? throw new InvalidOperationException($"The default {typeof(IBindContext)} is missing");

        /// <summary>
        /// All available <see cref="IBindContext"/> instances applicable to this registration
        /// </summary>
        public IBindContext[] BindContexts => _bindContexts?.ToArray();

        /// <summary>
        /// The scope
        /// </summary>
        public RegistryScope Scope { get; }

        /// <summary>
        /// The interception profile
        /// </summary>
        public InterceptorProfile Profile { get; }

        public RegistrationInfo(
            Type implementationType,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] bindContexts)
            : this(implementationType, IBindTarget.Of(implementationType), scope, profile, bindContexts)
        {
        }

        public RegistrationInfo(
            Type serviceType,
            IBindTarget target,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] bindContexts)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Scope = scope;
            Profile = profile;
            _bindContexts = new HashSet<IBindContext>() { IBindContext.Of(target) };

            if (ContainsDefaultContext(bindContexts))
                throw new ArgumentException($"Invalid {nameof(bindContexts)}: contains {typeof(IBindContext.DefaultContext)} instance.");

            if (!serviceType.IsAssignableFrom(target.Type))
                throw new IncompatibleTypesException(serviceType, target.Type);

            // add other contexts if available
            var namedHash = new HashSet<string>();
            foreach(var context in bindContexts)
            {
                if (context is null)
                    throw new ArgumentException($"Invalid {nameof(bindContexts)}: contains null value");

                // deny duplicate named context names
                else if (context is IBindContext.NamedContext namedContext
                    && !namedHash.Add(namedContext.Name.Name))
                    throw new ArgumentException(
                        $"Invalid {nameof(bindContexts)}: Duplicate {typeof(IBindContext.NamedContext)} name detected '{namedContext.Name}'");

                else if (!serviceType.IsAssignableFrom(context.Target.Type))
                    throw new IncompatibleTypesException(serviceType, target.Type);

                _ = _bindContexts
                    .Add(context)
                    .ThrowIf(false, _ => new ArgumentException(
                        $"Invalid {nameof(bindContexts)}: Duplicate {typeof(IBindContext)} detected"));
            }
        }

        public override int GetHashCode() => HashCode.Combine(ServiceType, Scope, Profile, ContextHashCode());

        public override bool Equals(object obj)
        {
            return obj is RegistrationInfo other
                && other.Scope.Equals(Scope)
                && other.Profile.Equals(Profile)
                && other.ServiceType.IsNullOrEquals(ServiceType)
                && other._bindContexts.IsNullOrTrue(_bindContexts, (x, y) => x.SetEquals(y));
        }

        private int ContextHashCode() => Common.ValueHash(_bindContexts ?? Enumerable.Empty<IBindContext>());

        private static bool ContainsDefaultContext(IEnumerable<IBindContext> contexts) => contexts.Any(c => c is IBindContext.DefaultContext);

        public static bool operator ==(RegistrationInfo first, RegistrationInfo second) => first.Equals(second);
        public static bool operator !=(RegistrationInfo first, RegistrationInfo second) => !first.Equals(second);
    }
}
