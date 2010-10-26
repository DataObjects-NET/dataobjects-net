// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.23

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Gets the specified <see cref="RangeSet{T}"/> from the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class RangeSetProvider : UnaryProvider
  {
    private Func<RangeSet<Entire<Tuple>>> compiledRange;

    /// <summary>
    /// Gets the range set parameter.
    /// </summary>
    public Expression<Func<RangeSet<Entire<Tuple>>>> Range { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Range"/>.
    /// </summary>
    public Func<RangeSet<Entire<Tuple>>> CompiledRange
    {
      get {
        if (compiledRange==null)
          compiledRange = Range.CachingCompile();
        return compiledRange;
      }
      internal set { compiledRange = value; }
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Range.ToString(true);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="range">The <see cref="Range"/> property value.</param>
    public RangeSetProvider(CompilableProvider source, Expression<Func<RangeSet<Entire<Tuple>>>> range)
      : base(ProviderType.RangeSet, source)
    {
      Range = range;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="range">The value for <see cref="Range"/> function property.</param>
    public RangeSetProvider(CompilableProvider source, RangeSet<Entire<Tuple>> range)
      : base(ProviderType.RangeSet, source)
    {
      Range = () => range;
    }
  }
}
