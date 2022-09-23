using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Proteus.SimpleInjector.Test.Functional
{
    [TestClass]
    public class SimpleInjectorResolverTests
    {
        [TestMethod]
        public void Resolve_WithDelegateRegisteredService_ShouldResolveAndHaveAccessToResolverContract()
        {
            var registrar = new SimpleInjectorRegistrar();

            // note that the Assert is called when Container.Verify() is called.
            registrar.Register<I1>(
                _resolver =>
                {
                    Assert.IsNotNull(_resolver);
                    return new C1();
                });

            var resolver = registrar.BuildResolver();

            var obj = resolver.Resolve<I1>();
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1);
        }
    }


    public interface I1
    {
        string Method();
    }

    public class C1 : I1
    {
        public string Method() => "Method called";
    }

    public interface I2
    {
        int Method();
    }

    public class C2 : I2
    {
        public int Method() => 2;
    }
}
