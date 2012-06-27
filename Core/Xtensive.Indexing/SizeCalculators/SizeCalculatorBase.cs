// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.23

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Diagnostics;
using Xtensive.Internals.DocTemplates;
using FieldInfo=System.Reflection.FieldInfo;

namespace Xtensive.Indexing.SizeCalculators
{
  /// <summary>
  /// Base class for any <see cref="ISizeCalculator{T}"/> implementor.
  /// </summary>
  /// <typeparam name="T">Type to calculate the size for.</typeparam>
  [Serializable]
  public abstract class SizeCalculatorBase<T> : ISizeCalculator<T>
  {
    /// <summary>
    /// Indicates whether <typeparamref name="T"/> is a value type.
    /// </summary>
    protected static readonly bool IsStruct = typeof (T).IsValueType;

    /// <summary>
    /// Indicates whether <typeparamref name="T"/> is a reference type.
    /// </summary>
    protected static readonly bool IsClass  = typeof (T).IsClass;
    
    private static object _lock = new object();
    private static volatile bool isWarningLogged;

    private ISizeCalculatorProvider provider;
    private int defaultSize;

    /// <inheritdoc/>
    public ISizeCalculatorProvider Provider
    {
      [DebuggerStepThrough]
      get { return provider; }
    }

    /// <summary>
    /// Gets the size of default instance of type <typeparamref name="T"/>.
    /// </summary>
    protected int DefaultSize {
      [DebuggerStepThrough]
      get {
        return defaultSize;
      }
    }

    /// <inheritdoc/>
    public virtual int GetDefaultSize()
    {
      return DefaultSize;
    }

    /// <inheritdoc/>
    public virtual int GetValueSize(T value)
    {
      if (IsClass && ReferenceEquals(value, null))
        return SizeCalculatorProvider.PointerFieldSize;

      if (!isWarningLogged) lock (_lock) if (!isWarningLogged) {
        if (typeof (T)!=typeof (object))
          Log.Warning("Default (approximate) size calculator is used for type '{0}'.", typeof(T).GetShortName());
        isWarningLogged = true;
      }
      return GetDefaultSize();
    }

    /// <inheritdoc />
    int ISizeCalculatorBase.GetInstanceSize(object instance)
    {
      return GetValueSize((T) instance);
    }

    /// <summary>
    /// Gets packed struct size (actual size) for the struct of specified unpacked size.
    /// </summary>
    /// <param name="minimalStructSize">Pre-calculated minimal struct size.</param>
    /// <returns>Actual struct size.</returns>
    public static int GetPackedStructSize(int minimalStructSize)
    {
      int remainder = minimalStructSize % RuntimeInfo.DefaultStructLayoutPack;
      if (remainder!=0)
        minimalStructSize = minimalStructSize - remainder + RuntimeInfo.DefaultStructLayoutPack;
      if (minimalStructSize!=0)
        return minimalStructSize;
      else
        return RuntimeInfo.DefaultStructLayoutPack;
    }

    #region Private \ internal methods

    private int CalculateDefaultSize()
    {
      int result = 0;
      Type type = typeof (T);
      if (!type.IsPrimitive) {
        FieldInfo[] fields = typeof (T).GetFields(
          BindingFlags.Instance |
          BindingFlags.NonPublic |
          BindingFlags.Public |
          BindingFlags.FlattenHierarchy);
        foreach (FieldInfo field in fields) {
          Type fieldType = field.FieldType;
          if (fieldType.IsValueType)
            result += Provider.GetSizeCalculatorByType(fieldType).GetDefaultSize();
          else
            result += SizeCalculatorProvider.PointerFieldSize;
        }
        result = GetPackedStructSize(result);
        if (IsClass)
          result += SizeCalculatorProvider.PointerFieldSize + SizeCalculatorProvider.HeapObjectHeaderSize;
      }
      return result;  
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">The provider this size calculator is bound to.</param>
    protected SizeCalculatorBase(ISizeCalculatorProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
      defaultSize = CalculateDefaultSize();
    }
  
    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender)
    {
      defaultSize = CalculateDefaultSize();
      if (provider==null || provider.GetType()==typeof (SizeCalculatorProvider))
        provider = SizeCalculatorProvider.Default;
    }
  }
}