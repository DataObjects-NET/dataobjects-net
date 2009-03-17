// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.16

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Set of not intersected <see cref="Range{T}"/>.
  /// </summary>
  public sealed class RangeSet<T> : IEnumerable<Range<T>>
  {
    private readonly Comparison<Range<T>> leftPointsComparison;
    private readonly AdvancedComparer<T> pointTypeComparer;
    private const Int32 defaultRangeCashSize = 20;
    private readonly List<Range<T>> rangeCash = new List<Range<T>>(defaultRangeCashSize);
    private readonly SetSlim<Range<T>> ranges =
        new SetSlim<Range<T>>();

    #region Implementation of IEnumerable

    public IEnumerator<Range<T>> GetEnumerator()
    {
      return ranges.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    ///<summary>
    /// Unites the current RangeSet with the other one.
    ///</summary>
    ///<param name="otherSet">the other RangeSet</param>
    public void Unite(RangeSet<T> otherSet)
    {
      foreach (var range in otherSet) {
        Unite(range);
      }
    }

    ///<summary>
    /// Unite the current RangeSet with the given <see cref="Range{T}"/>
    ///</summary>
    ///<param name="otherRange">the range to unite with</param>
    public void Unite(Range<T> otherRange)
    {
      rangeCash.Clear();
      var mergedRange = NormalizeRangeDirection(otherRange);
      foreach (var range in ranges) {
        if (mergedRange.Intersects(range, pointTypeComparer)) {
          mergedRange = mergedRange.Merge(range, pointTypeComparer).Pop();
          rangeCash.Add(range);
        }
      }
      foreach (var range in rangeCash) {
        ranges.Remove(range);
      }
      ranges.Add(mergedRange);
    }

    /// <summary>
    /// Intersects the current RangeSet with the other one.
    /// </summary>
    /// <param name="otherSet">other RangeSet</param>
    public void Intersect(RangeSet<T> otherSet)
    {
      rangeCash.Clear();
      foreach (var otherRange in otherSet) {
        CalculateIntersections(otherRange, rangeCash);
      }
      ReplaceAllRanges(rangeCash);
    }

    ///<summary>
    /// Intersects the current RangeSet with the given <see cref="Range{T}"/>
    ///</summary>
    ///<param name="otherRange">the range to intersect with</param>
    public void Intersect(Range<T> otherRange)
    {
      rangeCash.Clear();
      CalculateIntersections(otherRange, rangeCash);
      ReplaceAllRanges(rangeCash);
    }

    /// <summary>
    /// Inverts current RangeSet.
    /// </summary>
    public void Invert()
    {
      LoadAllRangesTo(rangeCash);
      rangeCash.Sort(leftPointsComparison);
      ranges.Clear();
      T left = pointTypeComparer.ValueRangeInfo.MinValue;
      Int32 idxOfRangeBeforeLast = rangeCash.Count - 1;
      for (Int32 i = 0; i < rangeCash.Count; i++) {
        var range = rangeCash[i];
        if (pointTypeComparer.Compare(left, range.EndPoints.First) < 0)
          ranges.Add(new Range<T>(left,
                                  pointTypeComparer.GetNearestValue(range.EndPoints.First,
                                                                        Direction.Negative)));
        if (i < idxOfRangeBeforeLast)
            left = pointTypeComparer.GetNearestValue(range.EndPoints.Second, Direction.Positive);
          else if (pointTypeComparer.Compare(range.EndPoints.Second, pointTypeComparer.ValueRangeInfo.MaxValue) < 0) {
            left = pointTypeComparer.GetNearestValue(range.EndPoints.Second, Direction.Positive);
            ranges.Add(new Range<T>(left, pointTypeComparer.ValueRangeInfo.MaxValue));
        }
      }
    }

    private Range<T> NormalizeRangeDirection(Range<T> range)
    {
      if (range.GetDirection(pointTypeComparer) == Direction.Negative)
        return range.Redirect(Direction.Positive, pointTypeComparer);
      return range;
    }

    private void LoadAllRangesTo(List<Range<T>> cash)
    {
      cash.Clear();
      cash.AddRange(ranges);
    }

    private void CalculateIntersections(Range<T> otherRange, List<Range<T>> intersections)
    {
      var normilized = NormalizeRangeDirection(otherRange);
      foreach (var range in ranges) {
        if (normilized.Intersects(range, pointTypeComparer))
          intersections.Add(normilized.Intersect(range, pointTypeComparer));
      }
    }

    private void ReplaceAllRanges(List<Range<T>> newRanges)
    {
      ranges.Clear();
      foreach (var range in newRanges) {
        ranges.Add(range);
      }
    }

    /// <summary>
    /// Creates RangeSet containing single <see cref="Range{T}"/>.
    /// </summary>
    /// <param name="firstRange"><see cref="Range{T}"/> to be used as base for RangeSet.</param>
    /// <param name="pointTypeComparer">comparer to type of endpoints</param>
    public RangeSet(Range<T> firstRange, AdvancedComparer<T> pointTypeComparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(pointTypeComparer, "pointTypeComparer");
      this.pointTypeComparer = pointTypeComparer;
      leftPointsComparison =
      (x, y) => pointTypeComparer.Compare(x.EndPoints.First, y.EndPoints.First);
      ranges.Add(NormalizeRangeDirection(firstRange));
    }
  }
}
