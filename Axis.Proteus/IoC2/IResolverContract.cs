using Axis.Proteus.IoC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Axis.Proteus.IoC2
{
    /// <summary>
    /// Interface defining contract for resolving instances from the underlying IoC container.
    /// </summary>
    public interface IResolverContract : IDisposable
    {
        /// <summary>
        /// Resolve one instance of the specified service - guaranteed to be the first implementation to be registered.
        /// </summary>
        /// <typeparam name="Service">The type of the service to be resovled</typeparam>
        /// <returns>The resolved service, or null if it was not registered</returns>
        Service Resolve<Service>() where Service: class;

        /// <summary>
        /// Resolve one instance of the specified service - guaranteed to be the first implementation to be registered.
        /// </summary>
        /// <param name="serviceType">The typeof the service to be resolved</param>
        /// <returns>The resolved service, or null if it was not registered</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Resolves a collection of instances registered for a service
        /// </summary>
        /// <typeparam name="Service">The service to be resolved</typeparam>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        IEnumerable<Service> ResolveAll<Service>() where Service: class;

        /// <summary>
        /// Resolves a collection of instances registered for a service
        /// </summary>
        /// <param name="serviceType">The service to be resolved</param>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        IEnumerable<object> ResolveAll(Type serviceType);

        /// <summary>
        /// Retrieves a list of all registrations that can be resolved by this resolver
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<Type, RegistrationInfo[]> Manifest();
    }
}
