// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.13

using System;
using System.Runtime.Serialization;
using Xtensive.Arithmetic;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Base class for any additive measure.
  /// </summary>
  /// <typeparam name="TItem">Type of measured item.</typeparam>
  /// <typeparam name="TResult">Type of measure value.</typeparam>
  [Serializable]
  public abstract class AdditiveMeasure<TItem, TResult> : MeasureBase<TItem, TResult>
  {
    /// <summary>
    /// The arithmetics.
    /// </summary>
    protected static readonly Arithmetic<TResult> Arithmetic = Arithmetic<TResult>.Default;

    /// <inheritdoc/>
    protected override bool Add(TResult extracted)
    {
      Result = Arithmetic.Add(Result, extracted);
      return true;
    }

    /// <inheritdoc/>
    protected override bool Subtract(TResult extracted)
    {
      Result = Arithmetic.Subtract(Result, extracted);
      return true;
    }

    /// <inheritdoc/>
    public override bool AddWith(IMeasure<TItem> measure)
    {
      var typedMeasure = (IMeasure<TItem, TResult>) measure;
      return Add(typedMeasure.Result);
    }

    /// <inheritdoc/>
    public override bool SubtractWith(IMeasure<TItem> measure)
    {
      var typedMeasure = (IMeasure<TItem, TResult>) measure;
      return Subtract(typedMeasure.Result);
    }

    /// <inheritdoc/>
    public override IMeasure<TItem> Add(IMeasure<TItem> measure)
    {
      var typedMeasure = (IMeasure<TItem, TResult>) measure;
      return CreateNew(Arithmetic.Add(Result, typedMeasure.Result));
    }

    /// <inheritdoc/>
    public override IMeasure<TItem> Subtract(IMeasure<TItem> measure)
    {
      var typedMeasure = (IMeasure<TItem, TResult>)measure;
      TResult result = HasResult ? Result : default(TResult);
      return CreateNew(Arithmetic.Subtract(Result, typedMeasure.Result));
    }

    /// <inheritdoc/>
    public override void Reset()
    {
      Result = default(TResult);
    }


    // Constructors

    /// <inheritdoc/>
    protected AdditiveMeasure(string name, Converter<TItem, TResult> resultExtractor)
      : base(name, resultExtractor, default(TResult))
    {
    }

    /// <inheritdoc/>
    protected AdditiveMeasure(string name, Converter<TItem, TResult> resultExtractor, TResult result)
      : base(name, resultExtractor, result)
    {
    }
  }
}