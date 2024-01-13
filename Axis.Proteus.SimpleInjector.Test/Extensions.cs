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

                    return registration
                        .Enumerate()
                        .Concat(producer
                            .GetRelationships()
                            .Select(r => r.Dependency)
                            .FlattenRegistrations());
                })
                .Distinct()
                .ToArray();
        }
    }
}
