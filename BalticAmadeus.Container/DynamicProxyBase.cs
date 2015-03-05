using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace BalticAmadeus.Container
{
    /// <summary>
    /// Defines base functionality with restrictions to use <see cref="RealProxy"/>. 
    /// This class is abstract.
    /// </summary>
    public abstract class DynamicProxyBase : RealProxy
    {
        /// <summary>
        /// Gets the decorated instance object.
        /// </summary>
        protected object Target { get; private set; }
        
        private readonly string[] _methodsToInvoke;

        /// <summary>
        /// Initializes a new instance of <see cref="DynamicProxyBase"/> with required parameters.
        /// This constructor is used to make derived classes set the decorated instance and its type.
        /// </summary>
        /// <param name="target">Decorable instance object.</param>
        /// <param name="abstractionType">Decorable instance interface. 
        /// The parameter should be interface or class must derive from <see cref="MarshalByRefObject"/>.</param>
        protected DynamicProxyBase(object target, Type abstractionType) : base(abstractionType)
        {
            if (target == null)
                throw new ArgumentNullException("target", "Decorated object can't be null.");

            if (abstractionType == null)
                throw new ArgumentNullException("abstractionType", "Abstraction type can't be null.");

            Target = target;
            _methodsToInvoke =
                abstractionType.GetMethods().Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Invokes the wrapped instance method. Method wrapping can be overriden in derived classes.
        /// This method should not be overriden in derived classes.
        /// </summary>
        /// <param name="msg">Contains the call data.</param>
        /// <returns>Contains the returned data.</returns>
        public sealed override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;

            if (_methodsToInvoke.Contains(methodCall.MethodName))
                return InvokeProtected(methodCall);

            return InvokeUndecorated(methodCall);
        }

        /// <summary>
        /// Contains the additional activies that can be activated during method call. 
        /// This method must be overriden in derived classes. 
        /// </summary>
        /// <param name="methodCall">Contains method call data.</param>
        /// <returns>Contains the returned data.</returns>
        protected abstract IMessage InvokeProtected(IMethodCallMessage methodCall);

        private IMessage InvokeUndecorated(IMethodCallMessage methodCall)
        {
            try
            {
                var result = methodCall.MethodBase.Invoke(Target, methodCall.InArgs);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (TargetInvocationException invocationException)
            {
                var exception = invocationException.InnerException;
                return new ReturnMessage(exception, methodCall);
            }
        }
    }
}
