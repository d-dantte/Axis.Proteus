using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axis.Luna.Operation;
using Axis.Proteus.Interception;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Proteus.Test
{
    [TestClass]
    public class InterceptorRegistryTests
    {
        [TestMethod]
        public void HappyPath()
        {

            var registry = new InterceptorRegistry(typeof(SampleInterceptor));
            Assert.AreEqual(1, registry.Interceptors().Count());
            Assert.AreEqual(typeof(SampleInterceptor), registry.Interceptors().First());

            registry = new InterceptorRegistry();
            Assert.AreEqual(0, registry.Interceptors().Count());

            registry.RegisterInterceptor(typeof(SampleInterceptor));
            Assert.AreEqual(1, registry.Interceptors().Count());
            Assert.AreEqual(typeof(SampleInterceptor), registry.Interceptors().First());
        }

        [TestMethod]
        public void UniqueInterceptorTest()
        {
            var registry = new InterceptorRegistry();
            registry.RegisterInterceptor(typeof(SampleInterceptor));
            registry.RegisterInterceptor(typeof(SampleInterceptor));

            Assert.AreEqual(1, registry.Interceptors().Count());
        }

        [TestMethod]
        public void ValidInterceptorTest()
        {
            var registry = new InterceptorRegistry();

            try
            {
                registry.RegisterInterceptor(typeof(IProxyInterceptor));
            }
            catch (Exception e)
            {
                Assert.AreEqual(0, registry.Interceptors().Count());
            }

            try
            {
                registry.RegisterInterceptor(typeof(IExtendedProxyInterceptor));
            }
            catch (Exception e)
            {
                Assert.AreEqual(0, registry.Interceptors().Count());
            }
        }

    }

    public class SampleInterceptor: IProxyInterceptor
    {
        public Operation<object> Intercept(InvocationContext context)
        => Operation.Try(() => (object) null);
    }

    public interface  IExtendedProxyInterceptor: IProxyInterceptor
    { }
}
