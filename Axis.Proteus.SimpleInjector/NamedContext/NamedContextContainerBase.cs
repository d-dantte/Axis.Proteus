using System;

namespace Axis.Proteus.SimpleInjector.NamedContext
{
    /// <summary>
    /// </summary>
    public abstract class NamedContextContainerBase
    {
        public object Instance { get; }

        public NamedContextContainerBase(object instance)
        {
            Instance = instance;
        }
    }
}
