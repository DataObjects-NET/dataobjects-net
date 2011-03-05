// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Simple unique ordered in-memory index based on ordered <see cref="List{T}"/>.
  /// </summary>
  /// <typeparam name="TKey">Type of index key.</typeparam>
  /// <typeparam name="TItem">Type of index value.</typeparam>
  public sealed class SortedListIndex<TKey, TItem>: UniqueOrderedIndexBase<TKey, TItem>,
    IHasMeasureResults<TItem>,
    IHasVersion<int>
  {
    private List<TItem> items;
    private int version;
    private IMeasureResultSet<TItem> measureResults;

    /// <inheritdoc/>
    public IMeasureResultSet<TItem> MeasureResults
    {
      [DebuggerStepThrough]
      get { return measureResults; }
    }

    /// <inheritdoc/>
    public override long Count
    {
      [DebuggerStepThrough]
      get { return items.Count; }
    }

    /// <summary>
    /// Gets the item from the underlying sorted list by its <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified <paramref name="index"/>.</returns>
    public TItem this[int index] {
      [DebuggerStepThrough]
      get { return items[index]; }
    }

    #region GetItem, Contains, ContainsKey methods

    /// <inheritdoc/>
    public override TItem GetItem(TKey key)
    {
      SeekResult<TItem> seekResult = Seek(new Ray<Entire<TKey>>(new Entire<TKey>(key)));
      if (seekResult.ResultType==SeekResultType.Exact) {
        return seekResult.Result;
      }
      else {
        throw new KeyNotFoundException();
      }
    }

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      return ContainsKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      return InternalSeek(new Ray<Entire<TKey>>(new Entire<TKey>(key))).ResultType == SeekResultType.Exact;
    }

    #endregion

    #region Seek, CreateReader methods

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(TKey key)
    {
      TItem result;
      var seekResult = InternalSeek(key);
      if (seekResult.ResultType == SeekResultType.Exact || seekResult.ResultType == SeekResultType.Nearest)
        result = seekResult.Pointer.Current;
      else
        result = default(TItem);
      return new SeekResult<TItem>(seekResult.ResultType, result);
    }

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(Ray<Entire<TKey>> ray)
    {
      TItem result;
      var seekResult = InternalSeek(ray);
      if (seekResult.ResultType == SeekResultType.Exact || seekResult.ResultType == SeekResultType.Nearest)
        result = seekResult.Pointer.Current;
      else
        result = default(TItem);
      return new SeekResult<TItem>(seekResult.ResultType, result);
    }

    /// <inheritdoc/>
    public override IIndexReader<TKey, TItem> CreateReader(Range<Entire<TKey>> range)
    {
      return new SortedListIndexReader<TKey, TItem>(this, range);
    }

    #endregion

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"><paramref name="item"/> is already added.</exception>
    public override void Add(TItem item)
    {
      var seekResult = InternalSeek(new Ray<Entire<TKey>>(new Entire<TKey>(KeyExtractor(item))));
      if (seekResult.ResultType==SeekResultType.Exact)
        throw new InvalidOperationException();
      Changed();
      items.Insert(seekResult.Pointer.Index, item);
      measureResults.Add(item);
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      return RemoveKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="item"/> could not be replaced.</exception>
    public override void Replace(TItem item)
    {
      var seekResult = InternalSeek(new Ray<Entire<TKey>>(new Entire<TKey>(KeyExtractor(item))));
      if (seekResult.ResultType != SeekResultType.Exact)
        throw new ArgumentOutOfRangeException("item");
      Changed();
      items[seekResult.Pointer.Index] = item;
    }

    /// <inheritdoc/>
    public override bool RemoveKey(TKey key)
    {
      var seekResult = InternalSeek(new Ray<Entire<TKey>>(new Entire<TKey>(key)));
      if (seekResult.ResultType != SeekResultType.Exact)
        return false;
      Changed();
      TItem item = seekResult.Pointer.Current;
      items.RemoveAt(seekResult.Pointer.Index);
      bool result = measureResults.Subtract(item);
      if (!result)
        MeasureUtils<TItem>.BatchRecalculate(measureResults, items);
      return true;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      Cleared();
      items.Clear();
      measureResults.Reset();
    }

    #endregion

    #region Measure related methods

    /// <inheritdoc/>
    public override object GetMeasureResult(string name)
    {
      return measureResults[name].Result;
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(params string[] names)
    {
      return MeasureUtils<TItem>.GetMeasurements(measureResults, names);
    }

    /// <inheritdoc/>
    public override object GetMeasureResult(Range<Entire<TKey>> range, string name)
    {
      IMeasure<TItem> measure = Measures[name];
      if (measure == null)
        throw new InvalidOperationException(String.Format(Strings.ExMeasureIsNotDefined, name));
      if (range.IsEmpty)
        return measure.CreateNew().Result;

      if (range.Equals(this.GetFullRange()))
        return measureResults[name].Result;
      
      IMeasure<TItem> result = MeasureUtils<TItem>.BatchCalculate(measure, GetItems(range));
      return result.Result;
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(Range<Entire<TKey>> range, params string[] names)
    {
      IMeasure<TItem> measure;

      foreach(string name in names) {
        measure = Measures[name];
        if (measure == null)
          throw new InvalidOperationException(String.Format(Strings.ExMeasureIsNotDefined, name));
     }

      if (range.IsEmpty) {
        object[] empty = new object[names.Length];
        int i = 0;
        foreach (string name in names) {
          measure = Measures[name];
          empty[i++] = measure.CreateNew().Result;
        }
        return empty;
      }

      if (range.Equals(this.GetFullRange()))
        return MeasureUtils<TItem>.GetMeasurements(measureResults, names);

      IMeasureSet<TItem> measureSet = MeasureUtils<TItem>.GetMeasures(Measures, names);
      IMeasureResultSet<TItem> result = MeasureUtils<TItem>.BatchCalculate(measureSet, GetItems(range));
      return MeasureUtils<TItem>.GetMeasurements(result, names);
    }

    #endregion

    #region IHasVersion<int> methods

    public int Version
    {
      [DebuggerStepThrough]
      get { return version; }
    }

    object IHasVersion.Version
    {
      [DebuggerStepThrough]
      get { return Version; }
    }

    #endregion

    #region Private \ internal methods

    private void Changed()
    {
      version++;
    }

    private void Cleared()
    {
      version = 0;
    }

    internal SeekResultPointer<SortedListIndexPointer<TKey, TItem>> InternalSeek(TKey key)
    {
      Func<TKey, TKey, int> compare = KeyComparer.Compare;
      SeekResultType resultType = SeekResultType.None;
      int index = 0;
      int maxIndex = items.Count - 1;
      while (index <= maxIndex) {
        int nextIndex = index + ((maxIndex - index) >> 1);
        int comparison = compare(key, KeyExtractor(items[nextIndex]));
        if (comparison == 0) {
          index = nextIndex;
          resultType = SeekResultType.Exact;
          break;
        }
        if (comparison > 0)
          index = nextIndex + 1;
        else
          maxIndex = nextIndex - 1;
      }
      if (resultType!=SeekResultType.Exact && index < items.Count)
        resultType = SeekResultType.Nearest;
      return new SeekResultPointer<SortedListIndexPointer<TKey,TItem>>(resultType, 
        new SortedListIndexPointer<TKey, TItem>(this, index));
    }

    internal SeekResultPointer<SortedListIndexPointer<TKey, TItem>> InternalSeek(Ray<Entire<TKey>> ray)
    {
      Func<Entire<TKey>, TKey, int> asymmetricKeyCompare = AsymmetricKeyCompare;
      SeekResultType resultType = SeekResultType.None;
      int index = 0;
      int maxIndex = items.Count - 1;
      while (index <= maxIndex) {
        int nextIndex = index + ((maxIndex - index) >> 1);
        int comparison = asymmetricKeyCompare(ray.Point, KeyExtractor(items[nextIndex]));
        if (comparison == 0) {
          index = nextIndex;
          resultType = SeekResultType.Exact;
          break;
        }
        if (comparison > 0)
          index = nextIndex + 1;
        else
          maxIndex = nextIndex - 1;
      }
      if (resultType != SeekResultType.Exact) {
        if (index < items.Count) {
          resultType = SeekResultType.Nearest;
          if (ray.Direction == Direction.Negative) {
            index--;
            if (index < 0)
              resultType = SeekResultType.None;
          }
        }
        else if (ray.Direction == Direction.Negative) {
          index = items.Count - 1;
          resultType = SeekResultType.Nearest;
        }
      }
      return new SeekResultPointer<SortedListIndexPointer<TKey,TItem>>(resultType, 
        new SortedListIndexPointer<TKey, TItem>(this, index));
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      items = new List<TItem>();
      measureResults = new MeasureResultSet<TItem>(Measures);
    }


    // Constructors

    /// <inheritdoc/>
    public SortedListIndex()
    {
    }

    /// <inheritdoc/>
    public SortedListIndex(IndexConfigurationBase<TKey, TItem> configuration)
      : base(configuration)
    {
    }
  }
}