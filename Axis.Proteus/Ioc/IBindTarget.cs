using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

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
        public static IBindTarget Of(Type t, Delegate factory) => new FactoryTarget(t, factory);

        #region interface members
        Type Type { get; }
        #endregion

        #region Union types
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

            public static bool operator ==(TypeTarget first, TypeTarget second) => first.NullOrEquals(second);
            public static bool operator !=(TypeTarget first, TypeTarget second) => !first.NullOrEquals(second);
        }

        public class FactoryTarget : IBindTarget
        {
            /// <summary>
            /// A delegate with the signature: <c>T (IResolverContract)</c>
            /// </summary>
            public Delegate Factory { get; }

            public Type Type { get; }

            internal FactoryTarget(Type type, Delegate factory)
            {
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
                Type = type ?? throw new ArgumentNullException(nameof(type));

                if (!Type.IsAssignableFrom(Factory.Method.ReturnType))
                    throw new IncompatibleTypesException(type, Factory.Method.ReturnType);
            }

            public override int GetHashCode() => HashCode.Combine(Type, Factory);

            public override bool Equals(object obj)
            {
                return obj is FactoryTarget other
                    && other.Factory.Equals(Factory)
                    && other.Type.Equals(Type);
            }

            public static bool operator ==(FactoryTarget first, FactoryTarget second) => first.NullOrEquals(second);
            public static bool operator !=(FactoryTarget first, FactoryTarget second) => !first.NullOrEquals(second);
        }
        #endregion
    }
}
