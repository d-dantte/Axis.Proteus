using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Axis.Luna.Extensions;
using Axis.Proteus.Interception;
using Axis.Proteus.Ioc;
using Castle.DynamicProxy;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    public class ContainerRegistrar : IServiceRegistrar//, IDependencyResolver
    {
        private readonly Container _container;
        private readonly IProxyGenerator _generator;
        private readonly IServiceResolver _resolver;

        public ContainerRegistrar(Container container)
        {
            _container = container;
            _resolver = new ContainerResolver(container);
            _generator = new ProxyGenerator();
        }

        #region IServiceRegistrar
        public IServiceRegistrar Register(Type serviceType, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            Expression<Func<IServiceRegistrar, IServiceRegistrar>> exp = r => r.Register<object>(scope, registry);
            return exp.Body
                .As<MethodCallExpression>().Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(serviceType)
                .Pipe(methodInfo => this.CallNormalizedFunc<IServiceRegistrar>(methodInfo, scope, registry));
        }

        public IServiceRegistrar Register(Type serviceType, Type concreteType, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            Expression<Func<IServiceRegistrar, IServiceRegistrar>> exp = r => r.Register<object, object>(scope, registry);
            return exp.Body
                .As<MethodCallExpression>().Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(serviceType)
                .Pipe(methodInfo => this.CallNormalizedFunc<IServiceRegistrar>(methodInfo, scope, registry));
        }

        public IServiceRegistrar Register(Type serviceType, Func<object> factory, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            if (registry == null)
                _container.Register(serviceType, factory, ToLifestyle(scope));
            else
            {
                if (!serviceType.IsInterface)
                {
                    _container.Register(() =>
                    {
                        var generator = _generator;
                        return generator.CreateClassProxy(serviceType, new InterceptorRoot(_resolver, factory, registry));
                    },
                    ToLifestyle(scope));
                }
                else
                {
                    _container.Register(() =>
                    {
                        var generator = _generator;
                        return generator.CreateInterfaceProxyWithoutTarget(serviceType, new InterceptorRoot(_resolver, factory, registry));
                    },
                    ToLifestyle(scope));
                }
            }

            return this;
        }

        public IServiceRegistrar Register(Type serviceType, IEnumerable<Type> implementationTypes, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            //register the individual types first
            var implementations = implementationTypes.ToArray();
            implementations.ForAll(t => Register(t, scope, registry));

            _container.Collection.Register(serviceType, implementations);

            return this;
        }


        public IServiceRegistrar Register<Impl>(RegistryScope scope = null, InterceptorRegistry registry = null)
        where Impl : class
        {
            if (registry == null)
                _container.Register<Impl>(ToLifestyle(scope));

            else
            {
                //Not sure i should still enforce this - especially since we are using interceptors in this case
                var serviceType = typeof(Impl);
                if (!serviceType.IsInterface)
                    throw new Exception("Cannot intercept Concrete Services");
                else
                {
                    _container.Register(() =>
                    {
                        var generator = _generator;
                        return generator.CreateInterfaceProxyWithoutTarget<Impl>(new InterceptorRoot(_resolver, registry));
                    },
                    ToLifestyle(scope));
                }
            }

            return this;
        }

        public IServiceRegistrar Register<Service, Impl>(RegistryScope scope, InterceptorRegistry registry)
        where Service : class
        where Impl : class, Service
        {
            if (registry == null)
                _container.Register<Service, Impl>(ToLifestyle(scope));
            else
            {
                var serviceType = typeof(Service);
                if (!serviceType.IsInterface)
                {
                    _container.Register(() =>
                    {
                        var generator = _generator;
                        return generator.CreateClassProxy<Service>(new InterceptorRoot(_resolver, _container.GetInstance<Impl>, registry));
                    },
                    ToLifestyle(scope));
                }
                else
                {
                    _container.Register(() =>
                    {
                        var generator = _generator;
                        return generator.CreateInterfaceProxyWithoutTarget<Service>(new InterceptorRoot(_resolver, _container.GetInstance<Impl>, registry));
                    },
                    ToLifestyle(scope));
                }

                _container.Register<Impl>(ToLifestyle(scope));
            }

            return this;
        }

        public IServiceRegistrar Register<Service>(Func<Service> factory, RegistryScope scope = null, InterceptorRegistry registry = null)
        where Service : class
        {
            if (registry == null)
                _container.Register(factory, ToLifestyle(scope));
            else
            {
                var serviceType = typeof(Service);
                if (!serviceType.IsInterface)
                {
                    _container.Register(() =>
                    {
                        var generator = _generator;
                        return generator.CreateClassProxy<Service>(new InterceptorRoot(_resolver, factory, registry));
                    },
                    ToLifestyle(scope));
                }
                else
                {
                    _container.Register(() =>
                    {
                        var generator = _generator;
                        return generator.CreateInterfaceProxyWithoutTarget<Service>(new InterceptorRoot(_resolver, factory, registry));
                    },
                    ToLifestyle(scope));
                }
            }

            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Service"></typeparam>
        /// <param name="implementationTypes"></param>
        /// <param name="scope"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public IServiceRegistrar Register<Service>(IEnumerable<Type> implementationTypes, RegistryScope scope = null, InterceptorRegistry registry = null)
        where Service : class
        {
            var implementations = implementationTypes.ToArray();
            if (registry == null)
            {
                //Register the individual concrete types
                implementations.ForAll(t => Register(t, scope));

                _container.Collection.Register<Service>(implementations);
            }

            else
            {
                //ensure implementations are CONCRETE types
                implementations
                    .Any(t => !t.IsClass || t.IsAbstract)
                    .ThrowIf(true, "Non-Concrete implementation found");

                //Register the individual concrete types
                implementations.ForAll(t => Register(t, scope));

                //project a factory that lazily loads the service proxies, then register the enumerable of factories
                implementations
                    .Select(t =>
                    {
                        Func<Service> factory = () => _container.GetInstance(t).As<Service>();
                        var serviceType = typeof(Service);
                        if (!serviceType.IsInterface)
                        {
                            var generator = _generator;
                            return generator.CreateClassProxy<Service>(new InterceptorRoot(_resolver, factory, registry));
                        }
                        else
                        {
                            var generator = _generator;
                            return generator.CreateInterfaceProxyWithoutTarget<Service>(new InterceptorRoot(_resolver, factory, registry));
                        }
                    })
                    .Pipe(_container.Collection.Register);
            }

            return this;
        }
        #endregion

        private static Lifestyle ToLifestyle(RegistryScope scope)
        {
            if (scope == null || scope.Name == RegistryScope.Transient.Name)
                return Lifestyle.Transient;

            else if (scope.Name == RegistryScope.Singleton.Name)
                return Lifestyle.Singleton;

            else return Lifestyle.Scoped;
        }
    }
}
