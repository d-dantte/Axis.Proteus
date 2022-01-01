using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System.Linq;

namespace Axis.Proteus.SimpleInjector.Test.Unit
{
    [TestClass]
    public class Sample
    {
        [TestMethod]
        public void TestMethod1()
        {
            Container c = new Container();
            var registration = Lifestyle.Singleton.CreateRegistration(typeof(C1), c);

            c.Collection.Append<I1, C1>(Lifestyle.Singleton);
            c.Collection.Append<I2, C1>(Lifestyle.Singleton);
            c.Collection.Append<I2, C2>(Lifestyle.Singleton);
            c.Collection.Append<I2, C2>(Lifestyle.Singleton);

            var t = c.GetAllInstances<I1>().FirstOrDefault();
            Assert.IsNotNull(t);

            var t2 = c.GetAllInstances<I2>().FirstOrDefault();
            Assert.IsNotNull(t2);

            Assert.AreEqual(t, t2);

            var instances = c.GetAllInstances<I2>().ToArray();
        }
    }

    public interface I1
    { }

    public interface I2
    { }

    public interface I3
    { }

    public class C1 : I1, I2, I3
    { }

    public class C2 : I1, I2, I3 
    { }
}
