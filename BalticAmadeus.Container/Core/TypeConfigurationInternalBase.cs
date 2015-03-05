using System;
using System.Collections.Generic;

namespace BalticAmadeus.Container.Core
{
    /// <summary>
    /// Provides the base implementation used to create configuration of specific type for container to allow resolve the instances of this type.
    /// </summary>
    internal abstract class TypeConfigurationInternalBase : ITypeConfiguration
    {
        /// <summary>
        /// Gets the primary type the configuration was requested for.
        /// </summary>
        public Type AbstractType { get; private set; }
        
        /// <summary>
        /// Gets the implementation type for abstract type to implement.
        /// </summary>
        public Type ConcreteType { get; private set; }
        
        /// <summary>
        /// Gets the dictionary of type and value mapping for implementation type constructor parameters.
        /// </summary>
        public IDictionary<Type, object> Parameters { get; private set; }
        
        /// <summary>
        /// Gets the stack of decorators that must be applied for specified type.
        /// </summary>
        public Stack<Type> Decorators { get; private set; }
        
        /// <summary>
        /// Gets or sets the action that must be called during the <see cref="Container.Release{TInterface}"/> call.
        /// </summary>
        public Action<object> ReleaseAction { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TypeConfigurationInternalBase"/> class using
        /// the abstraction and implementation types.
        /// </summary>
        /// <param name="abstractType">Interface or class defining the abstraction type.</param>
        /// <param name="concreteType">Class defining the imlementation type.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected TypeConfigurationInternalBase(Type abstractType, Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException("concreteType");

            AbstractType = abstractType;
            ConcreteType = concreteType;
            Parameters = new Dictionary<Type, object>();
            Decorators = new Stack<Type>();
        }

        /// <summary>
        /// Creates or resolves the instance with the help of container instance. After the instance is resolved it will be decorated with
        /// applied decorators.
        /// </summary>
        /// <param name="container">Container containing the configuration of additional types used in resolvable tree.</param>
        /// <returns>Created or resolved instance.</returns>
        public object CreateInstance(IContainer container)
        {
            var instance = CreateInstanceProtected(container);
            return DecorateWithProxies(container, instance);
        }

        /// <summary>
        /// Inner method that creates or resolves the instance with the help of container instance. This method can be overriden in derived
        /// classes to apply additional logic for class instance creation.
        /// </summary>
        /// <param name="container">Container containing the configuration of additional types used in resolvable tree.</param>
        /// <returns>Created or resolved instance.</returns>
        protected abstract object CreateInstanceProtected(IContainer container);

        /// <summary>
        /// Decorates the provided instance with registered decorators. It uses container to resolved additional dependencies that 
        /// may be required by specific decorator.
        /// </summary>
        /// <param name="container">Container containing the configuration of additional types used in resolvable tree.</param>
        /// <param name="instance">Instance to be decorated.</param>
        /// <returns>Decorated instance.</returns>
        protected object DecorateWithProxies(IContainer container, object instance)
        {
            foreach (var decoratorType in Decorators)
            {
                var configurableTypeInstance = new SimpleTypeConfigurationInternal(decoratorType, decoratorType);

                if (typeof(DynamicProxyBase).IsAssignableFrom(decoratorType))
                {
                    configurableTypeInstance.Parameters.Add(typeof (object), instance);
                    configurableTypeInstance.Parameters.Add(typeof (Type), AbstractType);
                }
                else
                    configurableTypeInstance.Parameters.Add(AbstractType, instance);

                instance = configurableTypeInstance.CreateInstance(container);

                if (typeof(DynamicProxyBase).IsAssignableFrom(decoratorType))
                    instance = ((DynamicProxyBase)instance).GetTransparentProxy();
            }

            return instance;
        }
    }
}