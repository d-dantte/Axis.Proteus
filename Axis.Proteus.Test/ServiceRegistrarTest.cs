using Axis.Proteus.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Proteus.Test
{
	[TestClass]
	public class ServiceRegistrarTest
	{
		Mock<IRegistrarContract> _registrarMock = new Mock<IRegistrarContract>();

		public ServiceRegistrarTest()
		{
			//setup mock
		}

		[TestMethod]
		public void RegisterServiceType_WithValidServiceType_ShouldRegister()
		{

		}
	}
}
