using System;
using System.Collections.Generic;
using System.Linq;
using Axis.Luna.Extensions;

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
            if(!type.Implements(typeof(IProxyInterceptor)))
                throw new Exception("Invalid Interceptor Type");

            _proxyInterceptorTypes.Add(type);
        }

        public virtual IEnumerable<Type> Interceptors() => _proxyInterceptorTypes.ToArray();
    }
}