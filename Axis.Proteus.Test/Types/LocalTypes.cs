using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Proteus.Test.Types
{

    public interface I1
    { }

    public class C_I1_1 : I1
    { }

    public class C_I1_2 : I1
    { }

    public interface I2 { }

    public class C_I2_1 : I2 { }

    public class Class1
    {
        public I1 Service { get; set; }

        public Class1(I1 service)
        {
            Service = service;
        }
    }
}
