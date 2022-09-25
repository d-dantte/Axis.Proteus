using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using Axis.Proteus.IoC;
using Axis.Proteus.Test.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
    public class BindTargetTests
    {
        #region TypeTarget
        [TestMethod]
        public void TypeTargetType_Constructor_WithValidArgs_ShouldCreateInstance()
        {
            var target = IBindTarget.Of(typeof(string));
            Assert.IsNotNull(target);
            Assert.IsTrue(target is IBindTarget.TypeTarget);
        }
        [TestMethod]
        public void TypeTargetType_Constructor_WithInvalidArgs_ShouldCreateInstance()
        {
            Assert.ThrowsException<ArgumentNullException>(() => IBindTarget.Of(null));
        }

        [TestMethod]
        public void TypeTargetType_ShouldHoldAssignedType()
        {
            var target = IBindTarget.Of(typeof(Guid));
            Assert.AreEqual(typeof(Guid), target.Type);
        }

        [TestMethod]
        public void TypeTargetHashcode_ShouldBeEqualToTypesHashcode()
        {
            var target = IBindTarget.Of(typeof(Guid));
            Assert.AreEqual(target.Type.GetHashCode(), target.GetHashCode());
        }

        [TestMethod]
        public void TypeTargetEqualityTest()
        {
            var target1 = (IBindTarget.TypeTarget)IBindTarget.Of(typeof(Guid));
            var target2 = (IBindTarget.TypeTarget)IBindTarget.Of(typeof(int));
            var target3 = (IBindTarget.TypeTarget)IBindTarget.Of(typeof(Guid));
            IBindTarget.TypeTarget target4 = null;

            Assert.IsTrue(target1.Equals(target1));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(target1 == target1);
            Assert.IsFalse(target1 != target1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(target2.Equals(target2));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(target2 == target2);
            Assert.IsFalse(target2 != target2);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(target3.Equals(target3));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(target3 == target3);
            Assert.IsFalse(target3 != target3);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(target1.Equals(target3));
            Assert.IsTrue(target1 == target3);

            Assert.IsFalse(target1.Equals(target2));
            Assert.IsFalse(target1.Equals(target4));
            Assert.IsTrue(target1 != target2);
            Assert.IsTrue(target1 != target4);
        }
        #endregion

        #region FactoryTarget
        [TestMethod]
        public void FactoryTarget_Constructor_WithValidArgs_ShouldCreateInstance()
        {
            Func<IResolverContract, I1> del = CreateC1;
            var target = IBindTarget.Of(typeof(I1), del);
            Assert.IsNotNull(target);
            Assert.IsTrue(target is IBindTarget.FactoryTarget);
        }

        [TestMethod]
        public void FactoryTarget_Constructor_WithInvalidArgs_ShouldCreateInstance()
        {
            Func<IResolverContract, I1> del = CreateC1;
            Assert.ThrowsException<ArgumentNullException>(() => IBindTarget.Of(null, del));
            Assert.ThrowsException<ArgumentNullException>(() => IBindTarget.Of(typeof(I1), null));
            Assert.ThrowsException<IncompatibleTypesException>(() => IBindTarget.Of(typeof(int), del));
        }

        [TestMethod]
        public void FactoryTarget_Type_ShouldHoldAssignedType()
        {
            Func<IResolverContract, I1> del = CreateC1;
            var target = IBindTarget.Of(typeof(I1), del);
            Assert.AreEqual(typeof(I1), target.Type);
        }

        [TestMethod]
        public void FactoryTarget_Hashcode_ShouldBeEqualToHashcodeCombinedHashcode()
        {
            Func<IResolverContract, I1> del = CreateC1;
            var target = IBindTarget.Of(typeof(I1), del).As<IBindTarget.FactoryTarget>();
            Assert.AreEqual(HashCode.Combine(target.Type, target.Factory), target.GetHashCode());
        }

        [TestMethod]
        public void FactoryTarget_EqualityTest()
        {
            Func<IResolverContract, I1> delI1 = CreateC1;
            Func<IResolverContract, I2> delI2 = CreateC2;
            var target1 = (IBindTarget.FactoryTarget)IBindTarget.Of(typeof(I1), delI1);
            var target2 = (IBindTarget.FactoryTarget)IBindTarget.Of(typeof(I2), delI2);
            var target3 = (IBindTarget.FactoryTarget)IBindTarget.Of(typeof(I1), delI1);
            IBindTarget.FactoryTarget target4 = null;

            Assert.IsTrue(target1.Equals(target1));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(target1 == target1);
            Assert.IsFalse(target1 != target1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(target2.Equals(target2));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(target2 == target2);
            Assert.IsFalse(target2 != target2);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(target3.Equals(target3));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(target3 == target3);
            Assert.IsFalse(target3 != target3);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(target1.Equals(target3));
            Assert.IsTrue(target1 == target3);

            Assert.IsFalse(target1.Equals(target2));
            Assert.IsFalse(target1.Equals(target4));
            Assert.IsTrue(target1 != target2);
            Assert.IsTrue(target1 != target4);
        }
        #endregion

        private static I1 CreateC1(IResolverContract contract) => new C_I1_1();

        private static I2 CreateC2(IResolverContract contract) => new C_I2_1();
    }
}
