using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using Axis.Proteus.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct RegistrationInfo :
        IDefaultValueProvider<RegistrationInfo>
    {
        private readonly HashSet<IBindContext> _bindContexts;
        private readonly Type _serviceType;
        private readonly InterceptorProfile _profile;
        private readonly DeferredValue<int> _contextHash;

        /// <summary>
        /// The service type
        /// </summary>
        public Type ServiceType => _serviceType;

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
        public IBindContext[] BindContexts => _bindContexts?.ToArray() ?? Array.Empty<IBindContext>();

        /// <summary>
        /// The scope
        /// </summary>
        //public ResolutionScope Scope => _scope;

        /// <summary>
        /// The interception profile
        /// </summary>
        public InterceptorProfile Profile => _profile;

        public bool IsDefault =>
            _bindContexts is null
            && _serviceType is null
            && _profile.IsDefault;

        public static RegistrationInfo Default => throw new NotImplementedException();

        public RegistrationInfo(
            Type implementationType,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] bindContexts)
            : this(implementationType, IBindTarget.Of(implementationType), scope, profile, bindContexts)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="target"></param>
        /// <param name="scope"></param>
        /// <param name="profile"></param>
        /// <param name="bindContexts"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IncompatibleTypesException"></exception>
        public RegistrationInfo(
            Type serviceType,
            IBindTarget target,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] bindContexts)
        {
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(bindContexts);

            _serviceType = serviceType;
            _profile = profile;
            _bindContexts = new HashSet<IBindContext>() { IBindContext.Of(target, scope) };

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

            var _this = this;
            _contextHash = DeferredValue<int>.Of(() =>
            {
                return _this._bindContexts.Aggregate(0, HashCode.Combine);
            });
        }

        public override int GetHashCode() => HashCode.Combine(ServiceType, Profile, _contextHash.Value);

        public override bool Equals(object? obj)
        {
            return obj is RegistrationInfo other
                && other.Profile.Equals(Profile)
                && other.ServiceType.IsNullOrEquals(ServiceType)
                && other._bindContexts.IsNullOrTrue(_bindContexts, (x, y) => x.SetEquals(y));
        }

        private static bool ContainsDefaultContext(
            IEnumerable<IBindContext> contexts)
            => contexts.Any(c => c is IBindContext.DefaultContext dc && !dc.IsDefault);

        public static bool operator ==(RegistrationInfo first, RegistrationInfo second) => first.Equals(second);
        public static bool operator !=(RegistrationInfo first, RegistrationInfo second) => !first.Equals(second);
    }
}
