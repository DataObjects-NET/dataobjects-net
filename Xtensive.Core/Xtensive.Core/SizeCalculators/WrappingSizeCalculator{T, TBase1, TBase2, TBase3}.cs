// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Reflection;

namespace Xtensive.SizeCalculators
{
  /// <summary>
  /// Base class for any wrapping <see cref="ISizeCalculator{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to calculate sizes for.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  /// <typeparam name="TBase3">Third base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingSizeCalculator<T, TBase1, TBase2, TBase3> : SizeCalculatorBase<T>
  {
    /// <summary>
    /// Size calculator for the first base (wrapped) type.
    /// </summary>
    protected readonly SizeCalculator<TBase1> BaseSizeCalculator1;
    /// <summary>
    /// Size calculator for the second base (wrapped) type.
    /// </summary>
    protected readonly SizeCalculator<TBase2> BaseSizeCalculator2;
    /// <summary>
    /// Size calculator for the third base (wrapped) type.
    /// </summary>
    protected readonly SizeCalculator<TBase3> BaseSizeCalculator3;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Size calculator provider this wrapper bound to.</param>
    public WrappingSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
      BaseSizeCalculator1 = provider.GetSizeCalculator<TBase1>();
      if (!TypeHelper.IsFinal<TBase1>() && !(BaseSizeCalculator1.Implementation is IFinalAssociate))
        BaseSizeCalculator1 = null;
      BaseSizeCalculator2 = provider.GetSizeCalculator<TBase2>();
      if (!TypeHelper.IsFinal<TBase2>() && !(BaseSizeCalculator2.Implementation is IFinalAssociate))
        BaseSizeCalculator2 = null;
      BaseSizeCalculator3 = provider.GetSizeCalculator<TBase3>();
      if (!TypeHelper.IsFinal<TBase3>() && !(BaseSizeCalculator3.Implementation is IFinalAssociate))
        BaseSizeCalculator3 = null;
    }
  }
}