namespace Axis.SimpleInjector.Tests.Types
{
    public class Super
    {
        public object Instance { get; }
        private int _id;

        public Super(object instance)
        {
            Instance = instance;
            _id = 0;
        }

        public Super(object instance, int a, int b, int c)
        {
            Instance = instance;
            _id = a + b + c;
        }
    }

    public class NewClass : Super
    {
        public NewClass(object instance) 
            : base(instance)
        {
        }

        public NewClass(object instance, int a, int b, int c)
            : base(instance, a, b, c)
        {
        }
    }


    // trying to create a container type that we can pass
    public class NamedContextContainerType
    {
        public object? Instance { get; }

        public NamedContextContainerType(object instance)
        {
            Instance = instance;
        }
    }

    public class NamedContextSuper : NamedContextContainerType
    {
        public NamedContextSuper(object instance) 
            : base(instance)
        {
        }

        // ultimately, construct a delegate out of this method, given the dynamic type, cast it to Func<Func<obj>, NamedcontextSuper>,
        // and invoke using the factoryTarget.Factory as it's parameter
        public static NamedContextSuper NewInstance(object instance) => new NamedContextSuper(instance);
    }
}
