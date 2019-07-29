﻿using Disruptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DisruptorPlayground.Advanced3
{
    //https://github.com/disruptor-net/Disruptor-net/blob/2553b63650484f2189d37947db4599bfbec25887/src/Disruptor/Internal/StructProxy.cs
    internal static class StructProxy
    {
        private static readonly ModuleBuilder _moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(nameof(StructProxy) + ".DynamicAssembly"), AssemblyBuilderAccess.Run)
                                                                              .DefineDynamicModule(nameof(StructProxy));

        private static readonly Dictionary<Type, Type> _proxyTypes = new Dictionary<Type, Type>();

        public static TInterface CreateProxyInstance<TInterface>(TInterface target)
        {
            var targetType = target.GetType();

            if (targetType.IsValueType)
                return target;

            Type proxyType;
            lock (_proxyTypes)
            {
                if (!_proxyTypes.TryGetValue(targetType, out proxyType))
                {
                    proxyType = GenerateStructProxyType(targetType);
                    _proxyTypes.Add(targetType, proxyType);
                }
            }

            if (!typeof(TInterface).IsAssignableFrom(proxyType))
                return target;

            return (TInterface)Activator.CreateInstance(proxyType, target);
        }

        private static Type GenerateStructProxyType(Type targetType)
        {
            var interfaceTypes = targetType.GetInterfaces().Where(x => x.IsVisible).ToList();

            if (!CanGenerateStructProxy(targetType, interfaceTypes))
                return null;

            var typeBuilder = _moduleBuilder.DefineType($"StructProxy_{targetType.Name}_{Guid.NewGuid():N}", TypeAttributes.Public, typeof(ValueType));

            var field = typeBuilder.DefineField("_target", targetType, FieldAttributes.Private);

            GenerateConstructor(targetType, typeBuilder, field);

            foreach (var interfaceType in interfaceTypes)
            {
                GenerateInterfaceImplementation(interfaceType, targetType, typeBuilder, field);
            }

            return typeBuilder.CreateTypeInfo();
        }

        private static bool CanGenerateStructProxy(Type targetType, List<Type> interfaceTypes)
        {
            if (!targetType.IsVisible)
                return false;

            return interfaceTypes.SelectMany(x => targetType.GetInterfaceMap(x).TargetMethods).All(x => x.IsPublic);
        }

        private static void GenerateConstructor(Type targetType, TypeBuilder typeBuilder, FieldBuilder field)
        {
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { targetType });

            var constructorGenerator = constructor.GetILGenerator();
            constructorGenerator.Emit(OpCodes.Ldarg_0);
            constructorGenerator.Emit(OpCodes.Ldarg_1);
            constructorGenerator.Emit(OpCodes.Stfld, field);
            constructorGenerator.Emit(OpCodes.Ret);
        }

        private static void GenerateInterfaceImplementation(Type interfaceType, Type targetType, TypeBuilder typeBuilder, FieldBuilder field)
        {
            typeBuilder.AddInterfaceImplementation(interfaceType);

            var interfaceMap = targetType.GetInterfaceMap(interfaceType);

            for (var index = 0; index < interfaceMap.InterfaceMethods.Length; index++)
            {
                var interfaceMethod = interfaceMap.InterfaceMethods[index];
                var parameters = interfaceMethod.GetParameters();
                var targetMethod = interfaceMap.TargetMethods[index];

                var method = typeBuilder.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, interfaceMethod.ReturnType, parameters.Select(x => x.ParameterType).ToArray());

                if (targetMethod.IsGenericMethod)
                {
                    var genericArguments = targetMethod.GetGenericArguments();
                    method.DefineGenericParameters(genericArguments.Select((x, i) => $"T{i}").ToArray());
                }

                method.SetImplementationFlags(method.GetMethodImplementationFlags() | MethodImplAttributes.AggressiveInlining);

                var methodGenerator = method.GetILGenerator();

                if (interfaceType == typeof(IBatchStartAware) && targetMethod.Name == nameof(IBatchStartAware.OnBatchStart))
                {
                    GenerateOnBatchStart(methodGenerator, targetMethod, field);
                }
                else
                {
                    GenerateDefaultMethod(methodGenerator, targetMethod, field, parameters);
                }
            }
        }

        private static void GenerateOnBatchStart(ILGenerator methodGenerator, MethodInfo targetMethod, FieldBuilder field)
        {
            var returnLabel = methodGenerator.DefineLabel();

            // if (batchSize == 0) return
            methodGenerator.Emit(OpCodes.Ldarg_1);
            methodGenerator.Emit(OpCodes.Brfalse_S, returnLabel);

            // _target.OnBatchStart(batchSize)
            methodGenerator.Emit(OpCodes.Ldarg_0);
            methodGenerator.Emit(OpCodes.Ldfld, field);
            methodGenerator.Emit(OpCodes.Ldarg_1);
            methodGenerator.Emit(OpCodes.Call, targetMethod);

            methodGenerator.MarkLabel(returnLabel);
            methodGenerator.Emit(OpCodes.Ret);
        }

        private static void GenerateDefaultMethod(ILGenerator methodGenerator, MethodInfo targetMethod, FieldBuilder field, ParameterInfo[] parameters)
        {
            methodGenerator.Emit(OpCodes.Ldarg_0);
            methodGenerator.Emit(OpCodes.Ldfld, field);

            for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                methodGenerator.Emit(OpCodes.Ldarg_S, (byte)parameterIndex + 1);
            }

            methodGenerator.Emit(OpCodes.Call, targetMethod);
            methodGenerator.Emit(OpCodes.Ret);
        }
    }
}
