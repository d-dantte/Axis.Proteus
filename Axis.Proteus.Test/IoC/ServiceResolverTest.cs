using Axis.Luna.Extensions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.Test.IoC
{
    using Registration = ServiceRegistrar.RegistrationMap;

    [TestClass]
    public class ServiceResolverTest
    {
        private Mock<IResolverContract> _mockResolver = new Mock<IResolverContract>();
        private Mock<IProxyGenerator> _mockProxyGenerator = new Mock<IProxyGenerator>();

        #region Constructor

        [TestMethod]
        public void Constructor_WithValidArgs_ShouldReturnValidObject()
        {
            // setup

            // test
            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            Assert.AreEqual(0, resolver.Registrations.Count());
        }

        [TestMethod]
        public void Constructor_WithNullArgs_ShouldThrowNullArgumentException()
        {
            // setup

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new ServiceResolver(
                null,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>()));

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new ServiceResolver(
                _mockResolver.Object,
                null,
                Array.Empty<Registration>()));

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                null));
        }

        [TestMethod]
        public void Constructor_WithInvalidRegistration_ShouldFilterOutInvalidRegistrations()
        {
            // setup
            var profile = new InterceptorProfile(new DummyInterceptor());
            var registrations = default(Registration).Concat(
                new Registration(typeof(C1_I1)),
                new Registration(typeof(I2), typeof(C2_I2), profile));

            // test
            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                registrations);

            Assert.AreEqual(1, resolver.Registrations.Count());
        }
        #endregion

        #region Dispose

        [TestMethod]
        public void Dispose_ShouldCallResolverDispose()
        {
            // setup
            _mockResolver
                .Setup(r => r.Dispose())
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            resolver.Dispose();

            // assert
            _mockResolver.Verify();
        }
        #endregion

        #region Resolve<Service>();

        [TestMethod]
        public void Resolve_1_WithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve<I1>())
                .Returns(() => new C1_I1())
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.Resolve<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockResolver.Verify();
        }

        [TestMethod]
        public void Resolve_1_WithProxyMarkedService_AndWithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve<I1>())
                .Returns(() => new C1_I1_IProxyMarker())
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.Resolve<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1_IProxyMarker);
            _mockResolver.Verify();
        }


        [TestMethod]
        public void Resolve_1_WithInterfaceService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve<I1>())
                .Returns(() => new C1_I1())
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] { 
                    new Registration(
                        typeof(I1),
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }


        [TestMethod]
        public void Resolve_1_WithClassService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve<C1_I1>())
                .Returns(() => new C1_I1())
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateClassProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve<C1_I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }
        #endregion

        #region Resolve(Type);

        [TestMethod]
        public void Resolve_2_WithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve(typeof(I1)))
                .Returns(() => new C1_I1())
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.Resolve(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockResolver.Verify();
        }

        [TestMethod]
        public void Resolve_2_WithProxyMarkedService_AndWithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve(typeof(I1)))
                .Returns(() => new C1_I1_IProxyMarker())
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.Resolve(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1_IProxyMarker);
            _mockResolver.Verify();
        }


        [TestMethod]
        public void Resolve_2_WithInterfaceService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve(typeof(I1)))
                .Returns(() => new C1_I1())
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }


        [TestMethod]
        public void Resolve_2_WithClassService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.Resolve(typeof(C1_I1)))
                .Returns(() => new C1_I1())
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateClassProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve(typeof(C1_I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }
        #endregion

        #region ResolveAll<Service>();

        [TestMethod]
        public void ResolveAll_1_WithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll<I1>())
                .Returns(() => new I1[] { new C1_I1() })
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.ResolveAll<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
        }

        [TestMethod]
        public void ResolveAll_1_WithProxyMarkedService_AndWithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll<I1>())
                .Returns(() => new I1[] { new C1_I1_IProxyMarker() })
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.ResolveAll<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
        }


        [TestMethod]
        public void ResolveAll_1_WithInterfaceService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll<I1>())
                .Returns(() => new I1[] { new C1_I1() })
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.ResolveAll<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }


        [TestMethod]
        public void ResolveAll_1_WithClassService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll<C1_I1>())
                .Returns(() => new[] { new C1_I1() })
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateClassProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.ResolveAll<C1_I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }
        #endregion

        #region ResolveAll(Type);

        [TestMethod]
        public void ResolveAll_2_WithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll(typeof(I1)))
                .Returns(() => new I1[] { new C1_I1() })
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.ResolveAll(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
        }

        [TestMethod]
        public void ResolveAll_2_WithProxyMarkedService_AndWithoutInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll(typeof(I1)))
                .Returns(() => new I1[] { new C1_I1_IProxyMarker() })
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());

            // test
            var obj = resolver.ResolveAll(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
        }


        [TestMethod]
        public void ResolveAll_2_WithInterfaceService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll(typeof(I1)))
                .Returns(() => new I1[] { new C1_I1() })
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.ResolveAll(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }


        [TestMethod]
        public void ResolveAll_2_WithClassService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            _mockResolver
                .Setup(r => r.ResolveAll(typeof(C1_I1)))
                .Returns(() => new[] { new C1_I1() })
                .Verifiable();

            _mockProxyGenerator
                .Setup(g => g.CreateClassProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<Type[]>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, Type[], object, IInterceptor[]>((arg1, arg2, arg3, arg4) => arg3)
                .Verifiable();

            var resolver = new ServiceResolver(
                _mockResolver.Object,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(C1_I1),
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.ResolveAll(typeof(C1_I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count());
            _mockResolver.Verify();
            _mockProxyGenerator.Verify();
        }
        #endregion


        #region nested types
        public interface I1 { }
        public interface I2 { }

        public interface I3_I1I2: I1, I2 { }

        public class C1_I1 : I1 { }
        public class C1_I1_IProxyMarker : I1, IProxyMarker { }
        public class C2_I2 : I2 { }
        public class C3_I3 : I3 { }
        public class C4_I1I2 : I1, I2 { }

        public class DummyInterceptor : IInterceptor
        {
            public List<Castle.DynamicProxy.IInvocation> Invocations { get; } = new List<Castle.DynamicProxy.IInvocation>();

            public void Intercept(Castle.DynamicProxy.IInvocation invocation)
            {
                Invocations.Add(invocation);
            }
        }
        #endregion
    }
}
