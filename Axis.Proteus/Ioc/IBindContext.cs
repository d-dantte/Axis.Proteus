using Axis.Luna.Common;
using Axis.Luna.Extensions;
using System;
using System.Reflection;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// A bind context specifies a condition that has to be met for the given registered type to be resolved. There are 3 contexts:
    /// <list type="number">
    ///     <item>
    ///         <see cref="ParameterContext"/>: This specifies conditions in the context of injecting the target type into a method/constructor
    ///     </item>
    ///     <item>
    ///         <see cref="PropertyContext"/>: This specifies conditions in the context of injecting the target type into an instances property
    ///     </item>
    ///     <item>
    ///         <see cref="NamedContext"/>: This specifies conditions in the context of resolving the type directly from the IoC container.
    ///     </item>
    ///     <item>
    ///         <see cref="DefaultContext"/>: When there is no other context present, this is used. This context MUST be present. It is only ever created internally by the
    ///         <see cref="RegistrationInfo"/> instance upon creation.
    ///     </item>
    /// </list>
    /// </summary>
    public interface IBindContext
    {
        /// <summary>
        /// Creates a <see cref="ParameterContext"/>
        /// </summary>
        /// <param name="target">The target type of the injection</param>
        /// <param name="predicate">The predicate that must be true for this injection to be made</param>
        public static IBindContext OfParameter(
            IBindTarget target,
            Func<ParameterInfo, bool> predicate)
            => new ParameterContext(target, predicate);

        /// <summary>
        /// Creates a <see cref="PropertyContext"/>
        /// </summary>
        /// <param name="target">The target type of the injection</param>
        /// <param name="predicate">The predicate that must be true for this injection to be made</param>
        public static IBindContext OfProperty(
            IBindTarget target,
            Func<PropertyInfo, bool> predicate)
            => new PropertyContext(target, predicate);

        /// <summary>
        /// Creates a <see cref="NamedContext"/>
        /// </summary>
        /// <param name="name">The unique name given to this context. The name supplied upon resolution must match before the resolution is made/param>
        /// <param name="target">The target type of the injection</param>
        public static IBindContext Of(
            string name,
            IBindTarget target)
            => new NamedContext(name, target);

        /// <summary>
        /// Creates a <see cref="DefaultContext"/>. When no context is supplied, or all other contexts do not match, the MANDATORY default context is used.
        /// </summary>
        /// <param name="target">The target type of the injection</param>
        internal static IBindContext Of(IBindTarget target) => new DefaultContext(target);

        #region Members
        /// <summary>
        /// The target type being bound to
        /// </summary>
        IBindTarget Target { get; }
        #endregion

        #region Union

        /// <summary>
        /// The default context. There must be only one of these in any <see cref="RegistrationInfo"/> instance.
        /// </summary>
        public readonly struct DefaultContext :
            IBindContext,
            IDefaultValueProvider<DefaultContext>
        {
            public IBindTarget Target { get; }

            public bool IsDefault => Target is null;

            public static DefaultContext Default => default;

            internal DefaultContext(IBindTarget target)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
            }

            public override int GetHashCode() => HashCode.Combine(Target);

            public override bool Equals(object obj)
            {
                return obj is DefaultContext other
                    && Target.IsNullOrEquals(other.Target);
            }

            public static bool operator ==(DefaultContext first, DefaultContext second) => first.NullOrEquals(second);

            public static bool operator !=(DefaultContext first, DefaultContext second) => !first.NullOrEquals(second);


            public static implicit operator DefaultContext(IBindTarget.FactoryTarget target) => new(target);

            public static implicit operator DefaultContext(IBindTarget.TypeTarget target) => new(target);
        }

        public readonly struct NamedContext : IBindContext
        {
            public IBindTarget Target { get; }

            /// <summary>
            /// The unique name differentiating instance resolution intent
            /// </summary>
            public ResolutionContextName Name { get; }

            internal NamedContext(string name, IBindTarget target)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
                Name = name; // the resolution context name verifies the value.
            }

            public override int GetHashCode() => HashCode.Combine(Target, Name);

            public override bool Equals(object obj)
            {
                return obj is NamedContext other
                    && Target.IsNullOrEquals(other.Target)
                    && other.Name.Equals(Name);
            }

            public static bool operator ==(NamedContext first, NamedContext second) => first.NullOrEquals(second);
            public static bool operator !=(NamedContext first, NamedContext second) => !first.NullOrEquals(second);
        }

        public readonly struct ParameterContext :
            IBindContext,
            IDefaultValueProvider<ParameterContext>
        {
            public IBindTarget Target { get; }

            /// <summary>
            /// The condition that must be met for this context to be applied
            /// </summary>
            public Func<ParameterInfo, bool> Predicate { get; }

            public bool IsDefault => Predicate is null && Target is null;

            public static ParameterContext Default => default;

            internal ParameterContext(
                IBindTarget target,
                Func<ParameterInfo, bool> predicate)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
                Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            }

            public override int GetHashCode() => HashCode.Combine(Target, Predicate);

            public override bool Equals(object obj)
            {
                return obj is ParameterContext other
                    && Target.IsNullOrEquals(other.Target)
                    && Predicate.IsNullOrEquals(other.Predicate);
            }

            public static bool operator ==(ParameterContext first, ParameterContext second) => first.NullOrEquals(second);
            public static bool operator !=(ParameterContext first, ParameterContext second) => !first.NullOrEquals(second);
        }

        public readonly struct PropertyContext :
            IBindContext,
            IDefaultValueProvider<PropertyContext>
        {
            public IBindTarget Target { get; }

            /// <summary>
            /// The condition that must be met for this context to be applied
            /// </summary>
            public Func<PropertyInfo, bool> Predicate { get; }

            public bool IsDefault => Predicate is null && Target is null;

            public static PropertyContext Default => default;

            internal PropertyContext(
                IBindTarget target,
                Func<PropertyInfo, bool> predicate)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
                Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            }

            public override int GetHashCode() => HashCode.Combine(Target, Predicate);

            public override bool Equals(object obj)
            {
                return obj is PropertyContext other
                    && Target.IsNullOrEquals(other.Target)
                    && Predicate.IsNullOrEquals(other.Predicate);
            }

            public static bool operator ==(PropertyContext first, PropertyContext second) => first.NullOrEquals(second);
            public static bool operator !=(PropertyContext first, PropertyContext second) => !first.NullOrEquals(second);
        }

        #endregion
    }
}
