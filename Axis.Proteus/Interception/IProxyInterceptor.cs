using Axis.Luna.Operation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Proteus.Interception
{
    public interface IProxyInterceptor
    {
        Operation<object> Intercept(InvocationContext context);
    }
}
