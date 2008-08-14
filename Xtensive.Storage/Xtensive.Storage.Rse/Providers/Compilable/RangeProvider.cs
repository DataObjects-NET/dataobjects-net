// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider for range operation over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class RangeProvider : UnaryProvider
  {
    /// <summary>
    /// Range parameter function.
    /// </summary>
    public Func<Range<IEntire<Tuple>>> Range { get; private set; }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Range.ToString();
    }


    // Constructor

#pragma warning disable 168
    private RangeProvider(CompilableProvider source, Func<Range<IEntire<Tuple>>> range, bool hidden)
      : base(source)
    {
      Range = range;
    }
#pragma warning restore 168


    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="range">The <see cref="Range"/> property value.</param>
    public RangeProvider(CompilableProvider source, Func<Range<IEntire<Tuple>>> range)
      : this(source, range, true)
    {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="range">The value for <see cref="Range"/> function property.</param>
    public RangeProvider(CompilableProvider source, Range<IEntire<Tuple>> range)
      : this(source, () => range, true)
    {}
  }
}