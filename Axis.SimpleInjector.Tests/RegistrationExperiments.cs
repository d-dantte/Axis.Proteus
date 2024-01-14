using SimpleInjector;

namespace Axis.SimpleInjector.Tests
{
    [TestClass]
    public class RegistrationExperiments
    {
        [TestMethod]
        public void RegistermultipleImplemnetations()
        {
            var container = new Container();
            container.Register<IStuff, Stuff1>();
            Assert.ThrowsException<InvalidOperationException>(() => container.Register<IStuff, Stuff2>());
        }


        #region Nested types
        public interface IStuff
        {

        }

        public class Stuff1 : IStuff { }

        public class Stuff2 : IStuff { }

        #endregion
    }
}
