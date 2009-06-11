// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using FieldInfo=System.Reflection.FieldInfo;
using MethodInfo=System.Reflection.MethodInfo;

namespace Xtensive.Core.Tuples.Internals
{
  internal class TupleGenerator
  {
    // Static members
    private readonly static MethodInfo referenceEqualsMethod = typeof(object).GetMethod("ReferenceEquals");
    private readonly static MethodInfo staticEqualsMethod = typeof(object).GetMethod("Equals", new Type[]{typeof(object), typeof(object)});
    private readonly static MethodInfo objectGetHashCodeMethod = typeof(object).GetMethod("GetHashCode", ArrayUtils<Type>.EmptyArray);
    private readonly static AssemblyName assemblyName = new AssemblyName("Xtensive.GeneratedTuples");
    private readonly static MethodInfo getFlagsMethod = typeof(Tuple).GetMethod("GetFieldState");
    private readonly static MethodInfo getHashCodeMethod = typeof(Tuple).GetMethod("GetHashCode");
    private static readonly MethodInfo getDescriptorMethod = typeof(Tuple).GetMethod(WellKnown.GetterPrefix + WellKnown.Tuple.Descriptor);
    private readonly static MethodInfo setFlagsMethod = typeof(GeneratedTuple).GetMethod("SetFieldState", 
      BindingFlags.Instance | 
      BindingFlags.NonPublic | 
      BindingFlags.ExactBinding);
    private readonly static MethodInfo getValueOrDefaultMethod;
    private readonly static MethodInfo getValueOrDefaultGenericMethod;
    private readonly static MethodInfo setValueMethod;
    private readonly static MethodInfo setValueGenericMethod;
    private readonly static MethodInfo equalsMethod;
    private readonly static AssemblyBuilder assemblyBuilder;
    private readonly static ModuleBuilder moduleBuilder;
    private static volatile bool assemblyIsSaved;
    // Non-static members
    private TupleInfo tupleInfo;
    private TypeBuilder tupleType;
    private GenericTypeParameterBuilder tDescriptorParameter;
    private FieldBuilder descriptorField;
    private MethodBuilder getCountMethod;
    private ConstructorBuilder copyingCtor;
   

    public Tuple Compile(TupleInfo tupleInfo)
    {
      this.tupleInfo = tupleInfo;
      Debug.Assert(tupleInfo.Descriptor.Count==tupleInfo.Fields.Count);

      // "public sealed class GeneratedTuple[N]<TDescriptor>: GeneratedTuple"
      tupleType = moduleBuilder.DefineType(
        tupleInfo.Name,
        TypeAttributes.Public | TypeAttributes.Sealed,
        typeof (GeneratedTuple));
      GenericTypeParameterBuilder[] genericParameters = tupleType.DefineGenericParameters(new string[] {"TDescriptor"});
      tDescriptorParameter = genericParameters[0];
      
      // "where TDescriptor: GeneratedTupleDescriptor"
      tDescriptorParameter.SetBaseTypeConstraint(typeof(GeneratedTupleDescriptor));

      // "[Serializable]"
      CustomAttributeBuilder serializableAttribute = new CustomAttributeBuilder(
        typeof(SerializableAttribute).GetConstructor(ArrayUtils<Type>.EmptyArray),
        ArrayUtils<object>.EmptyArray);
      tupleType.SetCustomAttribute(serializableAttribute);

      AddStaticFields();
      AddDescriptorProperty();
      AddFields();
      AddCountProperty();
      AddGetFlags();
      AddSetFlags();
      AddCtors();
      AddClone();
      AddEquals();
      AddGetHashCode();
      AddGetValueOrDefault();
//      AddGetValueOrDefaultGeneric();
      AddSetValue();

      Tuple tuple = (Tuple)tupleType.CreateType().Activate(
        new Type[] {tupleInfo.Descriptor.GetType()}, 
        ArrayUtils<object>.EmptyArray);
      // For reverse-engineering of generated code only
      AppDomain.CurrentDomain.DomainUnload += 
        delegate(object sender, EventArgs eventArgs) {
          if (!assemblyIsSaved) {
            lock (assemblyBuilder) {
              if (!assemblyIsSaved)
                assemblyBuilder.Save(assemblyName.Name + ".dll");
              assemblyIsSaved = true;
            }
          }
        };
      if (tuple==null)
        throw Exceptions.InternalError(string.Format(
          "Tuple generation has failed for tuple descriptor {0}.", tupleInfo.Descriptor), Log.Instance);

      FieldInfo descriptorField = tuple.GetType().GetField(WellKnown.Tuple.DescriptorFieldName, 
        BindingFlags.Static | 
        BindingFlags.NonPublic);
      descriptorField.SetValue(null, tupleInfo.Descriptor);
      return tuple;
    }

