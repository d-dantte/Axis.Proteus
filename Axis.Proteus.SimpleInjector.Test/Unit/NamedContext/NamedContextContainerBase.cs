using System;

namespace Axis.Proteus.SimpleInjector.Test.Unit.NamedContext
{
    public class NamedContextContainerBase
    {
        public object Instance { get; }

        public NamedContextContainerBase(Func<object> instanceProducer)
        {
            Instance = instanceProducer.Invoke();
        }
    }
}
