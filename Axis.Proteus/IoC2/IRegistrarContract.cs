using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using System;
using System.Collections.ObjectModel;

namespace Axis.Proteus.IoC2
{
    /// <summary>
    /// Interface defining contract for registering services against a service type.
    /// <para>
    /// Note that registrations are "stackable", meaning the relationship between services and implementations is many-to-many.
    /// This means multiple services can be bound to the same implementation, and a single service can have multiple implementations.
    /// This is achievable by multiple calls to "register", using the appropriate arguments.
    /// </para>
    /// </summary>
    public interface IRegistrarContract
    {
        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <typeparam name="Impl">The concrete type to be registered and resolved</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        IRegistrarContract Register<Impl>(
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            where Impl : class;

        /// <summary>
        /// Register a concrete implementation for a service type
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <typeparam name="Impl">The service implementation to be resolved for the service type</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        IRegistrarContract Register<Service, Impl>(
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            where Service : class
            where Impl : class, Service;

        /// <summary>
        /// Register a factory method that should be used in resolving instances of a service interface/type
        /// </summary>
        /// <typeparam name="Impl">The service type to be registered and resolved</typeparam>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        IRegistrarContract Register<Service>(
            Func<IResolverContract, Service> factory,
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            where Service : class;

        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <param name="serviceType">The concrete type</param>
        /// <param name="scope">The resolution scope</param>
        /// <returns></returns>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        IRegistrarContract Register(
            Type serviceType,
            RegistryScope scope = default,
            InterceptorProfile profile = default);


        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="concreteType">The concrete service type to resolve to</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope scope = default,
            InterceptorProfile profile = default);


        /// <summary>
        /// Register a factory method to be used in resolving instances of the service interface/type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="factory">A factory delegate used to create the service type. Note that this delegate MUST safely be castable to: <c>Func&lt;IResolverContract, TServiceType&gt;</c></param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        IRegistrarContract Register(
            Type serviceType,
            Delegate factory,
            RegistryScope scope = default,
            InterceptorProfile profile = default);


        /// <summary>
        /// Returns a <see cref="IResolverContract"/> instance that can resolve all of the registered types within the resolver.
        /// Note: After this method is called, the  <see cref="IResolverContract"/> is effectively closed/locked, and further registratons cannot be added
        /// </summary>
        IResolverContract BuildResolver();

        /// <summary>
        /// Returns a value indicating if registrations are still possible with this contract instance.
        /// <para>
        /// This value is <c>true</c> upon creation, and <c>false</c> after <see cref="IRegistrarContract.BuildResolver"/> is called successfully.
        /// </para>
        /// </summary>
        bool IsRegistrationClosed();

        /// <summary>
        /// Retrieves a list of all currently added top-level registrations
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<Type, RegistrationInfo[]> Manifest();
    }

}
