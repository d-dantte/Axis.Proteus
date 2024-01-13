namespace Axis.Castle.Tests
{
    public interface ISomething
    {
        string Name();

        TimeSpan TimeToLive();

        string FaultingMethod();
    }

    public class ConcreteSomething : ISomething
    {
        public string Name() => Guid.NewGuid().ToString();

        public TimeSpan TimeToLive() => TimeSpan.FromHours(new Random(DateTime.Now.Millisecond).NextDouble());

        public string FaultingMethod() => throw new Exception();
    }

    public class VirtualSomething : ISomething
    {
        virtual public string Name() => $"{DateTime.Now.ToLongDateString()} - {DateTime.Now.ToLongTimeString()}";

        virtual public TimeSpan TimeToLive() => TimeSpan.FromHours(new Random(DateTime.Now.Millisecond).NextDouble());

        public string FaultingMethod() => throw new Exception();
    }

    public sealed class SealedSomething : ISomething
    {
        public string Name() => Guid.NewGuid().ToString();

        public TimeSpan TimeToLive() => TimeSpan.FromHours(new Random(DateTime.Now.Millisecond).NextDouble());

        public string FaultingMethod() => throw new Exception();
    }
}
