// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: 
// Created:    2007.10.25

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Xtensive.Collections;
using Xtensive.Core;



namespace Xtensive.Reflection
{
  /// <summary>
  /// Delegate helper \ extension methods. 
  /// Simplifies various delegate creation.
  /// </summary>
  public static class DelegateHelper
  {
    #region Constants

    private const string primitiveCastMethodName = "PrimitiveCast";
    private const string ctorMethodName = "Ctor";
    private const string createMethodName = "Create";

    private static readonly Dictionary<Type, OpCode> opCodeConv = new Dictionary<Type, OpCode>();
    private static readonly Dictionary<Type, Type> typeOnStack = new Dictionary<Type, Type>();

    private static readonly Type[] ActionTypes;
    private static readonly Type[] FuncTypes;

    public const int MaxNumberOfGenericDelegateParameters = 16;

    /// <summary>
    /// Aspected private field getter prefix.
    /// </summary>
    public static readonly string AspectedPrivateFieldGetterPrefix =
      "~Xtensive.Aspects.PrivateFieldAccessorAspect~Get_";

    /// <summary>
    /// Aspected private field setter prefix.
    /// </summary>
    public static readonly string AspectedPrivateFieldSetterPrefix =
      "~Xtensive.Aspects.PrivateFieldAccessorAspect~Set_";

    /// <summary>
    /// Aspected factory method name.
    /// </summary>
    public static readonly string AspectedFactoryMethodName = 
      "~Xtensive.Orm.CreateObject";

    #endregion

    #region Nested type: MethodCallDelegateKey 

    private sealed class MethodCallDelegateKey
    {
      private object[] items;
      private int hashCode;

      public bool Equals(MethodCallDelegateKey other)
      {
        if (ReferenceEquals(null, other))
          return false;
        if (other.hashCode!=hashCode)
          return false;
        var otherItems = other.items;
        if (otherItems.Length!=items.Length)
          return false;
        for (int i = 0; i < items.Length; i++)
          if (!Equals(otherItems[i], items[i]))
            return false;
        return true;
      }

      public override bool Equals(object obj) =>
        obj switch {
          null => false,
          MethodCallDelegateKey otherKey => Equals(otherKey),
          _ => false
        };

      public override int GetHashCode()
      {
        return hashCode;
      }

      
      // Constructors

      public MethodCallDelegateKey(params object[] items)
      {
        this.items = items;
        unchecked {
          foreach (var item in items)
            hashCode = hashCode * 397 + (item==null ? 0 : item.GetHashCode());
        }
      }
    }

    #endregion

    private static ThreadSafeDictionary<object, Delegate> cachedDelegates = 
      ThreadSafeDictionary<object, Delegate>.Create(new object());

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

    private static TDelegateType InnerCreateGetMemberDelegate<TObject, TValue, TDelegateType>(string memberName)
      where TDelegateType : class
    {
      Type type = typeof (TObject);
      Type tValue = typeof (TValue);
      var methodKey = new MethodCallDelegateKey(typeof (TDelegateType), memberName, type, tValue);

      TDelegateType result = GetCachedDelegate(methodKey) as TDelegateType;
      if (result==null)
        lock (cachedDelegates.SyncRoot) {
          result = GetCachedDelegate(methodKey) as TDelegateType;
          if (result!=null)
            return result;

          PropertyInfo pi = type.GetProperty(memberName);
          FieldInfo fi = type.GetField(memberName);
          MethodInfo smi;
          if (pi!=null) {
            // Member is a Property...
            MethodInfo mi = pi.GetGetMethod(true);
            if (mi!=null) {
              //  Calling a property's get accessor is faster/cleaner using
              //  Delegate.CreateDelegate rather than Reflection.Emit 
              result = Delegate.CreateDelegate(typeof (TDelegateType), mi) as TDelegateType;
            }
            else
              throw new InvalidOperationException(string.Format(Strings.ExPropertyDoesNotHaveGetter,
                memberName, type.GetShortName()));
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
            BindingFlags.Instance | 
            BindingFlags.Public | 
            BindingFlags.NonPublic |
            BindingFlags.ExactBinding))) {
            result = Delegate.CreateDelegate(typeof (Func<TObject, TValue>), smi) as TDelegateType;
          }
          else
            throw new InvalidOperationException(string.Format(Strings.ExMemberIsNotPublicPropertyOrField,
              memberName, type.GetShortName()));

          AddCachedDelegate(methodKey, result as Delegate);
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
      var methodKey = new MethodCallDelegateKey(memberName, type, tValue);

