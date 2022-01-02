using System;
using System.Collections.Generic;
using System.Linq;
using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{

    public class SimpleInjectorResolver : IResolverContract
    {
        private readonly Container _container;

        internal SimpleInjectorResolver(Container container)
        {
            _container = container;
        }

        #region IServiceResolver
        public void Dispose()
        {
            _container.Dispose();
        }

        public object Resolve(Type serviceType) => _container.GetInstance(serviceType);

        public Service Resolve<Service>()
            where Service : class => _container.GetInstance<Service>();

        public IEnumerable<Service> ResolveAll<Service>()
            where Service : class => _container.GetAllInstances<Service>();

        public IEnumerable<object> ResolveAll(Type serviceType) => _container.GetAllInstances(serviceType);
        #endregion
    }
}
