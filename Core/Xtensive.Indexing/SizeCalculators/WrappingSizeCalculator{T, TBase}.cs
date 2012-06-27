// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Reflection;

namespace Xtensive.Indexing.SizeCalculators
{
  /// <summary>
  /// Base class for any wrapping <see cref="ISizeCalculator{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to calculate sizes for.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingSizeCalculator<T, TBase> : SizeCalculatorBase<T>
  {
    /// <summary>
    /// Size calculator for base (wrapped) type.
    /// </summary>
    protected readonly SizeCalculator<TBase> BaseSizeCalculator;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Size calculator provider this wrapper bound to.</param>
    public WrappingSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
      BaseSizeCalculator = provider.GetSizeCalculator<TBase>();
      if (!TypeHelper.IsFinal<TBase>() && !(BaseSizeCalculator.Implementation is IFinalAssociate))
        BaseSizeCalculator = null;
    }
  }
}