      Action<TObject, TValue> result = (Action<TObject, TValue>)GetCachedDelegate(methodKey);
      if (result==null)
        lock (cachedDelegates.SyncRoot) {
          result = (Action<TObject, TValue>)GetCachedDelegate(methodKey);
          if (result!=null)
            return result;

          PropertyInfo pi = type.GetProperty(memberName);
          FieldInfo fi = type.GetField(memberName);
          if (pi!=null) {
            // Member is a Property...
            MethodInfo mi = pi.GetSetMethod(true);
            if (mi!=null) {
              //  Calling a property's get accessor is faster/cleaner using
              //  Delegate.CreateDelegate rather than Reflection.Emit 
              // TODO: Check that type conversion is adequate.
              result = (Action<TObject, TValue>)Delegate.CreateDelegate(typeof (Action<TObject, TValue>), mi);
            }
            else
              throw new InvalidOperationException(string.Format(Strings.ExPropertyDoesNotHaveSetter,
                memberName, type.GetShortName()));
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
            throw new InvalidOperationException(string.Format(Strings.ExMemberIsNotPublicPropertyOrField,
              memberName, type.GetShortName()));
          AddCachedDelegate(methodKey, result);
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
      string methodName = string.Format("{0}_{1}_{2}", primitiveCastMethodName, sourceType, targetType);
      var methodKey  = new MethodCallDelegateKey(methodName);

      var result = GetCachedDelegate(methodKey) as Converter<TSource, TTarget>;
      if (result==null)
        lock (cachedDelegates.SyncRoot) {
          result = GetCachedDelegate(methodKey) as Converter<TSource, TTarget>;
          if (result!=null)
            return result;

          Type actualSourceType = sourceType;
          if (sourceType.IsEnum)
            actualSourceType = Enum.GetUnderlyingType(sourceType);
          if (!opCodeConv.ContainsKey(actualSourceType))
            throw new InvalidCastException(string.Format(Strings.ExInvalidCast, 
              sourceType.GetShortName(),
              targetType.GetShortName()));

          Type actualTargetType = targetType;
          if (targetType.IsEnum)
            actualTargetType = Enum.GetUnderlyingType(targetType);
          if (!opCodeConv.ContainsKey(actualTargetType))
            throw new InvalidCastException(string.Format(Strings.ExInvalidCast, 
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

          AddCachedDelegate(methodKey, result);
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
      Type delegateType = typeof (TDelegate);
      if (!WellKnownTypes.Delegate.IsAssignableFrom(delegateType))
        throw new ArgumentException(string.Format(Strings.ExGenericParameterShouldBeOfTypeT,
          "TDelegate", WellKnownTypes.Delegate.GetShortName()));

      BindingFlags bindingFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.ExactBinding;
      if (callTarget == null) {
        bindingFlags |= BindingFlags.Static;
        bindingFlags |= BindingFlags.FlattenHierarchy;
      }
      else
        bindingFlags |= BindingFlags.Instance;
      string[] genericArgumentNames = new string[genericArgumentTypes.Length]; // Actual names doesn't matter
      Type[] parameterTypes = delegateType.GetInvokeMethod().GetParameterTypes();
      
      MethodInfo methodInfo = MethodHelper.GetMethod(type, methodName, bindingFlags, 
        genericArgumentNames, parameterTypes);
      if (methodInfo==null)
        return null;
      if (genericArgumentTypes.Length!=0)
        methodInfo = methodInfo.MakeGenericMethod(genericArgumentTypes);
      if (callTarget==null)
        return (TDelegate)(object)Delegate.CreateDelegate(delegateType, methodInfo, true);

      return (TDelegate)(object)Delegate.CreateDelegate(delegateType, callTarget, methodInfo, true);
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
      Type delegateType = typeof (TDelegate);
      if (!WellKnownTypes.Delegate.IsAssignableFrom(delegateType))
        throw new ArgumentException(string.Format(Strings.ExGenericParameterShouldBeOfTypeT,
          "TDelegate", WellKnownTypes.Delegate.GetShortName()));

      int count = genericArgumentVariants.Count;
      TDelegate[] delegates = new TDelegate[count];
      if (count==0)
        return delegates;

      BindingFlags bindingFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.ExactBinding;
      if (callTarget==null)
        bindingFlags |= BindingFlags.Static;
      else 
        bindingFlags |= BindingFlags.Instance;
      string[] genericArgumentNames = new string[1]; // Actual names doesn't matter
      Type[] parameterTypes = delegateType.GetInvokeMethod().GetParameterTypes();

      MethodInfo methodInfo = MethodHelper.GetMethod(type, methodName, bindingFlags, 
        genericArgumentNames, parameterTypes);
      if (methodInfo==null)
        return null;

      for (int i = 0; i<count; i++) {
        MethodInfo instantiatedMethodInfo = methodInfo.MakeGenericMethod(genericArgumentVariants[i]);
        if (callTarget==null)
          delegates[i] = (TDelegate)(object)Delegate.CreateDelegate(delegateType, instantiatedMethodInfo, true);
        else
          delegates[i] = (TDelegate)(object)Delegate.CreateDelegate(delegateType, callTarget, instantiatedMethodInfo, true);
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
    /// <exception cref="ArgumentOutOfRangeException">Invalid <paramref name="direction"/> value.</exception>
    public static void ExecuteDelegates<T>(ExecutionSequenceHandler<T>[] delegates, ref T argument, Direction direction)
      where T: struct
    {
      if (direction==Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");

      if (delegates==null)
        return;
      if (direction==Direction.Positive) {
        for (int i = 0; i<delegates.Length; i++)
          if (delegates[i].Invoke(ref argument, i))
            return;
      }
      else {
        for (int i = delegates.Length-1; i>=0; i--)
          if (delegates[i].Invoke(ref argument, i))
            return;
      }
    }


    /// <summary>
    /// Creates a delegate type that represents a delegate that calls a method with specified signature.
    /// </summary>
    /// <param name="returnType">Type of return value.</param>
    /// <param name="parameterTypes">Types of parameters.</param>
    /// <returns>Created delegate type.</returns>
    public static Type MakeDelegateType(Type returnType, params Type[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(parameterTypes, "parameterTypes");
      if (parameterTypes.Length > MaxNumberOfGenericDelegateParameters)
        throw new NotSupportedException();
      if (returnType == WellKnownTypes.Void || returnType == null) {
        if (parameterTypes.Length == 0)
          return ActionTypes[0];
        return ActionTypes[parameterTypes.Length].MakeGenericType(parameterTypes);
      }
      var funcGenericParameters = parameterTypes.Append(returnType);
      return FuncTypes[funcGenericParameters.Length - 1].MakeGenericType(funcGenericParameters);
    }

    /// <summary>
    /// Creates a delegate type that represents a delegate that calls a method with specified signature.
    /// </summary>
    /// <param name="returnType">Type of return value.</param>
    /// <param name="parameterTypes">Types of parameters.</param>
    /// <returns>Created delegate type.</returns>
    public static Type MakeDelegateType(Type returnType, IEnumerable<Type> parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(parameterTypes, "parameterTypes");
      return MakeDelegateType(returnType, parameterTypes.ToArray());
    }

    /// <summary>
    /// Gets signature of a delegate of a <paramref name="delegateType"/>.
    /// </summary>
    /// <param name="delegateType">Type of the delegate.</param>
    /// <returns>A pair that contains return type as first element and parameter types as second arguments.</returns>
    public static Pair<Type, Type[]> GetDelegateSignature(Type delegateType)
    {
      ArgumentValidator.EnsureArgumentNotNull(delegateType, "delegateType");
      // check for non-generic Action
      if (delegateType == ActionTypes[0])
        return new Pair<Type, Type[]>(WellKnownTypes.Void, ArrayUtils<Type>.EmptyArray);
      if (delegateType.IsGenericType) {
        var genericTypeDefinition = delegateType.GetGenericTypeDefinition();
        var genericArguments = delegateType.GetGenericArguments();
        int genericArgumentsLength = genericArguments.Length;
        // check for Func<>
        if (genericArgumentsLength >= 1
          && genericArgumentsLength <= MaxNumberOfGenericDelegateParameters + 1
          && FuncTypes[genericArgumentsLength-1] == genericTypeDefinition) {
          var parameterTypes = new Type[genericArguments.Length - 1];
          Array.Copy(genericArguments, parameterTypes, parameterTypes.Length);
          return new Pair<Type, Type[]>(genericArguments[genericArgumentsLength - 1], parameterTypes);
        }
        // check for Action<>
        if (genericArgumentsLength >= 1
          && genericArgumentsLength <= MaxNumberOfGenericDelegateParameters
          && ActionTypes[genericArgumentsLength] == genericTypeDefinition)
          return new Pair<Type, Type[]>(WellKnownTypes.Void, delegateType.GetGenericArguments());
      }
      // universal (but slow) strategy - reflect "Invoke" method
      var method = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
      return new Pair<Type, Type[]>(method.ReturnType, method.GetParameterTypes());
    }

    #region Private members

    private static Delegate GetCachedDelegate(object delegateKey)
    {
      Delegate result;
      return 
        cachedDelegates.TryGetValue(delegateKey, out result) ? result : null;      
    }

    private static void AddCachedDelegate(object delegateKey, Delegate value)
    {
      cachedDelegates.SetValue(delegateKey, value);
    }

    #endregion


    // Type initializer

    static DelegateHelper()
    {
      opCodeConv.Add(WellKnownTypes.SByte,  OpCodes.Conv_I1);
      opCodeConv.Add(WellKnownTypes.Byte,   OpCodes.Conv_U1);
      opCodeConv.Add(WellKnownTypes.Int16,  OpCodes.Conv_I2);
      opCodeConv.Add(WellKnownTypes.UInt16, OpCodes.Conv_U2);
      opCodeConv.Add(WellKnownTypes.Char,   OpCodes.Conv_U2);
      opCodeConv.Add(WellKnownTypes.Int32,  OpCodes.Conv_I4);
      opCodeConv.Add(WellKnownTypes.UInt32, OpCodes.Conv_U4);
      opCodeConv.Add(WellKnownTypes.Int64,  OpCodes.Conv_I8);
      opCodeConv.Add(WellKnownTypes.UInt64, OpCodes.Conv_U8);
      opCodeConv.Add(WellKnownTypes.Single, OpCodes.Conv_R4);
      opCodeConv.Add(WellKnownTypes.Double, OpCodes.Conv_R8);
      typeOnStack.Add(WellKnownTypes.SByte,  WellKnownTypes.Int32);
      typeOnStack.Add(WellKnownTypes.Byte,   WellKnownTypes.UInt32);
      typeOnStack.Add(WellKnownTypes.Int16,  WellKnownTypes.Int32);
      typeOnStack.Add(WellKnownTypes.UInt16, WellKnownTypes.UInt32);
      typeOnStack.Add(WellKnownTypes.Char,   WellKnownTypes.UInt32);
      typeOnStack.Add(WellKnownTypes.Int32,  WellKnownTypes.Int32);
      typeOnStack.Add(WellKnownTypes.UInt32, WellKnownTypes.UInt32);
      typeOnStack.Add(WellKnownTypes.Int64,  WellKnownTypes.Int64);
      typeOnStack.Add(WellKnownTypes.UInt64, WellKnownTypes.UInt64);
      typeOnStack.Add(WellKnownTypes.Single, WellKnownTypes.Single);
      typeOnStack.Add(WellKnownTypes.Double, WellKnownTypes.Double);

      FuncTypes = new[]
        {
          typeof (Func<>),

          typeof (Func<,>),
          typeof (Func<,,>),
          typeof (Func<,,,>),
          typeof (Func<,,,,>),

          typeof (Func<,,,,,>),
          typeof (Func<,,,,,,>),
          typeof (Func<,,,,,,,>),
          typeof (Func<,,,,,,,,>),

          typeof (Func<,,,,,,,,,>),
          typeof (Func<,,,,,,,,,,>),
          typeof (Func<,,,,,,,,,,,>),
          typeof (Func<,,,,,,,,,,,,>),

          typeof (Func<,,,,,,,,,,,,,>),
          typeof (Func<,,,,,,,,,,,,,,>),
          typeof (Func<,,,,,,,,,,,,,,,>),
          typeof (Func<,,,,,,,,,,,,,,,,>),
        };

      ActionTypes = new[]
        {
          typeof (Action),
          typeof (Action<>),
          typeof (Action<,>),
          typeof (Action<,,>),
          typeof (Action<,,,>),

          typeof (Action<,,,,>),
          typeof (Action<,,,,,>),
          typeof (Action<,,,,,,>),
          typeof (Action<,,,,,,,>),

          typeof (Action<,,,,,,,,>),
          typeof (Action<,,,,,,,,,>),
          typeof (Action<,,,,,,,,,,>),
          typeof (Action<,,,,,,,,,,,>),

          typeof (Action<,,,,,,,,,,,,>),
          typeof (Action<,,,,,,,,,,,,,>),
          typeof (Action<,,,,,,,,,,,,,,>),
          typeof (Action<,,,,,,,,,,,,,,,>),
        };
    }
  }
}