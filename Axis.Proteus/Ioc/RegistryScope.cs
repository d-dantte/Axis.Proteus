using Axis.Luna.Extensions;
using System;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Registry Scope
    /// </summary>
    public readonly struct RegistryScope
    {
        /// <summary>
        /// Singleton scope
        /// </summary>
        public static readonly RegistryScope Singleton = new("Singleton");

        /// <summary>
        /// Named-scope.
        /// </summary>
        public static readonly RegistryScope DefaultScope = new("DefaultScope");

        /// <summary>
        /// Transient scope
        /// </summary>
        public static readonly RegistryScope Transient = default;

        private readonly string _name;

        /// <summary>
        /// The name of the scope
        /// </summary>
        public string Name => _name ?? "Transient";


        /// <summary>
        /// Create a new instance of this struct
        /// </summary>
        /// <param name="name">the name of the scope</param>
        public RegistryScope(string name)
        {
            _name = name.ThrowIf(
                string.IsNullOrWhiteSpace,
                _ => new ArgumentNullException(nameof(name)));
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

        public static implicit operator RegistryScope(string scope) => new(scope);

        public static bool operator ==(RegistryScope first, RegistryScope second) => first.Equals(second);
        public static bool operator !=(RegistryScope first, RegistryScope second) => !first.Equals(second);
    }
}
