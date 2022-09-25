using System;

namespace Axis.Proteus.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="IoC.RegistryManifest"/> instance returns <c>null</c> when asked for the <see cref="IoC.RegistrationInfo"/> of a <c>ServiceType</c>
    /// That the manifest returned.
    /// <code>
    /// var serviceType = manifest.RootServices().First(); //serviceType is non-null
    /// var registration = manifest.RootRegistrationFor(serviceType);
    /// </code>
    /// If the <c>registration</c> variable holds <c>null</c>, it should be reported with this exception
    /// </summary>
    public class InvalidManifestRegistrationMappingException: Exception
    {
        /// <summary>
        /// The service type whose registration cannot be found.
        /// </summary>
        public Type ServiceType { get; }

        public InvalidManifestRegistrationMappingException(Type serviceType)
            : base($"The registration info for the given service type ({serviceType}) cannot be found.")
        {
            ServiceType = serviceType;
        }
    }
}
