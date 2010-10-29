// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.13

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// A measure providing maximum of the items.
  /// </summary>
  /// <typeparam name="TItem">Type of measured item.</typeparam>
  /// <typeparam name="TResult">Type of measure value.</typeparam>
  [Serializable]
  public sealed class MaxMeasure<TItem, TResult> : ComparableMeasure<TItem, TResult>
  {
    /// <inheritdoc/>
    public override IMeasure<TItem> CreateNew()
    {
      return new MaxMeasure<TItem, TResult>(Name, ResultExtractor);
    }

    /// <inheritdoc/>
    public override IMeasure<TItem, TResult> CreateNew(TResult result)
    {
      return new MaxMeasure<TItem, TResult>(Name, ResultExtractor, result);
    }

    /// <inheritdoc/>
    protected override bool Add(TResult extracted)
    {
      if (!HasResult) {
        Result = extracted;
        Count = 1;
        return true;
      }
      // TODO: This code is equal to MinMeasure.Add except comparisonResult conditions.
      int comparisonResult = comparer.Compare(Result, extracted);
      if (comparisonResult < 0) {
        Result = extracted;
        Count = 1;
      }
      else if (comparisonResult==0)
        Count++;
      return true;
    }

    /// <inheritdoc/>
    protected override bool Subtract(TResult extracted)
    {
      // TODO: This code is equal to MinMeasure.Subtract except comparisonResult conditions.
      TResult result = HasResult ? Result : default(TResult);
      int comparisonResult = comparer.Compare(result, extracted);
      if (comparisonResult < 0) {
        Reset();
        return false;
      }
      if (comparisonResult==0)
        return --Count > 0;
      return true;
    }

    /// <inheritdoc/>
    public override bool AddWith(IMeasure<TItem> measure)
    {
      if (!measure.HasResult)
        return true;
      var typedMeasure = (IMeasure<TItem, TResult>)measure;
      return Add(typedMeasure.Result);
    }

    /// <inheritdoc/>
    public override bool SubtractWith(IMeasure<TItem> measure)
    {
      if (!measure.HasResult)
        return true;
      var typedMeasure = (IMeasure<TItem, TResult>)measure;
      return Subtract(typedMeasure.Result);
    }

    /// <inheritdoc/>
    public override IMeasure<TItem> Add(IMeasure<TItem> measure)
    {
      if (!measure.HasResult)
        return this;
      if (!HasResult)
        return measure;

      // TODO: This code is equal to MinMeasure.Add except comparisonResult conditions.
      var typedMeasure = (ComparableMeasure<TItem,TResult>)measure;
      int comparisonResult = comparer.Compare(Result, typedMeasure.Result);
      if (comparisonResult < 0)
        return new MaxMeasure<TItem, TResult>(Name, ResultExtractor, typedMeasure.Result);
      if (comparisonResult==0)
        return new MaxMeasure<TItem, TResult>(Name, ResultExtractor, Result, Count += typedMeasure.Count);
      return this;    
    }

    /// <inheritdoc/>
    public override IMeasure<TItem> Subtract(IMeasure<TItem> measure)
    {
      if (!measure.HasResult)
        return this;

      // TODO: This code is equal to MinMeasure.Subtract except comparisonResult conditions.
      var typedMeasure = (ComparableMeasure<TItem, TResult>)measure;
      int comparisonResult = comparer.Compare(Result, typedMeasure.Result);
      if (comparisonResult < 0)
        return new MaxMeasure<TItem, TResult>(Name, ResultExtractor);
      if (comparisonResult == 0) {
        int count = Count - typedMeasure.Count;
        if (count <= 0)
          return new MaxMeasure<TItem, TResult>(Name, ResultExtractor);
        return new MaxMeasure<TItem, TResult>(Name, ResultExtractor, Result, count);
      }
      return this;
    }


    // Constructors

    /// <inheritdoc/>
    public MaxMeasure(string name, Converter<TItem, TResult> resultExtractor)
      : base(name, resultExtractor)
    {}

    /// <inheritdoc/>
    private MaxMeasure(string name, Converter<TItem, TResult> resultExtractor, TResult result)
      : base(name, resultExtractor, result)
    {}

    /// <inheritdoc/>
    private MaxMeasure(string name, Converter<TItem, TResult> resultExtractor, TResult result, int count)
      : base(name, resultExtractor, result, count)
    {}
  }
}