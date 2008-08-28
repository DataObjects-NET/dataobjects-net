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
    // Consts
    private const string descriptorFieldName = "descriptor";
    private const string descriptorPropertyName = "Descriptor";
    private const string countPropertyName = "Count";
    private const string valuePropertyName = "Value";
    private const string getValueOrDefaultMethodName = "GetValueOrDefault";
    private const string getValueOrDefaultGenericMethodName = "GetValueOrDefault";
    private const string hasValuePropertyName = "HasValue";
    private const string setValueMethodName = "SetValue";
    private const string getFieldStateMethodName = "GetFieldState";
    private const string setFieldStateMathodName = "SetFieldState";
    private const string createNewMethodName = "CreateNew";
    private const string cloneMethodName = "Clone";

    private readonly static AssemblyName assemblyName = new AssemblyName("Xtensive.GeneratedTuples");
    private readonly static MethodInfo getFlagsMethod = typeof(Tuple).GetMethod("GetFieldState");
    private readonly static MethodInfo setFlagsMethod = typeof(GeneratedTuple).GetMethod("SetFieldState", 
      BindingFlags.Instance | 
      BindingFlags.NonPublic | 
      BindingFlags.ExactBinding);
    private readonly static MethodInfo getValueOrDefaultMethod;
    private readonly static MethodInfo getValueOrDefaultGenericMethod;
    private readonly static MethodInfo setValueMethod;
    // Static members
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
      AddGetValueOrDefault();
      // Do not uncomment this line. It expects abstract GetValueOrDefault<T> method in Tuple class.
