namespace Axis.SimpleInjector.Tests.Types
{
    public class MixedImpl1 : IService1, IService2
    {
        public string DoSomething() => DateTimeOffset.Now.ToString();

        public Guid GetId() => Guid.NewGuid();
    }
}
