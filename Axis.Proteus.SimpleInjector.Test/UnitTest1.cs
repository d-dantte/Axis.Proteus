using System;
using System.Linq;
using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Axis.Nix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Proteus.SimpleInjector.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var container = new global::SimpleInjector.Container();
            var registrar = new ContainerRegistrar(container);
            var iregistry = new InterceptorRegistry(typeof(Interceptor));

            registrar.Register<ISomeService>(new[] { typeof(Service1), typeof(Service2) }, null, iregistry);

            var serviceOps = container
                .GetAllInstances<ISomeService>()
                .Select(_s => _s.Operation2())
                .Select(_op =>
                {
                    _op.ResolveSafely();
                    return _op;
                })
                .ToArray();
        }
    }


    public interface ISomeService
    {
        IOperation<string> Operation1();
        IOperation Operation2();
    }

    public class Interceptor : IProxyInterceptor
    {
        public IOperation<object> Intercept(InvocationContext context)
        => LazyOp.Try(() =>
        {
            if (context.Method.Name == "Operation1")
                return (object) context.Target.Cast<ISomeService>().Operation1();
            else return context.Target.Cast<ISomeService>().Operation2();
        });
    }

    public class Service1 : ISomeService
    {
        public IOperation<string> Operation1()
        => LazyOp.Try(() =>
        {
            return "op1";
        });

        public IOperation Operation2()
        => LazyOp.Try(() =>
        {
            throw new NotImplementedException();
        });
    }

    public class Service2 : ISomeService
    {
        public IOperation<string> Operation1()
        => LazyOp.Try(() =>
        {
            return "op2";
        });

        public IOperation Operation2()
        => LazyOp.Try(() =>
        {
            throw new NotImplementedException();
        });
    }
}
