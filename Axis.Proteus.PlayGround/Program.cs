using Castle.DynamicProxy;
using SimpleInjector;
using System;

namespace Axis.Proteus.PlayGround
{
	class Program
	{
		static void Main(string[] args)
		{
			var container = new Container();
			container.Register<ISomeInterface, ImplementingClass>();
			container.Register<ImplementingClass>();

			var x = container.GetInstance<ImplementingClass>();
			var y = container.GetInstance<ISomeInterface>();

			var generator = new ProxyGenerator();
			var interceptor = new Interceptor();

			var proxy1 = generator.CreateClassProxyWithTarget(
				new SomeClass(),
				interceptor);

			var proxy2 = generator.CreateClassProxyWithTarget(
				new ImplementingClass(),
				interceptor);

			proxy1.Method1("bleh");
			proxy2.Method2("helb");

			Console.ReadKey();
		}
	}

	public class Interceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			Console.WriteLine($"Intercepted: {invocation.InvocationTarget.GetType()}.[{invocation.MethodInvocationTarget}]");

			invocation.Proceed();
		}
	}

	public class SomeClass
	{
		private int _count = 0;

		virtual public int Method1(string param)
		{
			Console.WriteLine($"{nameof(SomeClass)}.{nameof(SomeClass.Method1)} called {++_count}x times with '{param}' param");

			return _count;
		}
	}

	public interface ISomeInterface
	{
		public int Method2(string param);
	}

	public class ImplementingClass : ISomeInterface
	{
		private int _count = 0;

		public int Method2(string param)
		{
			Console.WriteLine($"{nameof(SomeClass)}.{nameof(ImplementingClass.Method2)} called {++_count}x times with '{param}' param");

			return _count;
		}
	}
}
