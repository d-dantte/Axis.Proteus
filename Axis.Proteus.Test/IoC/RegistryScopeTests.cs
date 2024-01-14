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
            Assert.ThrowsException<ArgumentNullException>(() => new ResolutionScope(null));
        }

        [TestMethod]
        public void RegistryScope_WithIdenticalNames_ShouldBeEqual()
        {
            var bleh1 = new ResolutionScope("Bleh");
            var bleh2 = new ResolutionScope("Bleh");

            Assert.AreEqual(bleh1, bleh2);
            Assert.IsTrue(bleh1.Equals(bleh2));
            Assert.AreEqual(bleh1.GetHashCode(), bleh2.GetHashCode());
        }

        [TestMethod]
        public void RegistryScope_CreatedFromImplicitOperator_ShouldBeValid()
        {
            string scopeName = "MyScope";
            ResolutionScope scopeObject = scopeName;

            Assert.AreEqual(scopeName, scopeObject.Name);

            scopeName = scopeObject;
            Assert.AreEqual(scopeObject.Name, scopeName);
        }

        [TestMethod]
        public void RegistryScope_DefaultTests()
        {
            ResolutionScope defaultScope = default;
            ResolutionScope scope = ResolutionScope.Transient;

            Assert.AreEqual(defaultScope, scope);
            Assert.AreEqual("Transient", defaultScope.Name);
        }
    }
}
