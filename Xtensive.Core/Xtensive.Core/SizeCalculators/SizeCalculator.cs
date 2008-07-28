// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.11

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.SizeCalculators
{
  /// <summary>
  /// Provides delegates allowing to call size calculator methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="ISizeCalculator{T}"/> generic argument.</typeparam>
  [Serializable]
  public sealed class SizeCalculator<T> : MethodCacheBase<ISizeCalculator<T>>,
    IHasSizeCalculator
  {
    private static readonly object _lock = new object();
    private static volatile SizeCalculator<T> @default;

    /// <summary>
    /// Gets default size calculator for type <typeparamref name="T"/>
    /// (uses <see cref="SizeCalculatorProvider.Default"/> <see cref="SizeCalculatorProvider"/>).
    /// </summary>
    public static SizeCalculator<T> Default {
      get {
        if (@default==null) lock (_lock) if (@default==null) {
          try {
            @default = SizeCalculatorProvider.Default.GetSizeCalculator<T>();
          }
          catch {
          }
        }
        return @default;
      }
    }

    /// <summary>
    /// Gets the provider underlying size calculator is associated with.
    /// </summary>
    public readonly ISizeCalculatorProvider Provider;

    /// <summary>
    /// Gets <see cref="ISizeCalculatorBase.GetDefaultSize"/> method delegate.
    /// </summary>
    public readonly Func<int> GetDefaultSize;

    /// <summary>
    /// Gets <see cref="ISizeCalculator{T}.GetValueSize"/> method delegate.
    /// </summary>
    public readonly Func<T, int> GetValueSize;

    #region IHasSizeCalculator members

    ISizeCalculatorBase IHasSizeCalculator.SizeCalculator
    {
      get { return Implementation; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Size calculator to provide the delegates for.</param>
    public SizeCalculator(ISizeCalculator<T> implementation)
      : base(implementation)
    {
      Provider = Implementation.Provider;
      GetDefaultSize = Implementation.GetDefaultSize;
      GetValueSize = Implementation.GetValueSize;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    public SizeCalculator(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      GetDefaultSize = Implementation.GetDefaultSize;
      GetValueSize = Implementation.GetValueSize;
    }
  }
}