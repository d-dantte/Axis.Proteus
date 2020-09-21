using System;
using System.Collections.Generic;
using Axis.Proteus.IoC;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{

    public class SimpleInjectorResolver : IServiceResolver
    {
        private readonly Container _container;

        public SimpleInjectorResolver(Container container)
        {
            _container = container;
        }

        #region IServiceResolver
        public void Dispose()
        {
            _container.Dispose();
        }

        public Service Resolve<Service>()
            where Service : class => _container.GetInstance<Service>();

        public object Resolve(Type serviceType) => _container.GetInstance(serviceType);

        public IEnumerable<Service> ResolveAll<Service>()
            where Service : class => _container.GetAllInstances<Service>();

        public IEnumerable<object> ResolveAll(Type serviceType) => _container.GetAllInstances(serviceType);
        #endregion
    }
}
