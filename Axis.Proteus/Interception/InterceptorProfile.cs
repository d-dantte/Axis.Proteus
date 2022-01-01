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
				.ToArray()
				.ThrowIf(Empty, new ArgumentException("Interceptor list cannot be empty"));

			_hashCode = ValueHash(_interceptors);
		}

		/// <summary>
		/// Note that if no argument is passed, this constructor is skipped in favor of the default struct constructor.
		/// </summary>
		public InterceptorProfile(
			params IInterceptor[] rest)
			: this((IEnumerable<IInterceptor>)rest)
		{
		}

		private static bool ContainsNull(IEnumerable<IInterceptor> interceptors) => interceptors.Any(i => i == null);

		private static bool Empty(IInterceptor[] interceptors) => interceptors.Length <= 0;

		public override int GetHashCode() => _hashCode;

		public override bool Equals(object obj)
		{
			return obj is InterceptorProfile other
				&& _hashCode == other._hashCode
				&& _interceptors.NullOrTrue(
					other.Interceptors,
					(x, y) => x.SequenceEqual(y));
		}
	}
}
