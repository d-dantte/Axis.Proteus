using Axis.Luna.Common;
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
	public readonly struct InterceptorProfile :
		IDefaultValueProvider<InterceptorProfile>
	{
		private readonly IInterceptor[] _interceptors;
		private readonly int _hashCode;

		/// <summary>
		/// A sequence of interceptor instances, in a specific order, to be registered for services
		/// </summary>
		public IEnumerable<IInterceptor> Interceptors => _interceptors.AsEnumerable() ?? Enumerable.Empty<IInterceptor>();

		public bool IsDefault => _interceptors is null && _hashCode == 0;

		public static InterceptorProfile Default => default;

        public InterceptorProfile(IEnumerable<IInterceptor> interceptors)
		{
			_interceptors = interceptors
				.ThrowIfNull(() => new ArgumentNullException(nameof(interceptors)))
				.ToArray()
				.ThrowIf(ContainsNull, _ => new ArgumentException("Null Interceptor found"))
				.ThrowIf(IsEmpty, _ => new ArgumentException("Interceptor list cannot be empty"));

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

		private static bool IsEmpty(IInterceptor[] interceptors) => interceptors.Length <= 0;

		public override int GetHashCode() => _hashCode;

		public override bool Equals(object? obj)
		{
			return obj is InterceptorProfile other
				&& _hashCode == other._hashCode
				&& _interceptors.IsNullOrTrue(
					other._interceptors,
					Enumerable.SequenceEqual);
		}

		public static bool operator ==(InterceptorProfile first, InterceptorProfile second) => first.Equals(second);

		public static bool operator !=(InterceptorProfile first, InterceptorProfile second) => !first.Equals(second);

		public static implicit operator InterceptorProfile(IInterceptor[] interceptors) => new InterceptorProfile(interceptors);
	}
}