    private void AddGetHashCode()
    {
      MethodBuilder getHashCode = tupleType.DefineMethod(
         WellKnown.Object.GetHashCode,
         MethodAttributes.Public |
         MethodAttributes.Virtual,
         typeof(int),
         ArrayUtils<Type>.EmptyArray);

      ILGenerator il = getHashCode.GetILGenerator();
      var result = il.DeclareLocal(typeof (int));
      foreach (var fieldInfo in tupleInfo.Fields) {
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ldc_I4, Tuple.HashCodeMultiplier);
        il.Emit(OpCodes.Mul);
        if (fieldInfo.IsCompressed) {
          InlineGetField(il, fieldInfo);
          il.Emit(OpCodes.Xor);
        }
        else if (fieldInfo.IsValueType) {
          il.Emit(OpCodes.Ldarg_0);
          il.Emit(OpCodes.Ldflda, fieldInfo.FieldBuilder);
          var structGetHashCode = fieldInfo.Type.GetMethod(WellKnown.Object.GetHashCode);
          il.Emit(OpCodes.Call, structGetHashCode);
          il.Emit(OpCodes.Xor);
        }
        else {
          var skip = il.DefineLabel();
          il.Emit(OpCodes.Ldarg_0);
          il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
          il.Emit(OpCodes.Brfalse_S, skip);
          il.Emit(OpCodes.Ldarg_0);
          il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
          il.Emit(OpCodes.Callvirt, objectGetHashCodeMethod);
          il.Emit(OpCodes.Xor);
          il.MarkLabel(skip);
        }
        il.Emit(OpCodes.Stloc, result);
      }
      il.Emit(OpCodes.Ldloc, result);
      il.Emit(OpCodes.Ret);
      tupleType.DefineMethodOverride(getHashCode, getHashCodeMethod);
    }

