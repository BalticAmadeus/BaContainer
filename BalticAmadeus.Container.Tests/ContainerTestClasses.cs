using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace BalticAmadeus.Container.Tests
{
    internal class ListLogProxy : DynamicProxyBase
    {
        public static readonly List<string> LoggedMessages = new List<string>();

        public ListLogProxy(object target, Type type)
            : base(target, type)
        {
        }

        protected override IMessage InvokeProtected(IMethodCallMessage methodCall)
        {
            LoggedMessages.Add(string.Format("Calling method {0}...", methodCall.MethodName));
            Console.WriteLine("Calling overriden method {0}...", methodCall.MethodName);

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

    internal class OtherListLogProxy : ListLogProxy
    {
        public OtherListLogProxy(object target, Type type)
            : base(target, type)
        {  
        }

        protected override IMessage InvokeProtected(IMethodCallMessage methodCall)
        {
            LoggedMessages.Add(string.Format("Calling overriden method {0}...", methodCall.MethodName));

            return base.InvokeProtected(methodCall);
        }
    }

    internal class ProxyWithAdditionalArguments : ListLogProxy
    {
        public ProxyWithAdditionalArguments(object target, Type type, ISimple simple)
            : base(target, type)
        {
            Simple = simple;
            LastSimple = simple;
        }

        public static ISimple LastSimple { get; set; }
        public ISimple Simple { get; private set; }

        protected override IMessage InvokeProtected(IMethodCallMessage methodCall)
        {
            LoggedMessages.Add(string.Format("Calling overriden method {0}...", methodCall.MethodName));

            return base.InvokeProtected(methodCall);
        }
    }

    internal class ProxyWithSameAdditionalArguments : ListLogProxy
    {
        public ProxyWithSameAdditionalArguments(object target, Type type, ISimple simple)
            : base(target, type)
        {
            Simple = simple;
            LastSimple = simple;
        }

        public static ISimple LastSimple { get; set; }
        public ISimple Simple { get; private set; }

        protected override IMessage InvokeProtected(IMethodCallMessage methodCall)
        {
            LoggedMessages.Add(string.Format("Calling overriden method {0}...", methodCall.MethodName));

            return base.InvokeProtected(methodCall);
        }
    }

    public interface ISimple
    {
        void Test();
    }

    /// <summary>
    /// This interface must only be inherited by concrete classes
    /// </summary>
    public interface ISimpleOnlyInConcrete
    {
        
    }

    internal class Simple : ISimple, ISimpleOnlyInConcrete
    {
        public void Test()
        {

        }
    }

    public class SimpleDecorator : ISimple
    {
        private readonly ISimple _target;

        public SimpleDecorator(ISimple target)
        {
            _target = target;
        }

        public void Test()
        {
            DecoratorWasCalled = true;
            _target.Test();
        }

        public bool DecoratorWasCalled { get; set; }
    }

    internal interface INested
    {
        ISimple Simple { get; set; }
        void Test();
    }

    internal class Nested : INested
    {
        public Nested(ISimple simple)
        {
            Simple = simple;
        }

        public ISimple Simple { get; set; }

        public void Test()
        {

        }
    }

    internal interface ISingleton
    {
    }

    internal class Singleton : ISingleton
    {
    }
}