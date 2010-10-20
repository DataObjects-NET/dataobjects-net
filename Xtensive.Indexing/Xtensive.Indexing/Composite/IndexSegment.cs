// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// Describes segment of composite index.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class IndexSegment<TKey, TItem> : UniqueOrderedIndexBase<TKey, TItem>
    where TKey : Tuple
    where TItem : Tuple
  {
    private readonly CompositeIndex<TKey, TItem> compositeIndex;
    private string segmentName;
    private readonly int segmentNumber;
    private Converter<TKey, TKey> keyConverter;
    private Converter<TItem, TItem> itemConverter;
    private Converter<Entire<TKey>, Entire<TKey>> entireConverter;
    private IMeasureResultSet<TItem> measureResults;
    private Dictionary<TupleDescriptor, CutOutTransform> CutOutTransformDictionary;
    private Dictionary<TupleDescriptor, CutInTransform<int>> CutInTransformDictionary;

    #region Properties: SegmentName, SegmentNumber, CompositeIndex, EntireConverter,MeasureResults

    /// <summary>
    /// Gets the name of the segment.
    /// </summary>
    public string SegmentName
    {
      [DebuggerStepThrough]
      get { return segmentName; }
    }

    /// <summary>
    /// Gets the segment number.
    /// </summary>
    public int SegmentNumber
    {
      [DebuggerStepThrough]
      get { return segmentNumber; }
    }

    /// <summary>
    /// Gets the composite index this instance belongs to.
    /// </summary>
    public CompositeIndex<TKey, TItem> CompositeIndex
    {
      [DebuggerStepThrough]
      get { return compositeIndex; }
    }

    /// <summary>
    /// Gets the entire converter.
    /// </summary>
    public Converter<Entire<TKey>, Entire<TKey>> EntireConverter
    {
      [DebuggerStepThrough]
      get { return entireConverter; }
    }

    /// <inheritdoc/>
    public IMeasureResultSet<TItem> MeasureResults
    {
      [DebuggerStepThrough]
      get { return measureResults; }
    }

    #endregion

    #region GetItem, Contains, ContainsKey methods

    /// <inheritdoc/>
    public override TItem GetItem(TKey key)
    {
      CutOutTransform itemTransform = GetCutOutTransform(compositeIndex.Implementation.GetItem(keyConverter(key)).Descriptor, new Segment<int>(key.Count, 1));
      Tuple result = itemTransform.Apply(TupleTransformType.TransformedTuple, compositeIndex.Implementation.GetItem(keyConverter(key)));
      return (TItem) result;
    }

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      return compositeIndex.Implementation.Contains(itemConverter(item));
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      return compositeIndex.Implementation.ContainsKey(keyConverter(key));
    }

    #endregion

    #region Seek, CreateReader methods

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(TKey key)
    {
      return Seek(new Ray<Entire<TKey>>(new Entire<TKey>(key)));
    }

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(Ray<Entire<TKey>> ray)
    {
      Ray<Entire<TKey>> compositeRay = GetCompositeIndexRay(ray);

      SeekResult<TItem> result = compositeIndex.Implementation.Seek(compositeRay);
      if (result.ResultType!=SeekResultType.None) {
        var seekResult = new SeekResult<TItem>();
        if (result.Result.GetValueOrDefault<int>(compositeRay.Point.Value.Count - 1)==segmentNumber) {
          seekResult = new SeekResult<TItem>(result.ResultType, result.Result);
          CutOutTransform itemTransform = GetCutOutTransform(seekResult.Result.Descriptor, new Segment<int>(compositeRay.Point.Value.Count - 1, 1));
          Tuple resultTuple = itemTransform.Apply(TupleTransformType.TransformedTuple, seekResult.Result);
          return new SeekResult<TItem>(result.ResultType, (TItem) resultTuple);
        }
      }
      Entire<TKey> x, y;

      if (compositeRay.Direction==Direction.Negative) {
        x = new Entire<TKey>(InfinityType.Negative);
        y = new Entire<TKey>(compositeRay.Point.Value, compositeRay.Point.ValueType);
      }
      else if (compositeRay.Direction==Direction.Positive) {
        y = new Entire<TKey>(InfinityType.Positive);
        x = new Entire<TKey>(compositeRay.Point.Value, compositeRay.Point.ValueType);
      }
      else {
        x = new Entire<TKey>(compositeRay.Point.Value, compositeRay.Point.ValueType);
        y = x;
      }

      Range<Entire<TKey>> readerRange = new Range<Entire<TKey>>(x, y);
      IndexSegmentReader<TKey, TItem> reader = new IndexSegmentReader<TKey, TItem>(this, readerRange);
      reader.MoveTo(compositeRay.Point);
      if (reader.MoveNext())
        return new SeekResult<TItem>(SeekResultType.Nearest, reader.Current);
      return new SeekResult<TItem>(SeekResultType.None, default(TItem));
    }

    /// <inheritdoc/>
    public override IIndexReader<TKey, TItem> CreateReader(Range<Entire<TKey>> range)
    {
      return new IndexSegmentReader<TKey, TItem>(this, range);
    }

    #endregion

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    public override void Add(TItem item)
    {
      compositeIndex.Implementation.Add(itemConverter(item));
      measureResults.Add(item);
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      bool result = compositeIndex.Implementation.Remove(itemConverter(item));
      if (result)
        measureResults.Subtract(item);
      return result;
    }

    /// <inheritdoc/>
    public override void Replace(TItem item)
    {
      compositeIndex.Implementation.Replace(itemConverter(item));
    }

    /// <inheritdoc/>
    public override bool RemoveKey(TKey key)
    {
      bool result = compositeIndex.Implementation.RemoveKey(keyConverter(key));
      if (result) {
        TItem item = itemConverter(GetItem(key));
        measureResults.Subtract(item);
      }
      return result;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      foreach (TItem item in this)
        Remove(item);
      measureResults.Reset();
    }

    #endregion

    #region Measure related methods

    /// <inheritdoc/>
    public override object GetMeasureResult(Range<Entire<TKey>> range, string name)
    {
      Range<Entire<TKey>> compositeRange = new Range<Entire<TKey>>(
        entireConverter(range.EndPoints.First), entireConverter(range.EndPoints.Second));
      //string compositeName = GetCompositeIndexMeasureName(name);

      IMeasure<TItem> measure = Measures[name];
      if (measure==null)
        throw new InvalidOperationException(String.Format(Strings.ExMeasureIsNotDefined, name));
      if (compositeRange.IsEmpty)
        return measure.CreateNew().Result;

      if (range.Equals(this.GetFullRange()))
        return measureResults[name].Result;

      IMeasure<TItem> result = MeasureUtils<TItem>.BatchCalculate(measure, GetItems(compositeRange));
      return result.Result;
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(Range<Entire<TKey>> range, params string[] names)
    {
      Range<Entire<TKey>> compositeRange = new Range<Entire<TKey>>(
        entireConverter(range.EndPoints.First), entireConverter(range.EndPoints.Second));

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

      if (compositeRange.Equals(this.GetFullRange()))
        return MeasureUtils<TItem>.GetMeasurements(measureResults, names);

      IMeasureSet<TItem> measureSet = MeasureUtils<TItem>.GetMeasures(Measures, names);
      IMeasureResultSet<TItem> result = MeasureUtils<TItem>.BatchCalculate(measureSet, GetItems(compositeRange));
      return MeasureUtils<TItem>.GetMeasurements(result, names);
    }

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

    #endregion

    #region Private \ internal methods

    internal Ray<Entire<TKey>> GetCompositeIndexRay(Ray<Entire<TKey>> ray)
    {
      return new Ray<Entire<TKey>>(entireConverter(ray.Point), ray.Direction);
    }

    internal Range<Entire<TKey>> GetCompositeIndexRange(Range<Entire<TKey>> range)
    {
      return new Range<Entire<TKey>>(
        entireConverter(range.EndPoints.First),
        entireConverter(range.EndPoints.Second));
    }

    internal CutOutTransform GetCutOutTransform(TupleDescriptor descriptor, Segment<int> segment)
    {
      CutOutTransform result;
      if (CutOutTransformDictionary.TryGetValue(descriptor, out result))
        return result;

      result = new CutOutTransform(true, descriptor, segment);
      CutOutTransformDictionary.Add(descriptor, result);
      return result;
    }

    internal CutInTransform<int> GetCutInTransform(int count, TupleDescriptor descriptor)
    {
      CutInTransform<int> result;
      if (CutInTransformDictionary.TryGetValue(descriptor, out result))
        return result;

      result = new CutInTransform<int>(true, count, descriptor);
      CutInTransformDictionary.Add(descriptor, result);
      return result;
    }

    #endregion

    /// <inheritdoc/>
    public override void Configure(IndexConfigurationBase<TKey, TItem> configuration)
    {
      base.Configure(configuration);
      IndexSegmentConfiguration<TKey, TItem> indexConfiguration =
        (IndexSegmentConfiguration<TKey, TItem>) configuration;
      segmentName = indexConfiguration.SegmentName;
      keyConverter = delegate(TKey key) {
        CutInTransform<int> keyTransform = GetCutInTransform(key.Count, key.Descriptor);
        return (TKey) keyTransform.Apply(TupleTransformType.TransformedTuple, key, segmentNumber);
      };
      itemConverter = delegate(TItem item) {
        Tuple key = KeyExtractor(item);
        CutInTransform<int> keyTransform = GetCutInTransform(key.Count, item.Descriptor);
        return (TItem) keyTransform.Apply(TupleTransformType.TransformedTuple, item, segmentNumber);
      };
      entireConverter = delegate(Entire<TKey> entire) {
        if (!entire.HasValue)
          return entire;
        CutInTransform<int> entireTransform = GetCutInTransform(entire.Value.Count, entire.Value.Descriptor);
        Tuple key = entireTransform.Apply(TupleTransformType.TransformedTuple, entire.Value, segmentNumber);
        var result = new Entire<TKey>((TKey) key, entire.ValueType);
        return result;
      };
      measureResults = new MeasureResultSet<TItem>(Measures);
      CutOutTransformDictionary = new Dictionary<TupleDescriptor, CutOutTransform>();
      CutInTransformDictionary = new Dictionary<TupleDescriptor, CutInTransform<int>>();
    }


    // Constructors

    internal IndexSegment(IndexConfigurationBase<TKey, TItem> configuration, CompositeIndex<TKey, TItem> compositeIndex, int segmentNumber)
      : base(configuration)
    {
      this.compositeIndex = compositeIndex;
      this.segmentNumber = segmentNumber;
    }
  }
}