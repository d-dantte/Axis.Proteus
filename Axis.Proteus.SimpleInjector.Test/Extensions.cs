using Axis.Proteus.IoC;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using Axis.Luna.Extensions;

namespace Axis.Proteus.SimpleInjector.Test
{
    using TypeMap = KeyValuePair<Type, Type>;

    internal static class Extensions
    {

        /// <summary>
        /// Flattens the result of <see cref="global::SimpleInjector.Container.GetRootRegistrations"/>.
        /// </summary>
        /// <param name="producers">The list of instance producers</param>
        public static TypeMap[] FlattenRegistrations(this IEnumerable<InstanceProducer> producers)
        {
            if (producers == null)
                return Array.Empty<TypeMap>();

            return producers
                .SelectMany(producer =>
                {
                    var registration = new TypeMap(producer.ServiceType, producer.ImplementationType);

                    return registration.Concat(producer
                        .GetRelationships()
                        .Select(r => r.Dependency)
                        .FlattenRegistrations());
                })
                .Distinct()
                .ToArray();
        }

        public static HashSet<TypeMap> ExtractRootRegistrations(this Container container)
        {
            return new HashSet<TypeMap>()
                .Use(set => set
                    .AddRange(container
                        .GetRootRegistrations()
                        .FlattenRegistrations()));
        }

        public static HashSet<TypeMap> ExtractUnverifiedRegistrations(this Container container)
        {
            return new HashSet<TypeMap>()
                .Use(set => set
                    .AddRange(container
                        .GetCurrentRegistrations()
                        .FlattenRegistrations()));
        }

        public static bool ContainsRegistration(this HashSet<TypeMap> set, Type serviceType)
        {
            return set.Contains(new TypeMap(
                serviceType ?? throw new ArgumentNullException(nameof(serviceType)),
                serviceType ?? throw new ArgumentNullException(nameof(serviceType)))); // <-- this second test is redundant
        }

        public static bool ContainsRegistration(this HashSet<TypeMap> set, Type serviceType, Type implementationType)
        {
            return set.Contains(new TypeMap(
                serviceType ?? throw new ArgumentNullException(nameof(serviceType)),
                implementationType ?? throw new ArgumentNullException(nameof(implementationType))));
        }
    }
}
