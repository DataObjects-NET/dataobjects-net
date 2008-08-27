// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using R=Xtensive.Core.Reflection;

namespace Xtensive.Core.Tuples.Internals
{
  internal static class TupleDescriptorGenerator
  {
    // Consts
    private readonly static AssemblyName assemblyName = new AssemblyName("Xtensive.GeneratedTupleDescriptors");
    private readonly static string executeMethodName = "Execute";
    private readonly static string resultPropertyName = "Result";
    // Statics
    private readonly static AssemblyBuilder assemblyBuilder;
    private static ModuleBuilder moduleBuilder;
    private static int moduleNumber = 0;
    private readonly static Dictionary<string, Type> cachedTypes = new Dictionary<string, Type>();
    // Varying statics
    private static TupleDescriptor sampleDescriptor;
    private static TypeBuilder typeBuilder;
    private static Type[] ctorArgTypes;
    private static Type baseType;
    private static string genericTypeName;


    // Generator

    public static GeneratedTupleDescriptor Generate(TupleDescriptor sample)
    {
      Initialize(sample);
      GeneratedTupleDescriptor descriptor = TryCreateCached();
      if (descriptor!=null)
        return descriptor;

      // No cached class is found - let's create it
      string[] genericArgNames = new string[sample.Count];
      for (int i = 0; i<sample.Count;)
        genericArgNames[i] = "T" + (++i);

      // "public sealed class GenericTupleDescriptor"
      moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name + (moduleNumber++));
      typeBuilder = moduleBuilder.DefineType(
        genericTypeName,
        TypeAttributes.Public | TypeAttributes.Sealed,
        baseType,
        ArrayUtils<Type>.EmptyArray);
      
      // "<T1, T2, ...>"
      typeBuilder.DefineGenericParameters(genericArgNames);

      typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes),
                new object[] {}));

      AddConstructor();
      AddExecuteActionHandlerOne();
      AddExecuteActionHandlerAny();
      AddExecuteFunctionHandlerAny();

      // Done
      cachedTypes.Add(genericTypeName, typeBuilder.CreateType());
      return TryCreateCached();
    }

    private static void Initialize(TupleDescriptor sample)
    {
      sampleDescriptor = sample;
      ctorArgTypes = new Type[] {typeof (IList<Type>)};
      baseType = typeof (GeneratedTupleDescriptor);
      genericTypeName = string.Format("{0}.{1}`{2}", baseType.Namespace, baseType.Name, sample.Count);
    }

    private static void AddConstructor()
    {
      ConstructorInfo baseTypeCtor = baseType.GetConstructor(
        BindingFlags.Instance | 
        BindingFlags.NonPublic |
        BindingFlags.ExactBinding, 
        null, ctorArgTypes, null);

      // "private .ctor(IList<Type> fieldTypes)"
      ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
        MethodAttributes.Private |
        MethodAttributes.HideBySig |
        MethodAttributes.RTSpecialName |
        MethodAttributes.SpecialName,
        CallingConventions.Standard,
        ctorArgTypes);

      // ": base(fieldTypes)"
      ILGenerator il = constructorBuilder.GetILGenerator();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Call, baseTypeCtor);
      il.Emit(OpCodes.Ret);
    }

    private static void AddExecuteActionHandlerOne()
    {
      Type actionHandlerGenericType = typeof (ITupleActionHandler<>);
      
      // "public override bool Execute<TActionData>(ITupleActionHandler<TActionData> actionHandler, ref TActionData actionData, int fieldIndex)"
      // "{"
      MethodBuilder executeMethod = typeBuilder.DefineMethod(
        executeMethodName,
        MethodAttributes.Public | 
        MethodAttributes.Virtual |
        MethodAttributes.HideBySig,
        CallingConventions.Standard);
      // TActionData constraints
      GenericTypeParameterBuilder[] genericParameters = executeMethod.DefineGenericParameters(new string[] {"TActionData"});
      GenericTypeParameterBuilder tActionDataParameter = genericParameters[0];
      tActionDataParameter.SetGenericParameterAttributes(
        GenericParameterAttributes.NotNullableValueTypeConstraint |
        GenericParameterAttributes.DefaultConstructorConstraint);
      tActionDataParameter.SetBaseTypeConstraint(typeof(ValueType));
      Type actionHandlerType = actionHandlerGenericType.MakeGenericType(new Type[] {tActionDataParameter});
      executeMethod.SetReturnType(typeof(bool));
      executeMethod.SetParameters(new Type[] { actionHandlerType, tActionDataParameter.MakeByRefType(), typeof (int) });
      ILGenerator il = executeMethod.GetILGenerator();

      // "switch (fieldIndex) {"
      il.Emit(OpCodes.Ldarg_3);
      EmitHelper.EmitSwitch(il, sampleDescriptor.Count, true, delegate(int fieldIndex, bool isDefault) {
        if (!isDefault) {
          // "case [0...sampleDescriptor.Count]:"
          // "  return actionHandler.Execute<TFieldType>(ref actionData, [fieldIndex]);"
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);
          il.Emit(OpCodes.Ldc_I4, fieldIndex);
          il.EmitCall(OpCodes.Callvirt, 
            TypeBuilder.GetMethod(actionHandlerType, actionHandlerGenericType.GetMethod(executeMethodName))
              .MakeGenericMethod(new Type[] {typeBuilder.GetGenericArguments()[fieldIndex]}), null);
          il.Emit(OpCodes.Ret);
        }
        else {
          // "default:"
          // "  throw new ArgumentOutOfRangeException("fieldIndex");"
          il.Emit(OpCodes.Ldstr, "fieldIndex");
          il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] {typeof(string)} ));
          il.Emit(OpCodes.Throw);
        }
      });
      // "}"
      // "return;"
      il.Emit(OpCodes.Ret);
      // "}"
    }

    private static void AddExecuteActionHandlerAny()
    {
      Type actionHandlerGenericType = typeof (ITupleActionHandler<>);
      
      // "public override void Execute<TActionData>(ITupleActionHandler<TActionData> actionHandler, ref TActionData actionData, Direction direction)"
      // "{"
      MethodBuilder executeMethod = typeBuilder.DefineMethod(
        executeMethodName,
        MethodAttributes.Public | 
        MethodAttributes.Virtual |
        MethodAttributes.HideBySig,
        CallingConventions.Standard);
      // TActionData constraints
      GenericTypeParameterBuilder[] genericParameters = executeMethod.DefineGenericParameters(new string[] {"TActionData"});
      GenericTypeParameterBuilder tActionDataParameter = genericParameters[0];
      tActionDataParameter.SetGenericParameterAttributes(
        GenericParameterAttributes.NotNullableValueTypeConstraint |
        GenericParameterAttributes.DefaultConstructorConstraint);
      tActionDataParameter.SetBaseTypeConstraint(typeof(ValueType));
      Type actionHandlerType = actionHandlerGenericType.MakeGenericType(new Type[] {tActionDataParameter});
      executeMethod.SetReturnType(typeof(void));
      executeMethod.SetParameters(new Type[] { actionHandlerType, tActionDataParameter.MakeByRefType(), typeof (Direction) });
      ILGenerator il = executeMethod.GetILGenerator();
      
      Label lEnd = il.DefineLabel();
      // "if (direction==Direction.Positive) {"
      il.Emit(OpCodes.Ldarg_3);
      il.Emit(OpCodes.Ldc_I4, (int)Direction.Negative);
      EmitHelper.EmitIfElse(il, OpCodes.Beq, delegate {
        // Inline loop (i.e. enrolled loop)
        for (int i = 0; i<sampleDescriptor.Count; i++) {
          // "if (actionHandler.Execute<TFieldType>(ref actionData, [fieldIndex])"
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);
          il.Emit(OpCodes.Ldc_I4, i);
          il.EmitCall(OpCodes.Callvirt, 
            TypeBuilder.GetMethod(actionHandlerType, actionHandlerGenericType.GetMethod(executeMethodName))
              .MakeGenericMethod(new Type[] {typeBuilder.GetGenericArguments()[i]}), null);
          // "  goto lEnd;"
          il.Emit(OpCodes.Brtrue, lEnd);
        }
      },
      // "} else {"
      delegate {
        // Inline loop (i.e. enrolled loop)
        for (int i = sampleDescriptor.Count-1; i>=0; i--) {
          // "if (actionHandler.Execute<T>(ref actionData, [fieldIndex])"
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);
          il.Emit(OpCodes.Ldc_I4, i);
          il.EmitCall(OpCodes.Callvirt, 
            TypeBuilder.GetMethod(actionHandlerType, actionHandlerGenericType.GetMethod(executeMethodName))
              .MakeGenericMethod(new Type[] {typeBuilder.GetGenericArguments()[i]}), null);
          // "  goto lEnd;"
          il.Emit(OpCodes.Brtrue, lEnd);
        }
      });
      // "lEnd:"
      il.MarkLabel(lEnd);
      // "  return;"
      il.Emit(OpCodes.Ret);
      // "}"
    }

    private static void AddExecuteFunctionHandlerAny()
    {
      Type actionHandlerGenericType   = typeof (ITupleActionHandler<>);
      Type functionHandlerGenericType = typeof (ITupleFunctionHandler<,>);
      Type functionDataGenericType    = typeof (ITupleFunctionData<>);
      
      // "public override TResult Execute<TFunctionData, TResult>("
      // "  ITupleFunctionHandler<TFunctionData> functionHandler, ref TFunctionData functionData, Direction direction)"
      // "{"
      MethodBuilder executeMethod = typeBuilder.DefineMethod(
        executeMethodName,
        MethodAttributes.Public | 
        MethodAttributes.Virtual |
        MethodAttributes.HideBySig,
        CallingConventions.Standard);
      // TFunctionData, TResult constraints
      GenericTypeParameterBuilder[] genericParameters = executeMethod.DefineGenericParameters(new string[] {"TFunctionData", "TResult"});
      GenericTypeParameterBuilder tFunctionDataParameter = genericParameters[0];
      GenericTypeParameterBuilder tResultParameter = genericParameters[1];
//      tResultParameter.SetGenericParameterAttributes(
//        GenericParameterAttributes.NotNullableValueTypeConstraint |
//        GenericParameterAttributes.DefaultConstructorConstraint);
//      tResultParameter.SetBaseTypeConstraint(typeof(ValueType));
      tFunctionDataParameter.SetGenericParameterAttributes(
        GenericParameterAttributes.NotNullableValueTypeConstraint |
        GenericParameterAttributes.DefaultConstructorConstraint);
      tFunctionDataParameter.SetBaseTypeConstraint(typeof(ValueType));
      tFunctionDataParameter.SetInterfaceConstraints(new Type[] {
        typeof(ITupleFunctionData<>).MakeGenericType(new Type[] {tResultParameter})
        });
      
      Type actionHandlerType   = actionHandlerGenericType.MakeGenericType  (new Type[] {tFunctionDataParameter});
      Type functionHandlerType = functionHandlerGenericType.MakeGenericType(new Type[] {tFunctionDataParameter, tResultParameter});
      Type functionDataType    = functionDataGenericType.MakeGenericType(new Type[] {tResultParameter});
      executeMethod.SetReturnType(tResultParameter);
      executeMethod.SetParameters(new Type[] { functionHandlerType, tFunctionDataParameter.MakeByRefType(), typeof (Direction) });
      ILGenerator il = executeMethod.GetILGenerator();
      
      Label lEnd = il.DefineLabel();
      // "if (direction==Direction.Positive) {"
      il.Emit(OpCodes.Ldarg_3);
      il.Emit(OpCodes.Ldc_I4, (int)Direction.Negative);
      EmitHelper.EmitIfElse(il, OpCodes.Beq, delegate {
        // Inline loop (i.e. enrolled loop)
        for (int i = 0; i<sampleDescriptor.Count; i++) {
          // "if (functionHandler.Execute<TFieldType>(ref functionData, [fieldIndex])"
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);
          il.Emit(OpCodes.Ldc_I4, i);
          il.EmitCall(OpCodes.Callvirt, 
            TypeBuilder.GetMethod(actionHandlerType, actionHandlerGenericType.GetMethod(executeMethodName))
              .MakeGenericMethod(new Type[] {typeBuilder.GetGenericArguments()[i]}), null);
          // "  goto lEnd;"
          il.Emit(OpCodes.Brtrue, lEnd);
        }
      },
      // "} else {"
      delegate {
        // Inline loop (i.e. enrolled loop)
        for (int i = sampleDescriptor.Count-1; i>=0; i--) {
          // "if (functionHandler.Execute<TFieldType>(ref functionData, [fieldIndex])"
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);
          il.Emit(OpCodes.Ldc_I4, i);
          il.EmitCall(OpCodes.Callvirt, 
            TypeBuilder.GetMethod(actionHandlerType, actionHandlerGenericType.GetMethod(executeMethodName))
              .MakeGenericMethod(new Type[] {typeBuilder.GetGenericArguments()[i]}), null);
          // "  goto lEnd;"
          il.Emit(OpCodes.Brtrue, lEnd);
        }
      });
      // "lEnd:"
      il.MarkLabel(lEnd);
      
      // "return functionData.Result;"
      il.Emit(OpCodes.Ldarg_2);
      il.Emit(OpCodes.Constrained, tFunctionDataParameter);
      il.EmitCall(OpCodes.Callvirt, 
        TypeBuilder.GetMethod(
          functionDataType, 
          functionDataGenericType.GetProperty(resultPropertyName).GetGetMethod()), 
        null);
      il.Emit(OpCodes.Ret);
      // "}"
    }


    // Cache

    private static GeneratedTupleDescriptor TryCreateCached()
    {
      if (sampleDescriptor.Count==0)
        return EmptyTupleDescriptor.Instance;
      
      Type genericType;
      if (!cachedTypes.TryGetValue(genericTypeName, out genericType))
        return null;
      
      Type[] genericArgs = sampleDescriptor.ToArray();
      
      Type descriptorType = genericType.MakeGenericType(genericArgs);
      ConstructorInfo descriptorTypeCtor = descriptorType.GetConstructor(
        BindingFlags.Instance | 
        BindingFlags.NonPublic |
        BindingFlags.ExactBinding, 
        null, ctorArgTypes, null);

      object descriptor = descriptorTypeCtor.Invoke(
        BindingFlags.CreateInstance, null, new object[] {sampleDescriptor}, null);
      return (GeneratedTupleDescriptor) descriptor;
    }


    // Static constructor

    static TupleDescriptorGenerator()
    {
      assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
        assemblyName, AssemblyBuilderAccess.Run);
    }
  }
}