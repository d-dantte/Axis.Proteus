using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using Axis.Proteus.Test.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
    public class BindContextTests
    {
        #region Default Context
        [TestMethod]
        public void DefaultContext_Construction_WithValidArgs_ShouldContructInstance()
        {
            var target = IBindTarget.Of(typeof(string));
            var context = IBindContext.Of(target);
            Assert.IsNotNull(context);
            Assert.IsTrue(context is IBindContext.DefaultContext);
        }

        [TestMethod]
        public void DefaultContext_Construction_WithInvalidArgs_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => IBindContext.Of(null));
        }

        [TestMethod]
        public void DefaultContext_Target_ShouldContainAssignedTarget()
        {
            var target = IBindTarget.Of(typeof(string));
            var context = IBindContext.Of(target);

            Assert.AreEqual(target, context.Target);
        }

        [TestMethod]
        public void DefaultContext_EqualityTest()
        {
            IBindTarget target = IBindTarget.Of(typeof(string));
            IBindTarget target2 = IBindTarget.Of(new Func<IResolverContract, I1>(r => new C_I1_1()));

            var context1 = IBindContext.Of(target).As<IBindContext.DefaultContext>();
            var context2 = IBindContext.Of(target2).As<IBindContext.DefaultContext>();
            var context3 = IBindContext.Of(target).As<IBindContext.DefaultContext>();
            IBindContext.DefaultContext context4 = default;

            Assert.IsTrue(context1.Equals(context1));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context1 == context1);
            Assert.IsFalse(context1 != context1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context2.Equals(context2));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context2 == context2);
            Assert.IsFalse(context2 != context2);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context3.Equals(context3));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context3 == context3);
            Assert.IsFalse(context3 != context3);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context1.Equals(context3));
            Assert.IsTrue(context1 == context3);

            Assert.IsFalse(context1.Equals(context2));
            Assert.IsFalse(context1.Equals(context4));
            Assert.IsTrue(context1 != context2);
            Assert.IsTrue(context1 != context4);
        }
        #endregion

        #region Named Context
        [TestMethod]
        public void NamedContext_Construction_WithValidArgs_ShouldContructInstance()
        {
            var target = IBindTarget.Of(typeof(string));
            var context = IBindContext.Of("contextName", target);
            Assert.IsNotNull(context);
            Assert.IsTrue(context is IBindContext.NamedContext);
        }

        [TestMethod]
        public void NamedContext_Construction_WithInvalidArgs_ShouldThrowException()
        {
            var target = IBindTarget.Of(typeof(C_I1_1));
            Assert.ThrowsException<ArgumentNullException>(() => IBindContext.Of("contextName", null));
            Assert.ThrowsException<ArgumentException>(() => IBindContext.Of(null, target));
            Assert.ThrowsException<ArgumentException>(() => IBindContext.Of(" invalid Context Name", target));
        }

        [TestMethod]
        public void NamedContext_Target_ShouldContainAssignedTarget()
        {
            var target = IBindTarget.Of(typeof(string));
            var context = IBindContext.Of("context", target);

            Assert.AreEqual(target, context.Target);
        }

        [TestMethod]
        public void NamedContext_Name_ShouldContainAssignedTarget()
        {
            var target = IBindTarget.Of(typeof(string));
            var name = "context";
            var context = IBindContext.Of(name, target).As<IBindContext.NamedContext>();

            Assert.AreEqual(name, context.Name);
        }

        [TestMethod]
        public void NamedContext_EqualityTest()
        {
            IBindTarget target = IBindTarget.Of(typeof(string));
            IBindTarget target2 = IBindTarget.Of(new Func<IResolverContract, I1>(r => new C_I1_1()));

            var context1 = IBindContext.Of("context", target).As<IBindContext.NamedContext>();
            var context2 = IBindContext.Of("context", target2).As<IBindContext.NamedContext>();
            var context3 = IBindContext.Of("context", target).As<IBindContext.NamedContext>();
            IBindContext.NamedContext context4 = default;

            Assert.IsTrue(context1.Equals(context1));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context1 == context1);
            Assert.IsFalse(context1 != context1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context2.Equals(context2));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context2 == context2);
            Assert.IsFalse(context2 != context2);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context3.Equals(context3));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context3 == context3);
            Assert.IsFalse(context3 != context3);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context1.Equals(context3));
            Assert.IsTrue(context1 == context3);

            Assert.IsFalse(context1.Equals(context2));
            Assert.IsFalse(context1.Equals(context4));
            Assert.IsTrue(context1 != context2);
            Assert.IsTrue(context1 != context4);
        }
        #endregion

        #region Parameter Context
        [TestMethod]
        public void ParameterContext_Construction_WithValidArgs_ShouldContructInstance()
        {
            var target = IBindTarget.Of(typeof(C_I1_1));
            var context = IBindContext.OfParameter(
                target: target,
                predicate: pinfo => pinfo.Member.DeclaringType.Equals(typeof(Class1)));
            Assert.IsNotNull(context);
            Assert.IsTrue(context is IBindContext.ParameterContext);
        }

        [TestMethod]
        public void ParameterContext_Construction_WithInvalidArgs_ShouldThrowException()
        {
            var target = IBindTarget.Of(typeof(C_I1_1));
            var parameter = GetClass1ConstructorParameter();
            var predicate = new Func<ParameterInfo, bool>(pinfo => pinfo.Member.DeclaringType.Equals(typeof(Class1)));

            Assert.ThrowsException<ArgumentNullException>(() => IBindContext.OfParameter(null, predicate));
            Assert.ThrowsException<ArgumentNullException>(() => IBindContext.OfParameter(target, null));
        }

        [TestMethod]
        public void ParameterContext_Target_ShouldContainAssignedTarget()
        {
            var target = IBindTarget.Of(typeof(string));
            var context = IBindContext.OfParameter(
                target: target,
                predicate: pinfo => pinfo.Member.DeclaringType.Equals(typeof(Class1)));

            Assert.AreEqual(target, context.Target);
        }

        [TestMethod]
        public void ParameterContext_Predicate_ShouldContainAssignedPredicate()
        {
            var predicate = new Func<ParameterInfo, bool>(pinfo => pinfo.Member.DeclaringType.Equals(typeof(Class1)));
            var context = IBindContext
                .OfParameter(
                    target: IBindTarget.Of(typeof(string)),
                    predicate: predicate)
                .As<IBindContext.ParameterContext>();

            Assert.AreEqual(predicate, context.Predicate);
        }

        [TestMethod]
        public void ParameterContext_EqualityTest()
        {
            IBindTarget target = IBindTarget.Of(typeof(string));
            IBindTarget target2 = IBindTarget.Of(new Func<IResolverContract, I1>(r => new C_I1_1()));

            var predicate = new Func<ParameterInfo, bool>(pinfo => pinfo.Member.DeclaringType.Equals(typeof(Class1)));

            var context1 = IBindContext
                .OfParameter(
                    target: target,
                    predicate: predicate)
                .As<IBindContext.ParameterContext>();
            var context2 = IBindContext
                .OfParameter(
                    target: target2,
                    predicate: predicate)
                .As<IBindContext.ParameterContext>();
            var context3 = IBindContext
                .OfParameter(
                    target: target,
                    predicate: predicate)
                .As<IBindContext.ParameterContext>();
            IBindContext.ParameterContext context4 = default;

            Assert.IsTrue(context1.Equals(context1));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context1 == context1);
            Assert.IsFalse(context1 != context1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context2.Equals(context2));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context2 == context2);
            Assert.IsFalse(context2 != context2);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context3.Equals(context3));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context3 == context3);
            Assert.IsFalse(context3 != context3);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context1.Equals(context3));
            Assert.IsTrue(context1 == context3);

            Assert.IsFalse(context1.Equals(context2));
            Assert.IsFalse(context1.Equals(context4));
            Assert.IsTrue(context1 != context2);
            Assert.IsTrue(context1 != context4);
        }

        #endregion

        #region Property Context
        [TestMethod]
        public void PropertyContext_Construction_WithValidArgs_ShouldContructInstance()
        {
            var target = IBindTarget.Of(typeof(C_I1_1));
            var context = IBindContext.OfProperty(
                target: target,
                predicate: pinfo => pinfo.DeclaringType.Equals(typeof(Class1)));
            Assert.IsNotNull(context);
            Assert.IsTrue(context is IBindContext.PropertyContext);
        }

        [TestMethod]
        public void PropertyContext_Construction_WithInvalidArgs_ShouldThrowException()
        {
            var target = IBindTarget.Of(typeof(C_I1_1));
            var property = GetClass1Property();
            var predicate = new Func<PropertyInfo, bool>(pinfo => pinfo.DeclaringType.Equals(typeof(Class1)));

            Assert.ThrowsException<ArgumentNullException>(() => IBindContext.OfProperty(null, predicate));
            Assert.ThrowsException<ArgumentNullException>(() => IBindContext.OfProperty(target, null));
        }

        [TestMethod]
        public void PropertyContext_Target_ShouldContainAssignedTarget()
        {
            var target = IBindTarget.Of(typeof(string));
            var context = IBindContext.OfProperty(
                target: target,
                predicate: pinfo => pinfo.DeclaringType.Equals(typeof(Class1)));

            Assert.AreEqual(target, context.Target);
        }

        [TestMethod]
        public void PropertyContext_Predicate_ShouldContainAssignedTarget()
        {
            var predicate = new Func<PropertyInfo, bool>(pinfo => pinfo.DeclaringType.Equals(typeof(Class1)));
            var context = IBindContext
                .OfProperty(
                    target: IBindTarget.Of(typeof(string)),
                    predicate: predicate)
                .As<IBindContext.PropertyContext>();

            Assert.AreEqual(predicate, context.Predicate);
        }

        [TestMethod]
        public void PropertyContext_EqualityTest()
        {
            IBindTarget target = IBindTarget.Of(typeof(string));
            IBindTarget target2 = IBindTarget.Of(new Func<IResolverContract, I1>(r => new C_I1_1()));

            var predicate = new Func<PropertyInfo, bool>(pinfo => pinfo.DeclaringType.Equals(typeof(Class1)));

            var context1 = IBindContext
                .OfProperty(
                    target: target,
                    predicate: predicate)
                .As<IBindContext.PropertyContext>();
            var context2 = IBindContext
                .OfProperty(
                    target: target2,
                    predicate: predicate)
                .As<IBindContext.PropertyContext>();
            var context3 = IBindContext
                .OfProperty(
                    target: target,
                    predicate: predicate)
                .As<IBindContext.PropertyContext>();
            IBindContext.PropertyContext context4 = default;

            Assert.IsTrue(context1.Equals(context1));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context1 == context1);
            Assert.IsFalse(context1 != context1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context2.Equals(context2));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context2 == context2);
            Assert.IsFalse(context2 != context2);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context3.Equals(context3));
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(context3 == context3);
            Assert.IsFalse(context3 != context3);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.IsTrue(context1.Equals(context3));
            Assert.IsTrue(context1 == context3);

            Assert.IsFalse(context1.Equals(context2));
            Assert.IsFalse(context1.Equals(context4));
            Assert.IsTrue(context1 != context2);
            Assert.IsTrue(context1 != context4);
        }

        #endregion

        private ParameterInfo GetClass1ConstructorParameter()
            => typeof(Class1)
                .GetConstructor(new[] { typeof(I1) })
                .GetParameters()[0];

        private PropertyInfo GetClass1Property()
            => typeof(Class1).GetProperty(nameof(Class1.Service));
    }
}
