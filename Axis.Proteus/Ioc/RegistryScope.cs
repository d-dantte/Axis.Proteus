using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Proteus.Ioc
{
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
