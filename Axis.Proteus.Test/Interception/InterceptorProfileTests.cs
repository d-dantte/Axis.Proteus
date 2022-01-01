using System;
using System.Collections.Generic;
using System.Linq;
using Axis.Proteus.Interception;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Proteus.Test.Interception
{
    [TestClass]
    public class InterceptorProfileTests
    {
        [TestMethod]
        public void Constructor_ShouldReturn_1_InvocationProfile()
        {
            var interceptor = new SampleInterceptor();
            var profile = new InterceptorProfile(interceptor);

            Assert.IsNotNull(profile);

            var interceptors = profile.Interceptors.ToArray();
            Assert.AreEqual(1, interceptors.Length);
            Assert.AreEqual(interceptor, interceptors[0]);
        }

        [TestMethod]
        public void Constructor_ShouldReturn_Multiple_InvocationProfile()
        {
            var count = new Random(DateTime.Now.Millisecond).Next(2, 20);
            var interceptors = Enumerable
                .Range(0, count)
                .Select(i => new SampleInterceptor())
                .ToArray();

            var profile = new InterceptorProfile(interceptors);
            Assert.AreEqual(count, interceptors.Length);
            Assert.IsTrue(interceptors.SequenceEqual(profile.Interceptors));
        }

        [TestMethod]
        public void InvocatoinProfiles_WithSimilarInterceptorLists_ShouldBeEqual()
        {
            var count = new Random(DateTime.Now.Millisecond).Next(2, 20);
            var enumerable = Enumerable
                .Range(0, count)
                .Select(i => new SampleInterceptor())
                .ToArray();

            var interceptors1 = enumerable.ToArray();
            var interceptors2 = enumerable.ToArray();

            var profile1 = new InterceptorProfile(interceptors1);
            var profile2 = new InterceptorProfile(interceptors2);

            Assert.IsTrue(profile1.Equals(profile2));
            Assert.AreEqual(profile1.GetHashCode(), profile2.GetHashCode());
        }

        [TestMethod]
        public void Constructor_WithNullInterceptorList_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new InterceptorProfile((IEnumerable<IInterceptor>)null));
            Assert.ThrowsException<ArgumentNullException>(() => new InterceptorProfile((IInterceptor[])null));
            Assert.ThrowsException<ArgumentNullException>(() => new InterceptorProfile(null)); //equivalent to the above call
        }

        [TestMethod]
        public void Constructor_WithEmptyInterceptorList_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentException>(() => new InterceptorProfile(Array.Empty<IInterceptor>()));
            Assert.ThrowsException<ArgumentException>(() => new InterceptorProfile(new List<IInterceptor>()));
        }

        [TestMethod]
        public void Constructor_WithNullContainingInterceptorList_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentException>(() => new InterceptorProfile((IInterceptor)null));
            Assert.ThrowsException<ArgumentException>(() => new InterceptorProfile(null, new SampleInterceptor()));
            Assert.ThrowsException<ArgumentException>(() => new InterceptorProfile(new List<IInterceptor> { null }));
        }
    }


    public class SampleInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed(); //proceed to the next method
        }

        public override string ToString() => $"{nameof(SampleInterceptor)}[{GetHashCode()}]";
    }
}
