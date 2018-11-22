using System;
using System.Collections.Generic;
using System.Linq;
using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Axis.Proteus.Interception;
using Axis.Proteus.Ioc;
using Castle.DynamicProxy;

namespace Axis.Proteus.SimpleInjector
{
    public class InterceptorRoot : IInterceptor
    {
        private readonly List<IProxyInterceptor> _interceptors = new List<IProxyInterceptor>();
        private readonly Func<object> _targetProvider;

        public InterceptorRoot(IServiceResolver resolver, InterceptorRegistry registry)
        {
            _interceptors.AddRange(registry.Interceptors()
                                           .Select(type => resolver.Resolve(type).As<IProxyInterceptor>()));
        }
        public InterceptorRoot(IServiceResolver resolver, Func<object> targetProvider, InterceptorRegistry registry)
        {
            _targetProvider = targetProvider;
            _interceptors.AddRange(registry.Interceptors()
                                           .Select(type => resolver.Resolve(type).As<IProxyInterceptor>()));
        }
        

        public void Intercept(IInvocation invocation)
        {
            var context = new InvocationContext(invocation.Method,
                                                invocation.Proxy,
                                                _targetProvider,
                                                invocation.Arguments,
                                                invocation.GenericArguments,
                                                _interceptors.ToArray());

            context
                .Next()
                .Then(result => { invocation.ReturnValue = result; })
                .ReThrow(exception =>
                {
                    //if the interception failed, and the invocation is an operation (returns an operation),
                    //wrap the error of the invocation into a failed operation and return that
                    if (InvocationReturnsOperation(invocation))
                        invocation.ReturnValue = CreateFailedOperation(invocation.Method.ReturnType, exception);

                    return exception;
                })
                .Resolve();
        }

        private bool InvocationReturnsOperation(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;

            if (returnType.TypeLineage().Contains(typeof(Operation))) return true;
            else if (returnType.HasGenericAncestor(typeof(Operation<>))) return true;

            else return false;
        }

        public static object CreateFailedOperation(Type operationType, Exception ex)
        {
            var top = typeof(Operation);

            if (operationType == top || operationType.TypeLineage().Contains((top)))
                return Operation.Fail(ex);

            else //if(operationType.HasGenericAncestor(typeof(Operation<>)))
            {
                var genericParam = operationType.GetGenericArguments()[0];

                var failedMethod = top
                    .GetMethods()
                    .Where(m => m.IsStatic)
                    .Where(m => m.Name == nameof(Operation.Fail))
                    .Where(m => m.IsGenericMethod)
                    .FirstOrDefault()
                    .MakeGenericMethod(genericParam);

                return failedMethod.CallStaticFunc(ex);
            }
        }
    }
}
