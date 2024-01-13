using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using System;
using System.Reflection;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Represents the target/implementation of a Registration. This is either a type, or a factory lambda.
    /// Currently, the Target Type isn't validate beyond checking that it is not null.
    /// </summary>
    public interface IBindTarget
    {
        /// <summary>
        /// Creates an <see cref="IBindTarget"/> of the given type.
        /// </summary>
        /// <param name="t">The implementation type</param>
        public static IBindTarget Of(Type t) => new TypeTarget(t);

        /// <summary>
        /// Creates an <see cref="IBindTarget"/> of the given delegate and return type. Note that this target instance throws an exception 
        /// if the Target type and the delegate's return type aren't compatible.
        /// </summary>
        /// <param name="t">The implementation type</param>
        /// <param name="factory">The factory method that produces the instance of the implementation type</param>
        public static IBindTarget Of(Func<IResolverContract, object> factory) => new FactoryTarget(factory);

        #region interface members
        Type Type { get; }
        #endregion

        #region Union types
        /// <summary>
        /// Represents a binding that targets a type: meaning resolution of this registration will be done by activating a
        /// new instance of the target type - this is done by the underlying IoC container.
        /// </summary>
        public class TypeTarget : IBindTarget
        {
            public Type Type { get; }

            internal TypeTarget(Type type)
            {
                Type = type ?? throw new ArgumentNullException(nameof(type));
            }

            public override int GetHashCode() => Type.GetHashCode();

            public override bool Equals(object obj)
            {
                return obj is TypeTarget other
                    && other.Type.Equals(Type);
            }

            public static bool operator ==(TypeTarget first, TypeTarget second) => first.IsNullOrEquals(second);
            public static bool operator !=(TypeTarget first, TypeTarget second) => !first.IsNullOrEquals(second);
        }

        /// <summary>
        /// Represents a binding that targets a factory method: meaning resolution of this registration will be done by invoking
        /// the given delegate - this is done by the underlying IoC container.
        /// <para>
        /// Note: despite the Factory being "<c>Func&lt;IResolverContract, object&gt;</c>", it is IMPORTANT to make sure that the
        /// underlying <c><see cref="Delegate.Method"/></c> has a <c><see cref="MethodInfo.ReturnType"/></c> that is NOT <c>object</c>, but that is 
        /// compatible with the <see cref="RegistrationInfo.ServiceType"/> of the registration where this target applies.
        /// </para>
        /// </summary>
        public class FactoryTarget : IBindTarget
        {
            /// <summary>
            /// A delegate with n underlying method having the signature: <c>TService (IResolverContract)</c>
            /// </summary>
            public Func<IResolverContract, object> Factory { get; }

            public Type Type => Factory.Method.ReturnType;

            internal FactoryTarget(Func<IResolverContract, object> factory)
            {
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public override int GetHashCode() => HashCode.Combine(Factory);

            public override bool Equals(object obj)
            {
                return obj is FactoryTarget other
                    && other.Factory.Equals(Factory);
            }

            public static bool operator ==(FactoryTarget first, FactoryTarget second) => first.IsNullOrEquals(second);
            public static bool operator !=(FactoryTarget first, FactoryTarget second) => !first.IsNullOrEquals(second);
        }
        #endregion
    }
}
