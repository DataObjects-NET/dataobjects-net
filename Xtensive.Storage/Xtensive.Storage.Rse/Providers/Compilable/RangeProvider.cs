// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Gets the specified <see cref="Range"/> from the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class RangeProvider : UnaryProvider
  {
    private Func<Range<Entire<Tuple>>> compiledRange;

    /// <summary>
    /// Gets the range parameter.
    /// </summary>
    public Expression<Func<Range<Entire<Tuple>>>> Range { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Range"/>.
    /// </summary>
    public Func<Range<Entire<Tuple>>> CompiledRange {
      get {
        if (compiledRange==null)
          compiledRange = Range.Compile();
        return compiledRange;
      }
      internal set {
        compiledRange = value;
      }
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Range.ToString(true);
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="range">The <see cref="Range"/> property value.</param>
    public RangeProvider(CompilableProvider source, Expression<Func<Range<Entire<Tuple>>>> range)
      : base(ProviderType.Range, source)
    {
      Range = range;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="range">The value for <see cref="Range"/> function property.</param>
    public RangeProvider(CompilableProvider source, Range<Entire<Tuple>> range)
      : base(ProviderType.Range, source)
    {
      Range = () => range;
    }
  }
}