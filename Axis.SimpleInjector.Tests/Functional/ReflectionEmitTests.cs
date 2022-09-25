using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Axis.SimpleInjector.Tests.Functional
{
    [TestClass]
    public class ReflectionEmitTests
    {

        [TestMethod]
        public void LateAdditionToDynamicAssembly()
        {
            (var typeA, var builder) = GenerateType("ClassA", null);

            Console.WriteLine(typeA);

            (var typeB, builder) = GenerateType("ClassB", builder);

            Console.WriteLine(typeB);
        }

        private static (Type? type, ModuleBuilder builder) GenerateType(string name, ModuleBuilder? builder = null)
        {
            // dynamic assembly
            var assemblyBuilder = builder == null
                ? AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName($"{name}_assembly"),
                    AssemblyBuilderAccess.Run)
                : null;

            // module 
            var moduleBuilder = builder ?? assemblyBuilder?.DefineDynamicModule($"{name}_module") ?? throw new Exception();

            // type
            var typeBuilder = moduleBuilder.DefineType(
                name,
                TypeAttributes.Public | TypeAttributes.Class);

            // constructor
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                Type.EmptyTypes);

            // constructor IL
            var ctorBuilder = constructorBuilder.GetILGenerator();
            ctorBuilder.Emit(OpCodes.Ldarg_0); // load 'this'
            ctorBuilder.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes) ?? throw new Exception()); // call the base constructor
            ctorBuilder.Emit(OpCodes.Ret);

            return (typeBuilder.CreateType(), moduleBuilder);
        }
    }
}
