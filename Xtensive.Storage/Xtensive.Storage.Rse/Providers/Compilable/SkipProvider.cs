// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;
using System.Linq.Expressions;
using Xtensive.Internals.DocTemplates;
using Xtensive.Helpers;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that skips result records from <see cref="UnaryProvider.Source"/>. Skip amount is specified using <see cref="Count"/> property.
  /// </summary>
  [Serializable]
  public sealed class SkipProvider : UnaryProvider
  {
    /// <summary>
    /// Skip amount function.
    /// </summary>
    public Func<int> Count { get; private set; }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Count.ToString();
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="count">The <see cref="Count"/> property value.</param>
    public SkipProvider(CompilableProvider provider, Func<int> count)
      : base(ProviderType.Skip, provider)
    {
      Count = count;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="count">The value for <see cref="Count"/> function property.</param>
    public SkipProvider(CompilableProvider provider, int count)
      : base(ProviderType.Skip, provider)
    {
      Count = () => count;
    }
  }
}