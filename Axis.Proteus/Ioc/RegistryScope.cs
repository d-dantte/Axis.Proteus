using System;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Registry Scope
    /// </summary>
    public struct RegistryScope
    {
        public static readonly RegistryScope Singleton = new RegistryScope("Singleton");

        public static readonly RegistryScope Transient = new RegistryScope("Transient");

        public static readonly RegistryScope DefaultScope = new RegistryScope("DefaultScope");

        /// <summary>
        /// The name of the scope
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Create a new instance of this struct
        /// </summary>
        /// <param name="name">the name of the scope</param>
        public RegistryScope(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Name.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is RegistryScope other
                && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public override string ToString() => $"Scope: {Name.ToLowerInvariant()}";

        public static implicit operator string(RegistryScope scope) => scope.Name;

        public static implicit operator RegistryScope(string scope) => new RegistryScope(scope);
    }
}
