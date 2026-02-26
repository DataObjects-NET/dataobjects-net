// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

using System;
using Xtensive.Core;


namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Base class for any wrapping <see cref="IArithmetic{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to provide arithmetic operations for.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingArithmetic<T, TBase> : ArithmeticBase<T>
  {
    /// <summary>
    /// Arithmetic delegates for <typeparamref name="TBase"/> type.
    /// </summary>
    protected readonly ArithmeticStruct<TBase> BaseArithmetic;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">Arithmetic provider this instance is bound to.</param>
    /// <param name="rules">Arithmetic rules.</param>
    public WrappingArithmetic(IArithmeticProvider provider, ArithmeticRules rules)
      : base(provider, rules)
    {
      ArgumentNullException.ThrowIfNull(provider, "provider");
      BaseArithmetic = provider.GetArithmetic<TBase>();
    }
  }
}