using Axis.Luna.Extensions;
using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Axis.Proteus.SimpleInjector.NamedContext;
using Axis.Proteus.SimpleInjector.Test.Types;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Proteus.SimpleInjector.Test.Unit
{
    [TestClass]
	public class SimpleInjectorServiceRegistrarTest
	{
		private Mock<IProxyGenerator> _mockProxyGenerator = new Mock<IProxyGenerator>();

		public SimpleInjectorServiceRegistrarTest()
		{
			//setup mock
		}

        #region Constructor
        [TestMethod]
		public void Constructor_WithValidArgs_ShouldCreateInstance()
        {
			var registrar = new SimpleInjectorRegistrar(
				new Container(),
				_mockProxyGenerator.Object);

			Assert.IsNotNull(registrar);
        }

        [TestMethod]
		public void Constructor_WithInvalidArgs_ShouldThrowException()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new SimpleInjectorRegistrar(new Container(), null));
			Assert.ThrowsException<ArgumentNullException>(() => new SimpleInjectorRegistrar(null, _mockProxyGenerator.Object));
		}
		#endregion

		#region BuildResolver()
		[TestMethod]
		public void BuildResolver_WithValidManifest_ShouldBuildResolver()
        {
			// setup
			var container = new Container();
			var registrar = new SimpleInjectorRegistrar(
				container,
				_mockProxyGenerator.Object);
			var otherClassType = typeof(OtherClass);

			// test
			var resolver = registrar
				.Register<I1, C_I1>()
				.Register<I2, C_I2>(
					RegistryScope.Singleton,
					new[] { new DummyInterceptor() },
					IBindContext.Of( // named context
						"myContext",
						IBindTarget.Of(typeof(C_I2))),
					IBindContext.Of( // named context
						"myOtherContext",
						IBindTarget.Of(new Func<IResolverContract, C_I2>(r => new C_I2()))),
					IBindContext.OfParameter( // param context
						IBindTarget.Of(typeof(C_I1_I2)),
						param => param.Name.Equals("a")),
					IBindContext.OfParameter( // param context
						IBindTarget.Of(new Func<IResolverContract, C_I1_I2>(r => new C_I1_I2())),
						param => param.Name.Equals("b")),
					IBindContext.OfProperty( // property context
						IBindTarget.Of(typeof(C_I1I2A)),
						prop => prop.Name.Equals("a")),
					IBindContext.OfProperty( // property context
						IBindTarget.Of(new Func<IResolverContract, C_I1I2A>(r => new C_I1I2A())),
						prop => prop.Name.Equals("b")))
				.RegisterAll<I1>(
					default,
					default,
					IBindTarget.Of(typeof(C_I1)))
				.BuildResolver();

			Assert.IsNotNull(resolver);

            #region Root
            // assert root manifest
            Assert.AreEqual(2, registrar.RootManifest().Count);
			Assert.IsTrue(registrar.RootManifest().Keys.Contains(typeof(I1)));
			Assert.IsTrue(registrar.RootManifest().Keys.Contains(typeof(I2)));

			/// assert container has correct registrations
			// 1. I1
			var containerRegistration = container.GetRegistration<I1>();
			Assert.IsNotNull(containerRegistration);
			Assert.AreEqual(typeof(I1), containerRegistration.ServiceType);
			Assert.AreEqual(typeof(C_I1), containerRegistration.ImplementationType);
			Assert.AreEqual(Lifestyle.Transient, containerRegistration.Lifestyle);

			// 2. I2
			var replacementName = DynamicTypeUtil.ToTypeName(
				"myContext",
				typeof(I2),
				typeof(C_I2),
				DynamicTypeTag.Replacement,
				out _);
			var containerName = DynamicTypeUtil.ToTypeName(
				"myOtherContext",
				typeof(I2),
				typeof(C_I2),
				DynamicTypeTag.Container,
				out _);

			var rootRegistrations = container.GetRootRegistrations();
			Assert.AreEqual(7, rootRegistrations.Count(r =>
			{
				return r.Lifestyle == Lifestyle.Singleton
					&& (r.ServiceType == typeof(I2)
					|| r.ServiceType.Name.Equals(replacementName)
					|| r.ServiceType.Name.Equals(containerName));
			}));
            #endregion

            #region Collection
            // assert collection manifest
            Assert.AreEqual(1, registrar.CollectionManifest().Count);
			Assert.IsTrue(registrar.CollectionManifest().Keys.Contains(typeof(I1)));

			/// assert container has correct registrations
			// 1. I1
			containerRegistration = container.GetRegistration<IEnumerable<I1>>();
			Assert.IsNotNull(containerRegistration);
			Assert.AreEqual(typeof(IEnumerable<I1>), containerRegistration.ServiceType);

			// collection registration uses Singleton for the collection itself, and 
			// regular lifestyles for individual types
			Assert.AreEqual(Lifestyle.Singleton, containerRegistration.Lifestyle);

			#endregion
		}
        #endregion

        #region IsRegistrationClosed()
        [TestMethod]
		public void IsRegistrationClosed_ShouldIndicateRegistrationClosedStatus()
        {
			var container = new Container();
			var registrar = new SimpleInjectorRegistrar(container);
			Assert.IsFalse(registrar.IsRegistrationClosed());

			container.Register<I1, C_I1>();
			Assert.IsFalse(registrar.IsRegistrationClosed());

			_ = container.GetInstance<I1>();
			Assert.IsTrue(registrar.IsRegistrationClosed());
        }
		#endregion
	}

    public class DummyInterceptor : IInterceptor
    {
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
			Console.WriteLine("Intercepted: " + invocation);
        }
    }

}
