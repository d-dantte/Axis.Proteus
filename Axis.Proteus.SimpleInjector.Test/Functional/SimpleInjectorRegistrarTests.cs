using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System;
using System.Linq;

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
            var registrar = new SimpleInjectorRegistrar();

            // test
            _ = registrar.Register(typeof(C_I1), RegistryScope.Transient);

            // assert
            registrar
                .Manifest()
                [typeof(C_I1)]
                .First()
                .Consume(info => Assert.AreEqual(typeof(C_I1), info.ServiceType));
        }
        #endregion

        #region Register(Type, Type, RegistryScope?);

        [TestMethod]
        public void Register_2_WithValidArgs_RegistersService()
        {
            // setup
            var registrar = new SimpleInjectorRegistrar();

            // test
            _ = registrar.Register(typeof(I1), typeof(C_I1), RegistryScope.Transient);

            // assert
            registrar
                .Manifest()
                [typeof(I1)]
                .First()
                .Use(info => Assert.AreEqual(typeof(C_I1), info.Implementation.Type))
                .Consume(info => Assert.AreEqual(typeof(I1), info.ServiceType));
        }
        #endregion

        #region Register(Type, Func<IResolverContract, object>, RegistryScope?);

        [TestMethod]
        public void Register_3_WithValidArgs_RegistersService()
        {
            // setup
            var registrar = new SimpleInjectorRegistrar();

            // test
            _ = registrar.Register(
                serviceType: typeof(I1),
                scope: RegistryScope.Transient,
                factory: new Func<IResolverContract, I1>(resolver => new C_I1()));

            // assert
            registrar
                .Manifest()
                [typeof(I1)]
                .First()
                .Use(info => Assert.AreEqual(typeof(I1), info.ServiceType))
                .Use(info => Assert.AreEqual(typeof(I1), info.Implementation.Type))
                .Consume(info =>
                    Assert.IsNotNull((info.Implementation as IBindTarget.FactoryTarget).Factory as Func<I1>));
        }
        #endregion

        #region Register<Impl>(RegistryScope?);

        [TestMethod]
        public void Register_4_WithValidArgs_RegistersService()
        {
            // setup
            var registrar = new SimpleInjectorRegistrar();

            // test
            _ = registrar.Register<C_I1>(RegistryScope.Transient);

            // assert
            registrar
                .Manifest()
                [typeof(C_I1)]
                .First()
                .Use(info => Assert.AreEqual(typeof(C_I1), info.ServiceType))
                .Consume(info => Assert.AreEqual(typeof(C_I1), info.Implementation.Type));
        }
        #endregion

        #region Register<Service, Impl>(RegistryScope?);

        [TestMethod]
        public void Register_5_WithValidArgs_RegistersService()
        {
            // setup
            var registrar = new SimpleInjectorRegistrar();

            // test
            _ = registrar.Register<I1, C_I1>(RegistryScope.Transient);

            // assert
            registrar
                .Manifest()
                [typeof(I1)]
                .First()
                .Use(info => Assert.AreEqual(typeof(C_I1), info.Implementation.Type))
                .Consume(info => Assert.AreEqual(typeof(I1), info.ServiceType));
        }
        #endregion

        #region Register<Service>(Func<IResolverContract, Service>, RegistryScope?);

        [TestMethod]
        public void Register_6_WithValidArgs_RegistersService()
        {
            // setup
            var registrar = new SimpleInjectorRegistrar();

            // test
            _ = registrar.Register<I1>(
                scope: RegistryScope.Transient,
                factory: resolver => new C_I1());

            // assert
            registrar
                .Manifest()
                [typeof(I1)]
                .First()
                .Use(info => Assert.AreEqual(typeof(I1), info.ServiceType))
                .Use(info => Assert.AreEqual(typeof(I1), info.Implementation.Type))
                .Consume(info =>
                    Assert.IsNotNull((info.Implementation as IBindTarget.FactoryTarget).Factory as Func<I1>));
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
