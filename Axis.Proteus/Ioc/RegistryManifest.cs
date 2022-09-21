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

        public RegistryManifest(Dictionary<Type, IEnumerable<RegistrationInfo>> manifest)
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
    }
}
