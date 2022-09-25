using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
    ///         <see cref="DefaultContext"/>: When there is no other context present this is used. This context MUST be present. It is only ever created internally by the
    ///         <see cref="RegistrationInfo"/> instance upon creation.
    ///     </item>
    /// </list>
    /// </summary>
    public interface IBindContext
    {
        /// <summary>
        /// Creates a <see cref="ParameterContext"/>
        /// </summary>
        /// <param name="parameter">The (constructor) parameter receiving the injection</param>
        /// <param name="target">The target type of the injection</param>
        /// <param name="predicate">The predicate that must be true for this injection to be made</param>
        public static IBindContext Of(
            ParameterInfo parameter,
            IBindTarget target,
            Func<ParameterInfo, bool> predicate)
            => new ParameterContext(parameter, target, predicate);

        /// <summary>
        /// Creates a <see cref="PropertyContext"/>
        /// </summary>
        /// <param name="property">The property receiving the injection</param>
        /// <param name="target">The target type of the injection</param>
        /// <param name="predicate">The predicate that must be true for this injection to be made</param>
        public static IBindContext Of(
            PropertyInfo property,
            IBindTarget target,
            Func<PropertyInfo, bool> predicate)
            => new PropertyContext(property, target, predicate);

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
        public class DefaultContext : IBindContext
        {
            public IBindTarget Target { get; }

            internal DefaultContext(IBindTarget target)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
            }

            public override int GetHashCode() => HashCode.Combine(Target);

            public override bool Equals(object obj)
            {
                return obj is DefaultContext other
                    && other.Target.Equals(Target); // Target MUST never be null
            }

            public static bool operator ==(DefaultContext first, DefaultContext second) => first.NullOrEquals(second);
            public static bool operator !=(DefaultContext first, DefaultContext second) => !first.NullOrEquals(second);
        }

        public class NamedContext : IBindContext
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
                    && other.Target.Equals(Target)
                    && other.Name.Equals(Name);
            }

            public static bool operator ==(NamedContext first, NamedContext second) => first.NullOrEquals(second);
            public static bool operator !=(NamedContext first, NamedContext second) => !first.NullOrEquals(second);
        }

        public class ParameterContext : IBindContext
        {
            public IBindTarget Target { get; }

            /// <summary>
            /// The parameter receiving the instance of the injection
            /// </summary>
            public ParameterInfo Parameter { get; }

            /// <summary>
            /// The condition that must be met for this context to be applied
            /// </summary>
            public Func<ParameterInfo, bool> Predicate { get; }

            internal ParameterContext(
                ParameterInfo parameter,
                IBindTarget target,
                Func<ParameterInfo, bool> predicate)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
                Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
                Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            }

            public override int GetHashCode() => HashCode.Combine(Target, Parameter, Predicate);

            public override bool Equals(object obj)
            {
                return obj is ParameterContext other
                    && other.Target.Equals(Target)
                    && other.Parameter.Equals(Parameter)
                    && other.Predicate.Equals(Predicate);
            }

            public static bool operator ==(ParameterContext first, ParameterContext second) => first.NullOrEquals(second);
            public static bool operator !=(ParameterContext first, ParameterContext second) => !first.NullOrEquals(second);
        }

        public class PropertyContext : IBindContext
        {
            public IBindTarget Target { get; }

            /// <summary>
            /// The property receiving the instance of the injection
            /// </summary>
            public PropertyInfo Property { get; }

            /// <summary>
            /// The condition that must be met for this context to be applied
            /// </summary>
            public Func<PropertyInfo, bool> Predicate { get; }

            internal PropertyContext(
                PropertyInfo property,
                IBindTarget target,
                Func<PropertyInfo, bool> predicate)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
                Property = property ?? throw new ArgumentNullException(nameof(property));
                Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            }

            public override int GetHashCode() => HashCode.Combine(Target, Property, Predicate);

            public override bool Equals(object obj)
            {
                return obj is PropertyContext other
                    && other.Target.Equals(Target)
                    && other.Property.Equals(Property)
                    && other.Predicate.Equals(Predicate);
            }

            public static bool operator ==(PropertyContext first, PropertyContext second) => first.NullOrEquals(second);
            public static bool operator !=(PropertyContext first, PropertyContext second) => !first.NullOrEquals(second);
        }

        #endregion
    }
}
