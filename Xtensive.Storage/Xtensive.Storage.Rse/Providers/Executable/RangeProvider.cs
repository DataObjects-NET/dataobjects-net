// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.12

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Indexing.Measures;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class RangeProvider : UnaryExecutableProvider,
    IOrderedEnumerable<Tuple,Tuple>,
    IRangeMeasurable<Tuple,Tuple>
  {
    private readonly Range<IEntire<Tuple>> tupleRange;

    #region Implementation of IOrderedEnumerable<Tuple,Tuple>

    public Converter<Tuple, Tuple> KeyExtractor
    {
      get
      {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.KeyExtractor;
      }
    }

    public AdvancedComparer<Tuple> KeyComparer
    {
      get
      {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.KeyComparer;
      }
    }

    public AdvancedComparer<IEntire<Tuple>> EntireKeyComparer
    {
      get
      {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.EntireKeyComparer;
      }
    }

    public Func<IEntire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get
      {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.AsymmetricKeyCompare;
      }
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<IEntire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      Range<IEntire<Tuple>> intersected = tupleRange.Intersect(range, EntireKeyComparer);
      return sourceEnumerable.GetKeys(intersected);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<IEntire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      Range<IEntire<Tuple>> intersected = tupleRange.Intersect(range, EntireKeyComparer);
      return sourceEnumerable.GetItems(intersected);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<IEntire<Tuple>> ray)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      if (tupleRange.Contains(ray.Point, EntireKeyComparer))
        return sourceEnumerable.Seek(ray);
      return new SeekResult<Tuple>(SeekResultType.None, null);
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<IEntire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      var intersect = tupleRange.Intersect(range, EntireKeyComparer);
      IIndexReader<Tuple, Tuple> indexReader = sourceEnumerable.CreateReader(intersect);
      return indexReader;
    }

    #endregion
    
    #region Implementation of IRangeMeasurable<Tuple,Tuple>

    /// <inheritdoc/>
    public long Count
    {
      get
      {
        return (long)GetMeasureResult("count");
      }
    }

    public bool HasMeasures
    {
      get
      {
        var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
        return sourceMeasurable.HasMeasures;
      }
    }

    public IMeasureSet<Tuple> Measures
    {
      get
      {
        var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
        return sourceMeasurable.Measures;
      }
    }

    /// <inheritdoc/>
    public object GetMeasureResult(string name)
    {
      var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
      return sourceMeasurable.GetMeasureResult(tupleRange, name);
    }

    /// <inheritdoc/>
    public object[] GetMeasureResults(params string[] names)
    {
      var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
      return sourceMeasurable.GetMeasureResults(tupleRange, names);
    }

    /// <inheritdoc/>
    public object GetMeasureResult(Range<IEntire<Tuple>> range, string name)
    {
      var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
      Range<IEntire<Tuple>> intersected = tupleRange.Intersect(range, EntireKeyComparer);
      return sourceMeasurable.GetMeasureResult(intersected, name);
    }

    /// <inheritdoc/>
    public object[] GetMeasureResults(Range<IEntire<Tuple>> range, params string[] names)
    {
      var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
      Range<IEntire<Tuple>> intersected = tupleRange.Intersect(range, EntireKeyComparer);
      return sourceMeasurable.GetMeasureResults(intersected, names);
    }

    #endregion

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      return sourceEnumerable.GetItems(tupleRange);
    }

    protected override void Initialize()
    {
    }


    // Constructors

    public RangeProvider(CompilableProvider origin, ExecutableProvider provider, Range<IEntire<Tuple>> range) 
      : base (origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      tupleRange = range;
    }
  }
}