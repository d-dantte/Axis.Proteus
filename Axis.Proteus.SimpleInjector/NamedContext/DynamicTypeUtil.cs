using Axis.Luna.Extensions;
using Axis.Proteus.IoC;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Axis.Proteus.SimpleInjector.NamedContext
{
    internal static class DynamicTypeUtil
    {
        public static readonly string NamedContextContainerTypeNewInstanceMethodName = "NewInstance";

        private static readonly AssemblyBuilder LocalAssemblyBuilder = 
            AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Axis_Proteus_SimpleInjector_NamedContext_DynamicTypeAssembly"),
                AssemblyBuilderAccess.Run);

        private static readonly ModuleBuilder LocalModuleBuilder = 
            LocalAssemblyBuilder.DefineDynamicModule("Main");

        private static readonly string TypeNamePrefix = "NamedContextType";
        private static readonly ConcurrentDictionary<(Type, Type, string), Guid> TypeIDCache = new ConcurrentDictionary<(Type, Type, string), Guid>();
        private static readonly ConcurrentDictionary<Guid, Type> TypeCache = new ConcurrentDictionary<Guid, Type>();

        internal static string ToTypeName(this 
            ResolutionContextName name,
            Type serviceType,
            Type implType,
            DynamicTypeTag tag,
            out Guid typeId)
        {
            typeId = TypeIDCache.GetOrAdd((serviceType, implType, name.Name), _ => Guid.NewGuid());
            return $"{TypeNamePrefix}_{name.Name}_{tag}_{typeId.ToString().Replace("-", "_")}";
        }

        internal static Type ToReplacementTypeForNamedContext(this
            ResolutionContextName name,
            Type serviceType,
            Type implType)
        {
            var typeName = name.ToTypeName(serviceType, implType, DynamicTypeTag.Replacement, out var typeId);
            return TypeCache.GetOrAdd(typeId, _ =>
            {
                // Generate the Replacement type
                // type
                var baseType = implType;
                var typeBuilder = LocalModuleBuilder.DefineType(
                    typeName,
                    TypeAttributes.Public | TypeAttributes.Class,
                    baseType,
                    new[] { typeof(INamedContextReplacement) });

                // constructor
                baseType
                    .GetConstructors()
                    .ForAll(baseConstructor => EmitBaseConstructorCallConstructor(typeBuilder, baseConstructor));

                return typeBuilder.CreateType();
            });
        }

        internal static Type ToContainerTypeForNamedContext(this
            ResolutionContextName name,
            Type serviceType,
            Type implType)
        {
            var typeName = name.ToTypeName(serviceType, implType, DynamicTypeTag.Container, out var typeId);
            return TypeCache.GetOrAdd(typeId, _ =>
            {
                // Generate the container type
                var baseType = typeof(NamedContextContainerBase);
                var typeBuilder = LocalModuleBuilder.DefineType(
                    typeName,
                    TypeAttributes.Public | TypeAttributes.Class,
                    baseType);

                // define constructor that calls base constructor
                var constructor = baseType
                    .GetConstructors()
                    .Select(baseConstructor => EmitBaseConstructorCallConstructor(typeBuilder, baseConstructor))
                    .First();

                // define static NewInstance method that creates instance of this type
                EmitBaseConstructorCallNewInstanceMethod(typeBuilder, constructor);

                return typeBuilder.CreateType();
            });
        }

        /// <summary>
        /// Create a constructor that calls the given base constructor with the supplied arguments,
        /// Or with the replicated arguments from the base constructor.
        /// If the <paramref name="constructorArgumentTypes"/> param is null or empty, the <paramref name="baseConstructor"/>'s
        /// arguments are used to build the arguments for this constructor, else they are used themselves. This thus
        /// assumes that the length and types are compatible. 
        /// </summary>
        /// <param name="typeBuilder">The type builder</param>
        /// <param name="baseConstructor">THe base constructor</param>
        /// <param name="constructorArgumentTypes"></param>
        private static ConstructorBuilder EmitBaseConstructorCallConstructor(
            TypeBuilder typeBuilder,
            ConstructorInfo baseConstructor,
            params Type[] constructorArgumentTypes)
        {
            var parameterTypes = constructorArgumentTypes.IsNullOrEmpty()
                    ? baseConstructor
                        .GetParameters()
                        .Select(param => param.ParameterType)
                        .ToArray()
                    : constructorArgumentTypes;

            var constructorBuilder = typeBuilder.DefineConstructor(
                baseConstructor.Attributes,
                CallingConventions.HasThis,
                parameterTypes);

            var ilEmitter = constructorBuilder.GetILGenerator();
            ilEmitter.Emit(OpCodes.Ldarg_0); // load 'this'

            // load the constructor arguments onto the stack
            for (int index = 1; index <= parameterTypes.Length; index++)
            {
                switch (index)
                {
                    case 1: ilEmitter.Emit(OpCodes.Ldarg_1); break;
                    case 2: ilEmitter.Emit(OpCodes.Ldarg_2); break;
                    case 3: ilEmitter.Emit(OpCodes.Ldarg_3); break;
                    default: ilEmitter.Emit(OpCodes.Ldarg_S, index); break;
                }
            }

            // call the base constructor
            ilEmitter.Emit(OpCodes.Call, baseConstructor);

            // return
            ilEmitter.Emit(OpCodes.Ret);

            return constructorBuilder;
        }

        private static MethodBuilder EmitBaseConstructorCallNewInstanceMethod(
            TypeBuilder typeBuilder,
            ConstructorInfo constructor)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                NamedContextContainerTypeNewInstanceMethodName,
                MethodAttributes.Public
                | MethodAttributes.Static
                | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                typeBuilder.UnderlyingSystemType,
                new[] { typeof(object) });

            var ilEmitter = methodBuilder.GetILGenerator();
            ilEmitter.Emit(OpCodes.Ldarg_0);
            ilEmitter.Emit(OpCodes.Newobj, constructor);
            ilEmitter.Emit(OpCodes.Ret);

            return methodBuilder;
        }
    }

    internal enum DynamicTypeTag
    {
        Container,
        Replacement
    }
}
