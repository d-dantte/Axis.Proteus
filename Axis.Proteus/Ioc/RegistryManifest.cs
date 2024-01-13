using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.IoC
{
    public class RegistryManifest
    {
        private readonly Dictionary<Type, List<RegistrationInfo>> _collectionManifest = new Dictionary<Type, List<RegistrationInfo>>();

        private readonly Dictionary<Type, RegistrationInfo> _rootManifest = new Dictionary<Type, RegistrationInfo>();

        #region Construction
        public RegistryManifest()
        {
        }
        #endregion

        #region Collections
        /// <summary>
        /// Gets all the Collection types registered in this manifest
        /// </summary>
        public virtual Type[] CollectionServices() => _collectionManifest.Keys.ToArray();

        /// <summary>
        /// Gets all the <see cref="RegistrationInfo"/> instances registered against the given collection service type. Returns null if non have been registered
        /// </summary>
        /// <param name="collectionServiceType">The collection type</param>
        public virtual RegistrationInfo[] CollectionRegistrationsFor(Type collectionServiceType)
            => _collectionManifest.TryGetValue(collectionServiceType, out var registrations)
                ? registrations.ToArray()
                : null;

        /// <summary>
        /// Indicates if the given collection service type has been registered in this manifest
        /// </summary>
        /// <param name="collectionServiceType">The collection type</param>
        public virtual bool HasCollectionRegistrations(Type collectionServiceType) => _collectionManifest.ContainsKey(collectionServiceType);

        /// <summary>
        /// Appends a new <see cref="RegistrationInfo"/> instance to the manifest. Duplicates are allowed.
        /// </summary>
        /// <param name="registrations">The registration info list</param>
        public virtual RegistryManifest AddCollectionRegistrations(params RegistrationInfo[] registrations)
        {
            registrations?
                .ThrowIf(ContainsDefault, new ArgumentException($"Invalid registration detected"))
                .ForAll(registration =>
                {
                    _collectionManifest
                        .GetOrAdd(
                            registration.ServiceType,
                            _ => new List<RegistrationInfo>())
                        .Add(registration);
                });

            return this;
        }

        #endregion

        #region Root
        /// <summary>
        /// Gets all the root service types registered in this manifest
        /// </summary>
        public virtual Type[] RootServices() => _rootManifest.Keys.ToArray();


        /// <summary>
        /// Gets the <see cref="RegistrationInfo"/> instance registered against the given root service type. Returns null if non have been registered
        /// </summary>
        /// <param name="rootServiceType">The root type</param>
        public virtual RegistrationInfo? RootRegistrationFor(Type rootServiceType)
            => _rootManifest.TryGetValue(rootServiceType, out var registration)
                ? registration
                : (RegistrationInfo?) null;


        /// <summary>
        /// Indicates if the given root service type has been registered in this manifest
        /// </summary>
        /// <param name="rootServiceType">The collection type</param>
        public virtual bool HasRootRegistration(Type rootServiceType) => _rootManifest.ContainsKey(rootServiceType);

        /// <summary>
        /// Adds a new <see cref="RegistrationInfo"/> instance to the manifest. Duplicates are not allowed.
        /// </summary>
        /// <param name="registration">The registration instance</param>
        /// <exception cref="DuplicateRegistrationException"></exception>
        public virtual RegistryManifest AddRootRegistration(RegistrationInfo registration)
        {
            if (registration == default)
                throw new ArgumentException($"Invalid registration supplied: {registration}");

            if (!_rootManifest.TryAdd(registration.ServiceType, registration))
                throw new DuplicateRegistrationException(registration.ServiceType);

            return this;
        }

        #endregion

        private static bool ContainsDefault(IEnumerable<RegistrationInfo> registrations)
            => registrations.Any(registration => default == registration);
    }
}
