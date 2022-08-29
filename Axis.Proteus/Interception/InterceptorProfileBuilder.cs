using Castle.DynamicProxy;
using System;
using System.Collections.Generic;

namespace Axis.Proteus.Interception
{
    public class InterceptorProfileBuilder
    {
        private readonly List<IInterceptor> _interceptors = new List<IInterceptor>();

        private InterceptorProfileBuilder()
        {
        }

        public InterceptorProfile Build() => new InterceptorProfile(_interceptors);

        public InterceptorProfileBuilder With<TInterceptor>()
        where TInterceptor: IInterceptor, new()
        {
            _interceptors.Add(new TInterceptor());
            return this;
        }

        public InterceptorProfileBuilder WithInterceptorInstance(IInterceptor instance)
        {
            _interceptors.Add(instance ?? throw new ArgumentNullException(nameof(instance)));
            return this;
        }



        /// <summary>
        /// Creates a new instance of the <see cref="InterceptorProfileBuilder"/>
        /// </summary>
        public static InterceptorProfileBuilder NewBuilder() => new InterceptorProfileBuilder();
    }
}
