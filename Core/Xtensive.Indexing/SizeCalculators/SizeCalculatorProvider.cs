// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Diagnostics;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Threading;

namespace Xtensive.Indexing.SizeCalculators
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
    private ThreadSafeDictionary<Type, ISizeCalculatorBase> calculators = 
      ThreadSafeDictionary<Type, ISizeCalculatorBase>.Create(new object());
    private ThreadSafeDictionary<Type, ISizeCalculatorBase> boxingAwareCalculators = 
      ThreadSafeDictionary<Type, ISizeCalculatorBase>.Create(new object());
    private ThreadSafeCached<ISizeCalculatorBase> objectSizeCalculator = ThreadSafeCached<ISizeCalculatorBase>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static ISizeCalculatorProvider Default
    {
      [DebuggerStepThrough]
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
      if (value==null)
        return objectSizeCalculator.GetValue(
          _this => _this.GetSizeCalculator<object>().Implementation, 
          this);
      else
        return boxingAwareCalculators.GetValue(value.GetType(),
          (_type, _this) => _this
            .GetType()
            .GetMethod("InnerGetSizeCalculatorByInstance",
              BindingFlags.Instance | BindingFlags.NonPublic, 
              null, ArrayUtils<Type>.EmptyArray, null)
            .GetGenericMethodDefinition()
            .MakeGenericMethod(new[] {_type})
            .Invoke(_this, null)
            as ISizeCalculatorBase,
          this);
    }

    /// <inheritdoc/>
    public ISizeCalculatorBase GetSizeCalculatorByType(Type type)
    {
      return calculators.GetValue(type, 
        (_type, _this) => {
          var hsc = _this
            .GetType()
            .GetMethod("GetSizeCalculator", ArrayUtils<Type>.EmptyArray)
            .GetGenericMethodDefinition()
            .MakeGenericMethod(new[] {_type})
            .Invoke(_this, null)
            as IHasSizeCalculator;
          return hsc!=null ? hsc.SizeCalculator : null;
        }, 
        this);
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

    #region Private \ internal methods

    protected ISizeCalculatorBase InnerGetSizeCalculatorByInstance<T>()
    {
      Type type = typeof (T);
      if (!type.IsValueType)
        return GetSizeCalculator<T>().Implementation;
      else
        return new BoxSizeCalculator<T>(this);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected SizeCalculatorProvider()
    {
      TypeSuffixes = new[] {"SizeCalculator"};
      Type t = typeof (SizeCalculatorProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}