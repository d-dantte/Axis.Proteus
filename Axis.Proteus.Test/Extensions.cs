using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using System;
using System.Linq;

namespace Axis.Proteus.Test
{
    internal static class Extensions
    {

        public static Type[] RootServices(this IRegistrarContract contract)
            => contract
                .ThrowIfNull(() => new ArgumentNullException(nameof(contract)))
                .RootManifest().Keys
                .ToArray();

        public static Type[] CollectionServices(this IRegistrarContract contract)
            => contract
                .ThrowIfNull(() => new ArgumentNullException(nameof(contract)))
                .CollectionManifest().Keys
                .ToArray();

        public static RegistrationInfo? RootRegistrationFor(this
            IRegistrarContract contract,
            Type serviceType)
            => contract
                .ThrowIfNull(() => new ArgumentNullException(nameof(contract)))
                .RootManifest()
                .TryGetValue(serviceType, out RegistrationInfo info)
                ? info : (RegistrationInfo?)null;

        /// <summary>
        /// Gets the list of registration info available for the given service type, or null if absent.
        /// </summary>
        /// <param name="contract">The contract</param>
        /// <param name="serviceType">The service type</param>
        public static RegistrationInfo[] CollectionRegistrationsFor(this
            IRegistrarContract contract,
            Type serviceType)
            => contract
                .ThrowIfNull(() => new ArgumentNullException(nameof(contract)))
                .CollectionManifest()
                .TryGetValue(serviceType, out RegistrationInfo[] infoList)
                ? infoList : null;
    }
}
