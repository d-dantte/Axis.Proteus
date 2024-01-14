using Axis.Luna.Common;
using Axis.Luna.Extensions;
using System;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Registry Scope
    /// </summary>
    public readonly struct ResolutionScope: IDefaultValueProvider<ResolutionScope>
    {
        /// <summary>
        /// Singleton scope
        /// </summary>
        public static readonly ResolutionScope Singleton = new("Singleton");

        /// <summary>
        /// Named-scope.
        /// </summary>
        public static readonly ResolutionScope DefaultScope = new("DefaultScope");

        /// <summary>
        /// Transient scope
        /// </summary>
        public static readonly ResolutionScope Transient = default;

        private readonly string _name;

        /// <summary>
        /// The name of the scope
        /// </summary>
        public string Name => _name ?? "Transient";

        public bool IsDefault => _name is null;

        public static ResolutionScope Default => default;


        /// <summary>
        /// Create a new instance of this struct
        /// </summary>
        /// <param name="name">the name of the scope</param>
        public ResolutionScope(string name)
        {
            _name = name.ThrowIf(
                string.IsNullOrWhiteSpace,
                _ => new ArgumentNullException(nameof(name)));
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Name.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ResolutionScope other
                && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public override string ToString() => $"Scope: {Name.ToLowerInvariant()}";

        public static implicit operator string(ResolutionScope scope) => scope.Name;

        public static implicit operator ResolutionScope(string scope) => new(scope);

        public static bool operator ==(ResolutionScope first, ResolutionScope second) => first.Equals(second);
        public static bool operator !=(ResolutionScope first, ResolutionScope second) => !first.Equals(second);
    }
}
