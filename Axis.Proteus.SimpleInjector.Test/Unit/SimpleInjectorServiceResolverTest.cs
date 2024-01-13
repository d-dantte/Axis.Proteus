using Axis.Luna.Extensions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Axis.Proteus.SimpleInjector.NamedContext;
using Axis.Proteus.SimpleInjector.Test.Types;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleInjector;
using System;
using System.Linq;

namespace Axis.Proteus.SimpleInjector.Test.Unit
{
    using Registration = RegistrationInfo;

    [TestClass]
    public class SimpleInjectorServiceResolverTest
    {
        private Mock<IProxyGenerator> _mockProxyGenerator = new Mock<IProxyGenerator>();
        private Mock<RegistryManifest> _mockManifest = new Mock<RegistryManifest>();

        #region Constructor

        [TestMethod]
        public void Constructor_WithValidArgs_ShouldReturnValidObject()
        {
            // test
            var resolver = new SimpleInjectorResolver(
                new Container(),
                _mockProxyGenerator.Object,
                new RegistryManifest());

            Assert.AreEqual(0, resolver.RootManifest().Count);
            Assert.AreEqual(0, resolver.CollectionManifest().Count);
        }

        [TestMethod]
        public void Constructor_WithNullArgs_ShouldThrowNullArgumentException()
        {
            // setup

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new SimpleInjectorResolver(
                null,
                _mockProxyGenerator.Object,
                new RegistryManifest()));

            //test
            Assert.ThrowsException<ArgumentNullException>(() => new SimpleInjectorResolver(
                new Container(),
                null,
                new RegistryManifest()));

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
                new RegistryManifest());
            var isContainerDisposed = false;
            container.ContainerScope.WhenScopeEnds(() => isContainerDisposed = true);

            // test
            resolver.Dispose();

