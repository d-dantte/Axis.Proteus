using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleInjector;
using System;
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

		#region Register(Type, RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_1_WithValidServiceType_ShouldAddRegistration()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(
				new Container(),
				_mockProxyGenerator.Object);
			var registrations = registrar
				.Register(
					typeof(C_I1),
					RegistryScope.Transient)
				.RegistrationsFor(typeof(C_I1));

			Assert.IsNotNull(registrations);
			var info = registrations.First();
			Assert.AreEqual(typeof(C_I1), info.ServiceType);
			var implT = info.Implementation as IBoundImplementation.ImplType;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);

			// test
			registrar = new SimpleInjectorRegistrar(
				new Container(),
				_mockProxyGenerator.Object);
			var profile = new InterceptorProfile(new DummyInterceptor());
			registrations = registrar
				.Register(
					typeof(C_I1),
					RegistryScope.Transient,
					profile)
				.RegistrationsFor(typeof(C_I1));

			Assert.IsNotNull(registrations);
			info = registrations.First();
			Assert.AreEqual(typeof(C_I1), info.ServiceType);
			implT = info.Implementation as IBoundImplementation.ImplType;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.AreEqual(profile, info.Profile);
		}

		[TestMethod]
		public void Register_1_WithNullServiceType_ShouldThrowException()
        {
			// nothing to setup

			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			Assert.ThrowsException<ArgumentNullException>(
				() => registrar.Register(
					null,
					RegistryScope.Transient));
		}

		[TestMethod]
		public void Register_1_WithDuplicateRegistration_ShouldAddRegistration()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register(
					typeof(C_I1),
					RegistryScope.Transient)
				.Register(
					typeof(C_I1),
					RegistryScope.Transient)
				.RegistrationsFor(typeof(C_I1))
				.ToArray();

			Assert.AreEqual(2, registrations.Length);
			Assert.AreEqual(registrations[0], registrations[1]);
		}

		#endregion

		#region Register(Type, Type, RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_2_WithValidServiceType_ShouldAddRegistration()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register(
					typeof(I1),
					typeof(C_I1),
					RegistryScope.Transient)
				.RegistrationsFor(typeof(I1));

			Assert.IsNotNull(registrations);
			var info = registrations.First();
			Assert.AreEqual(typeof(I1), info.ServiceType);
			var implT = info.Implementation as IBoundImplementation.ImplType;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);
		}

		[TestMethod]
		public void Register_2_WithNullTypes_ShouldThrowException()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			Assert.ThrowsException<ArgumentNullException>(
				() => registrar.Register(
					null,
					typeof(C_I1),
					RegistryScope.Transient));

			Assert.ThrowsException<ArgumentNullException>(
				() => registrar.Register(
					typeof(I1),
					(Type)null,
					RegistryScope.Transient));
		}

		[TestMethod]
		public void Register_2_WithDuplicateRegistration_ShouldThrowException()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register(
					typeof(I1),
					typeof(C_I1),
					RegistryScope.Transient)
				.Register(
					typeof(I1),
					typeof(C_I1),
					RegistryScope.Transient)
				.RegistrationsFor(typeof(I1))
				.ToArray();

			Assert.AreEqual(2, registrations.Length);
			Assert.AreEqual(registrations[0], registrations[1]);
		}
		#endregion

		#region Register(Type, Func<IResolverContract, object>, RegistryScope?, InterceptorProfile?);

		[TestMethod]
		public void Register_3_WithValidArgs_ShouldRegister()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register(
					typeof(I1),
					new Func<IResolverContract, I1>(_r => new C_I1()),
					RegistryScope.Transient)
				.Register(
					typeof(I1),
					new Func<IResolverContract, I1>(_r => new C_I1_I2()),
					RegistryScope.Singleton,
					new InterceptorProfile(new DummyInterceptor()))
				.RegistrationsFor(typeof(I1))
				.ToArray();

			Assert.AreEqual(2, registrations.Length);
			Assert.AreEqual(RegistryScope.Singleton, registrations[1].Scope);
		}
		#endregion

		#region Register<Impl>(RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_4_WithValidServiceType_ShouldRegister()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register<C_I1>(RegistryScope.Transient)
				.RegistrationsFor(typeof(C_I1));

			Assert.IsNotNull(registrations);
			var info = registrations.First();
			Assert.AreEqual(typeof(C_I1), info.ServiceType);
			var implT = info.Implementation as IBoundImplementation.ImplType;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);

			// test
			registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var profile = new InterceptorProfile(new DummyInterceptor());
			registrations = registrar
				.Register<C_I1>(RegistryScope.Transient, profile)
				.RegistrationsFor(typeof(C_I1));

			Assert.IsNotNull(registrations);
			info = registrations.First();
			Assert.AreEqual(typeof(C_I1), info.ServiceType);
			implT = info.Implementation as IBoundImplementation.ImplType;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.AreEqual(profile, info.Profile);
		}

		[TestMethod]
		public void Register_4_WithDuplicateRegistration_ShouldAddRegistration()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register<C_I1>(RegistryScope.Transient)
				.Register<C_I1>(RegistryScope.Transient)
				.RegistrationsFor(typeof(C_I1))
				.ToArray();

			Assert.AreEqual(2, registrations.Length);
			Assert.AreEqual(registrations[0], registrations[1]);
		}
		#endregion

		#region Register<TService, TImpl>(RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_5_WithValidServiceType_ShouldRegister()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register<I1, C_I1>(RegistryScope.Transient, default)
				.RegistrationsFor(typeof(I1));

			Assert.IsNotNull(registrations);
			var info = registrations.First();
			Assert.AreEqual(typeof(I1), info.ServiceType);
			var implT = info.Implementation as IBoundImplementation.ImplType;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);

			// test
			registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var profile = new InterceptorProfile(new DummyInterceptor());
			registrations = registrar
				.Register<I1, C_I1>(RegistryScope.Transient, profile)
				.RegistrationsFor(typeof(I1));

			Assert.IsNotNull(registrations);
			info = registrations.First();
			Assert.AreEqual(typeof(I1), info.ServiceType);
			implT = info.Implementation as IBoundImplementation.ImplType;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.AreEqual(profile, info.Profile);
		}
		#endregion

		#region Register<TService>(Func<IResolverContract, TService>, RegistryScope?, InterceptorProfile?);

		[TestMethod]
		public void Register_6_WithValidArgs_ShouldRegister()
		{
			// test
			var registrar = new SimpleInjectorRegistrar(new Container(),_mockProxyGenerator.Object);
			var registrations = registrar
				.Register(
					new Func<IResolverContract, I1>(_r => new C_I1()),
					RegistryScope.Transient)
				.Register(
					new Func<IResolverContract, I1>(_r => new C_I1_I2()),
					RegistryScope.Singleton,
					new InterceptorProfile(new DummyInterceptor()))
				.RegistrationsFor(typeof(I1))
				.ToArray();

			Assert.AreEqual(2, registrations.Length);
			Assert.AreEqual(RegistryScope.Singleton, registrations[1].Scope);
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

    #region Test Types
    public interface I1 { }

	public interface I2 { }

	public interface I3 { }


	public interface I1A : I1 { }

	public interface I2A : I2 { }

	public interface I1I2A: I1, I2{}

	public interface I1I2I3A : I1, I2, I3 { }


	public class C_I1 : I1 { }

	public class C_I2 : I2 { }

	public class C_I3 : I3 { }

	public class C_I1_I2 : I1, I2 { }

	public class C_I1I2A : I1I2A { }

	public class C_I1I2I3A : I1I2I3A { }

	public class C_I1_I1A : I1, I1A { }
	#endregion
}
