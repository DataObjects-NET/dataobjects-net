// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing.Composite;

namespace Xtensive.Indexing.Composite
{
  public class IndexSegment<TKey, TItem> : UniqueOrderedIndexBase<TKey, TItem> where TKey : Tuple where TItem : Tuple
  {
    private CompositeIndex<TKey, TItem>                             compositeIndex;
    private string                                                  segmentName;
    private int                                                     segmentNumber;
    private Converter<TKey, TKey>                                   keyConverter;
    private Converter<TItem,TItem>                                  itemConverter;
    private Converter<IEntire<TKey>, IEntire<TKey>>                 entireConverter;
    private Dictionary<string, string>                              measureMapping;

    #region Properties: SegmentName, SegmentNumber, CompositeIndex, EntireConverter

    /// <summary>
    /// Gets the name of the segment.
    /// </summary>
    /// <value>The name of the segment.</value>
    public string SegmentName
    {
      get { return segmentName; }
    }

    /// <summary>
    /// Gets the segment number.
    /// </summary>
    /// <value>The segment number.</value>
    public int SegmentNumber
    {
      get { return segmentNumber; }
    }

    /// <summary>
    /// Gets the composite index this instance belongs to.
    /// </summary>
    /// <value>The composite index.</value>
    public CompositeIndex<TKey, TItem> CompositeIndex
    {
      get { return compositeIndex; }
    }

    /// <summary>
    /// Gets the entire converter.
    /// </summary>
    /// <value>The entire converter.</value>
    public Converter<IEntire<TKey>, IEntire<TKey>> EntireConverter
    {
      get { return entireConverter; }
    }

    #endregion

    #region GetItem, Contains, ContainsKey methods

    /// <inheritdoc/>
    public override TItem GetItem(TKey key)
    {
      CutOutTransform itemTransform = new CutOutTransform(true,compositeIndex.implementation.GetItem(keyConverter(key)).Descriptor, new Segment<int>(key.Count,1));
      Tuple result = itemTransform.Apply(TupleTransformType.TransformedTuple, compositeIndex.implementation.GetItem(keyConverter(key)));
      return (TItem) result;
    }

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      return compositeIndex.implementation.Contains(itemConverter(item));
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      return compositeIndex.implementation.ContainsKey(keyConverter(key));
    }

    #endregion

    #region Seek, CreateReader methods

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(Ray<IEntire<TKey>> ray)
    {
      Ray<IEntire<TKey>> compositeRay = GetCompositeIndexRay(ray);

      SeekResult<TItem> result = compositeIndex.implementation.Seek(compositeRay);
      if ((int) result.Result[compositeRay.Point.Value.Count]/*.SegmentNumber*/ == segmentNumber)
        return new SeekResult<TItem>(result.ResultType, result.Result);

      throw new NotImplementedException();
//      IndexSegmentReader<TKey, TItem> reader = new IndexSegmentReader<TKey, TItem>(this, ray.Direction);
//      reader.MoveTo(ray.Point);
//      if (reader.HasCurrent)
//        return new SeekResult<TItem>(SeekResultType.Nearest, reader.Current);
//      return new SeekResult<TItem>(SeekResultType.None, default(TItem));
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
      compositeIndex.implementation.Add(itemConverter(item));
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      return compositeIndex.implementation.Remove(itemConverter(item));
    }

    /// <inheritdoc/>
    public override void Replace(TItem item)
    {
      compositeIndex.implementation.Replace(itemConverter(item));
    }

    /// <inheritdoc/>
    public override bool RemoveKey(TKey key)
    {
      return compositeIndex.implementation.RemoveKey(keyConverter(key));
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      foreach (TItem item in this)
        Remove(item);
    }

    #endregion

    #region Measure related methods

    private string GetCompositeIndexMeasureName(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      string resolvedName;
      if (!measureMapping.TryGetValue(name, out resolvedName))
        throw new ArgumentOutOfRangeException("name", "Unknown measure."); // TODO: To resource
      return resolvedName;
    }

    private string[] GetCompositeIndexMeasureNames(params string[] names)
    {
      string[] result = new string[names.Length];
      for(int i = 0, count = names.Length; i < count; i++)
        result[i] = GetCompositeIndexMeasureName(names[i]);

      return result;
    }

    /// <inheritdoc/>
    public override object GetMeasureResult(Range<IEntire<TKey>> range, string name)
    {
      Range<IEntire<TKey>> compositeRange = new Range<IEntire<TKey>>(
        entireConverter(range.EndPoints.First), entireConverter(range.EndPoints.Second));
      return compositeIndex.implementation.GetMeasureResult(compositeRange, GetCompositeIndexMeasureName(name));
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(Range<IEntire<TKey>> range, params string[] names)
    {
      Range<IEntire<TKey>> compositeRange = new Range<IEntire<TKey>>(
        entireConverter(range.EndPoints.First), entireConverter(range.EndPoints.Second));
      return compositeIndex.implementation.GetMeasureResults(compositeRange,GetCompositeIndexMeasureNames(names));
    }

    /// <inheritdoc/>
    public override object GetMeasureResult(string name)
    {
      return compositeIndex.implementation.GetMeasureResult(GetCompositeIndexMeasureName(name));
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(params string[] names)
    {
      return compositeIndex.implementation.GetMeasureResults(GetCompositeIndexMeasureNames(names));
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
    public override void Configure(Indexing.IndexConfigurationBase<TKey, TItem> configuration)
    {
      base.Configure(configuration);
      IndexSegmentConfiguration<TKey, TItem> indexConfiguration =
        (IndexSegmentConfiguration<TKey, TItem>)configuration;
      segmentName = indexConfiguration.SegmentName;
      keyConverter = delegate(TKey key)
      {
        CutInTransform<int> keyTransform = new CutInTransform<int>(false, key.Count, key.Descriptor, segmentNumber);
        return (TKey) keyTransform.Apply(TupleTransformType.TransformedTuple, key, segmentNumber);
      };
      itemConverter = delegate(TItem item)
      {
        Tuple key = KeyExtractor(item);
        CutInTransform<int> keyTransform = new CutInTransform<int>(false, key.Count, item.Descriptor, segmentNumber);
        return (TItem)keyTransform.Apply(TupleTransformType.TransformedTuple, item, segmentNumber);
      };
      entireConverter = delegate(IEntire<TKey> entire)
      {
        CutInTransform<int> entireTransform = new CutInTransform<int>(false, entire.Value.Count, entire.Value.Descriptor, segmentNumber);
        Tuple key = entireTransform.Apply(TupleTransformType.TransformedTuple, entire.Value, segmentNumber);
        EntireValueType[] valueType = new EntireValueType[entire.Count + 1];
        entire.ValueTypes.CopyTo(valueType, 0);
        valueType[entire.Count] = EntireValueType.Exact;
        IEntire<TKey> result = Entire<TKey>.Create((TKey)key, valueType);
        return result;
      }; 
      measureMapping = new Dictionary<string, string>(indexConfiguration.MeasureMapping);
    }


    // Constructors

    internal IndexSegment(Indexing.IndexConfigurationBase<TKey, TItem> configuration, CompositeIndex<TKey, TItem> compositeIndex, int segmentNumber)
      : base(configuration)
    {
      this.compositeIndex = compositeIndex;
      this.segmentNumber = segmentNumber;
    }
  }
}