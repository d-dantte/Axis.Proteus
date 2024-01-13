using Axis.Luna.Extensions;
using System;
using System.Text.RegularExpressions;

namespace Axis.Proteus.IoC
{
    public readonly struct ResolutionContextName
    {
        /// <summary>
        /// A pattern to which resolution context names must adhere
        /// </summary>
        public static readonly Regex ContextNamePattern = new("^[a-zA-Z_]\\w*$");

        /// <summary>
        /// The context name
        /// </summary>
        public string Name { get; }

        public ResolutionContextName(string name)
        {
            Name = name
                .ThrowIf(
                    string.IsNullOrWhiteSpace,
                    _ => new ArgumentException($"Invalid {nameof(name)}: null/whitespace"))
                .ThrowIf(
                    n => !ContextNamePattern.IsMatch(n),
                    _ => new ArgumentException($"Invalid {nameof(name)}: does not match pattern '{ContextNamePattern}'"));
        }

        /// <summary>
        /// The normalized context name.
        /// </summary>
        public override string ToString() => $"::{Name}";

        public override bool Equals(object obj)
        {
            return obj is ResolutionContextName other
                && other.Name.IsNullOrTrue(
                    Name,
                    (n1, n2) => n1.Equals(n2, StringComparison.InvariantCulture));
        }

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        #region operators
        public static bool operator ==(ResolutionContextName first, ResolutionContextName second) => first.Equals(second);

        public static bool operator !=(ResolutionContextName first, ResolutionContextName second) => !first.Equals(second);

        public static implicit operator ResolutionContextName(string value) => new ResolutionContextName(value);
        #endregion
    }
}
