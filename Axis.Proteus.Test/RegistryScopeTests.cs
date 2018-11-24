using Axis.Proteus.Ioc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Proteus.Test
{
    [TestClass]
    public class RegistryScopeTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var scopeName = "Tarnish";
            var scope = new RegistryScope(scopeName);

            Assert.AreEqual(scope.Name, scopeName);
        }
    }
}
