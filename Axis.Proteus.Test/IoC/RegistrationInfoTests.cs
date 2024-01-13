using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Axis.Proteus.Test.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
    public class RegistrationInfoTests
    {
        [TestMethod]
        public void Constructor_WithValidArguments_ShouldCreateInstance()
        {
            var info = new RegistrationInfo(
                typeof(C_I1_1),
                default,
                default);
            Assert.IsNotNull(info);

            info = new RegistrationInfo(
                typeof(C_I1_1),
                default,
                default,
                IBindContext.Of("context", IBindTarget.Of(typeof(C_I1_1))));
            Assert.IsNotNull(info);

            info = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                default,
                default);
            Assert.IsNotNull(info);

            info = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                default,
                default,
                IBindContext.Of("context", IBindTarget.Of(typeof(C_I1_1))));
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void Constructor_WithInvalidArguments_ShouldThrowException()
        {
            var serviceType = typeof(I1);
            var implType = typeof(C_I1_1);
            var target = IBindTarget.Of(implType);
            var scope = default(RegistryScope);
            var profile = default(InterceptorProfile);
            var namedContext = IBindContext.Of("name", target);
            var namedContext2 = IBindContext.Of("name", IBindTarget.Of(r => typeof(C_I1_1)));

            Assert.ThrowsException<ArgumentNullException>(() => new RegistrationInfo(null));
            Assert.ThrowsException<ArgumentNullException>(() => new RegistrationInfo(null, target, scope, profile));
            Assert.ThrowsException<ArgumentNullException>(() => new RegistrationInfo(serviceType, null, scope, profile));

            // assert invalid default context
            Assert.ThrowsException<ArgumentException>(() => new RegistrationInfo(
                serviceType,
                target,
                scope,
                profile,
                IBindContext.Of(target)));

            // assert duplicate named context
            Assert.ThrowsException<ArgumentException>(() => new RegistrationInfo(
                serviceType,
                target,
                scope,
                profile,
                namedContext,
                namedContext));
            Assert.ThrowsException<ArgumentException>(() => new RegistrationInfo(
                serviceType,
                target,
                scope,
                profile,
                namedContext,
                namedContext2));

            // assert incompatible default target type
            Assert.ThrowsException<IncompatibleTypesException>(() => new RegistrationInfo(
                serviceType,
                IBindTarget.Of(typeof(Guid))));

            // assert incompatible conditional target type
            Assert.ThrowsException<IncompatibleTypesException>(() => new RegistrationInfo(
                serviceType,
                target,
                scope,
                profile,
                IBindContext.Of("named", IBindTarget.Of(r => Guid.NewGuid()))));
        }

        [TestMethod]
        public void ServiceType_MustContainAssignedType()
        {
            var info = new RegistrationInfo(typeof(C_I1_1));
            Assert.AreEqual(typeof(C_I1_1), info.ServiceType);

            var target = IBindTarget.Of(typeof(C_I1_1));
            info = new RegistrationInfo(typeof(I1), target);
            Assert.AreEqual(typeof(I1), info.ServiceType);
        }

        [TestMethod]
        public void DefaultContext_IsNeverInvalid()
        {
            var target = IBindTarget.Of(typeof(C_I1_1));
            var info = new RegistrationInfo(typeof(I1), target);

            Assert.IsNotNull(info.DefaultContext);
            Assert.AreEqual(info.DefaultContext, info.BindContexts[0]);
            Assert.AreEqual(typeof(C_I1_1), info.DefaultContext.Target.Type);
        }

        [TestMethod]
        public void Scope_ShouldContainAssignedValue()
        {
            var info = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Singleton);

            Assert.AreEqual(RegistryScope.Singleton, info.Scope);
        }

        [TestMethod]
        public void Profile_ShouldContainAssignedValue()
        {
            var profile = new InterceptorProfile(new NoOpInterceptor());
            var info = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                default,
                profile);

            Assert.AreEqual(profile, info.Profile);
        }
    }
}
