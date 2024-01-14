using Axis.Proteus.Interception;
using System;
using System.Collections.ObjectModel;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Interface defining the contract for registering service implementations against a service type.
    /// <para>
    /// The registrar supports conditional registration via <see cref="IBindContext"/> implementations.
    /// </para>
    /// <para>
    /// </para>
    /// </summary>
    public interface IRegistrarContract
    {
        /// <summary>
        /// Register a concrete type.
        /// <para>
        /// NOTE: When present, the <paramref name="conditionalBindingContexts"/> must all have unique <see cref="IBindTarget.Type"/>s, also distinct from the <typeparamref name="Impl"/> type.
        /// Simply put: together, all <see cref="IBindContext"/>s must resolve to unique implementation types.
        /// </para>
        /// </summary>
        /// <typeparam name="Impl">The concrete type to be registered and resolved</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="conditionalBindingContexts">optional <see cref="IBindContext"/> instances specifiying additional conditional registrations.</param>
        IRegistrarContract Register<Impl>(
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Impl : class;

        /// <summary>
        /// Register a concrete implementation for a service type.
        /// <para>
        /// NOTE: When present, the <paramref name="conditionalBindingContexts"/> must all have unique <see cref="IBindTarget.Type"/>s, also distinct from the <typeparamref name="Impl"/> type.
        /// Simply put: together, all <see cref="IBindContext"/>s must resolve to unique implementation types.
        /// </para>
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <typeparam name="Impl">The service implementation to be resolved for the service type</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="conditionalBindingContexts">optional <see cref="IBindContext"/> instances specifiying additional conditional registrations.</param>
        IRegistrarContract Register<Service, Impl>(
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Service : class
            where Impl : class, Service;

        /// <summary>
        /// Register a factory method that should be used in resolving instances of a service interface/type
        /// <para>
        /// NOTE: When present, the <paramref name="conditionalBindingContexts"/> must all have unique <see cref="IBindTarget.Type"/>s, also distinct from the <typeparamref name="Service"/> type.
        /// Simply put: together, all <see cref="IBindContext"/>s must resolve to unique implementation types.
        /// </para>
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="conditionalBindingContexts">optional <see cref="IBindContext"/> instances specifiying additional conditional registrations.</param>
        IRegistrarContract Register<Service>(
            Func<IResolverContract, Service> factory,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Service : class;

        /// <summary>
        /// Register a concrete type.
        /// <para>
        /// NOTE: When present, the <paramref name="conditionalBindingContexts"/> must all have unique <see cref="IBindTarget.Type"/>s, also distinct from the <paramref name="serviceType"/> type.
        /// Simply put: together, all <see cref="IBindContext"/>s must resolve to unique implementation types.
        /// </para>
        /// </summary>
        /// <param name="serviceType">The concrete type</param>
        /// <param name="scope">The resolution scope</param>
        /// <returns></returns>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="conditionalBindingContexts">optional <see cref="IBindContext"/> instances specifiying additional conditional registrations.</param>
        IRegistrarContract Register(
            Type serviceType,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts);


        /// <summary>
        /// Register a concrete type.
        /// <para>
        /// NOTE: When present, the <paramref name="conditionalBindingContexts"/> must all have unique <see cref="IBindTarget.Type"/>s, also distinct from the <paramref name="concreteType"/> type.
        /// Simply put: together, all <see cref="IBindContext"/>s must resolve to unique implementation types.
        /// </para>
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="concreteType">The concrete service type to resolve to</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="conditionalBindingContexts">optional <see cref="IBindContext"/> instances specifiying additional conditional registrations.</param>
        IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts);


        /// <summary>
        /// Register a factory method to be used in resolving instances of the service interface/type.
        /// <para>
        /// NOTE: When present, the <paramref name="conditionalBindingContexts"/> must all have unique <see cref="IBindTarget.Type"/>s, also distinct from the <paramref name="serviceType"/> type.
        /// Simply put: together, all <see cref="IBindContext"/>s must resolve to unique implementation types.
        /// </para>
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="factory">A factory delegate used to create the service type. Note that this delegate's return instance MUST safely be castable to: <c>TService</c></param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="profile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="conditionalBindingContexts">optional <see cref="IBindContext"/> instances specifiying additional conditional registrations.</param>
        IRegistrarContract Register(
            Type serviceType,
            Func<IResolverContract, object> factory,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts);


        /// <summary>
        /// Registers a collection of <see cref="IBindTarget"/> instances against the given service type. Repeated calls to this method appends new targets to the underlying manifest. Duplicates
        /// are allowed.
        /// <para>
        /// If any invalid arguments are detected, this method fails atomically - i.e, nothing should be added to the manifest.
        /// </para>
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <param name="scope">the resolution scope</param>
        /// <param name="profile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="targets">The binding targets for each individual resolution</param>
        /// <returns></returns>
        IRegistrarContract RegisterAll(
            Type serviceType,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets);


        /// <summary>
        /// Registers a collection of <see cref="IBindTarget"/> instances against the given service type. Repeated calls to this method appends new targets to the underlying manifest. Duplicates
        /// are allowed.
        /// <para>
        /// If any invalid arguments are detected, this method fails atomically - i.e, nothing should be added to the manifest.
        /// </para>
        /// </summary>
        /// <typeparam name="Service">The service type</typeparam>
        /// <param name="scope">the resolution scope</param>
        /// <param name="profile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="targets">The binding targets for each individual resolution</param>
        /// <returns></returns>
        IRegistrarContract RegisterAll<Service>(
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets);


        /// <summary>
        /// Returns a <see cref="IResolverContract"/> instance that can resolve all of the registered types within the registry.
        /// Note: After this method is called, the  <see cref="IResolverContract"/> is effectively closed/locked, and further registratons cannot be added. Also,
        /// <see cref="IRegistrarContract.IsRegistrationClosed"/> will return true after this method is called.
        /// </summary>
        IResolverContract BuildResolver();

        /// <summary>
        /// Returns a value indicating if registrations are still possible with this contract instance.
        /// <para>
        /// This value is <c>true</c> upon creation, and <c>false</c> after <see cref="BuildResolver"/> is called successfully.
        /// </para>
        /// </summary>
        bool IsRegistrationClosed();

        /// <summary>
        /// Retrieves a list of all currently added top-level collection registrations
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<Type, RegistrationInfo[]> CollectionManifest();

        /// <summary>
        /// Retrieves a list of all currently added top-level root registrations
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<Type, RegistrationInfo> RootManifest();
    }

}