    private void AddEquals()
    {
      MethodBuilder equals = tupleType.DefineMethod(
         WellKnown.Object.Equals,
         MethodAttributes.Public |
         MethodAttributes.Virtual, 
         typeof(bool),
         new Type[]{typeof(Tuple)});

      ILGenerator il = equals.GetILGenerator();
      Label returnFalse = il.DefineLabel();
      Label compareDescriptor = il.DefineLabel();
      Label isSameType = il.DefineLabel();
      Label compareDefault = il.DefineLabel();
      var sameType = il.DeclareLocal(tupleType);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Ldnull);
      il.Emit(OpCodes.Ceq);
      il.Emit(OpCodes.Brtrue, returnFalse);
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Ceq);
      il.Emit(OpCodes.Brfalse, compareDescriptor);
      il.Emit(OpCodes.Ldc_I4_1);
      il.Emit(OpCodes.Ret);
      il.MarkLabel(compareDescriptor);
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Callvirt, getDescriptorMethod);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Callvirt, getDescriptorMethod);
      il.Emit(OpCodes.Ceq);
      il.Emit(OpCodes.Brtrue, isSameType);
      il.Emit(OpCodes.Ldc_I4_0);
      il.Emit(OpCodes.Ret);
      il.MarkLabel(isSameType);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Isinst, tupleType);
      il.Emit(OpCodes.Stloc, sameType);
      il.Emit(OpCodes.Ldloc, sameType);
      il.Emit(OpCodes.Ldnull);
      il.Emit(OpCodes.Ceq);
      il.Emit(OpCodes.Brtrue, compareDefault);
      foreach (var fieldInfo in tupleInfo.ActualCompressingFields) {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
        il.Emit(OpCodes.Ceq);
        il.Emit(OpCodes.Brfalse, returnFalse);
      }
      foreach (var fieldInfo in tupleInfo.ActualFields) {
        if (fieldInfo.IsValueType) {
          il.Emit(OpCodes.Ldarg_0);
          il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
          il.Emit(OpCodes.Ceq);
          il.Emit(OpCodes.Brfalse, returnFalse);
        }
        else {
          il.Emit(OpCodes.Ldarg_0);
          il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldfld, fieldInfo.FieldBuilder);
          il.Emit(OpCodes.Call, staticEqualsMethod);
          il.Emit(OpCodes.Brfalse, returnFalse);
        }
      }
      il.Emit(OpCodes.Ldc_I4_1);
      il.Emit(OpCodes.Ret);
      il.MarkLabel(compareDefault);
      for (int fieldIndex = 0; fieldIndex < tupleInfo.Fields.Count; fieldIndex++) {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, fieldIndex);
        il.Emit(OpCodes.Callvirt, getFlagsMethod);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4, fieldIndex);
        il.Emit(OpCodes.Callvirt, getFlagsMethod);
        il.Emit(OpCodes.Ceq);
        il.Emit(OpCodes.Brfalse, returnFalse);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, fieldIndex);
        il.Emit(OpCodes.Callvirt, getValueOrDefaultMethod);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4, fieldIndex);
        il.Emit(OpCodes.Callvirt, getValueOrDefaultMethod);
        il.Emit(OpCodes.Call, staticEqualsMethod);
        il.Emit(OpCodes.Brfalse, returnFalse);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Ret);
      }

      il.MarkLabel(returnFalse);
      il.Emit(OpCodes.Ldc_I4_0);
      il.Emit(OpCodes.Ret);
      tupleType.DefineMethodOverride(equals, equalsMethod);
    }

    private void AddGetValueOrDefaultGeneric()
    {
      MethodBuilder getValueOrDefault = tupleType.DefineMethod(
          WellKnown.Tuple.GetValueOrDefault,
          MethodAttributes.Public |
          MethodAttributes.Virtual
          );
      GenericTypeParameterBuilder[] genericParameters = getValueOrDefault.DefineGenericParameters("T");
      getValueOrDefault.SetReturnType(genericParameters[0]);
      getValueOrDefault.SetParameters(new Type[]{typeof(int)});

      ILGenerator il = getValueOrDefault.GetILGenerator();
      
      var localsList = new Dictionary<Type, Pair<Type, LocalBuilder>>();
      foreach (TupleFieldInfo fieldInfo in tupleInfo.Fields) {
        if (fieldInfo.IsValueType) {
          if (!localsList.ContainsKey(fieldInfo.Type)) {
            Type nullable = typeof (Nullable<>).MakeGenericType(fieldInfo.Type);
            Pair<Type, LocalBuilder> pair = new Pair<Type, LocalBuilder>(nullable, il.DeclareLocal(nullable));
            localsList.Add(fieldInfo.Type, pair);
          }
        }
      }

      Action<int, bool> action =
        delegate(int fieldIndex, bool isDefault) {
          if (!isDefault) {
            TupleFieldInfo field = tupleInfo.Fields[fieldIndex];
            il.Emit(OpCodes.Ldtoken, genericParameters[0]);
            il.Emit(OpCodes.Ldtoken, field.Type);
            il.Emit(OpCodes.Ceq);
            EmitHelper.EmitIfElse(il, OpCodes.Brtrue, 
              delegate {
                if (field.Type.IsValueType) {
                  il.Emit(OpCodes.Ldtoken, genericParameters[0]);
                  il.Emit(OpCodes.Ldtoken, localsList[field.Type].First);
                  il.Emit(OpCodes.Ceq);
                  EmitHelper.EmitIfElse(il, OpCodes.Brtrue,
                    delegate {
                      il.Emit(OpCodes.Newobj, typeof(InvalidCastException).GetConstructor(Type.EmptyTypes));
                      il.Emit(OpCodes.Throw);
                    },
                    delegate {
                      InlineGetField(il, field.FlagsField);
                      il.Emit(OpCodes.Ldc_I4, (int)TupleFieldState.Available);
                      il.Emit(OpCodes.Xor);
                      EmitHelper.EmitIfElse(il, OpCodes.Brtrue, 
                        delegate {
                          InlineGetField(il, field);
                          il.Emit(OpCodes.Newobj, localsList[field.Type].First.GetConstructor(new Type[] { field.Type }));
                          il.Emit(OpCodes.Ret);
                        },
                        delegate {
                          il.Emit(OpCodes.Ldloca, localsList[field.Type].Second);
                          il.Emit(OpCodes.Initobj, localsList[field.Type].First);
                          il.Emit(OpCodes.Ldloc, localsList[field.Type].Second);
                          il.Emit(OpCodes.Ret);
                        });
                    });
                }
                else {
                  il.Emit(OpCodes.Newobj, typeof(InvalidCastException).GetConstructor(Type.EmptyTypes));
                  il.Emit(OpCodes.Throw);
                }
              },
              delegate {
                InlineGetField(il, field);
                il.Emit(OpCodes.Ret);
              });
          }
          else {
            il.Emit(OpCodes.Ldstr, "fieldIndex");
            il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);
          }
        };
      il.Emit(OpCodes.Ldarg_1);
      EmitHelper.EmitSwitch(il, tupleInfo.Fields.Count, false, action);
      il.Emit(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes));
      il.Emit(OpCodes.Throw);
      tupleType.DefineMethodOverride(getValueOrDefault, getValueOrDefaultGenericMethod);
    }

    private void AddGetValueOrDefault()
    {
      MethodBuilder getValueOrDefault = tupleType.DefineMethod(
        WellKnown.Tuple.GetValueOrDefault,
        MethodAttributes.Public | MethodAttributes.Virtual,
        typeof(object),
        new Type[]{typeof(int)});

      ILGenerator il = getValueOrDefault.GetILGenerator();

      Label argumentOutOfRangeException = il.DefineLabel();
      Label returnNull = il.DefineLabel();
      il.Emit(OpCodes.Ldarg_1);
      Action<int, bool> getFlagsAction =
        delegate(int fieldIndex, bool isDefault) {
          if (!isDefault) {
            TupleFieldInfo field = tupleInfo.Fields[fieldIndex];
            InlineGetField(il, field.FlagsField);
            il.Emit(OpCodes.Ldc_I4, (int)TupleFieldState.Available);
            il.Emit(OpCodes.Xor);
            il.Emit(OpCodes.Brtrue, returnNull);
            InlineGetField(il, field);
            if (field.IsValueType)
              il.Emit(OpCodes.Box, field.Type);
            il.Emit(OpCodes.Ret);
          }
          else
            il.Emit(OpCodes.Br, argumentOutOfRangeException);
        };
      EmitHelper.EmitSwitch(il, tupleInfo.Fields.Count, false, getFlagsAction);
      il.MarkLabel(argumentOutOfRangeException);
      il.Emit(OpCodes.Ldstr, "fieldIndex");
      il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
      il.Emit(OpCodes.Throw);
      il.MarkLabel(returnNull);

      il.Emit(OpCodes.Ldnull);
      il.Emit(OpCodes.Ret);
      tupleType.DefineMethodOverride(getValueOrDefault, getValueOrDefaultMethod);
    }

    private void AddSetValueGeneric()
    {
      MethodBuilder setValue = tupleType.DefineMethod(
          WellKnown.Tuple.SetValue,
          MethodAttributes.Public |
          MethodAttributes.Virtual
          );
      GenericTypeParameterBuilder[] genericParameters = setValue.DefineGenericParameters("T");
      setValue.SetReturnType(null);
      setValue.SetParameters(new Type[] { typeof(int), genericParameters[0] });

      ILGenerator il = setValue.GetILGenerator();

//      MethodInfo interfaceMethod = interfaceInfo.InterfaceType.GetMethod(WellKnown.Tuple.SetValue);
//      MethodBuilder setValue = tupleType.DefineMethod(
//          WellKnown.Tuple.SetValue,
//          MethodAttributes.Private |
//          MethodAttributes.Virtual |
//          MethodAttributes.Final,
//          null,
//          new Type[] {typeof(int), interfaceInfo.InterfaceType.GetGenericArguments()[0]});
//
//      ILGenerator il = setValue.GetILGenerator();
//      Label invalidCastException = il.DefineLabel();
//      Label[] switchLabels = new Label[tupleInfo.Fields.Count];
//      for (int fieldIndex = 0; fieldIndex < tupleInfo.Fields.Count; fieldIndex++) {
//        if (tupleInfo.Fields[fieldIndex].Type == interfaceInfo.FieldType)
//          switchLabels[fieldIndex] = il.DefineLabel();
//        else
//          switchLabels[fieldIndex] = invalidCastException;
//      }
//
//
//      Type nullable = null;
//      il.DeclareLocal(typeof(TupleFieldState));
//      if (interfaceInfo.IsForNullableType) {
//        il.DeclareLocal(interfaceInfo.FieldType);
//        nullable = typeof(Nullable<>).MakeGenericType(interfaceInfo.FieldType);
//      }
//      il.Emit(OpCodes.Ldarg_1);
//      Action<int, bool> action =
//        delegate(int fieldIndex, bool isDefault) {
//          if (!isDefault) {
//            TupleFieldInfo field = tupleInfo.Fields[fieldIndex];
//            if (field.Type == interfaceInfo.FieldType) {
//              il.MarkLabel(switchLabels[fieldIndex]);
//              Label setFlags = il.DefineLabel();
//              if (interfaceInfo.IsForNullableType) {
//                Label isNotNull = il.DefineLabel();
//                il.Emit(OpCodes.Ldarga, 2);
//                il.Emit(OpCodes.Call, nullable.GetMethod(WellKnown.GetterPrefix+hasValuePropertyName));
//                il.Emit(OpCodes.Brtrue, isNotNull);
//                il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable | TupleFieldState.IsNull));
//                il.Emit(OpCodes.Stloc_0);
//                il.Emit(OpCodes.Br, setFlags);
//                il.MarkLabel(isNotNull);
//                il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable));
//                il.Emit(OpCodes.Stloc_0);
//              }
//              else if (!interfaceInfo.IsForValueType) {
//                Label isNotNull = il.DefineLabel();
//                Label isNull = il.DefineLabel();
//                il.Emit(OpCodes.Ldarg_2);
//                il.Emit(OpCodes.Ldnull);
//                il.Emit(OpCodes.Ceq);
//                il.Emit(OpCodes.Brfalse, isNotNull);
//                il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable | TupleFieldState.IsNull));
//                il.Emit(OpCodes.Stloc_0);
//                il.Emit(OpCodes.Br, isNull);
//                il.MarkLabel(isNotNull);
//                il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable));
//                il.Emit(OpCodes.Stloc_0);
//                il.MarkLabel(isNull);
//              }
//              else {
//                il.Emit(OpCodes.Ldc_I4, (int) (TupleFieldState.IsAvailable));
//                il.Emit(OpCodes.Stloc_0);
//              }
//              if (interfaceInfo.IsForNullableType) {
//                il.Emit(OpCodes.Ldarga, 2);
//                il.Emit(OpCodes.Call, nullable.GetMethod(WellKnown.GetterPrefix+valuePropertyName));
//                il.Emit(OpCodes.Stloc_1);
//                InlineSetField(il, field, OpCodes.Ldloc_1, false);
//              }
//              else
//                InlineSetField(il, field, OpCodes.Ldarg_2, false);
//              il.MarkLabel(setFlags);
//              InlineSetField(il, field.FlagsField, OpCodes.Ldloc_0, false);
//              il.Emit(OpCodes.Ret);
//            }
//          }
//          else {
//            il.Emit(OpCodes.Ldstr, "fieldIndex");
//            il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
//            il.Emit(OpCodes.Throw);
//          }
//        };
//      EmitHelper.EmitSwitch(il, switchLabels, false, action);
//      il.MarkLabel(invalidCastException);
//      il.Emit(OpCodes.Newobj, typeof(InvalidCastException).GetConstructor(Type.EmptyTypes));
//      il.Emit(OpCodes.Throw);
//      tupleType.DefineMethodOverride(setValue, interfaceMethod);
    }

    private void AddSetValue()
    {
      MethodBuilder setValue = tupleType.DefineMethod(
        WellKnown.Tuple.SetValue,
        MethodAttributes.Public | MethodAttributes.Virtual,
        null,
        new Type[] { typeof(int), typeof(object) });

      ILGenerator il = setValue.GetILGenerator();
      il.DeclareLocal(typeof (TupleFieldState));
      il.Emit(OpCodes.Ldarg_1);
      Action<int, bool> setValueAction =
        delegate(int fieldIndex, bool isDefault) {
          if (!isDefault) {
            Label setFlags = il.DefineLabel();
            TupleFieldInfo field = tupleInfo.Fields[fieldIndex];
            Label isNotNull = il.DefineLabel();
            Label isNull = il.DefineLabel();
            if (field.IsValueType) {
              il.Emit(OpCodes.Ldarg_2);
              il.Emit(OpCodes.Ldnull);
              il.Emit(OpCodes.Ceq);
              il.Emit(OpCodes.Brfalse, isNotNull);
              il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.Available | TupleFieldState.Null));
              il.Emit(OpCodes.Stloc_0);
              il.Emit(OpCodes.Br, setFlags);
            }
            else {
              il.Emit(OpCodes.Ldarg_2);
              il.Emit(OpCodes.Ldnull);
              il.Emit(OpCodes.Ceq);
              il.Emit(OpCodes.Brfalse, isNotNull);
              il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.Available | TupleFieldState.Null));
              il.Emit(OpCodes.Stloc_0);
              il.Emit(OpCodes.Br, isNull);
            }
            il.MarkLabel(isNotNull);
            il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.Available));
            il.Emit(OpCodes.Stloc_0);
            il.MarkLabel(isNull);
            InlineSetField(il, field, OpCodes.Ldarg_2, true);
            il.MarkLabel(setFlags);
            InlineSetField(il, field.FlagsField, OpCodes.Ldloc_0, false);
            il.Emit(OpCodes.Ret);
          }
        };
      EmitHelper.EmitSwitch(il, tupleInfo.Fields.Count, false, setValueAction);
      il.Emit(OpCodes.Ldstr, "fieldIndex");
      il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
      il.Emit(OpCodes.Throw);
      tupleType.DefineMethodOverride(setValue, setValueMethod);
    }

    private void AddStaticFields()
    {
      // "private readonly static TDescriptor descriptor;"
      descriptorField = tupleType.DefineField(
        WellKnown.Tuple.DescriptorFieldName,
        tDescriptorParameter,
        FieldAttributes.Private | 
        FieldAttributes.Static |
        FieldAttributes.InitOnly);
    }

    private void AddDescriptorProperty()
    {
      // "public override TDescriptor Descriptor {"
      MethodBuilder getDescriptorMethod = tupleType.DefineMethod(
        WellKnown.GetterPrefix+WellKnown.Tuple.Descriptor,
          MethodAttributes.Public |
          MethodAttributes.Virtual |
          MethodAttributes.SpecialName |
          MethodAttributes.HideBySig,
        typeof(TupleDescriptor),
        Type.EmptyTypes);
      ILGenerator il = getDescriptorMethod.GetILGenerator();

      // "  get { return descriptor; }"
      il.Emit(OpCodes.Ldsfld, descriptorField);
      il.Emit(OpCodes.Ret);
      // "}"

      tupleType.DefineMethodOverride(getDescriptorMethod, TupleGenerator.getDescriptorMethod);
    }

    private void AddFields()
    {
      // Defining actual compressing fields:
      // "private [compressingField.Type] [compressingField.Name];"
      foreach (TupleFieldInfo compressingField in tupleInfo.ActualCompressingFields)
        compressingField.FieldBuilder = tupleType.DefineField(
          compressingField.Name,
          compressingField.ActualType,
          FieldAttributes.Private);

      // Defining actual fields:
      // "private [field.Type] [field.Name];"
      foreach (TupleFieldInfo field in tupleInfo.ActualFields)
        field.FieldBuilder = tupleType.DefineField(
          field.Name,
          field.ActualType,
          FieldAttributes.Private);
    }

    private void AddCountProperty()
    {
      // "public override int Count {"
      getCountMethod =
        tupleType.DefineMethod(WellKnown.GetterPrefix+WellKnown.Tuple.Count,
          MethodAttributes.Public |
          MethodAttributes.Virtual |
          MethodAttributes.SpecialName |
          MethodAttributes.HideBySig,
          typeof (int),
          Type.EmptyTypes);
      ILGenerator il = getCountMethod.GetILGenerator();

      // "return [tupleInfo.Descriptor.Count];"
      il.Emit(OpCodes.Ldc_I4, tupleInfo.Descriptor.Count);
      il.Emit(OpCodes.Ret);
      // "}"
    }

    private void AddGetFlags()
    {
      MethodBuilder getFlags = tupleType.DefineMethod(WellKnown.Tuple.GetFieldState,
        MethodAttributes.Public |
        MethodAttributes.Virtual,
        typeof(TupleFieldState),
        new Type[] { typeof(int) });

      ILGenerator il = getFlags.GetILGenerator();
      il.Emit(OpCodes.Ldarg_1);
      Action<int, bool> getFlagsAction = 
        delegate(int fieldIndex, bool isDefault) {
          if (!isDefault) {
            TupleFieldInfo flagsField = tupleInfo.Fields[fieldIndex].FlagsField;
            InlineGetField(il, flagsField);
            il.Emit(OpCodes.Ret);
          }
        };
      EmitHelper.EmitSwitch(il, tupleInfo.Fields.Count, false, getFlagsAction);
      il.Emit(OpCodes.Ldstr, "fieldIndex");
      il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
      il.Emit(OpCodes.Throw);

      tupleType.DefineMethodOverride(getFlags, getFlagsMethod);
    }

    private void AddSetFlags()
    {
      MethodBuilder setFlags = tupleType.DefineMethod(WellKnown.Tuple.SetFieldState,
        MethodAttributes.Public |
        MethodAttributes.Virtual,
        null,
        new Type[] { typeof(int), typeof(TupleFieldState)});

      ILGenerator il = setFlags.GetILGenerator();
      il.Emit(OpCodes.Ldarg_1);
      Action<int, bool> getFlagsAction =
        delegate(int fieldIndex, bool isDefault) {
          if (!isDefault) {
            TupleFieldInfo flagsField = tupleInfo.Fields[fieldIndex].FlagsField;
            InlineSetField(il, flagsField, OpCodes.Ldarg_2, false);
            il.Emit(OpCodes.Ret);
          }
        };
      EmitHelper.EmitSwitch(il, tupleInfo.Fields.Count, false, getFlagsAction);
      il.Emit(OpCodes.Ldstr, "fieldIndex");
      il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
      il.Emit(OpCodes.Throw);

      tupleType.DefineMethodOverride(setFlags, setFlagsMethod);
    }

    private void AddCtors()
    {
      ConstructorBuilder constructorBuilder = tupleType.DefineConstructor(
        MethodAttributes.Public |
        MethodAttributes.HideBySig |
        MethodAttributes.RTSpecialName |
        MethodAttributes.SpecialName,
        CallingConventions.Standard,
        Type.EmptyTypes);

      ConstructorInfo tupleConstructorInfo = tupleType.BaseType.GetConstructor(
        BindingFlags.Instance | 
        BindingFlags.NonPublic, 
        null, Type.EmptyTypes, null);
      
      ILGenerator il = constructorBuilder.GetILGenerator();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Call, tupleConstructorInfo);
      il.Emit(OpCodes.Ret);

      MethodBuilder methodBuilder = tupleType.DefineMethod(WellKnown.Tuple.CreateNew,
        MethodAttributes.Public |
        MethodAttributes.Virtual,
        typeof(Tuple),
        Type.EmptyTypes);

      il = methodBuilder.GetILGenerator();
      il.Emit(OpCodes.Newobj, constructorBuilder);
      il.Emit(OpCodes.Ret);

      tupleType.DefineMethodOverride(methodBuilder,
        typeof(Tuple).GetMethod(WellKnown.Tuple.CreateNew));

      // Copying constructor.
      copyingCtor = tupleType.DefineConstructor(
        MethodAttributes.Private |
        MethodAttributes.HideBySig |
        MethodAttributes.RTSpecialName |
        MethodAttributes.SpecialName,
        CallingConventions.Standard,
        new Type[] { tupleType });

      il = copyingCtor.GetILGenerator();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Call, tupleConstructorInfo);
      foreach (TupleFieldInfo compressingField in tupleInfo.ActualCompressingFields) {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldfld, compressingField.FieldBuilder);
        il.Emit(OpCodes.Stfld, compressingField.FieldBuilder);
      }
      foreach (TupleFieldInfo actualField in tupleInfo.ActualFields) {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldfld, actualField.FieldBuilder);
        il.Emit(OpCodes.Stfld, actualField.FieldBuilder);
      }
      il.Emit(OpCodes.Ret);
    }

    private void AddClone()
    {
      MethodBuilder clone = tupleType.DefineMethod(
          WellKnown.Object.Clone,
          MethodAttributes.Public | MethodAttributes.Virtual,
          typeof(Tuple),
          Type.EmptyTypes);

      ILGenerator il = clone.GetILGenerator();

      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Newobj, copyingCtor);
      il.Emit(OpCodes.Ret);

      MethodInfo cloneMethod = typeof(Tuple).GetMethod(WellKnown.Object.Clone);
      tupleType.DefineMethodOverride(clone, cloneMethod);
    }

    private static void InlineSetField(ILGenerator il, TupleFieldInfo field, OpCode loadValueOpCode, bool unbox)
    {
      // loadValueOpcode - Ldloc or Ldarg
      if (field.IsCompressed) {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field.CompressingFieldInfo.FieldBuilder);
        il.Emit(OpCodes.Ldc_I4, field.InversedBitMask);
        il.Emit(OpCodes.And);
        il.Emit(loadValueOpCode);
        if (field.IsValueType && unbox)
          il.Emit(OpCodes.Unbox_Any, field.Type);
        il.Emit(OpCodes.Ldc_I4, field.ValueBitMask);
        il.Emit(OpCodes.And);
        il.Emit(OpCodes.Ldc_I4, field.BitShift);
        il.Emit(OpCodes.Shl);
        il.Emit(OpCodes.Or);
        il.Emit(OpCodes.Stfld, field.CompressingFieldInfo.FieldBuilder);
      }
      else {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(loadValueOpCode);
        if (field.IsValueType && unbox)
          il.Emit(OpCodes.Unbox_Any, field.Type);
        il.Emit(OpCodes.Stfld, field.FieldBuilder);
      }
    }

    private static void InlineGetField(ILGenerator il, TupleFieldInfo field)
    {
      if (field.IsCompressed) {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field.CompressingFieldInfo.FieldBuilder);
        il.Emit(OpCodes.Ldc_I4, field.BitMask);
        il.Emit(OpCodes.And);
        il.Emit(OpCodes.Ldc_I4, field.BitShift);
        il.Emit(OpCodes.Shr);
      }
      else {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field.FieldBuilder);
      }
    }


    // Constructors

    public TupleGenerator()
    {
    }

    // Static constructor
    
    static TupleGenerator()
    {
      assemblyBuilder =
        AppDomain.CurrentDomain.DefineDynamicAssembly(
          assemblyName,
          AssemblyBuilderAccess.RunAndSave);
      moduleBuilder =
        assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName + ".dll", true);

      Type tupleType = typeof(Tuple);
      foreach (MethodInfo info in tupleType.GetMethods()) {
        if (!info.IsPublic)
          continue;
        if (info.Name==WellKnown.Tuple.GetValueOrDefault && !info.IsGenericMethodDefinition) {
          getValueOrDefaultMethod = info;
          continue;
        }
        if (info.Name==WellKnown.Tuple.SetValue && !info.IsGenericMethodDefinition) {
          setValueMethod = info;
          continue;
        }
        if (info.Name == WellKnown.Tuple.GetValueOrDefault && info.IsGenericMethodDefinition)
          getValueOrDefaultGenericMethod = info;
        if (info.Name == WellKnown.Tuple.SetValue && info.IsGenericMethodDefinition)
          setValueGenericMethod = info;
        if (info.Name == WellKnown.Object.Equals) {
          var types = info.GetParameterTypes();
          if (types[0] == typeof(Tuple))
            equalsMethod = info;
        }
      }
    }
  }
}
