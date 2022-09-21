namespace Axis.Castle.Tests
{
    public interface ISomething
    {
        string Name();

        TimeSpan TimeToLive();
    }

    public class ConcreteSomething : ISomething
    {
        public string Name() => Guid.NewGuid().ToString();

        public TimeSpan TimeToLive() => TimeSpan.FromHours(new Random(DateTime.Now.Millisecond).NextDouble());
    }

    public class VirtualSomething : ISomething
    {
        virtual public string Name() => $"{DateTime.Now.ToLongDateString()} - {DateTime.Now.ToLongTimeString()}";

        virtual public TimeSpan TimeToLive() => TimeSpan.FromHours(new Random(DateTime.Now.Millisecond).NextDouble());
    }

    public sealed class SealedSomething : ISomething
    {
        public string Name() => Guid.NewGuid().ToString();

        public TimeSpan TimeToLive() => TimeSpan.FromHours(new Random(DateTime.Now.Millisecond).NextDouble());
    }
}
