// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System;
using Xtensive.Core;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// Measure wrapper for <see cref="CompositeIndex{TKey,TItem}"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class SegmentBoundMeasure<TItem,TResult> : MeasureBase<SegmentBound<TItem>, TResult>
  {
    private readonly int segmentNumber;
    private const int sharedSegmentNumber = Int32.MinValue;

    /// <summary>
    /// Gets the segment number.
    /// </summary>
    /// <value>The segment number.</value>
    public int SegmentNumber
    {
      get { return segmentNumber; }
    }

    public override IMeasure<SegmentBound<TItem>> CreateNew()
    {
      throw new System.NotImplementedException();
    }

    public override IMeasure<SegmentBound<TItem>, TResult> CreateNew(TResult result)
    {
      throw new System.NotImplementedException();
    }

    protected override bool Add(TResult extracted)
    {
      throw new System.NotImplementedException();
    }

    protected override bool Subtract(TResult extracted)
    {
      throw new System.NotImplementedException();
    }

    public override bool AddWith(IMeasure<SegmentBound<TItem>> measure)
    {
      throw new System.NotImplementedException();
    }

    public override bool SubtractWith(IMeasure<SegmentBound<TItem>> measure)
    {
      throw new System.NotImplementedException();
    }

    public override IMeasure<SegmentBound<TItem>> Add(IMeasure<SegmentBound<TItem>> measure)
    {
      throw new System.NotImplementedException();
    }

    public override IMeasure<SegmentBound<TItem>> Subtract(IMeasure<SegmentBound<TItem>> measure)
    {
      throw new System.NotImplementedException();
    }

//    /// <inheritdoc/>
//    public IMeasurementProxy CreateMeasurement()
//    {
//      return measure.CreateMeasurement();
//    }
//
//    /// <inheritdoc/>
//    public bool Add(IMeasurement<TResult> result, SegmentBound<TItem> item)
//    {
//      if (segmentNumber != sharedSegmentNumber && item.SegmentNumber != segmentNumber)
//        return true;
//      return measure.Add(result, item.Value);
//    }
//
//    /// <inheritdoc/>
//    public bool Subtract(IMeasurement<TResult> result, SegmentBound<TItem> item)
//    {
//      if (segmentNumber != sharedSegmentNumber && item.SegmentNumber != segmentNumber)
//        return true;
//      return measure.Subtract(result, item.Value);
//    }
//
//    /// <inheritdoc/>
//    public bool Add(IMeasurement<TResult> first, IMeasurement<TResult> second)
//    {
//      return measure.Add(first, second);
//    }
//
//    /// <inheritdoc/>
//    public bool Subtract(IMeasurement<TResult> first, IMeasurement<TResult> second)
//    {
//      return measure.Subtract(first, second);
//    }


    // Constructors

    /// <inheritdoc/>
    public SegmentBoundMeasure(string name, Converter<SegmentBound<TItem>,TResult> resultExtractor)
      : this(name, resultExtractor, sharedSegmentNumber)
    {
    }
  
    /// <inheritdoc/>
    public SegmentBoundMeasure(string name, Converter<SegmentBound<TItem>,TResult> resultExtractor, int segmentNumber) 
      : base(name, resultExtractor)
    {
      this.segmentNumber = segmentNumber;
    }
  }
}