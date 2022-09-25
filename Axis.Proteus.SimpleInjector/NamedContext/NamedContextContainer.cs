namespace Axis.Proteus.SimpleInjector.NamedContext
{
    /// <summary>
    /// Base class for all types that enable the <see cref="IoC.IBindContext.NamedContext"/> feature.
    /// <para>
    /// Essentially, a new type is dynamically generated to implement this interface, and named after some transformation of the
    /// unique <c>Name</c> property of the source <see cref="IoC.IBindContext.NamedContext"/> instance.
    /// 
    /// The dynamic type is registered conditionally in the container, predicated on the parameter in the constructor of the given type.
    /// </para>
    /// <para>
    /// Upon resolution, if the manifest finds a match for the supplied <see cref="IoC.ResolutionContextName"/>, the
    /// the resolver requests the dynamic type instead, and upon successful resolution, returns a cast of it's 
    /// <see cref="NamedContextContainer.Instance"/> property.
    /// </para>
    /// </summary>
    public abstract class NamedContextContainer
    {
        public object Instance { get; }

        public NamedContextContainer(object instance)
        {
            Instance = instance;
        }
    }
}
