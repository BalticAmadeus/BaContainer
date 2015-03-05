using System;
using System.Collections.Generic;

namespace BalticAmadeus.Container.Core
{
    /// <summary>
    /// Defines the methods and properties related to type resolve configuration.
    /// </summary>
    public interface ITypeConfiguration
    {
        /// <summary>
        /// Gets the implementation type of resolvable instance.
        /// </summary>
        Type ConcreteType { get; }

        /// <summary>
        /// Gets the map of parameter types and values used for certain type constructor.
        /// </summary>
        IDictionary<Type, object> Parameters { get; }
        
        /// <summary>
        /// Gets the stack of decorators that must be applied for specified type.
        /// </summary>
        Stack<Type> Decorators { get; }
        
        /// <summary>
        /// Gets or sets the action that must be called on method release in container.
        /// </summary>
        Action<object> ReleaseAction { get; set; }

        /// <summary>
        /// Creates or resolves the instance with the help of container instance.
        /// </summary>
        /// <param name="container">Container containing the configuration of additional types used in resolvable tree.</param>
        /// <returns>Created or resolved instance.</returns>
        object CreateInstance(IContainer container);
    }
}