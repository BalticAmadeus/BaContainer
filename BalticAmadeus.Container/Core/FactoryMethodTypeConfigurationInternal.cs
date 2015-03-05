using System;

namespace BalticAmadeus.Container.Core
{
    /// <summary>
    /// Configuration class for certain specified type that must be created using factory method. 
    /// </summary>
    /// <typeparam name="TClass">Implementation class of resolvable type.</typeparam>
    internal class FactoryMethodTypeConfigurationInternal<TClass> : TypeConfigurationInternalBase
    {
        private readonly Func<IContainer, TClass> _factory;
        private readonly Func<TClass> _simpleFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="FactoryMethodTypeConfigurationInternal{TClass}"/> class
        /// using abstraction type and a factory method that can create an instance of this type.
        /// </summary>
        /// <param name="abstractType">Interface or class defining the abstraction type.</param>
        /// <param name="func">Factory method that can create an instance of defined abstract type.
        /// Function may use the specified container to resolve the instance.</param>
        public FactoryMethodTypeConfigurationInternal(Type abstractType, Func<IContainer, TClass> func)
            : base(abstractType, typeof(TClass))
        {
            _factory = func;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FactoryMethodTypeConfigurationInternal{TClass}"/> class
        /// using abstraction type and a factory method that can create an instance of this type.
        /// </summary>
        /// <param name="abstractType">Interface or class defining the abstraction type.</param>
        /// <param name="func">Factory method that can create an instance of defined abstract type.</param>
        public FactoryMethodTypeConfigurationInternal(Type abstractType, Func<TClass> func)
            : base(abstractType, typeof(TClass))
        {
            _simpleFactory = func;
        }

        /// <summary>
        /// Uses specified factory methods to create an instance of requested type.
        /// </summary>
        /// <param name="container">Container containing the configuration of additional types used in resolvable tree.</param>
        /// <returns>Created instance.</returns>
        protected override object CreateInstanceProtected(IContainer container)
        {
            if (_simpleFactory != null)
                return _simpleFactory.Invoke();

            return _factory.Invoke(container);
        }
    }
}