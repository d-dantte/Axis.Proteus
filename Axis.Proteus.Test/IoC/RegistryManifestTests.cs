using Axis.Proteus.Exceptions;
using Axis.Proteus.IoC;
using Axis.Proteus.Test.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
    public class RegistryManifestTests
    {
        #region Root Registration

        [TestMethod]
        public void AddRootRegistration_ShouldAddTheRegistration()
        {
            var manifest = new RegistryManifest();

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);

            var manifest2 = manifest.AddRootRegistration(registration);

            Assert.AreEqual(manifest, manifest2);
            Assert.AreEqual(typeof(I1), manifest.RootServices()[0]);
            Assert.AreEqual(registration, manifest.RootRegistrationFor(typeof(I1)));
        }

        [TestMethod]
        public void AddRootRegistration_WithInvalidArgs_ShouldThrowException()
        {
            var manifest = new RegistryManifest();

            Assert.ThrowsException<ArgumentException>(() => manifest.AddRootRegistration(default));

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);
            manifest.AddRootRegistration(registration);
            Assert.ThrowsException<DuplicateRegistrationException>(() => manifest.AddRootRegistration(registration));
        }

        [TestMethod]
        public void HasRootRegistration_ShouldReturnIfRegistrationIsPresent()
        {
            var manifest = new RegistryManifest();

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);

            Assert.IsFalse(manifest.HasRootRegistration(typeof(I1)));

            manifest.AddRootRegistration(registration);
            Assert.IsTrue(manifest.HasRootRegistration(typeof(I1)));
        }

        [TestMethod]
        public void RootRegistrationFor_ShouldReturnRegistrationsForGivenServiceType_OrNull()
        {
            var manifest = new RegistryManifest();

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);

            var result = manifest.RootRegistrationFor(typeof(I1));
            Assert.IsNull(result);

            _ = manifest.AddRootRegistration(registration);
            result = manifest.RootRegistrationFor(typeof(I1));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RootServices_ShouldReturnAllRegisteredServices()
        {
            var manifest = new RegistryManifest();

            var services = manifest.RootServices();
            Assert.AreEqual(0, services.Length);

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);
            _ = manifest.AddRootRegistration(registration);
            services = manifest.RootServices();
            Assert.AreEqual(1, services.Length);
            Assert.AreEqual(typeof(I1), services[0]);

            registration = new RegistrationInfo(
                typeof(Class1),
                RegistryScope.Singleton,
                default);
            _ = manifest.AddRootRegistration(registration);
            var services2 = manifest.RootServices();
            Assert.AreNotEqual(services, services2);
            Assert.AreEqual(2, services2.Length);
            Assert.AreEqual(typeof(I1), services2[0]);
            Assert.AreEqual(typeof(Class1), services2[1]);
        }

        #endregion

        #region Collection Registration

        [TestMethod]
        public void AddCollectionRegistration_ShouldAddTheRegistration()
        {
            var manifest = new RegistryManifest();

            var registration1 = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);

            var registration2 = new RegistrationInfo(
                typeof(Class1),
                RegistryScope.Transient,
                default);

            var registration3 = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_2)),
                RegistryScope.Transient,
                default);

            var manifest2 = manifest.AddCollectionRegistrations(registration1, registration2, registration3);

            Assert.AreEqual(manifest, manifest2);
            Assert.IsTrue(new[] { typeof(I1), typeof(Class1) }.SequenceEqual(manifest.CollectionServices()));
            Assert.AreEqual(registration1, manifest.CollectionRegistrationsFor(typeof(I1))[0]);
            Assert.AreEqual(registration3, manifest.CollectionRegistrationsFor(typeof(I1))[1]);
            Assert.AreEqual(registration2, manifest.CollectionRegistrationsFor(typeof(Class1))[0]);
        }

        [TestMethod]
        public void AddCollectionRegistration_WithInvalidArgs_ShouldThrowException()
        {
            var manifest = new RegistryManifest();

            var registration1 = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);

            var registration2 = new RegistrationInfo(
                typeof(Class1),
                RegistryScope.Transient,
                default);

            Assert.ThrowsException<ArgumentException>(() => manifest.AddCollectionRegistrations(default(RegistrationInfo)));
            Assert.ThrowsException<ArgumentException>(() => manifest.AddCollectionRegistrations(registration1, default, registration2));
        }

        [TestMethod]
        public void HasCollectionRegistration_ShouldReturnIfRegistrationIsPresent()
        {
            var manifest = new RegistryManifest();

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);

            Assert.IsFalse(manifest.HasCollectionRegistrations(typeof(I1)));

            manifest.AddCollectionRegistrations(registration);
            Assert.IsTrue(manifest.HasCollectionRegistrations(typeof(I1)));
        }

        [TestMethod]
        public void CollectionRegistrationFor_ShouldReturnRegistrationsForGivenServiceType_OrNull()
        {
            var manifest = new RegistryManifest();

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);

            var result = manifest.CollectionRegistrationsFor(typeof(I1));
            Assert.IsNull(result);

            _ = manifest.AddCollectionRegistrations(registration);
            result = manifest.CollectionRegistrationsFor(typeof(I1));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(registration, result[0]);
        }

        [TestMethod]
        public void CollectionServices_ShouldReturnAllRegisteredServices()
        {
            var manifest = new RegistryManifest();

            var services = manifest.CollectionServices();
            Assert.AreEqual(0, services.Length);

            var registration = new RegistrationInfo(
                typeof(I1),
                IBindTarget.Of(typeof(C_I1_1)),
                RegistryScope.Transient,
                default);
            _ = manifest.AddCollectionRegistrations(registration);
            services = manifest.CollectionServices();
            Assert.AreEqual(1, services.Length);
            Assert.AreEqual(typeof(I1), services[0]);

            registration = new RegistrationInfo(
                typeof(Class1),
                RegistryScope.Singleton,
                default);
            _ = manifest.AddCollectionRegistrations(registration);
            var services2 = manifest.CollectionServices();
            Assert.AreNotEqual(services, services2);
            Assert.AreEqual(2, services2.Length);
            Assert.AreEqual(typeof(I1), services2[0]);
            Assert.AreEqual(typeof(Class1), services2[1]);
        }
        #endregion
    }

}
