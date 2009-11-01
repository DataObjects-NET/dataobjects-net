// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.SizeCalculators
{
  /// <summary>
  /// Default <see cref="ISizeCalculator{T}"/> provider. 
  /// Provides default size calculator for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class SizeCalculatorProvider : AssociateProvider, 
    ISizeCalculatorProvider
  {
    /// <summary>
    /// Gets the pointer size.
    /// </summary>
    public static readonly int PointerFieldSize     = RuntimeInfo.PointerSize;
    /// <summary>
    /// Gets the heap object header size.
    /// </summary>
    public static readonly int HeapObjectHeaderSize = RuntimeInfo.PointerSize * 2;

    private static readonly SizeCalculatorProvider @default = new SizeCalculatorProvider();
    private readonly object _lock = new object();
    private ThreadSafeDictionary<Type, ISizeCalculatorBase> cache1 = ThreadSafeDictionary<Type, ISizeCalculatorBase>.Create();
    private ThreadSafeDictionary<Type, ISizeCalculatorBase> cache2 = ThreadSafeDictionary<Type, ISizeCalculatorBase>.Create();
    private ISizeCalculatorBase objectSizeCalculator = null;

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static ISizeCalculatorProvider Default
    {
      get { return @default; }
    }

    #region ISizeCalculatorProvider members

    /// <inheritdoc/>
    public virtual SizeCalculator<T> GetSizeCalculator<T>()
    {
      return GetAssociate<T, ISizeCalculator<T>, SizeCalculator<T>>();
    }

    /// <inheritdoc/>
    public ISizeCalculatorBase GetSizeCalculatorByInstance(object value)
    {
      if (value==null) {
        if (objectSizeCalculator==null) lock (_lock) if (objectSizeCalculator==null)
          objectSizeCalculator = GetSizeCalculator<object>().Implementation;
        return objectSizeCalculator;
      }
      else {
        Type type = value.GetType();
        ISizeCalculatorBase result = cache2.GetValue(type);
        if (result!=null)
          return result;
        lock (_lock) {
          result = cache2.GetValue(type);
          if (result!=null)
            return result;
          MethodInfo innerGetSizeCalculatorByInstanceMethod =
            GetType()
              .GetMethod("InnerGetSizeCalculatorByInstance", BindingFlags.NonPublic | BindingFlags.Instance, null, ArrayUtils<Type>.EmptyArray, null)
                .GetGenericMethodDefinition()
                  .MakeGenericMethod(new Type[] {type});
          result = innerGetSizeCalculatorByInstanceMethod.Invoke(this, null) as ISizeCalculatorBase;
          cache2.SetValue(type, result);
          return result;
        }
      }
    }

// ReSharper disable UnusedPrivateMember
    private ISizeCalculatorBase InnerGetSizeCalculatorByInstance<T>()
// ReSharper restore UnusedPrivateMember
    {
      Type type = typeof (T);
      if (!type.IsValueType)
        return GetSizeCalculator<T>().Implementation;
      else
        return new BoxSizeCalculator<T>(this);
    }

    /// <inheritdoc/>
    public ISizeCalculatorBase GetSizeCalculatorByType(Type type)
    {
      ISizeCalculatorBase result = cache1.GetValue(type);
      if (result!=null)
        return result;
      lock (_lock) {
        result = cache1.GetValue(type);
        if (result!=null)
          return result;
        MethodInfo getSizeCalculatorMethod =
          GetType()
            .GetMethod("GetSizeCalculator", ArrayUtils<Type>.EmptyArray)
              .GetGenericMethodDefinition()
                .MakeGenericMethod(new Type[] {type});
        IHasSizeCalculator hsc = getSizeCalculatorMethod.Invoke(this, null) as IHasSizeCalculator;
        if (hsc!=null)
          result = hsc.SizeCalculator;
        cache1.SetValue(type, result);
        return result;
      }
    }

    #endregion

    #region Protected method overrides

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      else
        return (TResult) (object) new SizeCalculator<TKey>((ISizeCalculator<TKey>) associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected SizeCalculatorProvider()
    {
      TypeSuffixes = new string[] {"SizeCalculator"};
      Type t = typeof (SizeCalculatorProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}