// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Differential
{
  /// <summary>
  /// Differential index.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <typeparam name="TImpl">The type of the implementation.</typeparam>
  public class DifferentialIndex<TKey, TItem, TImpl> : UniqueOrderedIndexBase<TKey, TItem>
    where TImpl : IUniqueOrderedIndex<TKey, TItem>, IConfigurable<IndexConfigurationBase<TKey, TItem>>, new()
  {
    private IUniqueOrderedIndex<TKey, TItem> origin;
    private IUniqueOrderedIndex<TKey, TItem> insertions;
    private IUniqueOrderedIndex<TKey, TItem> removals;
    private Converter<TKey, IEntire<TKey>> entireConverter;
    private IMeasureResultSet<TItem> measureResults;

    #region Properties: Origin, Insertions, Removals, MeasureResults, EntireConverter

    /// <summary>
    /// Gets the origin.
    /// </summary>
    public IUniqueOrderedIndex<TKey, TItem> Origin
    {
      [DebuggerStepThrough]
      get { return origin; }
    }

    /// <summary>
    /// Gets the insertions.
    /// </summary>
    public IUniqueOrderedIndex<TKey, TItem> Insertions
    {
      [DebuggerStepThrough]
      get { return insertions; }
    }

    /// <summary>
    /// Gets the removals.
    /// </summary>
    public IUniqueOrderedIndex<TKey, TItem> Removals
    {
      [DebuggerStepThrough]
      get { return removals; }
    }

    /// <inheritdoc/>
    public IMeasureResultSet<TItem> MeasureResults
    {
      [DebuggerStepThrough]
      get { return measureResults; }
    }

    /// <summary>
    /// Gets the entire converter.
    /// </summary>
    public Converter<TKey, IEntire<TKey>> EntireConverter
    {
      [DebuggerStepThrough]
      get { return entireConverter; }
    }

    #endregion

    #region GetItem, Contains, ContainsKey methods

    /// <inheritdoc/>
    public override TItem GetItem(TKey key)
    {
      if (insertions.ContainsKey(key))
        return insertions.GetItem(key);
      if (removals.ContainsKey(key))
        throw new KeyNotFoundException();
      return origin.GetItem(key);
    }

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      if (insertions.Contains(item))
        return true;
      if (!removals.Contains(item) && origin.Contains(item))
        return true;
      return false;
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      if (insertions.ContainsKey(key))
        return true;
      if (!removals.ContainsKey(key) && origin.ContainsKey(key))
        return true;
      return false;
    }

    #endregion

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    public override void Add(TItem item)
    {
      if ((origin.Contains(item) && removals.Contains(item) && !insertions.Contains(item)) || (!origin.Contains(item) && !insertions.Contains(item))) {
        insertions.Add(item);
        measureResults.Add(item);
      }
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      if (origin.Contains(item) && !removals.Contains(item)) {
        removals.Add(item);
        measureResults.Subtract(item);
        return true;
      }
      if (insertions.Contains(item)) {
        insertions.Remove(item);
        if (!removals.Contains(item))
          removals.Add(item);
        measureResults.Subtract(item);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override bool RemoveKey(TKey key)
    {
      if (origin.ContainsKey(key) && !removals.ContainsKey(key)) {
        removals.Add(GetItem(key));
        measureResults.Subtract(GetItem(key));
        return true;
      }
      if (insertions.ContainsKey(key)) {
        insertions.RemoveKey(key);
        if (!removals.ContainsKey(key))
          removals.Add(GetItem(key));
        measureResults.Subtract(GetItem(key));
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override void Replace(TItem item)
    {
      if (insertions.Contains(item))
        insertions.Replace(item);
      else if (!removals.Contains(item) && origin.Contains(item)) {
        removals.Add(item);
        insertions.Add(item);
      }
      else
        throw new ArgumentOutOfRangeException("item");
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      foreach (TItem item in origin)
        if (!removals.Contains(item))
          removals.Add(item);

      foreach (TItem item in insertions)
        if (!removals.Contains(item))
          removals.Add(item);
      insertions.Clear();
      measureResults.Reset();
    }

    #endregion

    #region Measure related methods.

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
    public override object GetMeasureResult(Range<IEntire<TKey>> range, string name)
    {
      IMeasure<TItem> measure = Measures[name];
      if (measure==null)
        throw new InvalidOperationException(String.Format(Strings.ExMeasureIsNotDefined, name));
      if (range.IsEmpty)
        return measure.CreateNew().Result;

      if (range.Equals(this.GetFullRange()))
        return measureResults[name].Result;

      IMeasure<TItem> result = MeasureUtils<TItem>.BatchCalculate(measure, GetItems(range));
      return result.Result;
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(Range<IEntire<TKey>> range, params string[] names)
    {
      IMeasure<TItem> measure;
      foreach (string name in names) {
        measure = Measures[name];
        if (measure==null)
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

    #region Seek, CreateReader methods.

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(Ray<IEntire<TKey>> ray)
    {
      SeekResult<TItem> originResult = origin.Seek(ray);
      SeekResult<TItem> insertionsResult = insertions.Seek(ray);
      SeekResult<TItem> removalsResult = removals.Seek(ray);

      if ((originResult.ResultType==SeekResultType.Exact) && (removalsResult.ResultType!=SeekResultType.Exact))
        return originResult;

      if ((insertionsResult.ResultType==SeekResultType.Exact) || (originResult.ResultType==SeekResultType.None))
        return insertionsResult;

      if ((insertionsResult.ResultType==SeekResultType.None) && (!removals.Contains(originResult.Result)))
        return originResult;

      if (((insertionsResult.ResultType==SeekResultType.None) && (!removals.Contains(originResult.Result))) || ((((KeyComparer.Compare(KeyExtractor(originResult.Result), KeyExtractor(insertionsResult.Result)) < 0) &&
        (ray.Direction==Direction.Positive)) || ((KeyComparer.Compare(KeyExtractor(originResult.Result), KeyExtractor(insertionsResult.Result)) > 0) &&
          (ray.Direction==Direction.Negative))) && (!removals.Contains(originResult.Result))))
        return originResult;

      return insertionsResult;
    }

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(TKey key)
    {
      return Seek(new Ray<IEntire<TKey>>(Entire<TKey>.Create(key)));
    }

    /// <inheritdoc/>
    public override IIndexReader<TKey, TItem> CreateReader(Range<IEntire<TKey>> range)
    {
      return new DifferentialIndexReader<TKey, TItem, TImpl>(this, range);
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      var configuration = (DifferentialIndexConfiguration<TKey, TItem>) Configuration;
      origin = configuration.Origin;
      insertions = IndexFactory.CreateUniqueOrdered<TKey, TItem, TImpl>(((UniqueOrderedIndexBase<TKey, TItem>) origin).Configuration);
      removals = IndexFactory.CreateUniqueOrdered<TKey, TItem, TImpl>(((UniqueOrderedIndexBase<TKey, TItem>) origin).Configuration);
      entireConverter = (key => Entire<TKey>.Create(key));
      measureResults = new MeasureResultSet<TItem>(Measures);
      foreach (TItem item in origin)
        measureResults.Add(item);
    }


    // Constructors

    public DifferentialIndex()
    {
    }
  }
}
