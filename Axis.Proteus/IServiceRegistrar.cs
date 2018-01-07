using System;
using System.Collections.Generic;

namespace Axis.Proteus
{
    public interface IServiceRegistrar
    {
        IServiceRegistrar Register(Type serviceType, RegistryScope scope = null, InterceptorRegistry registry = null);
        IServiceRegistrar Register(Type serviceType, Type concreteType, RegistryScope scope = null, InterceptorRegistry registry = null);
        IServiceRegistrar Register(Type serviceType, Func<object> factory, RegistryScope scope = null, InterceptorRegistry registry = null);
        IServiceRegistrar Register(Type serviceType, IEnumerable<Type> implementationTypess, RegistryScope scope = null, InterceptorRegistry registry = null);

        IServiceRegistrar Register<Impl>(RegistryScope scope = null, InterceptorRegistry registry = null) where Impl : class;
        IServiceRegistrar Register<Service>(Func<Service> factory, RegistryScope scope = null, InterceptorRegistry registry = null) where Service : class;
        IServiceRegistrar Register<Service, Impl>(RegistryScope scope = null, InterceptorRegistry registry = null) where Service : class where Impl : class, Service;
        IServiceRegistrar Register<Service>(IEnumerable<Type> implementationTypes, RegistryScope scope = null, InterceptorRegistry registry = null) where Service : class;
    }

    public sealed class RegistryScope
    {
        public static readonly RegistryScope Singleton = new RegistryScope("Singleton");
        public static readonly RegistryScope Transient = new RegistryScope("Transient");


        public string Name { get; set; }

        public RegistryScope()
        { }

        public RegistryScope(string name)
        {
            Name = name;
        }
    }
}
