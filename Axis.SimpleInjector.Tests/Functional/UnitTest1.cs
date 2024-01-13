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

            var tlength = Lifestyle.Transient.Length;
            var slength = Lifestyle.Singleton.Length;
            var sclength = Lifestyle.Scoped.Length;

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


        [TestMethod]
        public void CollectionTests()
        {
            Container container = new Container();

            container.Collection.Append<A>(() => new A(1), Lifestyle.Transient);
            container.Collection.Append<A>(() => new A(2), Lifestyle.Transient);
            container.Collection.Append<A>(() => new A(3), Lifestyle.Transient);

            var instances = container.GetAllInstances<A>();
            Assert.IsTrue(new[] { 1, 2, 3 }.SequenceEqual(instances.Select(a => a.Id)));
        }

        [TestMethod]
        public void FactoryRegistrationTest()
        {
            Container container = new Container();

            Func<A> factory = () => new A(DateTime.Now.Millisecond);
            var castedFactory = (Func<object>)factory; 
            container.Register(typeof(A), factory);

            var instance = container.GetInstance<A>();
        }

        [TestMethod]
        public void ConditionalRegistrationTest()
        {
            Container container = new Container();

            container.Register<Class1>();
            container.RegisterConditional(
                typeof(object),
                typeof(Class2),
                context =>
                {
                    return true;
                });

            var x = container.GetInstance<Class1>();
        }
    }

    public class A 
    {
        public int Id { get; set; }

        public A(int id) => Id = id;
    }

    public interface I1 { }

    public interface I2: I1 { }

    public class X : I2 { }

    public class Class1
    {
        public object Instance { get; }

        public Class1(object bleh)
        {
            Instance = bleh;
        }
    }

    public class Class2
    {
    }
}