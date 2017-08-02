using System;
using System.Collections.Generic;

namespace Axis.Proteus
{
    public interface IServiceRegistrar
    {
        IServiceRegistrar Register(Type concreteType, object param = null);
        IServiceRegistrar Register(Type serviceType, Type concreteType, object param = null);
        IServiceRegistrar Register(Type serviceType, Func<object> factory, object param = null);
        IServiceRegistrar Register(Type serviceType, IEnumerable<Type> implementationTypess, object param = null);
        IServiceRegistrar Register(Type serviceType, IEnumerable<Func<object>> implementationFactories, object param = null);

        IServiceRegistrar Register<Impl>(object param = null) where Impl : class;
        IServiceRegistrar Register<Service>(Func<Service> factory, object param = null) where Service : class;
        IServiceRegistrar Register<Service, Impl>(object param = null) where Service : class where Impl : class, Service;
        IServiceRegistrar Register<Service>(IEnumerable<Type> implementationTypes, object param = null) where Service : class;
        IServiceRegistrar Register<Service>(IEnumerable<Func<Service>> factoryList, object param = null) where Service : class;
    }
}
