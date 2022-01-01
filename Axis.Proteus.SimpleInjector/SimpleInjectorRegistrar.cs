using System;
using System.Linq.Expressions;
using System.Reflection;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    public class SimpleInjectorRegistrar : IRegistrarContract
    {
        private readonly Container _container;
        private readonly IResolverContract _resolver;
        private readonly MethodInfo _convertDelegateMethod;
        private readonly MethodInfo _appendFactoryMethod;

        public SimpleInjectorRegistrar(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _resolver = new SimpleInjectorResolver(_container);
            _appendFactoryMethod = GetAppendFactoryMethod(_container);
            _convertDelegateMethod = typeof(SimpleInjectorRegistrar)
                .GetMethod(
                    nameof(SimpleInjectorRegistrar.ConvertDelegate),
                    BindingFlags.Static |
                    BindingFlags.NonPublic);
        }

        public SimpleInjectorRegistrar() : this(new Container())
        { }

        #region IServiceRegistrar

        public IRegistrarContract Register(
            Type serviceType,
            RegistryScope? scope = null)
        {
            _container.Collection.Append(serviceType, serviceType, ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null)
        {
            _container.Collection.Append(serviceType, concreteType, ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Func<IResolverContract, object> factory,
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

        public IRegistrarContract Register<Impl>(
            RegistryScope? scope = null)
            where Impl : class
        {
            var registration = ToLifestyle(scope).CreateRegistration(typeof(Impl), _container);
            _container.Collection.Register(registration.Concat());

            return this;
        }

        public IRegistrarContract Register<Service, Impl>(
            RegistryScope? scope = null)
            where Service : class
            where Impl : class, Service
        {
            _container.Register<Service, Impl>(ToLifestyle(scope));

            return this;
        }

        public IRegistrarContract Register<Service>(
            Func<IResolverContract, Service> factory,
            RegistryScope? scope = null)
            where Service : class
        {
            _container.Register<Service>(
                () => factory.Invoke(_resolver),
                ToLifestyle(scope));

            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IResolverContract BuildResolver()
        {
            // verify registrations. Note that the container isn't officially "locked", but no new registrations
            // can be added after verify is called.
            _container.Verify();

            return _resolver;
        }

        #endregion

        private static Lifestyle ToLifestyle(RegistryScope? scope) => scope.ToSimpleInjectorLifeStyle();

        private static Func<TService> ConvertDelegate<TService>(
            IResolverContract resolver,
            Func<IResolverContract, object> func)
            where TService: class
            => () => func.Invoke(resolver) as TService;

        private static MethodInfo GetAppendFactoryMethod(Container container)
        {
            Expression<Action<Func<object>, Lifestyle>> expression = (a, b) => container.Collection.Append(a, b);

            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

    }
}
