// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
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
    private ThreadSafeCached<Func<Range<IEntire<Tuple>>>> compiledRange = ThreadSafeCached<Func<Range<IEntire<Tuple>>>>.Create(new object());

    /// <summary>
    /// Gets the range parameter.
    /// </summary>
    public Expression<Func<Range<IEntire<Tuple>>>> Range { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Range"/>.
    /// </summary>
    public Func<Range<IEntire<Tuple>>> CompiledRange {
      get {
        return compiledRange.GetValue(_this => _this.Range.Compile(), this);
      }
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Range.ToString();
    }


    // Constructor

    private RangeProvider(CompilableProvider source, Expression<Func<Range<IEntire<Tuple>>>> range, bool hidden)
      : base(source)
    {
      Range = range;
    }


    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="range">The <see cref="Range"/> property value.</param>
    public RangeProvider(CompilableProvider source, Expression<Func<Range<IEntire<Tuple>>>> range)
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