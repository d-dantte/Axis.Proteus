using Axis.Luna.Extensions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Axis.Proteus.Test.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
    public class AbstractRegistrarTests
	{

		#region Register(Type, RegistryScope?, InterceptorProfile?, IBindContext[]);
		[TestMethod]
		public void Register_1_WithValidServiceType_ShouldAddRegistration()
		{
			// test
			var registrar = new DummyRegistrar();
			var registration = registrar
				.Register(
					typeof(C_I1_1),
					RegistryScope.Transient)
				.RootRegistrationFor(typeof(C_I1_1));

			Assert.IsNotNull(registration);
			var info = registration.Value;
			Assert.AreEqual(typeof(C_I1_1), info.ServiceType);
			var implT = info.DefaultContext.Target as IBindTarget.TypeTarget;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1_1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);

			// test
			registrar = new DummyRegistrar();
			var profile = new InterceptorProfile(new NoOpInterceptor());
			registration = registrar
				.Register(
					typeof(C_I1_1),
					RegistryScope.Transient,
					profile)
				.RootRegistrationFor(typeof(C_I1_1));

			Assert.IsNotNull(registration);
			info = registration.Value;
			Assert.AreEqual(typeof(C_I1_1), info.ServiceType);
			implT = info.DefaultContext.Target as IBindTarget.TypeTarget;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1_1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.AreEqual(profile, info.Profile);
		}
		#endregion

		#region Register(Type, Type, RegistryScope?, InterceptorProfile?, IBindContext[]);
		[TestMethod]
		public void Register_2_WithValidServiceType_ShouldAddRegistration()
		{
			// test
			var registrar = new DummyRegistrar(); ;
			var registration = registrar
				.Register(
					typeof(I1),
					typeof(C_I1_1),
					RegistryScope.Transient)
				.RootRegistrationFor(typeof(I1));

			Assert.IsNotNull(registration);
			var info = registration.Value;
			Assert.AreEqual(typeof(I1), info.ServiceType);
			var implT = info.DefaultContext.Target as IBindTarget.TypeTarget;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1_1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);
		}
		#endregion

		#region Register(Type, Func<IResolverContract, object>, RegistryScope?, InterceptorProfile?, IBindContext[]);

		[TestMethod]
		public void Register_3_WithValidArgs_ShouldRegister()
		{
			// test
			var registrar = new DummyRegistrar();
			var profile = new InterceptorProfile(new NoOpInterceptor());
			var registration = registrar
				.Register(
					typeof(I1),
					new Func<IResolverContract, I1>(_r => new C_I1_I2_1()),
					RegistryScope.Singleton,
					profile)
				.RootRegistrationFor(typeof(I1));

			Assert.IsNotNull(registration);
			Assert.AreEqual(profile, registration.Value.Profile);
			Assert.AreEqual(RegistryScope.Singleton, registration.Value.Scope);
		}
		#endregion

		#region Register<Impl>(RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_4_WithValidServiceType_ShouldRegister()
		{
			// test
			var registrar = new DummyRegistrar();
			var registration = registrar
				.Register<C_I1_1>(RegistryScope.Transient)
				.RootRegistrationFor(typeof(C_I1_1));

			Assert.IsNotNull(registration);
			var info = registration.Value;
			Assert.AreEqual(typeof(C_I1_1), info.ServiceType);
			var implT = info.DefaultContext.Target as IBindTarget.TypeTarget;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1_1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);

			// test
			registrar = new DummyRegistrar();
			var profile = new InterceptorProfile(new NoOpInterceptor());
			registration = registrar
				.Register<C_I1_1>(RegistryScope.Transient, profile)
				.RootRegistrationFor(typeof(C_I1_1));

			Assert.IsNotNull(registration);
			info = registration.Value;
			Assert.AreEqual(typeof(C_I1_1), info.ServiceType);
			implT = info.DefaultContext.Target as IBindTarget.TypeTarget;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1_1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.AreEqual(profile, info.Profile);
		}
		#endregion

		#region Register<TService, TImpl>(RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_5_WithValidServiceType_ShouldRegister()
		{
			// test
			var registrar = new DummyRegistrar();
			var registration = registrar
				.Register<I1, C_I1_1>(RegistryScope.Transient, default)
				.RootRegistrationFor(typeof(I1));

			Assert.IsNotNull(registration);
			var info = registration.Value;
			Assert.AreEqual(typeof(I1), info.ServiceType);
			var implT = info.DefaultContext.Target as IBindTarget.TypeTarget;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1_1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.IsTrue(info.Profile.Equals(default(InterceptorProfile)));
			Assert.AreEqual(default, info.Profile);

			// test
			registrar = new DummyRegistrar();
			var profile = new InterceptorProfile(new NoOpInterceptor());
			registration = registrar
				.Register<I1, C_I1_1>(RegistryScope.Transient, profile)
				.RootRegistrationFor(typeof(I1));

			Assert.IsNotNull(registration);
			info = registration.Value;
			Assert.AreEqual(typeof(I1), info.ServiceType);
			implT = info.DefaultContext.Target as IBindTarget.TypeTarget;
			Assert.IsNotNull(implT);
			Assert.AreEqual(typeof(C_I1_1), implT.Type);
			Assert.AreEqual(RegistryScope.Transient, info.Scope);
			Assert.AreEqual(profile, info.Profile);
		}
		#endregion

		#region Register<TService>(Func<IResolverContract, TService>, RegistryScope?, InterceptorProfile?);

		[TestMethod]
		public void Register_6_WithValidArgs_ShouldRegister()
		{
			// test
			var registrar = new DummyRegistrar();
			var profile = new InterceptorProfile(new NoOpInterceptor());
			var registration = registrar
				.Register(
					new Func<IResolverContract, I1>(_r => new C_I1_I2_1()),
					RegistryScope.Singleton,
					profile)
				.RootRegistrationFor(typeof(I1));

			Assert.IsNotNull(registration);
			Assert.AreEqual(RegistryScope.Singleton, registration.Value.Scope);
			Assert.AreEqual(profile, registration.Value.Profile);
		}
		#endregion

		#region RegisterAll<TService>(RegistryScope, InterceptorProfile, IBindTarget[])
		[TestMethod]
		public void RegisterAll_1_WithValidArguments_ShouldRegisterBindings()
		{
			// setup empty registration
			var registrar = new DummyRegistrar();

			// test empty registration
			registrar.RegisterAll<I1>(
				RegistryScope.Transient,
				default);

			// assert
			Assert.AreEqual(0, registrar.CollectionServices().Length);

			// setup single registration
			registrar = new DummyRegistrar();

			// test single registration
			registrar.RegisterAll<I1>(
				RegistryScope.Transient,
				default,
				IBindTarget.Of(typeof(C_I1_1)));

			// assert
			Assert.AreEqual(1, registrar.CollectionServices().Length);
			var registrations = registrar.CollectionRegistrationsFor(typeof(I1));
			Assert.AreEqual(1, registrations.Length);
			AssertRegistration(registrations[0], typeof(I1), RegistryScope.Transient, default(InterceptorProfile));
			Assert.AreEqual(typeof(C_I1_1), registrations[0].DefaultContext.Target.Type);

			// setup multiple registrations
			registrar = new DummyRegistrar();
			registrar
				.RegisterAll<I1>(
					RegistryScope.Transient,
					default,
					IBindTarget.Of(typeof(C_I1_1)),
					IBindTarget.Of(typeof(C_I1_2)),
					IBindTarget.Of(typeof(C_I1_I2_1)))
				.RegisterAll<I2>(
					RegistryScope.Singleton,
					default,
					IBindTarget.Of(typeof(C_I2_1)));

			// assert
			Assert.AreEqual(2, registrar.CollectionServices().Length);

			registrations = registrar.CollectionRegistrationsFor(typeof(I1));
			Assert.AreEqual(3, registrations.Length);
			registrations.ForAll(r => AssertRegistration(r, typeof(I1), RegistryScope.Transient, default(InterceptorProfile)));
			Assert.AreEqual(typeof(C_I1_1), registrations[0].DefaultContext.Target.Type);
			Assert.AreEqual(typeof(C_I1_2), registrations[1].DefaultContext.Target.Type);
			Assert.AreEqual(typeof(C_I1_I2_1), registrations[2].DefaultContext.Target.Type);

			registrations = registrar.CollectionRegistrationsFor(typeof(I2));
			Assert.AreEqual(1, registrations.Length);
			registrations.ForAll(r => AssertRegistration(r, typeof(I2), RegistryScope.Singleton, default(InterceptorProfile)));
			Assert.AreEqual(typeof(C_I2_1), registrations[0].DefaultContext.Target.Type);
		}
		#endregion

		#region RegisterAll(Type, RegistryScope, InterceptorProfile, IBindTarget[])
		[TestMethod]
		public void RegisterAll_2_WithValidArguments_ShouldRegisterBindings()
		{
			// setup empty registration
			var registrar = new DummyRegistrar();

			// test
			registrar.RegisterAll(
				typeof(I1),
				RegistryScope.Transient,
				default);

			// assert
			Assert.AreEqual(0, registrar.CollectionServices().Length);

			// setup single registration
			registrar = new DummyRegistrar();

			// test
			registrar.RegisterAll(
				typeof(I1),
				RegistryScope.Transient,
				default,
				IBindTarget.Of(typeof(C_I1_1)));

			// assert
			Assert.AreEqual(1, registrar.CollectionServices().Length);
			var registrations = registrar.CollectionRegistrationsFor(typeof(I1));
			Assert.AreEqual(1, registrations.Length);
			AssertRegistration(registrations[0], typeof(I1), RegistryScope.Transient, default(InterceptorProfile));
			Assert.AreEqual(typeof(C_I1_1), registrations[0].DefaultContext.Target.Type);

			// setup multiple registrations
			registrar = new DummyRegistrar();
			registrar
				.RegisterAll<I1>(
					RegistryScope.Transient,
					default,
					IBindTarget.Of(typeof(C_I1_1)),
					IBindTarget.Of(typeof(C_I1_2)),
					IBindTarget.Of(typeof(C_I1_I2_1)))
				.RegisterAll<I2>(
					RegistryScope.Singleton,
					default,
					IBindTarget.Of(typeof(C_I2_1)));

			// assert
			Assert.AreEqual(2, registrar.CollectionServices().Length);

			registrations = registrar.CollectionRegistrationsFor(typeof(I1));
			Assert.AreEqual(3, registrations.Length);
			registrations.ForAll(r => AssertRegistration(r, typeof(I1), RegistryScope.Transient, default(InterceptorProfile)));
			Assert.AreEqual(typeof(C_I1_1), registrations[0].DefaultContext.Target.Type);
			Assert.AreEqual(typeof(C_I1_2), registrations[1].DefaultContext.Target.Type);
			Assert.AreEqual(typeof(C_I1_I2_1), registrations[2].DefaultContext.Target.Type);

			registrations = registrar.CollectionRegistrationsFor(typeof(I2));
			Assert.AreEqual(1, registrations.Length);
			registrations.ForAll(r => AssertRegistration(r, typeof(I2), RegistryScope.Singleton, default(InterceptorProfile)));
			Assert.AreEqual(typeof(C_I2_1), registrations[0].DefaultContext.Target.Type);
		}
		#endregion


		private void AssertRegistration(
			RegistrationInfo info,
			Type serviceType,
			RegistryScope? scope,
			InterceptorProfile? profile)
		{
			if (serviceType != null)
				Assert.AreEqual(serviceType, info.ServiceType);

			if (scope != null)
				Assert.AreEqual(scope.Value, info.Scope);

			if (profile != null)
				Assert.AreEqual(profile.Value, info.Profile);
		}
	}

	public class DummyRegistrar : AbstractRegistrar
    {
		private readonly bool _isRegistrationClosed;
		private readonly IResolverContract _contract;

		public override IResolverContract BuildResolver() => _contract;

		public override bool IsRegistrationClosed() => _isRegistrationClosed;

		public DummyRegistrar(IResolverContract resolver = null, bool isClosed = false)
        {
			_isRegistrationClosed = isClosed;
			_contract = resolver;
        }
    }
}
