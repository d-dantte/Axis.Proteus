using Axis.Luna.Extensions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.SimpleInjector.Test.Unit
{
    using Registration = RegistrationInfo;

    [TestClass]
    public class SimpleInjectorServiceResolverTest
    {
        private Mock<IResolverContract> _mockResolver = new Mock<IResolverContract>();
        private Mock<IProxyGenerator> _mockProxyGenerator = new Mock<IProxyGenerator>();

        #region Constructor

        [TestMethod]
        public void Constructor_WithValidArgs_ShouldReturnValidObject()
        {
            // test
            var resolver = new SimpleInjectorResolver(
                new Container(),
                _mockProxyGenerator.Object,
                new Dictionary<Type, List<Registration>>());

            Assert.AreEqual(0, resolver.RegistrationCount());
        }

        [TestMethod]
        public void Constructor_WithNullArgs_ShouldThrowNullArgumentException()
        {
            // setup

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new SimpleInjectorResolver(
                null,
                _mockProxyGenerator.Object,
                new Dictionary<Type, List<Registration>>()));

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new SimpleInjectorResolver(
                new Container(),
                null,
                new Dictionary<Type, List<Registration>>()));

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new SimpleInjectorResolver(
                new Container(),
                _mockProxyGenerator.Object,
                null));
        }

        #endregion

        #region Dispose

        [TestMethod]
        public void Dispose_ShouldCallContainerDispose()
        {
            // setup
            var container = new Container();
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                Array.Empty<Registration>());
            var isContainerDisposed = false;
            container.ContainerScope.WhenScopeEnds(() => isContainerDisposed = true);

            // test
            resolver.Dispose();

            // assert
            Assert.IsTrue(isContainerDisposed);
        }
        #endregion

        #region Resolve<Service>();

        [TestMethod]
        public void Resolve_1_WithoutInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1)))
                });
            container.Collection.Append<I1, C1_I1>();

            // test
            var obj = resolver.Resolve<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
        }


        [TestMethod]
        public void Resolve_1_WithInterfaceService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<I1, C1_I1>();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();

            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1)),
                        default,
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockProxyGenerator.Verify();
        }


        [TestMethod]
        public void Resolve_1_WithMultipleInterfaceService_AndWithOneInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<I1, C1_I1>();
            container.Collection.Append<I1, C1_I1_IProxyMarker>();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();

            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1)),
                        default,
                        default),
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1_IProxyMarker)),
                        default,
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockProxyGenerator.Verify(
                t => t.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()),
                Times.Never);

            // test second object
            obj = resolver.ResolveAll<I1>().Skip(1).First();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1_IProxyMarker);
            _mockProxyGenerator.Verify(
                t => t.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()),
                Times.Once);
        }


        [TestMethod]
        public void Resolve_1_WithClassService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<C1_I1, C1_I1>();

            _mockProxyGenerator
                .Setup(g => g.CreateClassProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();

            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(C1_I1),
                        default,
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve<C1_I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockProxyGenerator.Verify();
        }
        #endregion

        #region Resolve(Type);

        [TestMethod]
        public void Resolve_2_WithoutInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1)))
                });
            container.Collection.Append<I1, C1_I1>();

            // test
            var obj = resolver.Resolve(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
        }


        [TestMethod]
        public void Resolve_2_WithInterfaceService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<I1, C1_I1>();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();

            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1)),
                        default,
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockProxyGenerator.Verify();
        }


        [TestMethod]
        public void Resolve_2_WithMultipleInterfaceService_AndWithOneInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<I1, C1_I1>();
            container.Collection.Append<I1, C1_I1_IProxyMarker>();

            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();

            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1)),
                        default,
                        default),
                    new Registration(
                        typeof(I1),
                        IBoundImplementation.Of(typeof(C1_I1_IProxyMarker)),
                        default,
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockProxyGenerator.Verify(
                t => t.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()),
                Times.Never);

            // test second object
            obj = resolver.ResolveAll<I1>().Skip(1).First();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1_IProxyMarker);
            _mockProxyGenerator.Verify(
                t => t.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()),
                Times.Once);
        }


        [TestMethod]
        public void Resolve_2_WithClassService_AndWithInterceptor_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<C1_I1, C1_I1>();

            _mockProxyGenerator
                .Setup(g => g.CreateClassProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();

            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                new[] {
                    new Registration(
                        typeof(C1_I1),
                        default,
                        new InterceptorProfile(new DummyInterceptor())) });

            // test
            var obj = resolver.Resolve(typeof(C1_I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C1_I1);
            _mockProxyGenerator.Verify();
        }

        #endregion

        #region ResolveAll<Service>();
        #endregion

        #region ResolveAll(Type);
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
