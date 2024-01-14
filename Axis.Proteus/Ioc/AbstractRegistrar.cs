using Axis.Luna.Extensions;
using Axis.Proteus.Interception;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Axis.Proteus.IoC
{
    /// <summary>
    /// Represents the base of registrar implementation
    /// </summary>
    public abstract class AbstractRegistrar : IRegistrarContract
    {
        protected RegistryManifest Manifest { get; } = new RegistryManifest();

        #region constructor
        protected AbstractRegistrar()
        {
        }
        #endregion

        #region IRegistrarContract

        public abstract IResolverContract BuildResolver();

        public abstract bool IsRegistrationClosed();

        #region Manifest

        public ReadOnlyDictionary<Type, RegistrationInfo[]> CollectionManifest()
            => Manifest
                .CollectionServices()
                .Select(type => (
                    Type: type,
                    Registrations: Manifest.CollectionRegistrationsFor(type) 
                        ?? throw new Exception($"no registration was found for the given type: {type}")))
                .ToDictionary(tuple => tuple.Type, tuple => tuple.Registrations)
                .ApplyTo(dict => new ReadOnlyDictionary<Type, RegistrationInfo[]>(dict));

        public ReadOnlyDictionary<Type, RegistrationInfo> RootManifest()
            => Manifest
                .RootServices()
                .ToDictionary(
                    type => type,
                    type => Manifest.RootRegistrationFor(type) 
                        ?? throw new Exception($"no registration was found for the given type: {type}"))
                .ApplyTo(dict => new ReadOnlyDictionary<Type, RegistrationInfo>(dict));

        #endregion

        #region Registration
        public IRegistrarContract Register<Impl>(
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Impl : class
            => Register<Impl, Impl>(scope, profile, conditionalBindingContexts);

        public IRegistrarContract Register<Service, Impl>(
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Service : class
            where Impl : class, Service
            => Register(typeof(Service), typeof(Impl), scope, profile, conditionalBindingContexts);

        public IRegistrarContract Register(
            Type serviceType,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            => Register(serviceType, serviceType, scope, profile, conditionalBindingContexts);

        public IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            Manifest.AddRootRegistration(
                new RegistrationInfo(
                    serviceType,
                    IBindTarget.Of(concreteType),
                    scope,
                    profile,
                    conditionalBindingContexts));

            return this;
        }

        public IRegistrarContract Register<Service>(
            Func<IResolverContract, Service> factory,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
            where Service : class
            => Register(typeof(Service), factory, scope, profile, conditionalBindingContexts);

        public IRegistrarContract Register(
            Type serviceType,
            Func<IResolverContract, object> factory,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindContext[] conditionalBindingContexts)
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            Manifest.AddRootRegistration(
                new RegistrationInfo(
                    serviceType,
                    IBindTarget.Of(factory),
                    scope,
                    profile,
                    conditionalBindingContexts));

            return this;
        }

        public IRegistrarContract RegisterAll<Service>(
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets)
            => RegisterAll(typeof(Service), scope, profile, targets);

        public IRegistrarContract RegisterAll(
            Type serviceType,
            ResolutionScope scope = default,
            InterceptorProfile profile = default,
            params IBindTarget[] targets)
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            if (serviceType is null)
                throw new ArgumentNullException(nameof(serviceType));

            targets?

                // throws exception if the registration is invalid
                .Select(target => new RegistrationInfo(
                    serviceType,
                    target,
                    scope,
                    profile))

                // ensures all registration instances are initialized
                .ToArray()

                // add VALID instances to the manifest - ensuring this method is atomic.
                .ForAll(registration => Manifest.AddCollectionRegistrations(registration));

            return this;
        }
        #endregion

        #endregion

    }
}
