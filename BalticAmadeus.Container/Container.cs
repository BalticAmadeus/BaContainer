using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BalticAmadeus.Container.Core;

namespace BalticAmadeus.Container
{
	/// <summary>
    /// Defines the container that can resolve dependency trees for specific types.
    /// </summary>
    public class Container : IDisposable, IContainer
	{
        [ThreadStatic]
        private static IList<InstanceToReleaseInternal> _currentInstanceList;
        private static readonly object Lock = new object();

        private readonly Dictionary<Type, ITypeConfiguration> _configurationMap;
        private readonly ConcurrentDictionary<int, InstanceToReleaseInternal[]> _trackedInstances;

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// This constructor is internal in case to make using <see cref="ContainerBuilder"/> class.
        /// </summary>
        /// <param name="configurationMap">Contains mappings between interface and specific configuration.</param>
        internal Container(Dictionary<Type, ITypeConfiguration> configurationMap)
        {
            _configurationMap = configurationMap;
            _trackedInstances = new ConcurrentDictionary<int, InstanceToReleaseInternal[]>();
        }

        /// <summary>
        /// Creates or resolves the given type's dependency tree using configuration.
        /// </summary>
        /// <typeparam name="TInterface">Interface or class type that should be resolved.</typeparam>
        /// <returns>Created or resolved instance.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public TInterface Resolve<TInterface>()
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        /// <summary>
        /// Creates or resolves the given type's dependency tree using configuration.
        /// Also this method stores the tree to allow to release it later using <see cref="Release{T}"/> method.
        /// </summary>
        /// <param name="type">Interface or class type that should be resolved.</param>
        /// <returns>Created or resolved instance.</returns>
        public object Resolve(Type type)
        {
            EnsureConfigurationExistsForClass(type);

            var configuration = _configurationMap[type];

            bool isRoot = false;

            if (_currentInstanceList == null)
            {
                _currentInstanceList = new List<InstanceToReleaseInternal>();
                isRoot = true;
            }

            object instance;

            var instanceToRelease = _currentInstanceList.FirstOrDefault(p => p.AbstractionType == type);
            if (instanceToRelease == null || !instanceToRelease.InstanceReference.IsAlive)
            {
                instance = configuration.CreateInstance(this);
                _currentInstanceList.Add(new InstanceToReleaseInternal(type, instance)
                {
                    ReleaseAction = configuration.ReleaseAction
                });
            }
            else
            {
                instance = instanceToRelease.InstanceReference.Target;
            }

            if (isRoot)
            {
                if (_currentInstanceList.Count > 0)
                {
                    var hashCode = instance.GetHashCode();

                    if (!_trackedInstances.ContainsKey(hashCode))
                        _trackedInstances.TryAdd(hashCode, _currentInstanceList.Where(p => p.ReleaseAction != null).ToArray());
                }

                _currentInstanceList = null;
            }

            return instance;
        }

        private void EnsureConfigurationExistsForClass(Type type)
        {
            if (_configurationMap.ContainsKey(type)) return;
            
            if (!type.IsClass)
                throw new InvalidOperationException(
                    string.Format("Configuration for ConcreteType {0} was not registered.", type.Name));

            lock (Lock)
            {
                if (!_configurationMap.ContainsKey(type))
                    _configurationMap.Add(type, new SimpleTypeConfigurationInternal(type, type));
            }
        }

        /// <summary>
        /// Releases the specified parameter and the all dependency tree that was created 
        /// during during the call of <see cref="Resolve{T}"/> method.
        /// </summary>
        /// <typeparam name="TInterface">Interface or class type that should be released.</typeparam>
        /// <param name="instance">Instance to release.</param>
		/// <remarks>This method should be called only in the same thread as previous <see cref="Resolve"/> call</remarks>
        public void Release<TInterface>(TInterface instance)
        {
            var hashCode = instance.GetHashCode();

            InstanceToReleaseInternal[] value;
            if (!_trackedInstances.TryGetValue(hashCode, out value)) 
                return;
            
            foreach (var instanceToRelease in value)
                instanceToRelease.Release();

            _trackedInstances.TryRemove(hashCode, out value);
        }
        
        /// <summary>
        /// Releases all instances that were not released before.
        /// </summary>
        public void Dispose()
        {
            foreach (var trackedInstance in _trackedInstances.SelectMany(x => x.Value))
                trackedInstance.Release();

            _trackedInstances.Clear();
        }
    }
}