using System;
using System.Runtime.Remoting.Proxies;
using BalticAmadeus.Container.Core;

namespace BalticAmadeus.Container
{
    /// <summary>
    /// Defines the configuration part for <see cref="IContainer"/>.
    /// Enables the easy readable definition for configuration using Internal DSL.
    /// </summary>
    /// <typeparam name="TInterface">The type that the configuration is used for.</typeparam>
    public class ContainerBuilderPart<TInterface> : IHasTypeContainerConfiguration
        where TInterface : class
    {
        /// <summary>
        /// Gets or sets the configuration for specific type instance.
        /// </summary>
        public ITypeConfiguration ConfigurableTypeInstance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerBuilderPart{TInterface}"/> class.
        /// Constructor is internal and can only be called using <see cref="ContainerBuilder"/> class.
        /// </summary>
        internal ContainerBuilderPart() { }

        /// <summary>
        /// Adds a decorator to the configuration, which would be applied after the requested instance was resolved.
        /// If <typeparamref name="TInterface"/> is class then it or it's base class should derive from <see cref="MarshalByRefObject"/> class.
        /// Only unique decorators types will be applied. If there are any decorator applied through configuration then default decorators
        /// would not be applied.
        /// </summary>
        /// <typeparam name="TProxy">Unique decorator class.</typeparam>
        /// <returns>Updated container builder part instance.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ContainerBuilderPart<TInterface> DecorateWithProxy<TProxy>() where TProxy : RealProxy
        {
            if (ConfigurableTypeInstance == null)
                throw new InvalidOperationException("Must call Use first!");

            var type = typeof(TProxy);
            if (ConfigurableTypeInstance.Decorators.Contains(type))
                return this;

            ConfigurableTypeInstance.Decorators.Push(type);

            return this;
        }

        public ContainerBuilderPart<TInterface> DecorateWith<TDecorator>() where TDecorator : TInterface
        {
            if (ConfigurableTypeInstance == null)
                throw new InvalidOperationException("Must call Use first!");

            var type = typeof(TDecorator);
            if (ConfigurableTypeInstance.Decorators.Contains(type))
                return this;

            ConfigurableTypeInstance.Decorators.Push(type);

            return this;
        }

        /// <summary>
        /// Sets the implementation type for <typeparamref name="TInterface" />. 
        /// This type would be used to resolve the instance for <typeparamref name="TInterface"/>.
        /// </summary>
        /// <typeparam name="TClass">Class type that implements the <typeparamref name="TInterface"/> interface.</typeparam>
        /// <returns>Updated container builder part instance.</returns>
        public ContainerBuilderPart<TInterface> Use<TClass>() where TClass : TInterface
        {
            if (ConfigurableTypeInstance != null)
                throw new InvalidOperationException("Already called");

            var implementationType = typeof (TClass);
            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new InvalidOperationException(string.Format("{0} type must be not abstract class or interface.", implementationType.Name));

            ConfigurableTypeInstance = new SimpleTypeConfigurationInternal(typeof(TInterface), implementationType);

            return this;
        }

        /// <summary>
        /// Sets the implementation resolve function for <typeparamref name="TInterface"/>.
        /// This method would be used to resolve the instance for <typeparamref name="TInterface"/>.
        /// </summary>
        /// <typeparam name="TClass">Class type that implements the <typeparamref name="TInterface"/> interface.</typeparam>
        /// <param name="func">Function that can use <see cref="IContainer"/> to resolve the instance.</param>
        /// <returns>Updated container builder part instance.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ContainerBuilderPart<TInterface> Use<TClass>(Func<IContainer, TClass> func) where TClass : TInterface
        {
            if (ConfigurableTypeInstance != null)
                throw new InvalidOperationException("Already called");

            ConfigurableTypeInstance = new FactoryMethodTypeConfigurationInternal<TClass>(typeof(TInterface), func);

            return this;
        }

        /// <summary>
        /// Sets the implementation resolve function for <typeparamref name="TInterface"/>.
        /// This method would be used to resolve the instance for <typeparamref name="TInterface"/>.
        /// </summary>
        /// <typeparam name="TClass">Class type that implements the <typeparamref name="TInterface"/> interface.</typeparam>
        /// <param name="func">Function that can resolve the instance.</param>
        /// <returns>Updated container builder part instance.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ContainerBuilderPart<TInterface> Use<TClass>(Func<TClass> func) where TClass : TInterface
        {
            if (ConfigurableTypeInstance != null)
                throw new InvalidOperationException("Already called");

            ConfigurableTypeInstance = new FactoryMethodTypeConfigurationInternal<TClass>(typeof(TInterface), func);

            return this;
        }

        /// <summary>
        /// Adds the parameter that should be used in <typeparamref name="TInterface"/> constructor.
        /// Method calls should be in contructor arguments order.
        /// </summary>
        /// <typeparam name="TParameter">Parameter type.</typeparam>
        /// <param name="parameter">The parameter that should be used in type contructor.</param>
        /// <returns>Updated container builder part instance.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ContainerBuilderPart<TInterface> WithParameter<TParameter>(TParameter parameter)
        {
            if (ConfigurableTypeInstance == null)
                throw new InvalidOperationException("Must call use first!");

            ConfigurableTypeInstance.Parameters.Add(typeof(TParameter), parameter);
            return this;
        }

        /// <summary>
        /// Updates the registered type instance to be Singleton.
        /// </summary>
        /// <returns></returns>
        /// <returns>Updated container builder part instance.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ContainerBuilderPart<TInterface> AsSingleton()
        {
            if (ConfigurableTypeInstance == null)
                throw new InvalidOperationException("Must call use first!");

            ConfigurableTypeInstance = new SingletonConfigDecoratorInternal(ConfigurableTypeInstance);
            return this;
        }

        /// <summary>
        /// Applies the specified method to be used on <see cref="IContainer.Resolve"/> call.
        /// </summary>
        /// <param name="action">Action method with instance release logic.</param>
        /// <returns>Updated container builder part instance.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ContainerBuilderPart<TInterface> OnRelease(Action<TInterface> action)
        {
            if (ConfigurableTypeInstance == null)
                throw new InvalidOperationException("Must call use first!");

            ConfigurableTypeInstance.ReleaseAction = obj => action((TInterface)obj);
            return this;
        }
    }
}