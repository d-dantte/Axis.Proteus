using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus
{
    public class InterceptorRegistry
    {
        private HashSet<Type> _proxyIntereptorTypes = new HashSet<Type>();


        public InterceptorRegistry(params Type[] types)
        {
            types?.ToList().ForEach(RegisterInterceptor);
        }

        public virtual void RegisterInterceptor(Type type)
        {
            _proxyIntereptorTypes.Add(type);
        }

        public virtual IEnumerable<Type> Interceptors() => _proxyIntereptorTypes.ToArray();
    }
}
