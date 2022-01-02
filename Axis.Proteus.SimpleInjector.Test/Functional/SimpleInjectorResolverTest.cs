using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Proteus.SimpleInjector.Test.Functional
{
    [TestClass]
    public class SimpleInjectorResolverTest
    {
        #region Resolve(Type)
        [TestMethod]
        public void Resolve_1_WithRegisteredType_ResolvesSuccessfully()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test
            var obj = registrar.Resolve(typeof(CC));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(CC));
        }

        [TestMethod]
        public void Resolve_1_WithUnregisteredType_ThrowsException()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test && assert
            Assert.ThrowsException<ActivationException>(() => registrar.Resolve(typeof(I2)));
        }
        #endregion

        #region Resolve<Service>()
        [TestMethod]
        public void Resolve_2_WithRegisteredType_ResolvesSuccessfully()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test
            var obj = registrar.Resolve<CC>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(CC));
        }

        [TestMethod]
        public void Resolve_2_WithUnregisteredType_ThrowsException()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test && assert
            Assert.ThrowsException<ActivationException>(() => registrar.Resolve<I2>());
        }
        #endregion

        #region ResolveAll(Type)
        [TestMethod]
        public void ResolveAll_1_WithRegisteredType_ResolvesSuccessfully()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test
            var objs = registrar.ResolveAll(typeof(CC));

            // assert
            Assert.IsNotNull(objs);
            Assert.IsTrue(objs.Any());
            objs.ForAll(obj => Assert.IsInstanceOfType(obj, typeof(CC)));
        }

        [TestMethod]
        public void ResolveAll_1_WithUnregisteredType_ThrowsException()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test && assert
            Assert.ThrowsException<ActivationException>(() => registrar.ResolveAll(typeof(I2)));
        }
        #endregion


        #region ResolveAll<Service>()
        [TestMethod]
        public void ResolveAll_2_WithRegisteredType_ResolvesSuccessfully()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test
            var objs = registrar.ResolveAll<CC>();

            // assert
            Assert.IsNotNull(objs);
            Assert.IsTrue(objs.Any());
            objs.ForAll(obj => Assert.IsInstanceOfType(obj, typeof(CC)));
        }

        [TestMethod]
        public void ResolveAll_2_WithUnregisteredType_ThrowsException()
        {
            // setup
            var container = new Container();
            var registrar = new SimpleInjectorResolver(container);

            container.Register(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Collection.Append(typeof(I1), typeof(C_I1), Lifestyle.Transient);
            container.Register(typeof(CC), typeof(CC), Lifestyle.Transient);
            container.Collection.Append(typeof(CC), typeof(CC), Lifestyle.Transient);

            // test && assert
            Assert.ThrowsException<ActivationException>(() => registrar.ResolveAll<I2>());
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
            public I1 Instance { get; }
            public CC(I1 stuff)
            {
                Instance = stuff;
            }
        }
        #endregion
    }
}
