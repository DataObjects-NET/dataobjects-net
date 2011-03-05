// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.21

using System;
using Xtensive.Resources;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Testing
{
  /// <summary>
  /// Base class for any wrapping <see cref="IInstanceGenerator{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to generate random instances for.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingInstanceGenerator<T, TBase> : InstanceGeneratorBase<T>
  {
    /// <summary>
    /// Generator for base (wrapped) type.
    /// </summary>
    protected readonly IInstanceGenerator<TBase> BaseGenerator;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Instance generator provider this generator is bound to.</param>
    public WrappingInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
      BaseGenerator = provider.GetInstanceGenerator<TBase>();
    }
  }
}