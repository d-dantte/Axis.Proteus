using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.Interception
{
    public class InterceptorRegistry
    {
        private readonly HashSet<Type> _proxyInterceptorTypes = new HashSet<Type>();


        public InterceptorRegistry(params Type[] types)
        {
            types?.ToList().ForEach(RegisterInterceptor);
        }

        public virtual void RegisterInterceptor(Type type)
        {
            _proxyInterceptorTypes.Add(type);
        }

        public virtual IEnumerable<Type> Interceptors() => _proxyInterceptorTypes.ToArray();
    }
}
