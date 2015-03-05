using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace BalticAmadeus.Container.ConsoleDemo
{
    public class FileLoggingProxy : DynamicProxyBase
    {
        private readonly FileLogger _logger;
        
        public FileLoggingProxy(object target, Type abstractionType, FileLogger logger) : base(target, abstractionType)
        {
            _logger = logger;
        }

        protected override IMessage InvokeProtected(IMethodCallMessage methodCall)
        {
            var argsBuilder = new StringBuilder();
            for (int i = 0; i < methodCall.ArgCount; i++)
            {
                argsBuilder.Append(methodCall.Args[i]);
                if (i == (methodCall.ArgCount - 1))
                    continue;

                argsBuilder.Append(", ");
            }

            _logger.Log(string.Format("Calling method {0} with data [{1}]", methodCall.MethodName, argsBuilder));

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
