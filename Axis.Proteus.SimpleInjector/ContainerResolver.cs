using System;
using System.Collections.Generic;
using SimpleInjector;

namespace Axis.Proteus.SimpleInjector
{
    public class ContainerResolver : IServiceResolver
    {
        private Container container = null;

        public ContainerResolver(Container container)
        {
            this.container = container;
        }

        #region IServiceResolver
        public void Dispose()
        {
            container.Dispose();
        }

        public Service Resolve<Service>()
        where Service: class => container.GetInstance<Service>();

        public object Resolve(Type serviceType) => container.GetInstance(serviceType);

        public IEnumerable<Service> ResolveAll<Service>()
        where Service : class => container.GetAllInstances<Service>();

        public IEnumerable<object> ResolveAll(Type serviceType) => container.GetAllInstances(serviceType);
        #endregion
    }
}


