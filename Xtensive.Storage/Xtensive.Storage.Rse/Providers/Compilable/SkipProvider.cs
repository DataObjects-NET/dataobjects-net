// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that skips result records from <see cref="UnaryProvider.Source"/>. Skip amount is specified using <see cref="Count"/> property.
  /// </summary>
  [Serializable]
  public sealed class SkipProvider : UnaryProvider
  {
    private Func<int> compiledCount;

    /// <summary>
    /// Skip amount function.
    /// </summary>
    public Expression<Func<int>> Count { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Count"/>.
    /// </summary>
    public Func<int> CompiledCount {
      get {
        if (compiledCount==null)
          compiledCount = Count.Compile();
        return compiledCount;
      }
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Count.ToString(true);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="count">The <see cref="Count"/> property value.</param>
    public SkipProvider(CompilableProvider provider, Expression<Func<int>> count)
      : base(provider)
    {
      Count = count;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="count">The value for <see cref="Count"/> function property.</param>
    public SkipProvider(CompilableProvider provider, int count)
      : base(provider)
    {
      Count = () => count;
    }
  }
}