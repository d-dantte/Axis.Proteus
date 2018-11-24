using System;
using System.Collections.Generic;
using System.Text;
using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Axis.Proteus.Interception;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Proteus.Test
{
    [TestClass]
    public class InvocationContextTest
    {

        [TestMethod]
        public void HappyPath()
        {
            var contractType = typeof(IContract);
            var methodInfo = contractType.GetMethod("StringFunction");
            var target = new DefaultContract();
            Func<object> targetFactory = () => target;
            var interceptor = new SampleInterceptor2();
            var arguments = new object[] {10};

            var invocationContext = new InvocationContext(methodInfo, new object(), targetFactory, arguments, null, interceptor);
            Assert.AreEqual(arguments, invocationContext.Arguments);
        }
    }

    public interface IContract
    {
        void Action();
        string StringFunction(int age);
        Operation OperationFunction();
        Operation<string> StringOperationFunction();
    }

    public class DefaultContract : IContract
    {
        public void Action()
        {
        }

        public string StringFunction(int age) => $"age is: {age}";

        public Operation OperationFunction()
        => Operation.Try(() => { });

        public Operation<string> StringOperationFunction()
        => Operation.Try(() => "result");
    }

    public class SampleInterceptor2 : IProxyInterceptor
    {
        public Operation<object> Intercept(InvocationContext context)
        => Operation.Try(() =>
        {
            if (context.Method.ReturnType != typeof(void))
                return context.Target.CallFunc(context.Method, context.Arguments);

            else
            {
                context.Target.CallAction(context.Method, context.Arguments);
                return null as object;
            }
        });
    }


}
