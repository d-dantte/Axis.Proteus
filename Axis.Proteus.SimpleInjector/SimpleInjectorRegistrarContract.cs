using System;
using System.Linq.Expressions;
using System.Reflection;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Axis.Proteus.IoC;
using Castle.DynamicProxy;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    public class SimpleInjectorRegistrar : IRegistrarContract
    {
        private static readonly MethodInfo _registerFactoryMethod = GetRegisterFactoryMethod();

        private readonly Container _container;

        private IResolverContract _resolver;

        public SimpleInjectorRegistrar() 
            : this(new Container())
        {
        }

        public SimpleInjectorRegistrar(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        #region IServiceRegistrar

        public bool IsRegistrationClosed() => _container.IsLocked;

        public IRegistrarContract Register(
            Type serviceType,
            RegistryScope? scope = null)
            => Register(serviceType, serviceType, scope);

        public IRegistrarContract Register(
            Type serviceType,
            Type concreteType,
            RegistryScope? scope = null)
        {
            _container.Collection.Append(
                serviceType,
                concreteType,
                scope.ToSimpleInjectorLifeStyle());

            return this;
        }

        public IRegistrarContract Register(
            Type serviceType,
            Delegate factory,
            RegistryScope? scope = null)
            => _registerFactoryMethod
                .MakeGenericMethod(serviceType)
                .ApplyTo(method => this.InvokeFunc(method, factory, scope))
                .As<IRegistrarContract>();

        public IRegistrarContract Register<Impl>(
            RegistryScope? scope = null)
            where Impl : class
            => Register<Impl, Impl>(scope);

        public IRegistrarContract Register<Service, Impl>(
            RegistryScope? scope = null)
            where Service : class
            where Impl : class, Service
            => Register(typeof(Service), typeof(Impl), scope);

        public IRegistrarContract Register<Service>(
            Func<ServiceResolver, Service> factory,
            RegistryScope? scope = null)
            where Service : class
        {
            _container.Collection.Append(
                lifestyle: scope.ToSimpleInjectorLifeStyle(),
                instanceCreator: () => factory.Invoke(_resolver.Resolve<ServiceResolver>()));

            return this;
        }


        public IResolverContract BuildResolver()
        {
            // register the singleton instance of the resolver contract
            _container.RegisterInstance<IResolverContract>(new SimpleInjectorResolver(_container));

            // verify registrations. Note that the container isn't officially "locked", but no new registrations
            // can be added after verify is called.
            _container.Verify();

            // resolve and assign the singleton instance of IResolverContract, effectively locking the container.
            return _resolver = _container.GetInstance<IResolverContract>();
        }

        #endregion

        #region Method accessors

        private static MethodInfo GetRegisterFactoryMethod()
        {
            IRegistrarContract c = null;
            Expression<Func<Func<ServiceResolver,IFake>, RegistryScope?, IRegistrarContract>> expression = (f, a) => c.Register(f, a);
            return (expression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }
        #endregion

        #region inner types
        internal interface IFake { }

        internal class FakeImpl : IFake { }
        #endregion
    }
}
