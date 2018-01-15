using System;
using System.Collections.Generic;

namespace Axis.Proteus
{
    public interface IServiceResolver : IDisposable
    {
        Service Resolve<Service>() where Service: class;
        object Resolve(Type serviceType);

        IEnumerable<Service> ResolveAll<Service>() where Service: class;
        IEnumerable<object> ResolveAll(Type serviceType);
    }
}
