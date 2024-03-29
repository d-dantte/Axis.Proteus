﻿using System;

namespace Axis.Proteus.Exceptions
{
	public class IncompatibleTypesException: Exception
	{
		public Type ServiceType { get; }

		public Type ImplementationType { get; }


		public IncompatibleTypesException(
			Type serviceType,
			Type implementationType)
			: base($"Invalid registration: service type '{serviceType}' is not assignable from the supplied implementation type '{implementationType}'")
		{
			ServiceType = serviceType;
			ImplementationType = implementationType;
		}

		public IncompatibleTypesException(
			string message,
			Type serviceType,
			Type implementationType)
			: base(message)
		{
			ServiceType = serviceType;
			ImplementationType = implementationType;
		}
	}
}
