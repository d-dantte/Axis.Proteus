using Axis.Proteus.Interception;
using System;

namespace Axis.Proteus.IoC
{
    public readonly struct RegistrationInfo
    {
        /// <summary>
        /// The service type
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// The implementation type
        /// </summary>
        public IBoundImplementation Implementation { get; }

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
            InterceptorProfile profile = default)
            : this(implementationType, IBoundImplementation.Of(implementationType), scope, profile)
        {
        }

        public RegistrationInfo(
            Type serviceType,
            IBoundImplementation implementationType,
            RegistryScope scope = default,
            InterceptorProfile profile = default)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Implementation = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Scope = scope;
            Profile = profile;
        }

        public override int GetHashCode() => HashCode.Combine(ServiceType, Implementation, Scope, Profile);

        public override bool Equals(object obj)
        {
            return obj is RegistrationInfo other
                && other.ServiceType.Equals(ServiceType)
                && other.Implementation.Equals(Implementation)
                && other.Scope.Equals(Scope)
                && other.Profile.Equals(Profile);
        }

        public static bool operator ==(RegistrationInfo first, RegistrationInfo second) => first.Equals(second);
        public static bool operator !=(RegistrationInfo first, RegistrationInfo second) => !first.Equals(second);
    }


    public interface IBoundImplementation
    {
        /// <summary>
        /// Creates an <see cref="IBoundImplementation"/> of the given type.
        /// </summary>
        /// <param name="t">The implementation type</param>
        public static IBoundImplementation Of(Type t) => new ImplType(t);

        /// <summary>
        /// Creates an <see cref="IBoundImplementation"/> of the given delegate and return type.
        /// </summary>
        /// <param name="t">The implementation type</param>
        /// <param name="factory">The factory method that produces the instance of the implementation type</param>
        public static IBoundImplementation Of(Type t, Delegate factory) => new ImplFactory(t, factory);

        #region interface members
        Type Type { get; }
        #endregion

        #region Union types
        public class ImplType: IBoundImplementation
        {
            public Type Type { get; }

            internal ImplType(Type type)
            {
                Type = type ?? throw new ArgumentNullException(nameof(type));
            }

            public override int GetHashCode() => Type.GetHashCode();

            public override bool Equals(object obj)
            {
                return obj is ImplType other
                    && other.Type.Equals(Type);
            }
        }

        public class ImplFactory: IBoundImplementation
        {
            /// <summary>
            /// A delegate with the signature: <c>T (IResolverContract)</c>
            /// </summary>
            public Delegate Factory { get; }

            public Type Type { get; }

            internal ImplFactory(Type type, Delegate factory)
            {
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
                Type = type ?? throw new ArgumentNullException(nameof(type));
            }

            public override int GetHashCode() => Factory.GetHashCode();

            public override bool Equals(object obj)
            {
                return obj is ImplFactory other
                    && other.Factory.Equals(Factory)
                    && other.Type.Equals(Type);
            }
        }
        #endregion
    }
}
