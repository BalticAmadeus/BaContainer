using System;

namespace BalticAmadeus.Container.Core
{
    /// <summary>
    /// Holds data that must be used during the resolve of instances in <see cref="Container"/>.
    /// </summary>
    internal class InstanceToReleaseInternal
    {
        /// <summary>
        /// Gets the primary type of instance.
        /// </summary>
        public Type AbstractionType { get; private set; }

        /// <summary>
        /// Gets the weak reference of instance.
        /// </summary>
        public WeakReference InstanceReference { get; private set; }

        /// <summary>
        /// Gets or sets the action that must be called during the <see cref="Container.Release{TInterface}"/> call.
        /// </summary>
        public Action<object> ReleaseAction { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceToReleaseInternal"/> class with specified abstraction type
        /// and instance.
        /// </summary>
        /// <param name="abstractionType">Primary type of instance.</param>
        /// <param name="instance">Instance that may be released.</param>
        public InstanceToReleaseInternal(Type abstractionType, object instance)
        {
            AbstractionType = abstractionType;
            InstanceReference = new WeakReference(instance);
        }

        /// <summary>
        /// Checks if the Garbage Collector has collected the instance reference and if not
        /// then calls the target release method.
        /// </summary>
        public void Release()
        {
            if (InstanceReference.IsAlive)
                ReleaseAction(InstanceReference.Target);
        }
    }
}