using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Axis.Proteus.SimpleInjector.NamedContext
{
    internal static class DynamicTypeUtil
    {
        private static readonly AssemblyBuilder LocalAssemblyBuilder = 
            AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Axis_Proteus_SimpleInjector_NamedContext_DynamicTypeAssembly"),
                AssemblyBuilderAccess.Run);

        private static readonly ModuleBuilder LocalModuleBuilder = 
            LocalAssemblyBuilder.DefineDynamicModule("Main");

        private static readonly string TypeNamePrefix = "NamedContextContainer";
        private static readonly Dictionary<(Type, Type), Guid> TypeIDCache = new Dictionary<(Type, Type), Guid>();
        private static readonly Dictionary<Guid, Type> DynamicTypeCache = new Dictionary<Guid, Type>();

        public static string ToTypeName(this 
            ResolutionContextName name,
            Type serviceType,
            Type implType,
            out Guid typeId)
        {
            typeId = TypeIDCache.GetOrAdd((serviceType, implType), _ => Guid.NewGuid());
            return $"{TypeNamePrefix}_{name.Name}_{typeId}";
        }

        public static Type ToNamedContextType(this
            ResolutionContextName name,
            Type serviceType,
            Type implType)
        {
            var typeName = name.ToTypeName(serviceType, implType, out var typeId);
            return DynamicTypeCache.GetOrAdd(typeId, _ =>
            {
                // type
                var baseType = typeof(NamedContextContainer);
                var typeBuilder = LocalModuleBuilder.DefineType(
                    typeName,
                    TypeAttributes.Public | TypeAttributes.Class,
                    baseType);

                // constructor
                var constructorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.HasThis,
                    new[] { typeof(object) });

                // constructor IL
                var ctorBuilder = constructorBuilder.GetILGenerator();
                ctorBuilder.Emit(OpCodes.Ldarg_0); // load 'this'
                ctorBuilder.Emit(OpCodes.Ldarg_1); // load the constructor argument
                ctorBuilder.Emit(OpCodes.Call, baseType.GetConstructor(new[] { typeof(object) })); // call the base constructor
                ctorBuilder.Emit(OpCodes.Ret);

                return typeBuilder.CreateType();
            });
        }
    }
}
