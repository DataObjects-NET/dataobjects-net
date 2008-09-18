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
  /// Gets the specified <see cref="Range"/> from the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class RangeProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the range parameter.
    /// </summary>
    public Func<Range<IEntire<Tuple>>> Range { get; private set; }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Range.ToString();
    }


    // Constructor

    private RangeProvider(CompilableProvider source, Func<Range<IEntire<Tuple>>> range, bool hidden)
      : base(source)
    {
      Range = range;
    }


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