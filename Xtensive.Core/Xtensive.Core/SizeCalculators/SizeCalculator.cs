// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.11

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Threading;

namespace Xtensive.SizeCalculators
{
  /// <summary>
  /// Provides delegates allowing to call size calculator methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="ISizeCalculator{T}"/> generic argument.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public sealed class SizeCalculator<T> : MethodCacheBase<ISizeCalculator<T>>,
    IHasSizeCalculator
  {
    private static ThreadSafeCached<SizeCalculator<T>> cachedCalculator =
      ThreadSafeCached<SizeCalculator<T>>.Create(new object());

    private static readonly object _lock = new object();
    private static volatile SizeCalculator<T> @default;

    /// <summary>
    /// Gets default size calculator for type <typeparamref name="T"/>
    /// (uses <see cref="SizeCalculatorProvider.Default"/> <see cref="SizeCalculatorProvider"/>).
    /// </summary>
    public static SizeCalculator<T> Default {
      [DebuggerStepThrough]
      get {
        return cachedCalculator.GetValue(
          () => SizeCalculatorProvider.Default.GetSizeCalculator<T>());
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