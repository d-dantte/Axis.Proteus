using System;
using System.Linq.Expressions;
using System.Reflection;
using Axis.Luna.FInvoke;
using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    public class SimpleInjectorRegistrarContract : IRegistrarContract
    {
        private readonly Container _container;
        private readonly IResolverContract _resolver;
        private readonly MethodInfo _convertDelegateMethod;
        private readonly MethodInfo _appendFactoryMethod;

        public SimpleInjectorRegistrarContract(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _resolver = new SimpleInjectorResolver(_container);
            _appendFactoryMethod = GetAppendFactoryMethod(_container);
            _convertDelegateMethod = typeof(SimpleInjectorRegistrarContract)
                .GetMethod(
                    nameof(SimpleInjectorRegistrar.ConvertDelegate),
                    BindingFlags.Static |
                    BindingFlags.NonPublic);
        }

        public SimpleInjectorRegistrarContract() : this(new Container())
        { }

        #region IServiceRegistrar

        public IRegistrarContract Register(
            Type serviceType,
            RegistryScope? scope = null)
        {
            if (!_container.ContainsUnverifiedRegistration(serviceType))
                _container.Register(
                    serviceType,
                    serviceType,
                    scope.ToSimpleInjectorLifeStyle());

            _container.Collection.Append(
                serviceType,
                serviceType,
                scope.ToSimpleInjectorLifeStyle());

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null)
        {
            if (!_container.ContainsUnverifiedRegistration(serviceType, concreteType))
                _container.Register(
                    serviceType,
                    concreteType,
                    scope.ToSimpleInjectorLifeStyle());

            _container.Collection.Append(
                serviceType,
                concreteType,
                scope.ToSimpleInjectorLifeStyle());

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Func<IResolverContract, object> factory,
            RegistryScope? scope = null)
        {
            if (!_container.ContainsUnverifiedRegistration(serviceType))
                _container.Register(
                    serviceType,
                    () => factory.Invoke(_resolver),
                    scope.ToSimpleInjectorLifeStyle());

            var convertDelegateFunction = _convertDelegateMethod.MakeGenericMethod(serviceType);
            var @delegate = convertDelegateFunction.InvokeFunc(_resolver, factory);

            var appendFactoryMethod = _appendFactoryMethod.MakeGenericMethod(serviceType);
            _container.Collection.InvokeAction(
                appendFactoryMethod,
                @delegate,
                scope.ToSimpleInjectorLifeStyle());

            return this;
        }

        public IRegistrarContract Register<Impl>(
            RegistryScope? scope = null)
            where Impl : class
        {
            var serviceType = typeof(Impl);
            if (!_container.ContainsUnverifiedRegistration(serviceType))
                _container.Register(
                    serviceType,
                    serviceType,
                    scope.ToSimpleInjectorLifeStyle());

            _container.Collection.Append(
                serviceType,
                serviceType,
                scope.ToSimpleInjectorLifeStyle());

            return this;
        }

        public IRegistrarContract Register<Service, Impl>(
            RegistryScope? scope = null)
            where Service : class
            where Impl : class, Service
        {
            if (!_container.ContainsUnverifiedRegistration(typeof(Service)))
                _container.Register(
                    typeof(Service),
                    typeof(Impl),
                    scope.ToSimpleInjectorLifeStyle());

            _container.Collection.Append(
                typeof(Service),
                typeof(Impl),
                scope.ToSimpleInjectorLifeStyle());

            return this;
        }

        public IRegistrarContract Register<Service>(
            Func<IResolverContract, Service> factory,
            RegistryScope? scope = null)
            where Service : class
        {
            if (!_container.ContainsUnverifiedRegistration(typeof(Service)))
                _container.Register(
                    typeof(Service),
                    () => factory.Invoke(_resolver),
                    scope.ToSimpleInjectorLifeStyle());

            _container.Collection.Append(
                lifestyle: scope.ToSimpleInjectorLifeStyle(),
                instanceCreator: () => factory.Invoke(_resolver));

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

        private static Func<TService> ConvertDelegate<TService>(
            IResolverContract resolver,
            Func<IResolverContract, object> func)
            where TService : class
            => () => func.Invoke(resolver) as TService;

        private static MethodInfo GetAppendFactoryMethod(Container container)
        {
            Expression<Action<Func<object>, Lifestyle>> expression = (a, b) => container.Collection.Append(a, b);

            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

    }

    public class SimpleInjectorRegistrar : SimpleInjectorRegistrarContract
    {

        public SimpleInjectorRegistrar(Container container)
            :base(container)
        { }

        public SimpleInjectorRegistrar() : this(new Container())
        { }
    }
}
