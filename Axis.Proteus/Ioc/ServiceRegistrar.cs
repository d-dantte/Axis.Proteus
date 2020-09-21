using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.IoC
{
    public class ServiceRegistrar
	{
		private readonly IRegistrarContract _registrar;
        private readonly IProxyGenerator _proxyGenerator;
        private readonly HashSet<Type> _rootRegistrations = new HashSet<Type>();

		public ServiceRegistrar(IRegistrarContract registrar)
		{
			_registrar = registrar ?? throw new ArgumentNullException(nameof(registrar));
            _proxyGenerator = new ProxyGenerator();
		}

        #region Registration Methods
        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <param name="serviceType">The concrete type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="registry">The interceptor to intercept calls to the service if needed</param>
        /// <returns></returns>
        public ServiceRegistrar Register(
            Type serviceType,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
        {
            if (!_rootRegistrations.Contains(serviceType))
            {
                if (interceptorProfile == null)
                    _ = _registrar.Register(serviceType, scope);

                else
                {
                    _ = _registrar.Register(
                        serviceType: serviceType,
                        scope: scope,
                        factory: resolver =>
                        {
                            var instance = resolver.Resolve(serviceType);
                            var proxy = _proxyGenerator.CreateClassProxyWithTarget(
                                serviceType,
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray());

                            return proxy;
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }
        
        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="concreteType">The concrete service type to resolve to</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="registry">The interceptor to intercept calls to the service if needed</param>
        public ServiceRegistrar Register(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
        {
            if (!serviceType.IsAssignableFrom(concreteType))
                throw new IncompatibleTypesException(serviceType, concreteType);

            else if (!_rootRegistrations.Contains(serviceType))
            {
                //register the concrete type just in case it wasn't done externally
                if (!_rootRegistrations.Contains(concreteType))
                    _ = Register(concreteType, scope);

                if (interceptorProfile == null)
                    _ = _registrar.Register(serviceType, concreteType, scope);

                else
                {
                    _ = _registrar.Register(
                        serviceType: serviceType,
                        scope: scope,
                        factory: resolver =>
                        {
                            var instance = resolver.Resolve(concreteType);
                            var proxy = _proxyGenerator.CreateClassProxyWithTarget(
                                serviceType,
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray());

                            return proxy;
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }
        
        /// <summary>
        /// Register a factory method to be used in resolving instances of the service interface/type
        /// </summary>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="registry">The interceptor to intercept calls to the service if needed</param>
        public ServiceRegistrar Register(
            Type serviceType,
            Func<IServiceResolver, object> factory,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
        {
            if (!_rootRegistrations.Contains(serviceType))
            {
                if (interceptorProfile == null)
                    _ = _registrar.Register(serviceType, factory, scope);

                else
                {
                    _ = _registrar.Register(
                        serviceType: serviceType,
                        scope: scope,
                        factory: resolver =>
                        {
                            var instance = factory.Invoke(resolver);
                            var proxy = _proxyGenerator.CreateClassProxyWithTarget(
                                serviceType,
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray());

                            return proxy;
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }

        /// <summary>
        /// Append a service registration to a collection of registrations made for a service
        /// </summary>
        /// <param name="serviceType">The service to be registered</param>
        /// <param name="concreteType">The concrete type to be appended to the collection registrations</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed</param>
        /// <returns></returns>
        public ServiceRegistrar RegisterCollection(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
        {
            if(!serviceType.IsAssignableFrom(concreteType))
                throw new IncompatibleTypesException(serviceType, concreteType);

            else if (!_rootRegistrations.Contains(serviceType))
            {
                //register the concrete type just in case it wasn't done externally
                if (!_rootRegistrations.Contains(concreteType))
                    _ = Register(concreteType, scope, interceptorProfile);

                if (interceptorProfile == null)
                    _ = _registrar.AppendCollectionRegistration(serviceType, concreteType, scope);

                else
                {
                    _ = _registrar.AppendCollectionRegistration(
                        serviceType: serviceType,
                        scope: scope,
                        factory: resolver =>
                        {
                            return resolver
                                .Resolve(concreteType)
                                .Pipe(instance => _proxyGenerator.CreateClassProxyWithTarget(
                                    serviceType,
                                    instance,
                                    interceptorProfile.Value.Interceptors.ToArray()));
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }


        /// <summary>
        /// Register a concrete type
        /// </summary>
        /// <typeparam name="Impl">The concrete type to be registered and resolved</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="registry">The interceptor to intercept calls to the service if needed</param>
        public ServiceRegistrar Register<Impl>(
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Impl : class
        {
            var serviceType = typeof(Impl);
            if (!_rootRegistrations.Contains(serviceType))
            {
                if (interceptorProfile == null)
                    _ = _registrar.Register<Impl>(scope);

                else
                {
                    _ = _registrar.Register(
                        scope: scope,
                        factory: resolver =>
                        {
                            var instance = resolver.Resolve<Impl>();
                            var proxy = _proxyGenerator.CreateClassProxyWithTarget(
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray());

                            return proxy;
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }

        /// <summary>
        /// Register a concrete implementation for a service type
        /// </summary>
        /// <typeparam name="Service">The service type to be registered and resolved</typeparam>
        /// <typeparam name="Impl">The service implementation to be resolved for the service type</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="registry">The interceptor to intercept calls to the service if needed</param>
        public ServiceRegistrar Register<Service, Impl>(
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Service : class where Impl : class, Service
        {
            var serviceType = typeof(Service);
            var concreteType = typeof(Impl);
            if (!_rootRegistrations.Contains(serviceType))
            {
                //register the concrete type just in case it wasn't done externally
                if (!_rootRegistrations.Contains(concreteType))
                    _ = Register<Impl>(scope, interceptorProfile);

                if (interceptorProfile == null)
                    _ = _registrar.Register<Service, Impl>(scope);

                else
                {
                    _ = _registrar.Register<Service>(
                        scope: scope,
                        factory: resolver =>
                        {
                            var instance = resolver.Resolve<Impl>();
                            var proxy = _proxyGenerator.CreateClassProxyWithTarget<Service>(
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray());

                            return proxy;
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }

        /// <summary>
        /// Register a factory method that should be used in resolving instances of a service interface/type
        /// </summary>
        /// <typeparam name="Impl">The service type to be registered and resolved</typeparam>
        /// <param name="factory">A factory method used to create the service type</param>
        /// <param name="scope">The resolution scope</param>
        /// <param name="registry">The interceptor to intercept calls to the service if needed</param>
        public ServiceRegistrar Register<Service>(
            Func<IServiceResolver, Service> factory,
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Service : class
        {
            var serviceType = typeof(Service);
            if (!_rootRegistrations.Contains(serviceType))
            {
                if (interceptorProfile == null)
                    _ = _registrar.Register(factory, scope);

                else
                {
                    _ = _registrar.Register(
                        serviceType: serviceType,
                        scope: scope,
                        factory: resolver =>
                        {
                            var instance = factory.Invoke(resolver);
                            var proxy = _proxyGenerator.CreateClassProxyWithTarget(
                                instance,
                                interceptorProfile.Value.Interceptors.ToArray());

                            return proxy;
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }

        /// <summary>
        /// Append a service registration to a collection of registrations made for a service
        /// </summary>
        /// <typeparam name="Service">The serivce to register</typeparam>
        /// <typeparam name="Impl">The implemnetation to add to the collection registration</typeparam>
        /// <param name="scope">The resolution scope</param>
        /// <param name="interceptorProfile">The interceptor to intercept calls to the service if needed</param>
        /// <returns></returns>
        public ServiceRegistrar RegisterCollection<Service, Impl>(
            RegistryScope? scope = null,
            InterceptorProfile? interceptorProfile = null)
            where Service : class
            where Impl: class, Service
        {
            var serviceType = typeof(Service);
            var concreteType = typeof(Impl);

            if (!serviceType.IsAssignableFrom(concreteType))
                throw new IncompatibleTypesException(serviceType, concreteType);

            else if (!_rootRegistrations.Contains(serviceType))
            {
                //register the concrete type just in case it wasn't done externally
                if (!_rootRegistrations.Contains(concreteType))
                    _ = Register<Impl>(scope, interceptorProfile);

                if (interceptorProfile == null)
                    _ = _registrar.AppendCollectionRegistration<Service, Impl>(scope);

                else
                {
                    _ = _registrar.AppendCollectionRegistration<Service, Impl>(
                        scope: scope,
                        factory: resolver =>
                        {
                            return resolver
                                .Resolve<Impl>()
                                .Pipe(instance => _proxyGenerator.CreateClassProxyWithTarget(
                                    instance,
                                    interceptorProfile.Value.Interceptors.ToArray()));
                        });
                }

                _rootRegistrations.Add(serviceType);
            }

            return this;
        }
        #endregion
    }
}
