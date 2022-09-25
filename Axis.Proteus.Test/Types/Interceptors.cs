using Castle.DynamicProxy;

namespace Axis.Proteus.Test.Types
{
    public class NoOpInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation) => invocation.Proceed();
    }
}
