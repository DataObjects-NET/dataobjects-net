// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Comparison;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Set of not intersected <see cref="Range{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of points.</typeparam>
  [Serializable]
  public sealed class RangeSet<T> : IEnumerable<Range<T>>
  {
    private const Int32 DefaultRangeCashSize = 20;
    private readonly Comparison<Range<T>> leftPointComparer;
    private readonly AdvancedComparer<T> pointComparer;
    private readonly List<Range<T>> rangeCash = new List<Range<T>>(DefaultRangeCashSize);
    private readonly HashSet<Range<T>> ranges = new HashSet<Range<T>>();

    /// <summary>
    /// Creates the full <see cref="RangeSet{T}"/> or empty <see cref="RangeSet{T}"/>.
    /// </summary>
    /// <param name="full"></param>
    /// <param name="pointTypeComparer">The comparer for the type of endpoints.</param>
    /// <returns>Newly created <see cref="RangeSet{T}"/>.</returns>
    public static RangeSet<T> CreateFullOrEmpty(bool full, AdvancedComparer<T> pointTypeComparer)
    {
      var range = full ? Range<T>.Full : Range<T>.Empty;
      return new RangeSet<T>(range, pointTypeComparer);
    }

    /// <summary>
    /// Determines whether this instance contains a single full range.
    /// </summary>
    /// <returns>
    ///  <see langword="true"/> if this instance is full; otherwise, <see langword="false"/>.
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

    ///<summary>
    /// Unites the current <see cref="RangeSet{T}"/> with the other one.
    ///</summary>
    ///<param name="otherSet">The other <see cref="RangeSet{T}"/>.</param>
    /// <returns><see langword="this" /></returns>
    public RangeSet<T> Unite(RangeSet<T> otherSet)
    {
      foreach (var range in otherSet) {
        Unite(range);
      }
      return this;
    }

    /// <summary>
    /// Intersects the current <see cref="RangeSet{T}"/> with the other one.
    /// </summary>
    /// <param name="otherSet">The other <see cref="RangeSet{T}"/>.</param>
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
    /// Inverts current <see cref="RangeSet{T}"/>.
    /// </summary>
    /// <returns><see langword="this" /></returns>
    public RangeSet<T> Invert()
    {
      if (ranges.Count == 0) {
        ranges.Add(Range<T>.Full);
        return this;
      }

      LoadAllRangesTo(rangeCash);
      rangeCash.Sort(leftPointComparer);
      ranges.Clear();
      T left = pointComparer.ValueRangeInfo.MinValue;
      Int32 idxOfRangeBeforeLast = rangeCash.Count - 1;
      for (Int32 i = 0; i < rangeCash.Count; i++) {
        var range = rangeCash[i];
        if (pointComparer.Compare(left, range.EndPoints.First) < 0)
          ranges.Add(new Range<T>(left,
            pointComparer.GetNearestValue(range.EndPoints.First,
              Direction.Negative)));
        if (i < idxOfRangeBeforeLast)
          left = pointComparer.GetNearestValue(range.EndPoints.Second, Direction.Positive);
        else if (pointComparer.Compare(range.EndPoints.Second, pointComparer.ValueRangeInfo.MaxValue) < 0) {
          left = pointComparer.GetNearestValue(range.EndPoints.Second, Direction.Positive);
          ranges.Add(new Range<T>(left, pointComparer.ValueRangeInfo.MaxValue));
        }
      }
      return this;
    }

    #region IEnumerable<...> methods

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

    #region Private \ internal methods

    private void Unite(Range<T> otherRange)
    {
      rangeCash.Clear();
      var mergedRange = NormalizeRangeDirection(otherRange);
      foreach (var range in ranges) {
        if (mergedRange.Intersects(range, pointComparer)) {
          mergedRange = mergedRange.Merge(range, pointComparer).Pop();
          rangeCash.Add(range);
        }
      }
      foreach (var range in rangeCash) {
        ranges.Remove(range);
      }
      ranges.Add(mergedRange);
    }

    private Range<T> NormalizeRangeDirection(Range<T> range)
    {
      if (range.GetDirection(pointComparer) == Direction.Negative)
        return range.Redirect(Direction.Positive, pointComparer);
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
        if (normilized.Intersects(range, pointComparer)) {
          var intersection = normilized.Intersect(range, pointComparer);
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

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      const string format = "RangeSet<{0}>({1})";
      return string.Format(format, 
        typeof(T).GetShortName(), ranges.ToCommaDelimitedString());
    }


    // Constructors

    /// <summary>
    /// Creates new RangeSet containing a single <see cref="Range{T}"/>.
    /// </summary>
    /// <param name="firstRange"><see cref="Range{T}"/> to be used as base for RangeSet.</param>
    /// <param name="pointComparer">The comparer for the endpoints.</param>
    public RangeSet(Range<T> firstRange, AdvancedComparer<T> pointComparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(pointComparer, "pointTypeComparer");
      this.pointComparer = pointComparer;
      leftPointComparer =
        (x, y) => pointComparer.Compare(x.EndPoints.First, y.EndPoints.First);
      if (!firstRange.IsEmpty)
        ranges.Add(NormalizeRangeDirection(firstRange));
    }
  }
}
