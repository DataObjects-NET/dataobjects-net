// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.21

using System;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Base class for any wrapping <see cref="IInstanceGenerator{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to generate random instances for.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  /// <typeparam name="TBase3">Third base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingInstanceGenerator<T, TBase1, TBase2, TBase3> : InstanceGeneratorBase<T>
  {
    /// <summary>
    /// Generator for the first base (wrapped) type.
    /// </summary>
    protected readonly IInstanceGenerator<TBase1> BaseGenerator1;

    /// <summary>
    /// Generator for the second base (wrapped) type.
    /// </summary>
    protected readonly IInstanceGenerator<TBase2> BaseGenerator2;

    /// <summary>
    /// Generator for the third base (wrapped) type.
    /// </summary>
    protected readonly IInstanceGenerator<TBase3> BaseGenerator3;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Instance generator provider this generator is bound to.</param>
    public WrappingInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
      BaseGenerator1 = provider.GetInstanceGenerator<TBase1>();
      BaseGenerator2 = provider.GetInstanceGenerator<TBase2>();
      BaseGenerator3 = provider.GetInstanceGenerator<TBase3>();
    }
  }
}