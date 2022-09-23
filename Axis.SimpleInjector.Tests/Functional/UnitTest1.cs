using Axis.SimpleInjector.Tests.Types;
using SimpleInjector;

namespace Axis.SimpleInjector.Tests.Functional
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var container = new Container();

            container.Collection.Append<IService1, Impl1>();
            container.Collection.Append<IService2, Impl2>();
            container.Collection.Append<IService1, MixedImpl1>();
            container.Collection.Append<IService2, MixedImpl1>();

            container.Verify();

            // service1
            var instances1 = container
                .GetAllInstances<IService1>()
                .ToArray();

            Assert.AreEqual(2, instances1.Length);
            Assert.AreEqual(typeof(Impl1), instances1[0].GetType());
            Assert.AreEqual(typeof(MixedImpl1), instances1[1].GetType());

            // service2
            var instances2 = container
                .GetAllInstances<IService2>()
                .ToArray();

            Assert.AreEqual(2, instances2.Length);
            Assert.AreEqual(typeof(Impl2), instances2[0].GetType());
            Assert.AreEqual(typeof(MixedImpl1), instances2[1].GetType());
        }

        [TestMethod]
        public void DuplicateRegistration()
        {
            // collection
            var container = new Container();

            container.Collection.Append<Impl1, Impl1>();
            container.Collection.Append<Impl1, Impl1>();
            container.Collection.Append<Impl1>(() => new Impl1(), Lifestyle.Transient);

            var instanceProducer = container.GetRegistration<IEnumerable<Impl1>>();
            Assert.AreEqual(3, instanceProducer?.GetRelationships().Length ?? -1);
            container.Verify();

            // regular container
            container = new Container();

            container.Register<Impl1>();
            instanceProducer = container.GetRegistration<Impl1>();
            Assert.AreEqual(0, instanceProducer?.GetRelationships().Length ?? -1);
            Assert.AreEqual(typeof(Impl1), instanceProducer?.ImplementationType);
            Assert.IsTrue(container.ContainsUnverifiedRegistration(typeof(Impl1)));
            Assert.ThrowsException<InvalidOperationException>(() => container.Register<Impl1>());
            container.Verify();
        }
    }

    public class A { }
}