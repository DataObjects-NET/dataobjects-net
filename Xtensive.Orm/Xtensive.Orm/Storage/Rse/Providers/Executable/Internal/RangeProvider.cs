// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.12

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Indexing.Measures;
using Xtensive.Helpers;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class RangeProvider : UnaryExecutableProvider<Compilable.RangeProvider>,
    IOrderedEnumerable<Tuple,Tuple>,
    IRangeMeasurable<Tuple,Tuple>
  {
    private const string ToString_RangeParameters = "{0}, Value: {1}";

    #region Cached properties

    private const string CachedRangeName = "CachedRange";

    private Range<Entire<Tuple>> CachedRange
    {
      get { return (Range<Entire<Tuple>>)GetCachedValue<object>(EnumerationContext.Current, CachedRangeName); }
      set { SetCachedValue(EnumerationContext.Current, CachedRangeName, (object)value); }
    }

    #endregion

    #region Implementation of IOrderedEnumerable<Tuple,Tuple>

    public Converter<Tuple, Tuple> KeyExtractor {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.KeyExtractor;
      }
    }

    public AdvancedComparer<Tuple> KeyComparer {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.KeyComparer;
      }
    }

    public AdvancedComparer<Entire<Tuple>> EntireKeyComparer {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.EntireKeyComparer;
      }
    }

    public Func<Entire<Tuple>, Tuple, int> AsymmetricKeyCompare {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.AsymmetricKeyCompare;
      }
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<Entire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      Range<Entire<Tuple>> intersected = CachedRange.Intersect(range, EntireKeyComparer);
      return sourceEnumerable.GetKeys(intersected);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<Entire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      Range<Entire<Tuple>> intersected = CachedRange.Intersect(range, EntireKeyComparer);
      return sourceEnumerable.GetItems(intersected);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(RangeSet<Entire<Tuple>> rangeSet)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      var intersected = rangeSet.Intersect(new RangeSet<Entire<Tuple>>(CachedRange, EntireKeyComparer));
      return sourceEnumerable.GetItems(intersected);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<Entire<Tuple>> ray)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      if (CachedRange.Contains(ray.Point, EntireKeyComparer))
        return sourceEnumerable.Seek(ray);
      return new SeekResult<Tuple>(SeekResultType.None, null);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Tuple key)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      if (CachedRange.Contains(new Entire<Tuple>(key), EntireKeyComparer))
        return sourceEnumerable.Seek(key);
      return new SeekResult<Tuple>(SeekResultType.None, null);
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<Entire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      var intersect = CachedRange.Intersect(range, EntireKeyComparer);
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
      return sourceMeasurable.GetMeasureResult(CachedRange, name);
    }

    /// <inheritdoc/>
    public object[] GetMeasureResults(params string[] names)
    {
      var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
      return sourceMeasurable.GetMeasureResults(CachedRange, names);
    }

    /// <inheritdoc/>
    public object GetMeasureResult(Range<Entire<Tuple>> range, string name)
    {
      var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
      Range<Entire<Tuple>> intersected = CachedRange.Intersect(range, EntireKeyComparer);
      return sourceMeasurable.GetMeasureResult(intersected, name);
    }

    /// <inheritdoc/>
    public object[] GetMeasureResults(Range<Entire<Tuple>> range, params string[] names)
    {
      var sourceMeasurable = Source.GetService<IRangeMeasurable<Tuple, Tuple>>(true);
      Range<Entire<Tuple>> intersected = CachedRange.Intersect(range, EntireKeyComparer);
      return sourceMeasurable.GetMeasureResults(intersected, names);
    }

    #endregion

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedRange = Origin.CompiledRange.Invoke();
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      return sourceEnumerable.GetItems(CachedRange);
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      Range<Entire<Tuple>>? range = null;
      try {
        range = Origin.CompiledRange.Invoke();
      }
      catch {}
      return string.Format(ToString_RangeParameters,
        base.ParametersToString(),
        range.HasValue ? range.GetValueOrDefault().ToString() : Strings.NotAvailable);
    }


    // Constructors

    public RangeProvider(Compilable.RangeProvider origin, ExecutableProvider provider) 
      : base (origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();
    }
  }
}