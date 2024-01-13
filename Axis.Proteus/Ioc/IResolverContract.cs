using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Interface defining contract for resolving instances from the underlying IoC container.
    /// </summary>
    public interface IResolverContract : IDisposable
    {
        /// <summary>
        /// Resolve one instance of the specified service, or <c>null</c> if the type was not registered.
        /// <para>
        /// If a <c>contextName</c> is supplied and non is found, it will be ignored. If non is supplied, same applies.
        /// </para>
        /// </summary>
        /// <typeparam name="Service">The type of the service to be resovled</typeparam>
        /// <param name="contextName">The optional context name to use in conditionally resolving the give type</param>
        /// <returns>The resolved service, or null if it was not registered</returns>
        Service Resolve<Service>(ResolutionContextName contextName = default) where Service : class;

        /// <summary>
        /// Resolve one instance of the specified service, or <c>null</c> if the type was not registered.
        /// <para>
        /// If a <c>contextName</c> is supplied and non is found, it will be ignored. If non is supplied, same applies.
        /// </para>
        /// </summary>
        /// <param name="serviceType">The typeof the service to be resolved</param>
        /// <param name="contextName">The optional context name to use in conditionally resolving the give type</param>
        /// <returns>The resolved service, or null if it was not registered</returns>
        object Resolve(Type serviceType, ResolutionContextName contextName = default);

        /// <summary>
        /// Resolves a collection of instances registered for a service.
        /// </summary>
        /// <typeparam name="Service">The service to be resolved</typeparam>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        IEnumerable<Service> ResolveAll<Service>() where Service : class;

        /// <summary>
        /// Resolves a collection of instances registered for a service
        /// </summary>
        /// <param name="serviceType">The service to be resolved</param>
        /// <returns>The instances registered, or an empty enumerable if non were registered</returns>
        IEnumerable<object> ResolveAll(Type serviceType);

        /// <summary>
        /// Retrieves a list of all top-level collection service registrations
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<Type, RegistrationInfo[]> CollectionManifest();

        /// <summary>
        /// Retrieves a list of all top-level root registrations
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<Type, RegistrationInfo> RootManifest();
    }
}
