using Castle.DynamicProxy;

namespace Axis.Castle.Tests.Functional
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void InterfaceInterceptionTests()
        {
            var generator = new ProxyGenerator();
            var interceptions = new List<DateTimeOffset>();

            // concrete target
            var somethingProxy = generator.CreateInterfaceProxyWithTarget<ISomething>(
                new ConcreteSomething(),
                new Interceptor(interceptions));

            Assert.IsNotNull(somethingProxy);

            Console.WriteLine(somethingProxy.Name());
            Assert.AreEqual(1, interceptions.Count);

            Console.WriteLine(somethingProxy.TimeToLive());
            Assert.AreEqual(2, interceptions.Count);

            // virtual target
            somethingProxy = generator.CreateInterfaceProxyWithTarget<ISomething>(
                new VirtualSomething(),
                new Interceptor(interceptions));

            Assert.IsNotNull(somethingProxy);

            Assert.IsNotNull(somethingProxy);

            Console.WriteLine(somethingProxy.Name());
            Assert.AreEqual(3, interceptions.Count);

            Console.WriteLine(somethingProxy.TimeToLive());
            Assert.AreEqual(4, interceptions.Count);

            // sealed target
            somethingProxy = generator.CreateInterfaceProxyWithTarget<ISomething>(
                new SealedSomething(),
                new Interceptor(interceptions));

            Assert.IsNotNull(somethingProxy);

            Assert.IsNotNull(somethingProxy);

            Console.WriteLine(somethingProxy.Name());
            Assert.AreEqual(5, interceptions.Count);

            Console.WriteLine(somethingProxy.TimeToLive());
            Assert.AreEqual(6, interceptions.Count);
        }

        [TestMethod]
        public void ConcreteInterceptionTests_NoInterceptions()
        {
            var generator = new ProxyGenerator();
            var interceptions = new List<DateTimeOffset>();

            // concrete target
            var concreteProxy = generator.CreateClassProxyWithTarget<ConcreteSomething>(
                new ConcreteSomething(),
                new Interceptor(interceptions));

            Assert.IsNotNull(concreteProxy);

            Console.WriteLine(concreteProxy.Name());
            Assert.AreEqual(0, interceptions.Count);

            Console.WriteLine(concreteProxy.TimeToLive());
            Assert.AreEqual(0, interceptions.Count);
        }

        [TestMethod]
        public void VirtualInterceptionTests_HasInterceptions()
        {
            var generator = new ProxyGenerator();
            var interceptions = new List<DateTimeOffset>();

            // virtual target
            var virtualProxy = generator.CreateClassProxyWithTarget<VirtualSomething>(
                new VirtualSomething(),
                new Interceptor(interceptions));

            Assert.IsNotNull(virtualProxy);

            Console.WriteLine(virtualProxy.Name());
            Assert.AreEqual(1, interceptions.Count);

            Console.WriteLine(virtualProxy.TimeToLive());
            Assert.AreEqual(2, interceptions.Count);
        }

        [TestMethod]
        public void SealedInterceptionTests_ThrowsExceptions()
        {
            var generator = new ProxyGenerator();
            var interceptions = new List<DateTimeOffset>();

            // sealed target
            Assert.ThrowsException<TypeLoadException>(() => generator.CreateClassProxyWithTarget<SealedSomething>(
                new SealedSomething(),
                new Interceptor(interceptions)));
        }
    }

    public class Interceptor : IInterceptor
    {
        private readonly List<DateTimeOffset> interceptionCount;

        public Interceptor(List<DateTimeOffset> interceptionCount)
        {
            this.interceptionCount = interceptionCount;
        }

        public void Intercept(IInvocation invocation)
        {
            interceptionCount.Add(DateTimeOffset.Now);
            invocation.Proceed();
        }
    }
}