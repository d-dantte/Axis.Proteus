using Axis.Proteus.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
    public class ResolutoinContextNameTests
    {
        #region constructor
        [TestMethod]
        public void Constructor_WithValidArg_ShouldConstructValidInstance()
        {
            var name = "some_context";
            var contextName = new ResolutionContextName(name);
            Assert.AreNotEqual(default, contextName);
            Assert.AreEqual(name, contextName.Name);
            Assert.AreEqual($"::{name}", contextName.ToString());

            name = "someContext_1234_bleh";
            contextName = new ResolutionContextName(name);
            Assert.AreNotEqual(default, contextName);
            Assert.AreEqual(name, contextName.Name);
            Assert.AreEqual($"::{name}", contextName.ToString());
        }

        [TestMethod]
        public void Constructor_WithInvalidArg_ShouldThrowException()
        {
            var name = "";
            Assert.ThrowsException<ArgumentException>(() => new ResolutionContextName(name));

            name = "-";
            Assert.ThrowsException<ArgumentException>(() => new ResolutionContextName(name));

            name = "1_context_name";
            Assert.ThrowsException<ArgumentException>(() => new ResolutionContextName(name));

            name = null;
            Assert.ThrowsException<ArgumentException>(() => new ResolutionContextName(name));
        }
        #endregion

        #region Implicit operator
        [TestMethod]
        public void ImplicitOperator_ShouldAssignFromString()
        {
            var name = "ValidContext";
            ResolutionContextName contextName = name;
            Assert.AreNotEqual(default, contextName);
            Assert.AreEqual(name, contextName.Name);
            Assert.AreEqual($"::{name}", contextName.ToString());
        }
        #endregion

        #region equality
        [TestMethod]
        public void EqualityOperator_ShouldTestEquality()
        {
            ResolutionContextName first = "firstContext";
            ResolutionContextName first2 = "firstContext";
            ResolutionContextName second = "secondContext";
            ResolutionContextName second2 = "secondContext";
            ResolutionContextName @default = default;

            Assert.IsTrue(first.Equals(first));
            Assert.IsTrue(second.Equals(second));
            Assert.IsTrue(@default.Equals(@default));
            Assert.AreEqual(first, first2);
            Assert.AreEqual(second, second2);
            Assert.AreEqual(default, @default);
        }
        #endregion

        #region Default Value
        [TestMethod]
        public void MiscTests()
        {
            ResolutionContextName contextName = default;

            Assert.AreEqual(0, contextName.GetHashCode());
            Assert.AreEqual("::", contextName.ToString());
            Assert.IsNull(contextName.Name);
        }
        #endregion
    }
}
