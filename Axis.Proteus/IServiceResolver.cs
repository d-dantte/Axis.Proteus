using System;
using System.Collections.Generic;

namespace Axis.Proteus
{
    public interface IServiceResolver : IDisposable
    {
        Service Resolve<Service>();
        object Resolve(Type serviceType);

        IEnumerable<Service> ResolveAll<Service>();
        IEnumerable<object> ResolveAll(Type serviceType);
    }
}
