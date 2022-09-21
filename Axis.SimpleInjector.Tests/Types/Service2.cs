namespace Axis.SimpleInjector.Tests.Types
{
    public interface IService2
    {
        Guid GetId();
    }

    public class Impl2 : IService2
    {
        public Guid GetId() => Guid.NewGuid();
    }
}
