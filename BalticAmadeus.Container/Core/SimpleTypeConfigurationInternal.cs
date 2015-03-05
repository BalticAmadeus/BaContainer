using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BalticAmadeus.Container.Core
{
    /// <summary>
    /// Configuration class for certain specified type.
    /// </summary>
    internal class SimpleTypeConfigurationInternal : TypeConfigurationInternalBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SimpleTypeConfigurationInternal"/> class using
        /// the abstraction and implementation types.
        /// </summary>
        /// <param name="abstractType">Interface or class defining the abstraction type.</param>
        /// <param name="concreteType">Class defining the imlementation type.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SimpleTypeConfigurationInternal(Type abstractType, Type concreteType)
            : base(abstractType, concreteType)
        {
        }

        /// <summary>
        /// Uses the constructor of implementation type using 
        /// registered or resolved by container parameters.
        /// </summary>
        /// <param name="container">Container containing the configuration of additional types used in resolvable tree.</param>
        /// <returns>Created instance.</returns>
        protected override object CreateInstanceProtected(IContainer container)
        {
            var ci = ConcreteType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();

            if (ci == null)
                return Activator.CreateInstance(ConcreteType);

            var parameters = new List<object>();

            foreach (var parameterInfo in ci.GetParameters())
            {
                var parameterValue = ResolveParameterValue(container, parameterInfo.ParameterType);
                parameters.Add(parameterValue);
            }

            return ci.Invoke(parameters.ToArray());
        }

        private object ResolveParameterValue(IContainer container, Type type)
        {
            if (Parameters.ContainsKey(type))
                return Parameters[type];

            return container.Resolve(type);
        }
    }
}