//      AddGetValueOrDefaultGeneric();
      AddSetValue();
      foreach (KeyValuePair<Type, TupleInterfaceInfo> keyValuePair in tupleInfo.Interfaces) {
        tupleType.AddInterfaceImplementation(keyValuePair.Value.InterfaceType);
        AddGetValueOrDefault(keyValuePair.Value);
        AddSetValue(keyValuePair.Value);
      }

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

      FieldInfo descriptorField = tuple.GetType().GetField(descriptorFieldName, 
        BindingFlags.Static | 
        BindingFlags.NonPublic);
      descriptorField.SetValue(null, tupleInfo.Descriptor);
      return tuple;
    }

    private void AddGetValueOrDefaultGeneric()
    {
      MethodBuilder getValueOrDefault = tupleType.DefineMethod(
          getValueOrDefaultGenericMethodName,
          MethodAttributes.Public |
          MethodAttributes.Virtual
          );
      GenericTypeParameterBuilder[] genericParameters = getValueOrDefault.DefineGenericParameters("T");
      getValueOrDefault.SetReturnType(genericParameters[0]);
      getValueOrDefault.SetParameters(new Type[]{typeof(int)});

      ILGenerator il = getValueOrDefault.GetILGenerator();
      
      Dictionary<Type, Pair<Type,LocalBuilder>> localsList = new Dictionary<Type, Pair<Type, LocalBuilder>>();
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
                      il.Emit(OpCodes.Ldc_I4, (int)TupleFieldState.IsAvailable);
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
            il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);
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

    private void AddStaticFields()
    {
      // "private readonly static TDescriptor descriptor;"
      descriptorField = tupleType.DefineField(
        descriptorFieldName,
        tDescriptorParameter,
        FieldAttributes.Private | 
        FieldAttributes.Static |
        FieldAttributes.InitOnly);
    }

    private void AddDescriptorProperty()
    {
      // "public override TDescriptor Descriptor {"
      MethodBuilder getDescriptorMethod = tupleType.DefineMethod(
        WellKnown.GetterPrefix+descriptorPropertyName,
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

      tupleType.DefineMethodOverride(getDescriptorMethod,
        typeof(Tuple).GetMethod(WellKnown.GetterPrefix+descriptorPropertyName));
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
        tupleType.DefineMethod(WellKnown.GetterPrefix+countPropertyName,
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
      MethodBuilder getFlags = tupleType.DefineMethod(getFieldStateMethodName,
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
      MethodBuilder setFlags = tupleType.DefineMethod(setFieldStateMathodName,
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

      MethodBuilder methodBuilder = tupleType.DefineMethod(createNewMethodName,
        MethodAttributes.Public |
        MethodAttributes.Virtual,
        typeof(Tuple),
        Type.EmptyTypes);

      il = methodBuilder.GetILGenerator();
      il.Emit(OpCodes.Newobj, constructorBuilder);
      il.Emit(OpCodes.Ret);

      tupleType.DefineMethodOverride(methodBuilder,
        typeof(Tuple).GetMethod(createNewMethodName));

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
          cloneMethodName,
          MethodAttributes.Public | MethodAttributes.Virtual,
          typeof(Tuple),
          Type.EmptyTypes);

      ILGenerator il = clone.GetILGenerator();

      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Newobj, copyingCtor);
      il.Emit(OpCodes.Ret);

      MethodInfo cloneMethod = typeof(Tuple).GetMethod(cloneMethodName);
      tupleType.DefineMethodOverride(clone, cloneMethod);
    }

    private void AddSetValue(TupleInterfaceInfo interfaceInfo)
    {
      MethodInfo interfaceMethod = interfaceInfo.InterfaceType.GetMethod(setValueMethodName);
      MethodBuilder setValue = tupleType.DefineMethod(
          setValueMethodName,
          MethodAttributes.Private |
          MethodAttributes.Virtual |
          MethodAttributes.HideBySig |
          MethodAttributes.NewSlot |
          MethodAttributes.Final,
          null,
          new Type[] {typeof(int), interfaceInfo.InterfaceType.GetGenericArguments()[0]});

      ILGenerator il = setValue.GetILGenerator();
      Label invalidCastException = il.DefineLabel();
      Label[] switchLabels = new Label[tupleInfo.Fields.Count];
      for (int fieldIndex = 0; fieldIndex < tupleInfo.Fields.Count; fieldIndex++) {
        if (tupleInfo.Fields[fieldIndex].Type == interfaceInfo.FieldType)
          switchLabels[fieldIndex] = il.DefineLabel();
        else
          switchLabels[fieldIndex] = invalidCastException;
      }


      Type nullable = null;
      il.DeclareLocal(typeof(TupleFieldState));
      if (interfaceInfo.IsForNullableType) {
        il.DeclareLocal(interfaceInfo.FieldType);
        nullable = typeof(Nullable<>).MakeGenericType(interfaceInfo.FieldType);
      }
      il.Emit(OpCodes.Ldarg_1);
      Action<int, bool> action =
        delegate(int fieldIndex, bool isDefault) {
          if (!isDefault) {
            TupleFieldInfo field = tupleInfo.Fields[fieldIndex];
            if (field.Type == interfaceInfo.FieldType) {
              il.MarkLabel(switchLabels[fieldIndex]);
              Label setFlags = il.DefineLabel();
              if (interfaceInfo.IsForNullableType) {
                Label isNotNull = il.DefineLabel();
                il.Emit(OpCodes.Ldarga, 2);
                il.Emit(OpCodes.Call, nullable.GetMethod(WellKnown.GetterPrefix+hasValuePropertyName));
                il.Emit(OpCodes.Brtrue, isNotNull);
                il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable | TupleFieldState.IsNull));
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Br, setFlags);
                il.MarkLabel(isNotNull);
              }
              else if (!interfaceInfo.IsForValueType) {
                Label isNotNull = il.DefineLabel();
                Label isNull = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brfalse, isNotNull);
                il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable | TupleFieldState.IsNull));
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Br, isNull);
                il.MarkLabel(isNotNull);
                il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable));
                il.Emit(OpCodes.Stloc_0);
                il.MarkLabel(isNull);
              }
              else {
                il.Emit(OpCodes.Ldc_I4, (int) (TupleFieldState.IsAvailable));
                il.Emit(OpCodes.Stloc_0);
              }
              if (interfaceInfo.IsForNullableType) {
                il.Emit(OpCodes.Ldarga, 2);
                il.Emit(OpCodes.Call, nullable.GetMethod(WellKnown.GetterPrefix+valuePropertyName));
                il.Emit(OpCodes.Stloc_1);
                InlineSetField(il, field, OpCodes.Ldloc_1, false);
              }
              else
                InlineSetField(il, field, OpCodes.Ldarg_2, false);
              il.MarkLabel(setFlags);
              InlineSetField(il, field.FlagsField, OpCodes.Ldloc_0, false);
              il.Emit(OpCodes.Ret);
            }
          }
          else {
            il.Emit(OpCodes.Ldstr, "fieldIndex");
            il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);
          }
        };
      EmitHelper.EmitSwitch(il, switchLabels, false, action);
      il.MarkLabel(invalidCastException);
      il.Emit(OpCodes.Newobj, typeof(InvalidCastException).GetConstructor(Type.EmptyTypes));
      il.Emit(OpCodes.Throw);
      tupleType.DefineMethodOverride(setValue, interfaceMethod);
    }

    private void  AddGetValueOrDefault(TupleInterfaceInfo interfaceInfo)
    {
      MethodInfo interfaceMethod = interfaceInfo.InterfaceType.GetMethod(getValueOrDefaultMethodName);
      MethodBuilder getValueOrDefault = tupleType.DefineMethod(
          getValueOrDefaultMethodName,
          MethodAttributes.Private | 
          MethodAttributes.Virtual | 
          MethodAttributes.HideBySig | 
          MethodAttributes.NewSlot | 
          MethodAttributes.Final,
          interfaceMethod.ReturnType,
          new Type[] { typeof(int) });

      ILGenerator il = getValueOrDefault.GetILGenerator();
      Label invalidCastException = il.DefineLabel();
      Label[] switchLabels = new Label[tupleInfo.Fields.Count];
      for (int fieldIndex = 0; fieldIndex < tupleInfo.Fields.Count; fieldIndex++)
      {
        if (tupleInfo.Fields[fieldIndex].Type == interfaceInfo.FieldType)
          switchLabels[fieldIndex] = il.DefineLabel();
        else
          switchLabels[fieldIndex] = invalidCastException;
      }

      Type nullable = null;
      if (interfaceInfo.IsForNullableType)
        nullable = typeof(Nullable<>).MakeGenericType(interfaceInfo.FieldType);

      
      Label returnNull = il.DefineLabel();
      LocalBuilder localBuilder = null;
      il.Emit(OpCodes.Ldarg_1);
      Action<int, bool> action =
        delegate(int fieldIndex, bool isDefault) {
          if (!isDefault) {
            TupleFieldInfo field = tupleInfo.Fields[fieldIndex];
            if (field.Type == interfaceInfo.FieldType) {
              il.MarkLabel(switchLabels[fieldIndex]);
              if (interfaceInfo.IsForNullableType) {
                if (localBuilder == null)
                  localBuilder = il.DeclareLocal(nullable);
                InlineGetField(il, field.FlagsField);
                il.Emit(OpCodes.Ldc_I4, (int)TupleFieldState.IsAvailable);
                il.Emit(OpCodes.Xor);
                il.Emit(OpCodes.Brtrue, returnNull);
                InlineGetField(il, field);
                il.Emit(OpCodes.Newobj, nullable.GetConstructor(nullable.GetGenericArguments()));
                il.Emit(OpCodes.Ret);
              }
              else {
                InlineGetField(il, field);
                il.Emit(OpCodes.Ret);
              }
            }
          }
          else {
            il.Emit(OpCodes.Ldstr, "fieldIndex");
            il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);
          }
        };
      EmitHelper.EmitSwitch(il, switchLabels, false, action);
      il.MarkLabel(invalidCastException);
      il.Emit(OpCodes.Newobj, typeof(InvalidCastException).GetConstructor(Type.EmptyTypes));
      il.Emit(OpCodes.Throw);
      il.MarkLabel(returnNull);
      if (interfaceInfo.IsForNullableType) {
        il.Emit(OpCodes.Ldloca, localBuilder);
        il.Emit(OpCodes.Initobj, nullable);
        il.Emit(OpCodes.Ldloc, localBuilder);
        il.Emit(OpCodes.Ret);
      }
      
      tupleType.DefineMethodOverride(getValueOrDefault, interfaceMethod);
    }

    private void AddSetValue()
    {
      MethodBuilder setValue = tupleType.DefineMethod(
          setValueMethodName,
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
              il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable | TupleFieldState.IsNull));
              il.Emit(OpCodes.Stloc_0);
              il.Emit(OpCodes.Br, setFlags);
            }
            else {
              il.Emit(OpCodes.Ldarg_2);
              il.Emit(OpCodes.Ldnull);
              il.Emit(OpCodes.Ceq);
              il.Emit(OpCodes.Brfalse, isNotNull);
              il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable | TupleFieldState.IsNull));
              il.Emit(OpCodes.Stloc_0);
              il.Emit(OpCodes.Br, isNull);
            }
            il.MarkLabel(isNotNull);
            il.Emit(OpCodes.Ldc_I4, (int)(TupleFieldState.IsAvailable));
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

    private void AddGetValueOrDefault()
    {
      MethodBuilder getValueOrDefault = tupleType.DefineMethod(
          getValueOrDefaultMethodName,
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
            il.Emit(OpCodes.Ldc_I4, (int)TupleFieldState.IsAvailable);
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
        if (info.Name==getValueOrDefaultMethodName) {
          if (!info.ReturnType.IsGenericParameter)
            getValueOrDefaultMethod = info;
        }
        else if (info.Name == setValueMethodName) {
          if (!info.ReturnType.IsGenericParameter) {
            setValueMethod = info;
          }
        }
        if (info.Name == getValueOrDefaultGenericMethodName && !info.ReturnType.IsGenericParameter)
          getValueOrDefaultGenericMethod = info;
      }
    }
  }
}
