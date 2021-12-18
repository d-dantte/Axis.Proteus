using Axis.Luna.Extensions;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;

using static Axis.Luna.Extensions.Common;

namespace Axis.Proteus.Interception
{
	/// <summary>
	/// Represents an ordered set of interceptors that are attached to instances generated for types registered with interceptors.
	/// The interceptors are executed in the order they are given in this struct.
	/// </summary>
	public struct InterceptorProfile
	{
		private readonly IInterceptor[] _interceptors;
		private readonly int _hashCode;

		/// <summary>
		/// A sequence of interceptor instances, in a specific order, to be registered for services
		/// </summary>
		public IEnumerable<IInterceptor> Interceptors => _interceptors ?? new IInterceptor[0];

		public InterceptorProfile(IEnumerable<IInterceptor> interceptors)
		{
			_interceptors = interceptors
				.ThrowIfNull(new ArgumentNullException(nameof(interceptors)))
				.ThrowIf(ContainsNull, new ArgumentException("Null Interceptor found"))
				.ToArray();

			_hashCode = ValueHash(_interceptors);
		}

		public InterceptorProfile(
			IInterceptor first, 
			params IInterceptor[] rest)
			: this(first.Concat(rest)) // :this(first.Enumerate().Concat(rest))
		{
		}

		private static bool ContainsNull(IEnumerable<IInterceptor> interceptors) => interceptors.Any(i => i == null);

		public override int GetHashCode() => _hashCode;

		public override bool Equals(object obj)
		{
			return obj is InterceptorProfile other
				&& _hashCode == other._hashCode
				&& _interceptors.SequenceEqual(other.Interceptors);
		}
	}
}
