using System;

namespace Axis.Proteus
{
    public interface IServiceRegistrar
    {
        IServiceRegistrar Register(Type concreteType, object param = null);
        IServiceRegistrar Register(Type serviceType, Type concreteType, object param = null);
        IServiceRegistrar Register(Type serviceType, Func<object> factory, object param = null);

        IServiceRegistrar Register<Impl>(object param = null);
        IServiceRegistrar Register<Service>(Func<Service> factory, object param = null);
        IServiceRegistrar Register<Service, Impl>(object param = null);
    }
}
