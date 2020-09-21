using System;
using System.Collections.Generic;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Interface defining contract for registering services against a service type
    /// </summary>
    public interface IRegistrarContract
    {
        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <param name="serviceType">The concrete type</param>
        /// <param name="scope">The resolution scope</param>
        /// <returns></returns>
        IRegistrarContract Register(
            Type serviceType,
            RegistryScope? scope = null);


        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="concreteType">The concrete service type to resolve to</param>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null);


        /// <summary>
        /// Register a factory method to be used in resolving instances of the service interface/type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract Register(
            Type serviceType,
            Func<IServiceResolver, object> factory,
            RegistryScope? scope = null);

        /// <summary>
        /// Register a collection of implementations against a service interface/type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="concreteType">a service implementation type to be added to the collection registration</param>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract AppendCollectionRegistration(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null);

        /// <summary>
        /// Register a collection of implementations against a service interface/type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="concreteType">a service implementation type to be added to the collection registration</param>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract AppendCollectionRegistration(
            Type serviceType,
            Func<IServiceResolver, object> factory,
            RegistryScope? scope = null);


        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <typeparam name="Impl">The concrete type to be registered and resolved</typeparam>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract Register<Impl>(
            RegistryScope? scope = null)
            where Impl : class;

        /// <summary>
        /// Register a factory method that should be used in resolving instances of a service interface/type
        /// </summary>
        /// <typeparam name="Impl">The service type to be registered and resolved</typeparam>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract Register<Service>(
            Func<IServiceResolver, Service> factory,
            RegistryScope? scope = null)
            where Service : class;

        /// <summary>
        /// Register a concrete implementation for a service type
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <typeparam name="Impl">The service implementation to be resolved for the service type</typeparam>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract Register<Service, Impl>(
            RegistryScope? scope = null)
            where Service : class 
            where Impl : class, Service;

        /// <summary>
        /// Register a factory method that should be used in resolving instances of a service interface/type
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <typeparam name="Impl">The implementation to be added to the collection registration</typeparam>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract AppendCollectionRegistration<Service, Impl>(
            RegistryScope? scope = null)
            where Service : class
            where Impl : class, Service;

        /// <summary>
        /// Register a factory method that should be used in resolving instances of a service interface/type
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <typeparam name="Impl">The implementation to be added to the collection registration</typeparam>
        /// <param name="scope">The resolution scope</param>
        IRegistrarContract AppendCollectionRegistration<Service, Impl>(
            Func<IServiceResolver, Impl> factory,
            RegistryScope? scope = null)
            where Service : class
            where Impl : class, Service;
    }

}
