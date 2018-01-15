using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Axis.Luna.Extensions;
using Axis.Nix;
using Axis.Rhea;
using Castle.DynamicProxy;
using SimpleInjector;

namespace Axis.Proteus.WebApi.SimpleInjector
{
    public class Container: IServiceResolver, IServiceRegistrar//, IDependencyResolver
    {
        private global::SimpleInjector.Container container = null;

        public Container(global::SimpleInjector.Container container)
        {
            this.container = container;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #region IServiceResolver
        public Service Resolve<Service>()
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Service> ResolveAll<Service>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region IServiceRegistrar
        public IServiceRegistrar Register(Type serviceType, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            Expression<Func<IServiceRegistrar, IServiceRegistrar>> exp = _r => _r.Register<object>(scope, registry);
            return exp.Body
                .Cast<MethodCallExpression>().Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(serviceType)
                .Pipe(_minfo => this.CallNormalized(_minfo, scope, registry))
                .Cast<IServiceRegistrar>();
        }

        public IServiceRegistrar Register(Type serviceType, Type concreteType, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            Expression<Func<IServiceRegistrar, IServiceRegistrar>> exp = _r => _r.Register<object, object>(scope, registry);
            return exp.Body
                .Cast<MethodCallExpression>().Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(serviceType)
                .Pipe(_minfo => this.CallNormalized(_minfo, scope, registry))
                .Cast<IServiceRegistrar>();
        }

        public IServiceRegistrar Register(Type serviceType, Func<object> factory, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            if (registry == null)
                container.Register(serviceType, factory, ToLifestyle(scope));
            else
            {
                if (!serviceType.IsInterface)
                {
                    container.Register(() =>
                    {
                        var generator = container.GetInstance<IProxyGenerator>();
                        return generator.CreateClassProxy(serviceType, new InterceptorRoot(container.GetInstance<IServiceResolver>(), factory, registry));
                    },
                    ToLifestyle(scope));
                }
                else
                {
                    container.Register(() =>
                    {
                        var generator = container.GetInstance<IProxyGenerator>();
                        return generator.CreateInterfaceProxyWithoutTarget(serviceType, new InterceptorRoot(container.GetInstance<IServiceResolver>(), factory, registry));
                    },
                    ToLifestyle(scope));
                }
            }

            return this;
        }

        public IServiceRegistrar Register(Type serviceType, IEnumerable<Type> implementationTypes, RegistryScope scope = null, InterceptorRegistry registry = null)
        {
            //register the individual types first
            implementationTypes.ForAll(_t => Register(_t, scope, registry));

            container.RegisterCollection(serviceType, implementationTypes);

            return this;
        }


        public IServiceRegistrar Register<Impl>(RegistryScope scope = null, InterceptorRegistry registry = null)
        where Impl : class
        {
            if (registry == null)
                container.Register<Impl>(ToLifestyle(scope));
            else
            {
                var serviceType = typeof(Impl);
                if (!serviceType.IsInterface)
                    throw new Exception("Cannot intercept Concrete Services");
                else
                {
                    container.Register(() =>
                    {
                        var generator = container.GetInstance<IProxyGenerator>();
                        return generator.CreateInterfaceProxyWithoutTarget<Impl>(new InterceptorRoot(container.GetInstance<IServiceResolver>(), registry));
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
                container.Register<Service, Impl>(ToLifestyle(scope));
            else
            {
                var serviceType = typeof(Service);
                if (!serviceType.IsInterface)
                {
                    container.Register(() =>
                    {
                        var generator = container.GetInstance<IProxyGenerator>();
                        return generator.CreateClassProxy<Service>(new InterceptorRoot(container.GetInstance<IServiceResolver>(), container.GetInstance<Impl>, registry));
                    },
                    ToLifestyle(scope));
                }
                else
                {
                    container.Register(() =>
                    {
                        var generator = container.GetInstance<IProxyGenerator>();
                        return generator.CreateInterfaceProxyWithoutTarget<Service>(new InterceptorRoot(container.GetInstance<IServiceResolver>(), container.GetInstance<Impl>, registry));
                    },
                    ToLifestyle(scope));
                }

                container.Register<Impl>(ToLifestyle(scope));
            }

            return this;
        }

        public IServiceRegistrar Register<Service>(Func<Service> factory, RegistryScope scope = null, InterceptorRegistry registry = null)
        where Service : class
        {
            if (registry == null)
                container.Register(factory, ToLifestyle(scope));
            else
            {
                var serviceType = typeof(Service);
                if (!serviceType.IsInterface)
                {
                    container.Register(() =>
                    {
                        var generator = container.GetInstance<IProxyGenerator>();
                        return generator.CreateClassProxy<Service>(new InterceptorRoot(container.GetInstance<IServiceResolver>(), factory, registry));
                    },
                    ToLifestyle(scope));
                }
                else
                {
                    container.Register(() =>
                    {
                        var generator = container.GetInstance<IProxyGenerator>();
                        return generator.CreateInterfaceProxyWithoutTarget<Service>(new InterceptorRoot(container.GetInstance<IServiceResolver>(), factory, registry));
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
            ///Modify this implementation to make sure implementationTypes are classes, then register them as concrete types on the container,
            ///and lastly, call container.RegisterCollection(...), passing a "container uncontrolled" collection whose individual elements are
            ///a projection of the implementation types that returns a castle proxy of the service interface that has been given a TargetProvider
            ///that returns the container resolved concrete class of the original implementation type.


            //register the individual types first
            implementationTypes.ForAll(_t => Register(_t, scope, registry));

            container.RegisterCollection<Service>(implementationTypes);

            return this;
        }
        #endregion
        


        private Lifestyle ToLifestyle(RegistryScope scope)
        {
            if (scope == null || scope.Name == RegistryScope.Transient.Name)
                return Lifestyle.Transient;
            if (scope.Name == RegistryScope.Singleton.Name)
                return Lifestyle.Singleton;

            else return Lifestyle.Scoped;
        }
    }
}