            // assert
            Assert.IsTrue(isContainerDisposed);
        }
        #endregion

        #region Resolve<Service>(ResolutionContextName);

        [TestMethod]
        public void Resolve_1_WithValidRegistration_ShouldResolve()
        {
            // setup
            var container = new Container()
                .Use(c => c.Register<I1, C_I1>());
            var manifest = new RegistryManifest()
                .AddRootRegistration(new Registration(
                    typeof(I1),
                    IBindTarget.Of(typeof(C_I1))));
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                manifest);

            // test
            var obj = resolver.Resolve<I1>();

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1);
        }

        [TestMethod]
        public void Resolve_1_WithValidRegistration_AndNamedContext_ShouldResolve()
        {
            #region TypeTarget context
            // setup 
            var container = new Container();
            var registrar = new SimpleInjectorRegistrar(container);
            registrar.Register<I1, C_I1>(
                default,
                default,
                IBindContext.Of(
                    "contextName",
                    IBindTarget.Of(typeof(C_I1_I2))));
            var resolver = registrar.BuildResolver();

            // test
            var obj = resolver.Resolve<I1>();
            var contextualObj = resolver.Resolve<I1>("contextName");

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1);
            Assert.IsNotNull(contextualObj);
            Assert.IsTrue(contextualObj is C_I1_I2);
            resolver.Dispose();
            #endregion

            #region FactoryTarget context
            // setup 
            container = new Container();
            registrar = new SimpleInjectorRegistrar(container);
            registrar.Register<TheClass>();
            registrar.Register<I1, C_I1_2>(
                default,
                default,
                IBindContext.Of(
                    "contextName",
                    IBindTarget.Of(
                        new Func<IResolverContract, C_I1_2>(r => new C_I1_2(r.Resolve<TheClass>())))));
            resolver = registrar.BuildResolver();

            // test
            obj = resolver.Resolve<I1>();
            contextualObj = resolver.Resolve<I1>("contextName");

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1_2);
            Assert.IsNotNull(contextualObj);
            Assert.IsTrue(contextualObj is C_I1_2);
            var actual = contextualObj as C_I1_2;
            Assert.IsNotNull(actual.Property);
            resolver.Dispose();
            #endregion

            #region multiple contexts
            // setup 
            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();
            container = new Container();
            registrar = new SimpleInjectorRegistrar(
                container,
                _mockProxyGenerator.Object);
            registrar.Register<TheClass>();
            registrar.Register<I1, C_I1>(
                default,
                new InterceptorProfile(new DummyInterceptor()),
                IBindContext.Of(
                    "contextName1",
                    IBindTarget.Of(typeof(C_I1_I2))),
                IBindContext.Of(
                    "contextName2",
                    IBindTarget.Of(
                        new Func<IResolverContract, C_I1_2>(r => new C_I1_2(r.Resolve<TheClass>())))));
            resolver = registrar.BuildResolver();

            // test
            obj = resolver.Resolve<I1>();
            var ignoredContextObj = resolver.Resolve<I1>("ignoredContext");
            contextualObj = resolver.Resolve<I1>("contextName1");
            var contextualObj2 = resolver.Resolve<I1>("contextName2");

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1);

            Assert.IsNotNull(ignoredContextObj);
            Assert.IsTrue(ignoredContextObj is C_I1); // ignored named-context requests should resolve using the default-context

            Assert.IsNotNull(contextualObj);
            Assert.IsTrue(contextualObj is C_I1_I2);
            Assert.IsTrue(contextualObj is INamedContextReplacement);

            Assert.IsNotNull(contextualObj2);
            Assert.IsTrue(contextualObj2 is C_I1_2);
            Assert.IsNotNull((contextualObj2 as C_I1_2).Property);

            _mockProxyGenerator.Verify();
            resolver.Dispose();
            #endregion
        }

        [TestMethod]
        public void Resolve_1_WithInvalidArguments()
        {
            // setup
            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();
            var container = new Container();
            var registrar = new SimpleInjectorRegistrar(
                container,
                _mockProxyGenerator.Object);
            registrar.Register<TheClass>();
            registrar.Register<I1, C_I1>(
                default,
                new InterceptorProfile(new DummyInterceptor()),
                IBindContext.Of(
                    "contextName1",
                    IBindTarget.Of(typeof(C_I1_I2))),
                IBindContext.Of(
                    "contextName2",
                    IBindTarget.Of(
                        new Func<IResolverContract, C_I1_2>(r => new C_I1_2(r.Resolve<TheClass>())))));
            var resolver = registrar.BuildResolver();

            // unregistered type
            var instance = resolver.Resolve<I2>();
            Assert.IsNull(instance);
        }
        #endregion

        #region Resolve(Type, ResolutionContextName);

        [TestMethod]
        public void Resolve_2_WithValidRegistration_ShouldResolve()
        {
            // setup
            var container = new Container()
                .Use(c => c.Register<I1, C_I1>());
            var manifest = new RegistryManifest()
                .AddRootRegistration(new Registration(
                    typeof(I1),
                    IBindTarget.Of(typeof(C_I1))));
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                manifest);

            // test
            var obj = resolver.Resolve(typeof(I1));

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1);
        }

        [TestMethod]
        public void Resolve_2_WithValidRegistration_AndNamedContext_ShouldResolve()
        {
            #region TypeTarget context
            // setup 
            var container = new Container();
            var registrar = new SimpleInjectorRegistrar(container);
            registrar.Register<I1, C_I1>(
                default,
                default,
                IBindContext.Of(
                    "contextName",
                    IBindTarget.Of(typeof(C_I1_I2))));
            var resolver = registrar.BuildResolver();

            // test
            var obj = resolver.Resolve(typeof(I1));
            var contextualObj = resolver.Resolve<I1>("contextName");

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1);
            Assert.IsNotNull(contextualObj);
            Assert.IsTrue(contextualObj is C_I1_I2);
            resolver.Dispose();
            #endregion

            #region FactoryTarget context
            // setup 
            container = new Container();
            registrar = new SimpleInjectorRegistrar(container);
            registrar.Register<TheClass>();
            registrar.Register<I1, C_I1_2>(
                default,
                default,
                IBindContext.Of(
                    "contextName",
                    IBindTarget.Of(
                        new Func<IResolverContract, C_I1_2>(r => new C_I1_2(r.Resolve<TheClass>())))));
            resolver = registrar.BuildResolver();

            // test
            obj = resolver.Resolve(typeof(I1));
            contextualObj = resolver.Resolve<I1>("contextName");

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1_2);
            Assert.IsNotNull(contextualObj);
            Assert.IsTrue(contextualObj is C_I1_2);
            var actual = contextualObj as C_I1_2;
            Assert.IsNotNull(actual.Property);
            resolver.Dispose();
            #endregion

            #region multiple contexts
            // setup 
            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();
            container = new Container();
            registrar = new SimpleInjectorRegistrar(
                container,
                _mockProxyGenerator.Object);
            registrar.Register<TheClass>();
            registrar.Register<I1, C_I1>(
                default,
                new InterceptorProfile(new DummyInterceptor()),
                IBindContext.Of(
                    "contextName1",
                    IBindTarget.Of(typeof(C_I1_I2))),
                IBindContext.Of(
                    "contextName2",
                    IBindTarget.Of(
                        new Func<IResolverContract, C_I1_2>(r => new C_I1_2(r.Resolve<TheClass>())))));
            resolver = registrar.BuildResolver();

            // test
            obj = resolver.Resolve(typeof(I1));
            var ignoredContextObj = resolver.Resolve<I1>("ignoredContext");
            contextualObj = resolver.Resolve<I1>("contextName1");
            var contextualObj2 = resolver.Resolve<I1>("contextName2");

            // assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is C_I1);

            Assert.IsNotNull(ignoredContextObj);
            Assert.IsTrue(ignoredContextObj is C_I1); // ignored named-context requests should resolve using the default-context

            Assert.IsNotNull(contextualObj);
            Assert.IsTrue(contextualObj is C_I1_I2);
            Assert.IsTrue(contextualObj is INamedContextReplacement);

            Assert.IsNotNull(contextualObj2);
            Assert.IsTrue(contextualObj2 is C_I1_2);
            Assert.IsNotNull((contextualObj2 as C_I1_2).Property);

            _mockProxyGenerator.Verify();
            resolver.Dispose();
            #endregion
        }

        [TestMethod]
        public void Resolve_2_WithInvalidArguments()
        {
            // setup
            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();
            var container = new Container();
            var registrar = new SimpleInjectorRegistrar(
                container,
                _mockProxyGenerator.Object);
            registrar.Register<TheClass>();
            registrar.Register<I1, C_I1>(
                default,
                new InterceptorProfile(new DummyInterceptor()),
                IBindContext.Of(
                    "contextName1",
                    IBindTarget.Of(typeof(C_I1_I2))),
                IBindContext.Of(
                    "contextName2",
                    IBindTarget.Of(
                        new Func<IResolverContract, C_I1_2>(r => new C_I1_2(r.Resolve<TheClass>())))));
            var resolver = registrar.BuildResolver();

            // unregistered type
            var instance = resolver.Resolve(typeof(I2));
            Assert.IsNull(instance);
        }

        #endregion

        #region ResolveAll<Service>();
        [TestMethod]
        public void ResolveAll_1_WithValidArgs_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<I1, C_I1>();
            var manifest = new RegistryManifest()
                .AddCollectionRegistrations(new Registration(
                    typeof(I1),
                    IBindTarget.Of(typeof(C_I1))));
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                manifest);

            // test
            var objs = resolver.ResolveAll<I1>();

            // assert
            Assert.IsNotNull(objs);
            var instances = objs.ToArray();
            Assert.IsTrue(instances[0] is C_I1);

            // setup with interceptor
            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();
            container = new Container();
            container.Collection.Append<I1, C_I1>();
            manifest = new RegistryManifest()
                .AddCollectionRegistrations(new Registration(
                    typeof(I1),
                    IBindTarget.Of(typeof(C_I1)),
                    default,
                    new InterceptorProfile(new DummyInterceptor())));
            resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                manifest);

            // test
            objs = resolver.ResolveAll<I1>();

            // assert
            Assert.IsNotNull(objs);
            instances = objs.ToArray();
            Assert.IsTrue(instances[0] is C_I1);
            _mockProxyGenerator.Verify();
        }

        [TestMethod]
        public void ResolveAll_1_WithInvalidArgs_ShouldResolve()
        {
            // 1. test default registration
            _mockManifest
                .Setup(m => m.CollectionRegistrationsFor(It.IsAny<Type>()))
                .Returns(new[] { default(Registration) });
            var container = new Container();
            container.Collection.Append<I1, C_I1>();
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                _mockManifest.Object);

            Assert.ThrowsException<InvalidOperationException>(() => resolver.ResolveAll<I1>().First());

        }
        #endregion

        #region ResolveAll(Type);
        [TestMethod]
        public void ResolveAll_2_WithValidArgs_ShouldResolve()
        {
            // setup
            var container = new Container();
            container.Collection.Append<I1, C_I1>();
            var manifest = new RegistryManifest()
                .AddCollectionRegistrations(new Registration(
                    typeof(I1),
                    IBindTarget.Of(typeof(C_I1))));
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                manifest);

            // test
            var objs = resolver.ResolveAll(typeof(I1));

            // assert
            Assert.IsNotNull(objs);
            var instances = objs.ToArray();
            Assert.IsTrue(instances[0] is C_I1);

            // setup with interceptor
            _mockProxyGenerator
                .Setup(g => g.CreateInterfaceProxyWithTarget(
                    It.IsAny<Type>(),
                    It.IsAny<object>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns<Type, object, IInterceptor[]>((arg1, arg2, arg3) => arg2)
                .Verifiable();
            container = new Container();
            container.Collection.Append<I1, C_I1>();
            manifest = new RegistryManifest()
                .AddCollectionRegistrations(new Registration(
                    typeof(I1),
                    IBindTarget.Of(typeof(C_I1)),
                    default,
                    new InterceptorProfile(new DummyInterceptor())));
            resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                manifest);

            // test
            objs = resolver.ResolveAll(typeof(I1));

            // assert
            Assert.IsNotNull(objs);
            instances = objs.ToArray();
            Assert.IsTrue(instances[0] is C_I1);
            _mockProxyGenerator.Verify();
        }

        [TestMethod]
        public void ResolveAll_2_WithInvalidArgs_ShouldResolve()
        {
            // 1. test default registration
            _mockManifest
                .Setup(m => m.CollectionRegistrationsFor(It.IsAny<Type>()))
                .Returns(new[] { default(Registration) });
            var container = new Container();
            container.Collection.Append<I1, C_I1>();
            var resolver = new SimpleInjectorResolver(
                container,
                _mockProxyGenerator.Object,
                _mockManifest.Object);

            Assert.ThrowsException<InvalidOperationException>(() => resolver.ResolveAll(typeof(I1)).First());

        }
        #endregion
    }
}
