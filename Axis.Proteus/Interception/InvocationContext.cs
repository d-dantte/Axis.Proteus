using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Axis.Luna.Extensions;
using Axis.Luna.Operation;

namespace Axis.Proteus.Interception
{
    public class InvocationContext
    {
        private readonly IEnumerator<IProxyInterceptor> _interceptors;
        private readonly IProxyInterceptor _currentInterceptor;
        private readonly Func<object> _targetFactory;
        private object _target;

        public object[] Arguments { get; }

        public Type[] GenericArguments { get; private set; }

        public MethodInfo Method { get; private set; }

        public object Target => _target ?? (_target = _targetFactory() ?? throw new Exception("Invalid Target Factory result: null"));

        public object Proxy { get; private set; }

        public Func<Operation<object>> Next { get; private set; }


        public InvocationContext(MethodInfo method, object proxy, Func<object> targetFactory, object[] arguments = null, Type[] genericArgs = null, params IProxyInterceptor[] interceptors)
        {
            Arguments = arguments ?? new object[0];
            GenericArguments = genericArgs ?? new Type[0];
            _targetFactory = targetFactory;
            Proxy = proxy.ThrowIfNull("Invalid proxy");
            Method = method.ThrowIfNull("Invalid invocation method");

            _interceptors = interceptors
                .Cast<IProxyInterceptor>()
                .GetEnumerator();
            _currentInterceptor = _interceptors.MoveNext() ? _interceptors.Current : null;
            InitNext();
        }

        private InvocationContext(InvocationContext parent)
        {
            Method = parent.Method;
            Proxy = parent.Proxy;
            Arguments = parent.Arguments?.Clone().As<object[]>();
            GenericArguments = parent.GenericArguments?.Clone().As<Type[]>();

            _interceptors = parent._interceptors;
            _currentInterceptor = _interceptors.MoveNext() ? _interceptors.Current : null;
            _target = parent._target;
            _targetFactory = parent._targetFactory;

            InitNext();
        }

        private void InitNext()
        {
            if (_currentInterceptor != null)
            {
                Next = () => Operation.Try(() =>
                {
                    var child = new InvocationContext(this);
                    return _currentInterceptor
                        .Intercept(child)
                        .Resolve();
                });
            }
        }
    }
}
