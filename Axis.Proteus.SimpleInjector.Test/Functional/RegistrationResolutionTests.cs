using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System;
using System.Collections.Generic;

namespace Axis.Proteus.SimpleInjector.Test.Functional
{
    [TestClass]
    public class RegistrationResolutionTests
    {
        [TestMethod]
        public void Register_SingleService_ResolvesAppropriately()
        {
            #region non-generics
            //register self-bound service
            var container = new Container();
            IRegistrarContract publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register(typeof(C1_I1), ResolutionScope.Transient);
            IResolverContract resolver = publicRegistrar.BuildResolver();

            var service = resolver.Resolve(typeof(C1_I1));
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());

            service = resolver.Resolve<C1_I1>();
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());
            resolver.Dispose();


            //register interface-bound service
            container = new Container();
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register(typeof(I1), typeof(C1_I1), ResolutionScope.Transient);
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());
            resolver.Dispose();


            //register interface-bound factory-generated service
            container = new Container();
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register(
                serviceType: typeof(I1),
                scope: ResolutionScope.Transient,
                profile: default,
                factory: new Func<IResolverContract, I1>(_resolver => new C1_I1()));
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());
            resolver.Dispose();
            #endregion

            #region generics
            //register self-bound service
            container = new Container();
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register<C1_I1>(ResolutionScope.Transient);
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(C1_I1));
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());

            service = resolver.Resolve<C1_I1>();
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());
            resolver.Dispose();


            //register interface-bound service
            container = new Container();
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register<I1, C1_I1>(ResolutionScope.Transient);
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());
            resolver.Dispose();


            //register interface-bound factory-generated service
            container = new Container();
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register<I1>(
                scope: ResolutionScope.Transient,
                profile: default,
                factory: _resolver => new C1_I1());
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.AreEqual(typeof(C1_I1), service.GetType());
            resolver.Dispose();
            #endregion
        }

        [TestMethod]
        public void Register_SingleService_WithInterception_ResolvesAppropriately()
        {
            Interceptor i = null;
            #region non-generics
            //register self-bound service
            var container = new Container();
            InterceptorProfile profile = new InterceptorProfile(i = new Interceptor());
            IRegistrarContract publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register(typeof(C1_I1), ResolutionScope.Transient, profile);
            IResolverContract resolver = publicRegistrar.BuildResolver();

            var service = resolver.Resolve(typeof(C1_I1));
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(C1_I1).IsAssignableFrom(service.GetType()));

            service = resolver.Resolve<C1_I1>();
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(C1_I1).IsAssignableFrom(service.GetType()));
            (service as C1_I1).NameMethod();
            Assert.AreEqual(1, i.Invocations.Count);
            resolver.Dispose();


            //register interface-bound service
            container = new Container();
            profile = new InterceptorProfile(i = new Interceptor());
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register(typeof(I1), typeof(C1_I1), ResolutionScope.Transient, profile);
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));
            (service as I1).NameMethod();
            Assert.AreEqual(1, i.Invocations.Count);
            resolver.Dispose();


            //register interface-bound factory-generated service
            container = new Container();
            profile = new InterceptorProfile(i = new Interceptor());
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register(
                serviceType: typeof(I1),
                scope: ResolutionScope.Transient,
                profile: profile,
                factory: new Func<IResolverContract, I1>(_resolver => new C1_I1()));
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));
            (service as I1).NameMethod();
            Assert.AreEqual(1, i.Invocations.Count);
            resolver.Dispose();
            #endregion

            #region generics
            //register self-bound service
            container = new Container();
            profile = new InterceptorProfile(i = new Interceptor());
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register<C1_I1>(ResolutionScope.Transient, profile);
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(C1_I1));
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(C1_I1).IsAssignableFrom(service.GetType()));

            service = resolver.Resolve<C1_I1>();
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(C1_I1).IsAssignableFrom(service.GetType()));
            (service as C1_I1).NameMethod();
            Assert.AreEqual(1, i.Invocations.Count);
            resolver.Dispose();


            //register interface-bound service
            container = new Container();
            profile = new InterceptorProfile(i = new Interceptor());
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register<I1, C1_I1>(ResolutionScope.Transient, profile);
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));
            (service as I1).NameMethod();
            Assert.AreEqual(1, i.Invocations.Count);
            resolver.Dispose();


            //register interface-bound factory-generated service
            container = new Container();
            profile = new InterceptorProfile(i = new Interceptor());
            publicRegistrar = new SimpleInjectorRegistrar(container);
            _ = publicRegistrar.Register<I1>(
                scope: ResolutionScope.Transient,
                profile: profile,
                factory: _resolver => new C1_I1());
            resolver = publicRegistrar.BuildResolver();

            service = resolver.Resolve(typeof(I1));
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));

            service = resolver.Resolve<I1>();
            Assert.IsNotNull(service);
            Assert.IsTrue(typeof(I1).IsAssignableFrom(service.GetType()));
            (service as I1).NameMethod();
            Assert.AreEqual(1, i.Invocations.Count);
            resolver.Dispose();
            #endregion
        }

         
        [TestMethod]
        public void bleh()
        {
        }

        #region Nested types

        public interface I1
        {
            string NameProperty { get; }

            string NameMethod();
        }

        public class C1_I1 : I1
        {
            public virtual string NameProperty { get; set; }

            public virtual string NameMethod() => NameProperty;
        }

        public class C2_I1 : I1
        {
            public string NameProperty { get; set; }

            public string NameMethod() => NameProperty;
        }

        public class Interceptor : IInterceptor
        {
            public List<IInvocation> Invocations { get; } = new List<IInvocation>();

            public void Intercept(IInvocation invocation)
            {
                Invocations.Add(invocation);
                invocation.Proceed();
            }
        }
        #endregion
    }

}
