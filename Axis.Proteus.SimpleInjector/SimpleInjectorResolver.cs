using System;
using System.Collections.Generic;
using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{

    public class SimpleInjectorResolverContract : IResolverContract
    {
        private readonly Container _container;

        internal SimpleInjectorResolverContract(Container container)
        {
            _container = container;
        }

        #region IServiceResolver
        public void Dispose()
        {
            _container.Dispose();
        }

        public virtual object Resolve(Type serviceType) => _container.GetInstance(serviceType);

        public virtual Service Resolve<Service>()
            where Service : class => _container.GetInstance<Service>();

        public virtual IEnumerable<Service> ResolveAll<Service>()
            where Service : class => _container.GetAllInstances<Service>();

        public virtual IEnumerable<object> ResolveAll(Type serviceType) => _container.GetAllInstances(serviceType);
        #endregion
    }

    public class SimpleInjectorResolver : SimpleInjectorResolverContract
    {
        internal SimpleInjectorResolver(Container container)
            :base(container)
        {
        }
    }
}
