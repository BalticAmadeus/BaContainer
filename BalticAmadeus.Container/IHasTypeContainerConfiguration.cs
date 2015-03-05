using BalticAmadeus.Container.Core;

namespace BalticAmadeus.Container
{
    /// <summary>
    /// Internal interface used to define the API for configuration with type and to skip the generics.
    /// </summary>
    internal interface IHasTypeContainerConfiguration
    {
        /// <summary>
        /// Gets the instance of type configuration.
        /// </summary>
        ITypeConfiguration ConfigurableTypeInstance { get; }
    }
}