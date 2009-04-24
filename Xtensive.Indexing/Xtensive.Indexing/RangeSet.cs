// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Set of not intersected <see cref="Range{T}"/>.
  /// </summary>
  [Serializable]
  public sealed class RangeSet<T> : IEnumerable<Range<T>>
  {
    private readonly Comparison<Range<T>> leftPointsComparison;
    private readonly AdvancedComparer<T> pointTypeComparer;
    private const Int32 defaultRangeCashSize = 20;
    private readonly List<Range<T>> rangeCash = new List<Range<T>>(defaultRangeCashSize);
    private readonly HashSet<Range<T>> ranges = new HashSet<Range<T>>();

    /// <summary>
    /// Creates the full <see cref="RangeSet{T}"/> or empty <see cref="RangeSet{T}"/>.
    /// </summary>
    /// <param name="full"></param>
    /// <param name="pointTypeComparer">The comparer for the type of endpoints.</param>
    /// <returns></returns>
    public static RangeSet<T> CreateFullOrEmpty(bool full, AdvancedComparer<T> pointTypeComparer)
    {
      var range = full ? Range<T>.Full : Range<T>.Empty;
      return new RangeSet<T>(range, pointTypeComparer);
    }

    /// <summary>
    /// Determines whether this instance contains a single full range.
    /// </summary>
    /// <returns>
    ///   <see langword="true"/> if this instance is full; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsFull()
    {
      if (ranges.Count != 1)
        return false;
      var range = ranges.First();
      if (range.IsEmpty)
        return false;
      return range.CompareTo(Range<T>.Full) == 0;
    }

    /// <summary>
    /// Determines whether this instance does not contain any <see cref="Range{T}"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this instance is empty; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsEmpty()
    {
      return ranges.Count == 0;
    }

    #region Implementation of IEnumerable

    /// <inheritdoc/>
    public IEnumerator<Range<T>> GetEnumerator()
    {
      return ranges.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    ///<summary>
    /// Unites the current RangeSet with the other one.
    ///</summary>
    ///<param name="otherSet">The other RangeSet</param>
    /// <returns><see langword="this" /></returns>
    public RangeSet<T> Unite(RangeSet<T> otherSet)
    {
      foreach (var range in otherSet) {
        Unite(range);
      }
      return this;
    }

    private void Unite(Range<T> otherRange)
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
    /// <param name="otherSet">The other RangeSet</param>
    /// <returns><see langword="this" /></returns>
    public RangeSet<T> Intersect(RangeSet<T> otherSet)
    {
      rangeCash.Clear();
      foreach (var otherRange in otherSet) {
        CalculateIntersections(otherRange, rangeCash);
      }
      ReplaceAllRanges(rangeCash);
      return this;
    }

    /// <summary>
    /// Inverts current RangeSet.
    /// </summary>
    /// <returns><see langword="this" /></returns>
    public RangeSet<T> Invert()
    {
      if (ranges.Count == 0) {
        ranges.Add(Range<T>.Full);
        return this;
      }

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
      return this;
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
        if (normilized.Intersects(range, pointTypeComparer)) {
          var intersection = normilized.Intersect(range, pointTypeComparer);
          if(!intersection.IsEmpty)
            intersections.Add(intersection);
        }
      }
    }

    private void ReplaceAllRanges(List<Range<T>> newRanges)
    {
      ranges.Clear();
      foreach (var range in newRanges) {
        ranges.Add(range);
      }
    }


    // Constructors

    /// <summary>
    /// Creates new RangeSet containing a single <see cref="Range{T}"/>.
    /// </summary>
    /// <param name="firstRange"><see cref="Range{T}"/> to be used as base for RangeSet.</param>
    /// <param name="pointTypeComparer">The comparer for the type of endpoints.</param>
    public RangeSet(Range<T> firstRange, AdvancedComparer<T> pointTypeComparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(pointTypeComparer, "pointTypeComparer");
      this.pointTypeComparer = pointTypeComparer;
      leftPointsComparison =
        (x, y) => pointTypeComparer.Compare(x.EndPoints.First, y.EndPoints.First);
      if(!firstRange.IsEmpty)
        ranges.Add(NormalizeRangeDirection(firstRange));
    }
  }
}
