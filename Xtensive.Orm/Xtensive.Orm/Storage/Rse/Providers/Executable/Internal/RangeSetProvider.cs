// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
  internal sealed class RangeSetProvider : UnaryExecutableProvider<Compilable.RangeSetProvider>,
    IOrderedEnumerable<Tuple, Tuple>
  {
    private const string ToString_RangeParameters = "{0}, Value: {1}";

    #region Cached properties

    private const string CachedRangeName = "CachedRangeSet";

    private RangeSet<Entire<Tuple>> CachedRangeSet
    {
      get { return (RangeSet<Entire<Tuple>>)GetCachedValue<object>(EnumerationContext.Current, CachedRangeName); }
      set { SetCachedValue(EnumerationContext.Current, CachedRangeName, (object)value); }
    }

    #endregion

    #region Implementation of IOrderedEnumerable<Tuple,Tuple>

    public Converter<Tuple, Tuple> KeyExtractor
    {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.KeyExtractor;
      }
    }

    public AdvancedComparer<Tuple> KeyComparer
    {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.KeyComparer;
      }
    }

    public AdvancedComparer<Entire<Tuple>> EntireKeyComparer
    {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.EntireKeyComparer;
      }
    }

    public Func<Entire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get {
        var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return sourceEnumerable.AsymmetricKeyCompare;
      }
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<Entire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      RangeSet<Entire<Tuple>> intersected = CachedRangeSet.Intersect(new RangeSet<Entire<Tuple>>(range, EntireKeyComparer));
      foreach (var r in intersected) {
        var keys = sourceEnumerable.GetKeys(r);
        foreach(var item in keys)
          yield return item;
      }
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<Entire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      RangeSet<Entire<Tuple>> intersected = CachedRangeSet.Intersect(new RangeSet<Entire<Tuple>>(range, EntireKeyComparer));
      return sourceEnumerable.GetItems(intersected);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(RangeSet<Entire<Tuple>> rangeSet)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      RangeSet<Entire<Tuple>> intersected = CachedRangeSet.Intersect(rangeSet);
      return sourceEnumerable.GetItems(intersected);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<Entire<Tuple>> ray)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      foreach (var range in CachedRangeSet)
        if (range.Contains(ray.Point, EntireKeyComparer))
          return sourceEnumerable.Seek(ray);
      return new SeekResult<Tuple>(SeekResultType.None, null);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Tuple key)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      foreach (var range in CachedRangeSet)
        if (range.Contains(new Entire<Tuple>(key), EntireKeyComparer))
          return sourceEnumerable.Seek(key);
      return new SeekResult<Tuple>(SeekResultType.None, null);
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<Entire<Tuple>> range)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      var intersect = CachedRangeSet.Intersect(new RangeSet<Entire<Tuple>>(range, EntireKeyComparer));
      var readers = new List<IIndexReader<Tuple, Tuple>>();
      foreach (var r in intersect)
        readers.Add(sourceEnumerable.CreateReader(r));
      return new RangeSetReader<Tuple, Tuple>(readers);
    }

    #endregion

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedRangeSet = Origin.CompiledRange.Invoke();
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      return sourceEnumerable.GetItems(CachedRangeSet);
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      RangeSet<Entire<Tuple>> range = null;
      try {
        range = Origin.CompiledRange.Invoke();
      }
      catch { }
      return string.Format(ToString_RangeParameters,
        base.ParametersToString(),
        range != null ? range.ToString() : Strings.NotAvailable);
    }


    // Constructors

    public RangeSetProvider(Compilable.RangeSetProvider origin, ExecutableProvider provider)
      : base(origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();
    }
  }
}