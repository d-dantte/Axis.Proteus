using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Proteus.SimpleInjector
{
    public class Ioc: IServiceRegistrar, IResolutionScopeProvider, IServiceResolver, IDisposable
    {
        private Container _container;
        private IServiceResolver _resolutionScope;

        private bool IsResolutionRoot => _container != null;

        #region Init
        public Ioc(Container container)
        {
            _container = container;
        }

        private Ioc(IServiceResolver resolutionScope)
        {
            _resolutionScope = resolutionScope;
        }
        #endregion

        public void Dispose()
        {
            _container.Dispose();
        }


        #region ISesrviceRegistrar

        public IServiceRegistrar Register(Type concreteType, object param = null)
        {
            throw new NotImplementedException();
        }

        public IServiceRegistrar Register(Type serviceType, Func<object> factory, object param = null)
        {
            throw new NotImplementedException();
        }

        public IServiceRegistrar Register(Type serviceType, Type concreteType, object param = null)
        {
            throw new NotImplementedException();
        }

        public IServiceRegistrar Register<Impl>(object param = null)
        {
            throw new NotImplementedException();
        }

        public IServiceRegistrar Register<Service>(Func<Service> factory, object param = null)
        {
            throw new NotImplementedException();
        }

        public IServiceRegistrar Register<Service, Impl>(object param = null)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IResolutionScopeProvider
        public IServiceResolver ResolutionScope(object parameter)
        {
            return new Ioc()
        }
        
        public IServiceResolver ResolutionScope()
        {

        }
        #endregion

        #region IServiceResolver

        public object Resolve(Type serviceType, params object[] args)
        {
            throw new NotImplementedException();
        }

        public Service Resolve<Service>(params object[] args)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveAll(Type serviceType, params object[] args)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Service> ResolveAll<Service>(params object[] args)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    
}
