﻿using Axis.Proteus.Interception;
using System;
using System.Collections.ObjectModel;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Interface defining contract for registering services against a service type.
    /// <para>
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
        /// <param name="conditionalBindingContexts">optional <see cref="IBindContext"/> instances specifiying additional conditional registrations</param>
        IRegistrarContract Register<Impl>(
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
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
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
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
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
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
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts);


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
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts);


        /// <summary>
        /// Register a factory method to be used in resolving instances of the service interface/type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="factory">A factory delegate used to create the service type. Note that this delegate MUST safely be castable to: <c>Func&lt;IResolverContract, TServiceType&gt;</c></param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="profile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        IRegistrarContract Register(
            Type serviceType,
            Delegate factory,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts);


        /// <summary>
        /// Registers a collection of <see cref="IBindTarget"/> instances against the given service type. Repeated calls to this method appends new targets to the underlying manifest. Duplicates
        /// are allowed.
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <param name="scope">the resolution scope</param>
        /// <param name="profile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="targets">The binding targets</param>
        /// <returns></returns>
        IRegistrarContract RegisterAll(
            Type serviceType,
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets);


        /// <summary>
        /// Registers a collection of <see cref="IBindTarget"/> instances against the given service type. Repeated calls to this method appends new targets to the underlying manifest. Duplicates
        /// are allowed.
        /// </summary>
        /// <typeparam name="Service">The service type</typeparam>
        /// <param name="scope">the resolution scope</param>
        /// <param name="profile">The interceptor to intercept calls to the service if needed. NOTE however that interception only works for <c>virtual</c> methods and properties.</param>
        /// <param name="targets">The binding targets</param>
        /// <returns></returns>
        IRegistrarContract RegisterAll<Service>(
            RegistryScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets);


        /// <summary>
        /// Returns a <see cref="IResolverContract"/> instance that can resolve all of the registered types within the resolver.
        /// Note: After this method is called, the  <see cref="IResolverContract"/> is effectively closed/locked, and further registratons cannot be added
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
