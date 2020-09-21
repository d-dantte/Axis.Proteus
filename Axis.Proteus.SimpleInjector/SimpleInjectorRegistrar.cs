using System;
using System.Linq.Expressions;
using System.Reflection;
using Axis.Luna.FInvoke;
using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    public class SimpleInjectorRegistrar : IRegistrarContract//, IDependencyResolver
    {
        private readonly Container _container;
        private readonly IServiceResolver _resolver;
        private readonly MethodInfo _convertDelegateMethod;
        private readonly MethodInfo _appendFactoryMethod;

        public SimpleInjectorRegistrar(Container container)
        {
            _container = container;
            _resolver = new SimpleInjectorResolver(container);
            _appendFactoryMethod = GetAppendFactoryMethod(container);
            _convertDelegateMethod = typeof(SimpleInjectorRegistrar)
                .GetMethod(
                    nameof(SimpleInjectorRegistrar.ConvertDelegate),
                    BindingFlags.Static |
                    BindingFlags.NonPublic);
        }

        #region IServiceRegistrar
        public IRegistrarContract AppendCollectionRegistration(
            Type serviceType, Type concreteType,
            RegistryScope? scope = null)
        {
            _container.Collection.Append(
                serviceType,
                concreteType,
                ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract AppendCollectionRegistration(
            Type serviceType,
            Func<IServiceResolver, object> factory,
            RegistryScope? scope = null)
        {
            var convertDelegateFunction = _convertDelegateMethod.MakeGenericMethod(serviceType);
            var @delegate = convertDelegateFunction.InvokeFunc(_resolver, factory);

            var appendFactoryMethod = _appendFactoryMethod.MakeGenericMethod(serviceType);
            _container.Collection.InvokeAction(
                appendFactoryMethod,
                @delegate,
                ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            RegistryScope? scope = null)
        {
            _container.Register(serviceType, serviceType, ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null)
        {
            _container.Register(serviceType, concreteType, ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Func<IServiceResolver, object> factory,
            RegistryScope? scope = null)
        {
            _container.Register(
                serviceType,
                () => factory.Invoke(_resolver),
                ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register<Impl>(
            RegistryScope? scope = null)
            where Impl : class
        {
            _container.Register<Impl>(ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register<Service>(
            Func<IServiceResolver, Service> factory,
            RegistryScope? scope = null)
            where Service : class
        {
            _container.Register<Service>(
                () => factory.Invoke(_resolver),
                ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract AppendCollectionRegistration<Service, Impl>(
            RegistryScope? scope)
            where Service: class
            where Impl: class, Service
        {
            _container.Collection.Append<Service, Impl>(ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract AppendCollectionRegistration<Service, Impl>(
            Func<IServiceResolver, Impl> factory,
            RegistryScope? scope)
            where Service : class
            where Impl : class, Service
        {
            _container.Collection.Append<Service>(
                () => factory.Invoke(_resolver),
                ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register<Service, Impl>(
            RegistryScope? scope)
            where Service : class
            where Impl : class, Service
        {
            _container.Register<Service, Impl>(ToLifestyle(scope));

            return this;
        }
        #endregion

        private static Lifestyle ToLifestyle(RegistryScope? scope)
        {
            if (scope == null)
                return Lifestyle.Transient;

            else if (scope?.Name == RegistryScope.Transient.Name)
                return Lifestyle.Transient;

            else if (scope?.Name == RegistryScope.DefaultScope.Name)
                return Lifestyle.Scoped;

            else if (scope?.Name == RegistryScope.Singleton.Name)
                return Lifestyle.Singleton;

            else return Lifestyle.Scoped; //<-- till custom scopes are supported
        }

        private static Func<TService> ConvertDelegate<TService>(
            IServiceResolver resolver,
            Func<IServiceResolver, object> func)
            where TService: class
            => () => func.Invoke(resolver) as TService;

        private static MethodInfo GetAppendFactoryMethod(Container container)
        {
            Expression<Action<Func<object>, Lifestyle>> expression = (a, b) => container.Collection.Append(a, b);

            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }
    }
}
