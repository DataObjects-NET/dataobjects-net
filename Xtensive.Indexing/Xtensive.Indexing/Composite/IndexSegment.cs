// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.04

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Composite
{
  public class IndexSegment<TKey, TItem> : UniqueOrderedIndexBase<TKey, TItem>
    where TKey : Tuple
    where TItem : Tuple
  {
    private readonly CompositeIndex<TKey, TItem> compositeIndex;
    private string segmentName;
    private readonly int segmentNumber;
    private Converter<TKey, TKey> keyConverter;
    private Converter<TItem, TItem> itemConverter;
    private Converter<IEntire<TKey>, IEntire<TKey>> entireConverter;
    private IMeasureResultSet<TItem> measureResults;

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
    public Converter<IEntire<TKey>, IEntire<TKey>> EntireConverter
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
      CutOutTransform itemTransform = new CutOutTransform(true, compositeIndex.Implementation.GetItem(keyConverter(key)).Descriptor, new Segment<int>(key.Count, 1));
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
      return Seek(new Ray<IEntire<TKey>>(Entire<TKey>.Create(key)));
    }

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(Ray<IEntire<TKey>> ray)
    {
      Ray<IEntire<TKey>> compositeRay = GetCompositeIndexRay(ray);

      SeekResult<TItem> result = compositeIndex.Implementation.Seek(compositeRay);
      if (result.ResultType!=SeekResultType.None) {
        SeekResult<TItem> seekResult = new SeekResult<TItem>();
        if ((int) result.Result[compositeRay.Point.Value.Count - 1]==segmentNumber) {
          seekResult = new SeekResult<TItem>(result.ResultType, result.Result);
          CutOutTransform itemTransform = new CutOutTransform(true, seekResult.Result.Descriptor, new Segment<int>(compositeRay.Point.Value.Count - 1, 1));
          Tuple resultTuple = itemTransform.Apply(TupleTransformType.TransformedTuple, seekResult.Result);
          return new SeekResult<TItem>(result.ResultType, (TItem) resultTuple);
        }
      }
      IEntire<TKey> x, y;

      if (compositeRay.Direction==Direction.Negative) {
        x = Entire<TKey>.Create(InfinityType.Negative);
        y = Entire<TKey>.Create(compositeRay.Point.Value, compositeRay.Point.ValueTypes);
      }
      else if (compositeRay.Direction==Direction.Positive) {
        y = Entire<TKey>.Create(InfinityType.Positive);
        x = Entire<TKey>.Create(compositeRay.Point.Value, compositeRay.Point.ValueTypes);
      }
      else {
        x = Entire<TKey>.Create(compositeRay.Point.Value, compositeRay.Point.ValueTypes);
        y = x;
      }

      Range<IEntire<TKey>> readerRange = new Range<IEntire<TKey>>(x, y);
      IndexSegmentReader<TKey, TItem> reader = new IndexSegmentReader<TKey, TItem>(this, readerRange);
      reader.MoveTo(compositeRay.Point);
      if (reader.MoveNext())
        return new SeekResult<TItem>(SeekResultType.Nearest, reader.Current);
      return new SeekResult<TItem>(SeekResultType.None, default(TItem));
    }

    /// <inheritdoc/>
    public override IIndexReader<TKey, TItem> CreateReader(Range<IEntire<TKey>> range)
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
    public override object GetMeasureResult(Range<IEntire<TKey>> range, string name)
    {
      Range<IEntire<TKey>> compositeRange = new Range<IEntire<TKey>>(
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
    public override object[] GetMeasureResults(Range<IEntire<TKey>> range, params string[] names)
    {
      Range<IEntire<TKey>> compositeRange = new Range<IEntire<TKey>>(
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

    internal Ray<IEntire<TKey>> GetCompositeIndexRay(Ray<IEntire<TKey>> ray)
    {
      return new Ray<IEntire<TKey>>(entireConverter(ray.Point), ray.Direction);
    }

    internal Range<IEntire<TKey>> GetCompositeIndexRange(Range<IEntire<TKey>> range)
    {
      return new Range<IEntire<TKey>>(
        entireConverter(range.EndPoints.First),
        entireConverter(range.EndPoints.Second));
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
        CutInTransform<int> keyTransform = new CutInTransform<int>(false, key.Count, key.Descriptor, segmentNumber);
        return (TKey) keyTransform.Apply(TupleTransformType.TransformedTuple, key, segmentNumber);
      };
      itemConverter = delegate(TItem item) {
        Tuple key = KeyExtractor(item);
        CutInTransform<int> keyTransform = new CutInTransform<int>(false, key.Count, item.Descriptor, segmentNumber);
        return (TItem) keyTransform.Apply(TupleTransformType.TransformedTuple, item, segmentNumber);
      };
      entireConverter = delegate(IEntire<TKey> entire) {
        CutInTransform<int> entireTransform = new CutInTransform<int>(false, entire.Value.Count, entire.Value.Descriptor, segmentNumber);
        Tuple key = entireTransform.Apply(TupleTransformType.TransformedTuple, entire.Value, segmentNumber);
        EntireValueType[] valueType = new EntireValueType[entire.Count + 1];
        entire.ValueTypes.CopyTo(valueType, 0);
        valueType[entire.Count] = EntireValueType.Exact;
        IEntire<TKey> result = Entire<TKey>.Create((TKey) key, valueType);
        return result;
      };
      measureResults = new MeasureResultSet<TItem>(Measures);
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