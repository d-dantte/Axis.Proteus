using Axis.Proteus.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Proteus.SimpleInjector.Test.Functional
{
    [TestClass]
    public class SimpleInjectorRegistrarTests
    {
        #region Register(Type, RegistryScope?);

        [TestMethod]
        public void Register_1_WithValidArgs_RegistersService()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorRegistrar(container);

            // test
            _ = registrar.Register(typeof(C_I1), RegistryScope.Transient);

            // assert
            container.Verify();
            var registrations = container.ExtractRegistrations();
            Assert.IsTrue(registrations.ContainsRegistration(typeof(C_I1)));
        }
        #endregion

        #region Nested Types
        public interface I1 { }
        public interface I2 { }
        public interface I3 : I1 { }
        public interface I4 : I2 { }
        public interface I5_I1_I2 : I1, I2 { }

        public class C_I1 : I1 { }
        public class C2_I1 : I1 { }
        public class C_I2 : I2 { }
        public class CC
        {
            public CC(I1 stuff)
            { }
        }
        #endregion
    }
}
