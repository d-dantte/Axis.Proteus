using Axis.Proteus.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
    public class RegistryScopeTests
    {
        [TestMethod]
        public void Constructor_WithNullName_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RegistryScope(null));
        }

        [TestMethod]
        public void RegistryScope_WithIdenticalNames_ShouldBeEqual()
        {
            var bleh1 = new RegistryScope("Bleh");
            var bleh2 = new RegistryScope("Bleh");

            Assert.AreEqual(bleh1, bleh2);
            Assert.IsTrue(bleh1.Equals(bleh2));
            Assert.AreEqual(bleh1.GetHashCode(), bleh2.GetHashCode());
        }

        [TestMethod]
        public void RegistryScope_CreatedFromImplicitOperator_ShouldBeValid()
        {
            string scopeName = "MyScope";
            RegistryScope scopeObject = scopeName;

            Assert.AreEqual(scopeName, scopeObject.Name);

            scopeName = scopeObject;
            Assert.AreEqual(scopeObject.Name, scopeName);
        }

        [TestMethod]
        public void RegistryScope_DefaultTests()
        {
            RegistryScope defaultScope = default;
            RegistryScope scope = RegistryScope.Transient;

            Assert.AreEqual(defaultScope, scope);
            Assert.AreEqual("Transient", defaultScope.Name);
        }
    }
}
