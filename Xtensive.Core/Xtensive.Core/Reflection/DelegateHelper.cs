// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.25

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Reflection
{
  /// <summary>
  /// Delegate helper \ extension methods. 
  /// Simplifies various delegate creation.
  /// </summary>
  public static class DelegateHelper
  {
    // TODO: AY: Cache all generated gelegates by the same way
    private static readonly Dictionary<string, Delegate> cachedDelegates = new Dictionary<string, Delegate>();
    private static readonly Dictionary<Type, OpCode> opCodeConv = new Dictionary<Type, OpCode>();
    private static readonly Dictionary<Type, Type> typeOnStack = new Dictionary<Type, Type>();
    private static readonly string primitiveCastMethodName = "PrimitiveCast";

    /// <summary>
    /// Aspected private field getter prefix.
    /// </summary>
    public static readonly string AspectedPrivateFieldGetterPrefix =
      "~Xtensive.Core.Aspects.ImplementPrivateFieldAccessorAspect~Get_";

    /// <summary>
    /// Aspected private field setter prefix.
    /// </summary>
    public static readonly string AspectedPrivateFieldSetterPrefix =
      "~Xtensive.Core.Aspects.ImplementPrivateFieldAccessorAspect~Set_";


    /// <summary>
    /// Creates get member delegate.
    /// </summary>
    /// <typeparam name="TObject">Declaring type.</typeparam>
    /// <typeparam name="TValue">Member type.</typeparam>
    /// <param name="memberName">Member name.</param>
    /// <returns><see cref="Func{A,R}"/> delegate 
    /// that gets member value.</returns>
    public static Func<TObject, TValue> CreateGetMemberDelegate<TObject, TValue>(string memberName)
    {
      return InnerCreateGetMemberDelegate<TObject, TValue, Func<TObject, TValue>>(memberName);
    }

    /// <summary>
    /// Creates property \ member getter delegate.
    /// </summary>
    /// <typeparam name="TObject">Declaring type.</typeparam>
    /// <typeparam name="TValue">Member type.</typeparam>
    /// <param name="memberName">Member name.</param>
    /// <returns><see cref="Func{A,R}"/> delegate 
    /// that gets member value.</returns>
    public static Converter<TObject, TValue> CreateGetMemberConverterDelegate<TObject, TValue>(string memberName)
    {
      return InnerCreateGetMemberDelegate<TObject, TValue, Converter<TObject, TValue>>(memberName);
    }

    private static TDelegateType InnerCreateGetMemberDelegate<TObject, TValue, TDelegateType>(string memberName)
      where TDelegateType : class
    {
      Type type = typeof (TObject);
      Type tValue = typeof (TValue);
      string methodKey = GetMethodCallDelegateKey((typeof (TDelegateType).FullName + " " + memberName), type, tValue);

      TDelegateType result = GetCachedDelegate(methodKey) as TDelegateType;
      if (result==null)
        lock (cachedDelegates) {
          result = GetCachedDelegate(methodKey) as TDelegateType;
          if (result!=null)
            return result;
          methodKey = String.Intern(methodKey);

          PropertyInfo pi = type.GetProperty(memberName);
          FieldInfo fi = type.GetField(memberName);
          MethodInfo smi;
          if (pi!=null) {
            // Member is a Property...
            MethodInfo mi = pi.GetGetMethod();
            if (mi!=null) {
              //  Calling a property's get accessor is faster/cleaner using
              //  Delegate.CreateDelegate rather than Reflection.Emit 
              result = Delegate.CreateDelegate(typeof (TDelegateType), mi) as TDelegateType;
            }
            else
              throw new InvalidOperationException(String.Format(Strings.ExPropertyDoesNotHaveGetter,
                memberName, type.Name));
          }
          else if (fi!=null) {
            // Member is a Field...
            DynamicMethod dm = new DynamicMethod("Get" + memberName,
              typeof (TValue), new Type[] {type}, type);
            ILGenerator il = dm.GetILGenerator();
            // Load the instance of the object (argument 0) onto the stack
            il.Emit(OpCodes.Ldarg_0);
            // Load the value of the object's field (fi) onto the stack
            il.Emit(OpCodes.Ldfld, fi);
            // return the value on the top of the stack
            il.Emit(OpCodes.Ret);

            result = dm.CreateDelegate(typeof (TDelegateType)) as TDelegateType;
          }
          else if (null!=(smi = type.GetMethod(AspectedPrivateFieldGetterPrefix + memberName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))) {
            result = Delegate.CreateDelegate(typeof (Func<TObject, TValue>), smi) as TDelegateType;
          }
          else
            throw new InvalidOperationException(String.Format(Strings.ExMemberIsNotPublicPropertyOrField,
              memberName, type.Name));

          cachedDelegates[methodKey] = result as Delegate;
        }
      return result;
    }

    /// <summary>
    /// Creates property \ member setter delegate.
    /// </summary>
    /// <typeparam name="TObject">Declaring type.</typeparam>
    /// <typeparam name="TValue">Member type.</typeparam>
    /// <param name="memberName">Member name.</param>
    /// <returns><see cref="Action{T,R}"/> delegate 
    /// that sets member value.</returns>
    public static Action<TObject, TValue> CreateSetMemberDelegate<TObject, TValue>(string memberName)
    {
      Type type = typeof (TObject);
      Type tValue = typeof (TValue);
      string methodKey = GetMethodCallDelegateKey(memberName, type, tValue);

      Action<TObject, TValue> result = (Action<TObject, TValue>)GetCachedDelegate(methodKey);
      if (result==null)
        lock (cachedDelegates) {
          result = (Action<TObject, TValue>)GetCachedDelegate(methodKey);
          if (result!=null)
            return result;
          methodKey = String.Intern(methodKey);

          PropertyInfo pi = type.GetProperty(memberName);
          FieldInfo fi = type.GetField(memberName);
          if (pi!=null) {
            // Member is a Property...
            MethodInfo mi = pi.GetSetMethod();
            if (mi!=null) {
              //  Calling a property's get accessor is faster/cleaner using
              //  Delegate.CreateDelegate rather than Reflection.Emit 
              // TODO: Check that type conversion is adequate.
              result = (Action<TObject, TValue>)Delegate.CreateDelegate(typeof (Action<TObject, TValue>), mi);
            }
            else
              throw new InvalidOperationException(String.Format(Strings.ExPropertyDoesNotHaveSetter,
                memberName, type.Name));
          }
          else if (fi!=null) {
            // Member is a Field...
            DynamicMethod dm = new DynamicMethod("Set" + memberName,
              typeof (TValue), new Type[] {type}, type);
            ILGenerator il = dm.GetILGenerator();
            // Load the instance of the object (argument 0) onto the stack
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            // Load the value of the object's field (fi) onto the stack
            il.Emit(OpCodes.Stfld, fi);
            // return the value on the top of the stack
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);

            result = (Action<TObject, TValue>)dm.CreateDelegate(typeof (Func<TObject, TValue>));
          }
          else
            throw new InvalidOperationException(String.Format(Strings.ExMemberIsNotPublicPropertyOrField,
              memberName, type.Name));
          cachedDelegates[methodKey] = result;
        }
      return result;
    }

    /// <summary>
    /// Creates primitive type cast delegate - e.g. <see cref="Enum"/> to <see cref="sbyte"/>.
    /// </summary>
    /// <typeparam name="TSource">The type to cast.</typeparam>
    /// <typeparam name="TTarget">Target type.</typeparam>
    /// <returns>A delegate allowing to cast <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</returns>
    /// <exception cref="InvalidCastException"><c>InvalidCastException</c>.</exception>
    public static Converter<TSource, TTarget> CreatePrimitiveCastDelegate<TSource, TTarget>()
      where TSource: struct
      where TTarget: struct
    {
      Type sourceType = typeof (TSource);
      Type targetType = typeof (TTarget);
      string methodName = String.Format("{0}_{1}_{2}", primitiveCastMethodName, sourceType, targetType);
      string methodKey  = GetMethodCallDelegateKey(methodName);

      Converter<TSource, TTarget> result = GetCachedDelegate(methodKey) as Converter<TSource, TTarget>;
      if (result==null)
        lock (cachedDelegates) {
          result = GetCachedDelegate(methodKey) as Converter<TSource, TTarget>;
          if (result!=null)
            return result;
          methodKey = String.Intern(methodKey);

          Type actualSourceType = sourceType;
          if (sourceType.IsEnum)
            actualSourceType = Enum.GetUnderlyingType(sourceType);
          if (!opCodeConv.ContainsKey(actualSourceType))
            throw new InvalidCastException(String.Format(Strings.ExInvalidCast, 
              sourceType.GetShortName(),
              targetType.GetShortName()));

          Type actualTargetType = targetType;
          if (targetType.IsEnum)
            actualTargetType = Enum.GetUnderlyingType(targetType);
          if (!opCodeConv.ContainsKey(actualTargetType))
            throw new InvalidCastException(String.Format(Strings.ExInvalidCast, 
              sourceType.GetShortName(),
              targetType.GetShortName()));

          DynamicMethod dm = new DynamicMethod(methodName,
            typeof(TTarget), new Type[] {sourceType});
          ILGenerator il = dm.GetILGenerator();
          il.Emit(OpCodes.Ldarg_0);

          if (targetType.IsEnum)
          if (typeOnStack[actualSourceType]!=typeOnStack[actualSourceType])
            il.Emit(opCodeConv[actualTargetType]);
          il.Emit(OpCodes.Ret);
          result = dm.CreateDelegate(typeof(Converter<TSource, TTarget>)) as Converter<TSource, TTarget>;

          cachedDelegates[methodKey] = result;
        }
      return result;
    }

    /// <summary>
    /// Creates (generic) method invocation delegate with specified generic argument types.
    /// </summary>
    /// <param name="callTarget">The delegate call target. <see langword="Null"/>, if static method should be called.</param>
    /// <param name="type">Type, which method should be called by delegate.</param>
    /// <param name="methodName">The name of the method to call by delegate.</param>
    /// <param name="genericArgumentTypes">Generic method arguments.</param>
    /// <returns>New delegate allowing to call specified generic method on <paramref name="callTarget"/>.</returns>
    /// <typeparam name="TDelegate">Type of delegate to create.</typeparam>
    public static TDelegate CreateDelegate<TDelegate>(object callTarget, Type type, string methodName, params Type[] genericArgumentTypes)
      where TDelegate : class
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(methodName, "methodName");
      ArgumentValidator.EnsureArgumentNotNull(genericArgumentTypes, "genericArgumentTypes");
      Type tDelegate = typeof (TDelegate);
      if (!typeof(Delegate).IsAssignableFrom(tDelegate))
        throw new ArgumentException(String.Format(Strings.ExGenericParameterShouldBeOfTypeT,
          "TDelegate", typeof(Delegate).GetShortName()));

      BindingFlags bindingFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic;
      if (callTarget==null)
        bindingFlags |= BindingFlags.Static;
      else 
        bindingFlags |= BindingFlags.Instance;
      string[] genericArgumentNames = new string[genericArgumentTypes.Length]; // Actual names doesn't matter
      ParameterInfo[] parameterInfos = tDelegate.GetMethod("Invoke").GetParameters();
      Type[] parameterTypes = new Type[parameterInfos.Length];
      int i = 0;
      foreach (ParameterInfo parameterInfo in parameterInfos)
        parameterTypes[i++] = parameterInfo.ParameterType;
      MethodInfo methodInfo = MethodHelper.GetMethod(type, methodName, bindingFlags, genericArgumentNames, parameterTypes);
      if (methodInfo==null)
        return null;
      if (genericArgumentTypes.Length!=0)
        methodInfo = methodInfo.MakeGenericMethod(genericArgumentTypes);
      if (callTarget==null)
        return (TDelegate)(object)Delegate.CreateDelegate(tDelegate, methodInfo, true);
      else
        return (TDelegate)(object)Delegate.CreateDelegate(tDelegate, callTarget, methodInfo, true);
    }

    /// <summary>
    /// Creates an array of generic method invocation delegates matching the method instance 
    /// with specified generic argument variants.
    /// </summary>
    /// <param name="callTarget">The delegate call target. <see langword="Null"/>, if static method should be called.</param>
    /// <param name="type">Type, which method should be called by delegate.</param>
    /// <param name="methodName">The name of the method to call by delegate.</param>
    /// <param name="genericArgumentVariants">Generic method argument variants.</param>
    /// <returns>An array of delegate allowing to call specified generic method instances on <paramref name="callTarget"/>.</returns>
    /// <typeparam name="TDelegate">Type of delegate to create.</typeparam>
    /// <exception cref="ArgumentException"><c>ArgumentException</c>.</exception>
    public static TDelegate[] CreateDelegates<TDelegate>(object callTarget, Type type, string methodName, IList<Type> genericArgumentVariants)
      where TDelegate : class
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(methodName, "methodName");
      ArgumentValidator.EnsureArgumentNotNull(genericArgumentVariants, "genericArgumentVariants");
      Type tDelegate = typeof (TDelegate);
      if (!typeof(Delegate).IsAssignableFrom(tDelegate))
        throw new ArgumentException(String.Format(Strings.ExGenericParameterShouldBeOfTypeT,
          "TDelegate", typeof(Delegate).GetShortName()));

      int count = genericArgumentVariants.Count;
      TDelegate[] delegates = new TDelegate[count];
      if (count==0)
        return delegates;

      BindingFlags bindingFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic;
      if (callTarget==null)
        bindingFlags |= BindingFlags.Static;
      else 
        bindingFlags |= BindingFlags.Instance;
      string[] genericArgumentNames = new string[1]; // Actual names doesn't matter
      ParameterInfo[] parameterInfos = tDelegate.GetMethod("Invoke").GetParameters();
      Type[] parameterTypes = new Type[parameterInfos.Length];
      int i = 0;
      foreach (ParameterInfo parameterInfo in parameterInfos)
        parameterTypes[i++] = parameterInfo.ParameterType;
      MethodInfo methodInfo = MethodHelper.GetMethod(type, methodName, bindingFlags, genericArgumentNames, parameterTypes);
      if (methodInfo==null)
        return null;

      for (i = 0; i<count; i++) {
        MethodInfo instantiatedMethodInfo = methodInfo.MakeGenericMethod(genericArgumentVariants[i]);
        if (callTarget==null)
          delegates[i] = (TDelegate)(object)Delegate.CreateDelegate(tDelegate, instantiatedMethodInfo, true);
        else
          delegates[i] = (TDelegate)(object)Delegate.CreateDelegate(tDelegate, callTarget, instantiatedMethodInfo, true);
      }
      return delegates;
    }

    /// <summary>
    /// Executes sequence of <see cref="ExecutionSequenceHandler{T}"/>s.
    /// Stops when the executed delegate returns <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Type of argument to pass to each delegate.</typeparam>
    /// <param name="delegates">Delegates to execute.</param>
    /// <param name="argument">Argument to pass to each delegate.</param>
    /// <param name="direction">Direction of execution.</param>
    public static void ExecuteDelegates<T>(ExecutionSequenceHandler<T>[] delegates, ref T argument, Direction direction)
      where T: struct
    {
      if (direction==Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");

      if (delegates==null)
        return;
      int count = delegates.Length;
      if (direction==Direction.Positive) {
        for (int i = 0; i<count; i++)
          if (delegates[i].Invoke(ref argument, i))
            return;
      }
      else {
        for (int i = count-1; i>=0; i--)
          if (delegates[i].Invoke(ref argument, i))
            return;
      }
    }

    /// <summary>
    /// Creates constructor invocation delegate.
    /// </summary>
    /// <typeparam name="TObject">Type to create.</typeparam>
    /// <returns>Constructor delegate.</returns>
    public static Func<TObject> CreateClassConstructorDelegate<TObject>()
      where TObject : new()
    {
      return (Func<TObject>)CreateClassConstructorDelegate(typeof(TObject), typeof(Func<TObject>));
    }

    public static Delegate CreateClassConstructorDelegate(Type type)
    {
      return CreateClassConstructorDelegate(type, typeof (Func<>).MakeGenericType(type));
    }

    public static Delegate CreateClassConstructorDelegate(Type type, Type delegateType)
    {
      string methodKey = GetMethodCallDelegateKey(type.FullName + " .ctor");

      Delegate result = GetCachedDelegate(methodKey);
      if (result == null)
        lock (cachedDelegates) {
          result = GetCachedDelegate(methodKey);
          if (result != null)
            return result;
          methodKey = String.Intern(methodKey);

          DynamicMethod dm = new DynamicMethod("Create " + type.FullName,
                                               type, new Type[] { });
          ILGenerator il = dm.GetILGenerator();
          il.Emit(OpCodes.Newobj, type.GetConstructor(new Type[]{}));
          il.Emit(OpCodes.Ret);

          result = dm.CreateDelegate(delegateType);
          cachedDelegates[methodKey] = result;
        }
      return result;
    }

    #region Private members

    private static Delegate GetCachedDelegate(string delegateKey)
    {
      Delegate result;
      if (cachedDelegates.TryGetValue(delegateKey, out result))
        return result;
      return null;
    }

    private static string GetMethodCallDelegateKey(string methodName, params Type[] argumentTypes)
    {
      StringBuilder sb = new StringBuilder(128);
      sb.Append(methodName);
      for (int i = 0; i < argumentTypes.Length; i++) {
        sb.Append(", ");
        sb.Append(argumentTypes[i]);
      }
      return sb.ToString();
    }

    #endregion


    // Type initializer

    static DelegateHelper()
    {
      opCodeConv.Add(typeof(sbyte),  OpCodes.Conv_I1);
      opCodeConv.Add(typeof(byte),   OpCodes.Conv_U1);
      opCodeConv.Add(typeof(short),  OpCodes.Conv_I2);
      opCodeConv.Add(typeof(ushort), OpCodes.Conv_U2);
      opCodeConv.Add(typeof(char),   OpCodes.Conv_U2);
      opCodeConv.Add(typeof(int),    OpCodes.Conv_I4);
      opCodeConv.Add(typeof(uint),   OpCodes.Conv_U4);
      opCodeConv.Add(typeof(long),   OpCodes.Conv_I8);
      opCodeConv.Add(typeof(ulong),  OpCodes.Conv_U8);
      opCodeConv.Add(typeof(float),  OpCodes.Conv_R4);
      opCodeConv.Add(typeof(double), OpCodes.Conv_R8);
      typeOnStack.Add(typeof(sbyte),  typeof(int));
      typeOnStack.Add(typeof(byte),   typeof(uint));
      typeOnStack.Add(typeof(short),  typeof(int));
      typeOnStack.Add(typeof(ushort), typeof(uint));
      typeOnStack.Add(typeof(char),   typeof(uint));
      typeOnStack.Add(typeof(int),    typeof(int));
      typeOnStack.Add(typeof(uint),   typeof(uint));
      typeOnStack.Add(typeof(long),   typeof(long));
      typeOnStack.Add(typeof(ulong),  typeof(ulong));
      typeOnStack.Add(typeof(float),  typeof(float));
      typeOnStack.Add(typeof(double), typeof(double));
    }
  }
}