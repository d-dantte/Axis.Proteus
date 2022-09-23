using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.IoC
{
    public class RegistryManifest
    {
        private readonly Dictionary<Type, List<RegistrationInfo>> _manifest = new Dictionary<Type, List<RegistrationInfo>>();

        #region Construction
        public RegistryManifest()
        {
        }

        public RegistryManifest(IDictionary<Type, IEnumerable<RegistrationInfo>> manifest)
            : this(manifest.Values.SelectMany())
        {
        }

        public RegistryManifest(IEnumerable<RegistrationInfo> flatManifest)
        {
            flatManifest
                .ThrowIfNull(new ArgumentNullException(nameof(flatManifest)))
                .ForAll(info => AddRegistration(info));
        }
        #endregion


        public Type[] ServiceTypes() => _manifest.Keys.ToArray();

        public RegistrationInfo[] Registrations() => _manifest.Values.SelectMany().ToArray();

        public RegistrationInfo[] RegistrationsFor(Type serviceType) => _manifest.GetOrDefault(serviceType)?.ToArray();

        public RegistrationInfo[] RegistrationsFor<Service>() => RegistrationsFor(typeof(Service));


        public RegistryManifest AddRegistration(RegistrationInfo registration)
        {
            _manifest
                .GetOrAdd(registration.ServiceType, _ => new List<RegistrationInfo>())
                .Add(registration);

            return this;
        }

        public static implicit operator RegistryManifest(
            Dictionary<Type, IEnumerable<RegistrationInfo>> manifest)
        {
            return new RegistryManifest(manifest);
        }

        public static implicit operator RegistryManifest(
            Dictionary<Type, List<RegistrationInfo>> manifest)
        {
            return new RegistryManifest(manifest.Values.SelectMany());
        }

        public static implicit operator RegistryManifest(
            Dictionary<Type, RegistrationInfo[]> manifest)
        {
            return new RegistryManifest(manifest.Values.SelectMany());
        }

        public static implicit operator RegistryManifest(RegistrationInfo[] manifest)
        {
            return new RegistryManifest(manifest);
        }
    }
}
