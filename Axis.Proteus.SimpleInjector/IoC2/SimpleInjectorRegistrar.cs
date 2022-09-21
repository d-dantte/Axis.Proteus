﻿using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Castle.DynamicProxy;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Axis.Proteus.SimpleInjector.IoC2
{
    public class SimpleInjectorRegistrar : Proteus.IoC2.IRegistrarContract
    {
        private static readonly MethodInfo _registerFactoryMethod = GetRegisterFactoryMethod();
        private static readonly MethodInfo _containerAppendFactoryMethod = GetContainerAppendFactoryMethod();

        private readonly RegistryManifest _manifest = new RegistryManifest();
        private readonly Container _container;
        private readonly IProxyGenerator _proxyGenerator;
        private Proteus.IoC2.IResolverContract _resolverContract;

        #region constructor
        public SimpleInjectorRegistrar(Container container, IProxyGenerator proxyGenerator)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _proxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
        }

        public SimpleInjectorRegistrar(Container container)
            :this (container, new ProxyGenerator())
        {
        }

        public SimpleInjectorRegistrar()
            :this (new Container(), new ProxyGenerator())
        {
        }
        #endregion


        #region IRegistrarContract
        public Proteus.IoC2.IResolverContract BuildResolver()
        {
            if (IsRegistrationClosed())
                return _resolverContract ?? throw new InvalidOperationException("The resolver is not yet initialized");

            // register the types in the manifest on the IRegistrarContract
            foreach (var serviceType in _manifest.ServiceTypes())
            {
                foreach (var info in _manifest.RegistrationsFor(serviceType))
                {
                    if (info.Implementation is IBoundImplementation.ImplType implType)
                        _container.Collection.Append(
                            info.ServiceType,
                            implType.Type,
                            info.Scope.ToSimpleInjectorLifeStyle());

                    else if (info.Implementation is IBoundImplementation.ImplFactory factory)
                        _containerAppendFactoryMethod
                            .MakeGenericMethod(info.ServiceType)
                            .Consume(method => _container.Collection.InvokeAction(
                                method,
                                factory.Factory,
                                info.Scope.ToSimpleInjectorLifeStyle()));

                    else
                        throw new InvalidOperationException($"Invalid implementation type: {info.Implementation}");
                }
            }

            // register the IResolverContract on the container. Note that duplicate registrations should fail
            // as there should be only one such registration on this container.
            _container.RegisterInstance<Proteus.IoC2.IResolverContract>(
                new SimpleInjectorResolver(
                    _container,
                    _proxyGenerator,
                    _manifest));

            // resolve the IResolverContract, effectively locking the container.
            return _resolverContract = _container.GetInstance<Proteus.IoC2.IResolverContract>();
        }

        public bool IsRegistrationClosed() => _container.IsLocked;

        public ReadOnlyDictionary<Type, RegistrationInfo[]> Manifest()
            => _manifest
                .Registrations()
                .GroupBy(info => info.ServiceType)
                .ToDictionary(group => group.Key, group => group.ToArray())
                .ApplyTo(dict => new ReadOnlyDictionary<Type, RegistrationInfo[]>(dict));

        public Proteus.IoC2.IRegistrarContract Register<Impl>(
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            where Impl : class
            => Register<Impl, Impl>(scope, profile);

        public Proteus.IoC2.IRegistrarContract Register<Service, Impl>(
            RegistryScope scope,
            InterceptorProfile profile)
            where Service : class
            where Impl : class, Service
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            var serviceType = typeof(Service);
            var implType = typeof(Impl);
            _manifest.AddRegistration(
                new RegistrationInfo(
                    serviceType, 
                    IBoundImplementation.Of(implType),
                    scope,
                    profile));

            return this;
        }

        public Proteus.IoC2.IRegistrarContract Register<Service>(
            Func<Proteus.IoC2.IResolverContract, Service> factory,
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            where Service : class
        {
            if (IsRegistrationClosed())
                throw new InvalidOperationException("The registry is locked to further registrations");

            Func<Service> resolvedFactory = () => factory.Invoke(_resolverContract);
            var serviceType = typeof(Service);
            _manifest.AddRegistration(
                new RegistrationInfo(
                    serviceType,
                    IBoundImplementation.Of(serviceType, resolvedFactory),
                    scope,
                    profile));

            return this;
        }

        public Proteus.IoC2.IRegistrarContract Register(
            Type serviceType,
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            => Register(serviceType, serviceType, scope, profile);

        public Proteus.IoC2.IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope scope = default,
            InterceptorProfile profile = default)
        {
            _manifest.AddRegistration(
                new RegistrationInfo(
                    serviceType,
                    IBoundImplementation.Of(concreteType),
                    scope,
                    profile));

            return this;
        }

        public Proteus.IoC2.IRegistrarContract Register(
            Type serviceType,
            Delegate factory,
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            => _registerFactoryMethod
                .MakeGenericMethod(serviceType)
                .ApplyTo(method => this.InvokeFunc(method, factory, scope, profile))
                .As<Proteus.IoC2.IRegistrarContract>();
        #endregion

        #region Method Access
        public static MethodInfo GetRegisterFactoryMethod()
        {
            Proteus.IoC2.IRegistrarContract contract = null;
            Expression<Func<
                Func<Proteus.IoC2.IResolverContract, IEnumerable<int>>,
                RegistryScope,
                InterceptorProfile,
                Proteus.IoC2.IRegistrarContract>> expression = (f, a, b) => contract.Register(f, a, b);
            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        public static MethodInfo GetContainerAppendFactoryMethod()
        {
            ContainerCollectionRegistrator collectionContainer = null;
            Expression<Action<Func<IEnumerable<int>>, Lifestyle>> expression = (f, l) => collectionContainer.Append(f, l);
            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }
        #endregion
    }
}
