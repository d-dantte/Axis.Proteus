namespace Axis.SimpleInjector.Tests.Types
{
    public interface IService1
    {
        string DoSomething();
    }

    public class Impl1 : IService1
    {
        public string DoSomething() => Guid.NewGuid().ToString();
    }
}
