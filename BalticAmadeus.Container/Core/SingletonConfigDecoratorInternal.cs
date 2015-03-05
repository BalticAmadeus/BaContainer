using System;
using System.Collections.Generic;

namespace BalticAmadeus.Container.Core
{
    /// <summary>
    /// Configuration class that makes the specific type instance must be Singleton.
    /// </summary>
    internal class SingletonConfigDecoratorInternal : ITypeConfiguration
    {
        private readonly ITypeConfiguration _original;
        private object _instance;

        /// <summary>
        /// Initializes a new instance of <see cref="SingletonConfigDecoratorInternal"/> class that wraps
        /// the original configuration.
        /// </summary>
        /// <param name="original"></param>
        public SingletonConfigDecoratorInternal(ITypeConfiguration original)
        {
            _original = original;
        }

        /// <summary>
        /// Gets the implementation type for abstract type to implement.
        /// </summary>
        public Type ConcreteType
        {
            get { return _original.ConcreteType; }
        }

        /// <summary>
        /// Gets the dictionary of type and value mapping for implementation type constructor parameters.
        /// </summary>
        public IDictionary<Type, object> Parameters
        {
            get { return _original.Parameters; }
        }

        /// <summary>
        /// Gets the stack of decorators that must be applied for specified type.
        /// </summary>
        public Stack<Type> Decorators
        {
            get { return _original.Decorators; }
        }

        /// <summary>
        /// Gets or sets the action that must be called during the <see cref="Container.Release{TInterface}"/> call.
        /// </summary>
        public Action<object> ReleaseAction
        {
            get { return _original.ReleaseAction; }
            set { _original.ReleaseAction = value; }
        }

        /// <summary>
        /// Resolves the Singleton instance or creates a new one using container.
        /// </summary>
        /// <param name="container">Container containing the configuration of additional types used in resolvable tree.</param>
        /// <returns>Created or resolved instance.</returns>
        public object CreateInstance(IContainer container)
        {
            return _instance ?? (_instance = _original.CreateInstance(container));
        }
    }
}