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

        public virtual object Resolve(Type serviceType)
            => _container
                .GetAllInstances(serviceType)
                .FirstOrDefault()
                ?? throw new Exception("Could not resolve the type");

        public virtual Service Resolve<Service>()
            where Service : class
            => _container
                .GetAllInstances<Service>()
                .FirstOrDefault()
                ?? throw new Exception("Could not resolve the type");

        public virtual IEnumerable<Service> ResolveAll<Service>()
            where Service : class => _container.GetAllInstances<Service>();

        public virtual IEnumerable<object> ResolveAll(Type serviceType) => _container.GetAllInstances(serviceType);
        #endregion
    }
}
