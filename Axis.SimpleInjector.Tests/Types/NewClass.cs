namespace Axis.SimpleInjector.Tests.Types
{
    public class Super
    {
        public object Instance { get; }

        public Super(object instance)
        {
            Instance = instance;
        }
    }

    public class NewClass : Super
    {
        public NewClass(object instance) 
            : base(instance)
        {
        }
    }
}
