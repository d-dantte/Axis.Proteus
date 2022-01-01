using Axis.Proteus.Exceptions;
using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Axis.Proteus.Test.IoC
{
    [TestClass]
	public class ServiceRegistrarTest
	{
		private Mock<IRegistrarContract> _registrarMock = new Mock<IRegistrarContract>();

		public ServiceRegistrarTest()
		{
			//setup mock
		}

		#region Register(Type, RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_1_WithValidServiceType_ShouldRegister()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register(
				typeof(C_I1),
				RegistryScope.Transient,
				null);

			_registrarMock.Verify(
				times: Times.Once,
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()));

			// test
			registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register(
				typeof(C_I1),
				RegistryScope.Transient,
				new InterceptorProfile(new DummyInterceptor()));

			_registrarMock.Verify(
				times: Times.Exactly(2),
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()));
		}

		[TestMethod]
		public void Register_1_WithNullServiceType_ShouldThrowException()
        {
			// nothing to setup

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			Assert.ThrowsException<ArgumentNullException>(
				() => registrar.Register(
					null,
					RegistryScope.Transient,
					null));
		}

		[TestMethod]
		public void Register_1_WithDuplicateRegistration_ShouldThrowException()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register(
				typeof(C_I1),
				RegistryScope.Transient,
				null);
			Assert.ThrowsException<DuplicateRegistrationException>(
				() => registrar.Register(
					typeof(C_I1),
					RegistryScope.Transient,
					null));
		}

		#endregion

		#region Register(Type, Type, RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_2_WithValidServiceType_ShouldRegister()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register(
				typeof(I1),
				typeof(C_I1),
				RegistryScope.Transient,
				null);

			_registrarMock.Verify(
				times: Times.Once,
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()));
		}

		[TestMethod]
		public void Register_2_WithNullTypes_ShouldThrowException()
		{
			// nothing to setup

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			Assert.ThrowsException<ArgumentNullException>(
				() => registrar.Register(
					null,
					typeof(C_I1),
					RegistryScope.Transient,
					null));

			Assert.ThrowsException<ArgumentNullException>(
				() => registrar.Register(
					typeof(I1),
					(Type)null,
					RegistryScope.Transient,
					null));
		}

		[TestMethod]
		public void Register_2_WithIncompatibleTypes_ShouldThrowException()
		{
			// nothing to setup

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			Assert.ThrowsException<IncompatibleTypesException>(
				() => registrar.Register(
					typeof(I2),
					typeof(C_I1),
					RegistryScope.Transient,
					null));
		}

		[TestMethod]
		public void Register_2_WithDuplicateRegistration_ShouldThrowException()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register(
				typeof(I1),
				typeof(C_I1),
				RegistryScope.Transient,
				null);
			Assert.ThrowsException<DuplicateRegistrationException>(
				() => registrar.Register(
					typeof(I1),
					typeof(C_I1),
					RegistryScope.Transient,
					null));
		}
		#endregion

		#region Register(Type, Func<IResolverContract, object>, RegistryScope?, InterceptorProfile?);

		[TestMethod]
		public void Register_3_WithValidArgs_ShouldRegister()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Func<IResolverContract, object>>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register(
				typeof(I1),
				_r => new C_I1(),
				RegistryScope.Transient,
				null);

			_registrarMock.Verify(
				times: Times.Once,
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Func<IResolverContract, object>>(),
					It.IsAny<RegistryScope?>()));


			_ = registrar.Register(
				typeof(I1),
				_r => new C_I1(),
				RegistryScope.Transient,
				new InterceptorProfile(new DummyInterceptor()));

			_registrarMock.Verify(
				times: Times.Exactly(2),
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Func<IResolverContract, object>>(),
					It.IsAny<RegistryScope?>()));
		}
		#endregion

		#region Register<Impl>(RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_4_WithValidServiceType_ShouldRegister()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register<C_I1>(It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			_registrarMock
				.Setup(r => r.Register<C_I1, C_I1>(It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register<C_I1>(
				RegistryScope.Transient,
				null);

			_registrarMock.Verify(
				times: Times.Once,
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()));

			// test
			registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register<C_I1>(
				RegistryScope.Transient,
				new InterceptorProfile(new DummyInterceptor()));

			_registrarMock.Verify(
				times: Times.Exactly(2),
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()));
		}
		#endregion

		#region Register<TService, TImpl>(RegistryScope?, InterceptorProfile?);
		[TestMethod]
		public void Register_5_WithValidServiceType_ShouldRegister()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register<I1, C_I1>(It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register<I1, C_I1>(
				RegistryScope.Transient,
				null);

			_registrarMock.Verify(
				times: Times.Once,
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Type>(),
					It.IsAny<RegistryScope?>()));
		}
		#endregion

		#region Register<TService>(Func<IResolverContract, TService>, RegistryScope?, InterceptorProfile?);

		[TestMethod]
		public void Register_6_WithValidArgs_ShouldRegister()
		{
			// setup
			_registrarMock
				.Setup(r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Func<IResolverContract, object>>(),
					It.IsAny<RegistryScope?>()))
				.Returns(_registrarMock.Object);

			// test 1
			var registrar = new ServiceRegistrar(_registrarMock.Object);
			_ = registrar.Register<I1>(
				_r => new C_I1(),
				RegistryScope.Transient,
				null);

			_registrarMock.Verify(
				times: Times.Once,
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Func<IResolverContract, object>>(),
					It.IsAny<RegistryScope?>()));

			// test 2
			_ = registrar.Register<I2>(
				_r => new C_I2(),
				RegistryScope.Transient,
				new InterceptorProfile(new DummyInterceptor()));

			_registrarMock.Verify(
				times: Times.Exactly(2),
				expression: r => r.Register(
					It.IsAny<Type>(),
					It.IsAny<Func<IResolverContract, object>>(),
					It.IsAny<RegistryScope?>()));
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
