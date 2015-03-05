using System;
using System.Collections.Generic;
using System.Linq;

namespace BalticAmadeus.Container
{
    /// <summary>
    /// Allows to specify the configuration for container and create it.
    /// </summary>
    public class ContainerBuilder
    {
        private readonly Dictionary<Type, IHasTypeContainerConfiguration> _configurationPartMap;
        private readonly Dictionary<Type, List<Type>> _decoratorsMap; 
        
        /// <summary>
        /// Initializes a new instance of <see cref="ContainerBuilder"/> class.
        /// </summary>
        public ContainerBuilder()
        {
            _configurationPartMap = new Dictionary<Type, IHasTypeContainerConfiguration>();
            _decoratorsMap = new Dictionary<Type, List<Type>>();
        }

        /// <summary>
        /// Creates the <see cref="ContainerBuilderPart{TInterface}"/> configuration class for specified type and registers it for container.
        /// </summary>
        /// <typeparam name="TInterface">Interface or class that may be resolved by created container.</typeparam>
        /// <returns>Configuration instance.</returns>
        public ContainerBuilderPart<TInterface> For<TInterface>() where TInterface : class
        {
            var type = typeof(TInterface);
            if (_configurationPartMap.ContainsKey(type))
                throw new InvalidOperationException("Cannot register the same type more than once.");

            var configuration = new ContainerBuilderPart<TInterface>();

            _configurationPartMap.Add(type, configuration);

            return configuration;
        }

        /// <summary>
        /// Specifies the <see cref="DynamicProxyBase"/> decorator type that should be applied for all resolved instances.
        /// If <typeparamref name="TFor"/> is class then it or it's base class should derive from <see cref="MarshalByRefObject"/> class.
        /// Only unique <typeparamref name="TFor"/> and <typeparamref name="TDecorator"/> pairs will be applied.
        /// If there are decorators in specific configuration then default decorators would not be applied.
        /// </summary>
        /// <typeparam name="TFor">Interface the decorated is requested for. This interface must be derived in extending interface, so to be applied.</typeparam>
        /// <typeparam name="TDecorator">Decorator class type derived from <see cref="DynamicProxyBase"/>.</typeparam>
        /// <returns>Updated current builder instance.</returns>
        public ContainerBuilder WithDefaultDecorator<TFor, TDecorator>() 
            where TFor : class 
            where TDecorator : DynamicProxyBase
        {
            var forType = typeof (TFor);
            var decoratorType = typeof (TDecorator);

            if (!_decoratorsMap.ContainsKey(forType))
                _decoratorsMap.Add(forType, new List<Type>());

            var decoratorsList = _decoratorsMap[forType];
            if (decoratorsList.Contains(decoratorType))
                return this;

            decoratorsList.Add(decoratorType);
            return this;
        }

        /// <summary>
        /// Creates the <see cref="IContainer"/> using specified configuration.
        /// </summary>
        /// <returns>Newly created container instance.</returns>
        public IContainer Build()
        {
            //TODO: validate
            return new Container(_configurationPartMap.ToDictionary(x => x.Key, x =>
            {
                var a = x.Value.ConfigurableTypeInstance;
                if (a.Decorators.Count != 0)
                    return a;

                foreach (var map in _decoratorsMap)
                {
                    if (!map.Key.IsAssignableFrom(a.ConcreteType))
                        continue;

                    foreach (var decoratorType in map.Value)
                        a.Decorators.Push(decoratorType);
                }
                return a;
            }));
        }
    }
}