// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.12

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  public struct RangeUtils<T>
  {
    private readonly AdvancedComparer<T> comparer;

    #region Nested type: PointComparisonResult

    private struct PointComparisonResult
    {
      public readonly int ToFirst;
      public readonly bool HasInclusion;
      public readonly int ToSecond;

      public PointComparisonResult(int first, int second)
      {
        ToFirst = first;
        ToSecond = second;
        HasInclusion = ToFirst * ToSecond <= 0;
      }
    }

    #endregion

    #region Nested type: RangeComparisonResult

    private struct RangeComparisonResult
    {
      public readonly PointComparisonResult First;
      public readonly bool HasIntersection;
      public readonly PointComparisonResult Second;

      public RangeComparisonResult(PointComparisonResult first, PointComparisonResult second)
      {
        First = first;
        Second = second;
        HasIntersection = Math.Abs(First.ToFirst + First.ToSecond + Second.ToFirst + Second.ToSecond) < 4;
      }
    }

    #endregion

    #region Static methods

    public static Direction GetDirection(Range<T> range, AdvancedComparer<T> comparer)
    {
      return comparer.Compare(range.EndPoints.First, range.EndPoints.Second) <= 0 ? Direction.Positive : Direction.Negative;
    }

    public static Range<T> EnsurePositiveDirection(Range<T> range, AdvancedComparer<T> comparer)
    {
      if(GetDirection(range, comparer)==Direction.Positive) {
        return range;
      }
      else {
        return new Range<T>(range.EndPoints.Second, range.EndPoints.First);
      }
    }

    /// <summary>
    /// Check if range contains specified point.
    /// </summary>
    /// <param name="point">Point to check for containment.</param>
    /// <param name="comparer">Point comparer to use.</param>
    /// <returns><see langword="True"/> if range contains specified point;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool Contains(Range<T> range,  T point, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (range.IsEmpty)
        return false;
      return Compare(point, range.EndPoints, comparer).HasInclusion;
    }

    /// <summary>
    /// Check if range intersects with the specified one.
    /// </summary>
    /// <param name="other">Range to check for intersection.</param>
    /// <param name="comparer">Point comparer to use.</param>
    /// <returns><see langword="True"/> if range intersects with the specified one;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool Intersects(Range<T> range, Range<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (range.IsEmpty || other.IsEmpty)
        return false;
      return Compare(range, other, comparer).HasIntersection;
    }

    #region IHasRange method helpers

    public static FixedList3<Range<T>> Merge(Range<T> range ,Range<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");

      if (range.IsEmpty)
        return new FixedList3<Range<T>>(other);
      if (other.IsEmpty)
        return new FixedList3<Range<T>>(range);

      // Intersection check
      RangeComparisonResult comparisonResult = Compare(range, other, comparer);
      if (!comparisonResult.HasIntersection)
        throw new ArgumentOutOfRangeException("other", Strings.ExMergeOperationRequireIntersectionOfOperands);

      // Direction check
      Pair<int> order = DetectEndPointsOrder(comparisonResult, range, other, comparer);
      if (order.First != 0 && order.First + order.Second == 0)
        throw new InvalidOperationException(Strings.ExEndPointOrderMustBeEqual);

      // Getting the leftmost from left point and the rightmost from right points dependently on direction
      T first, second;
      switch (order.First)
      {
      case -1:
        first = comparisonResult.First.ToFirst < 0 ? range.EndPoints.First : other.EndPoints.First;
        second = comparisonResult.Second.ToSecond > 0 ? range.EndPoints.Second : other.EndPoints.Second;
        break;
      case 1:
        first = comparisonResult.First.ToFirst > 0 ? range.EndPoints.First : other.EndPoints.First;
        second = comparisonResult.Second.ToSecond < 0 ? range.EndPoints.Second : other.EndPoints.Second;
        break;
      default:
        first = other.EndPoints.First;
        second = other.EndPoints.Second;
        break;
      }

      return new FixedList3<Range<T>>(new Range<T>(first, second));
    }


    public static FixedList3<Range<T>> Subtract(Range<T> range, Range<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");

      if (range.IsEmpty)
        return new FixedList3<Range<T>>(Range<T>.Empty);
      if (other.IsEmpty)
        return new FixedList3<Range<T>>(range);

      // Intersection check
      RangeComparisonResult comparisonResult = Compare(range, other, comparer);
      if (!comparisonResult.HasIntersection)
        throw new ArgumentOutOfRangeException("other", Strings.ExMergeOperationRequireIntersectionOfOperands);

      // Direction check
      Pair<int> order = DetectEndPointsOrder(comparisonResult, range, other, comparer);
      if (order.First != 0 && order.First + order.Second == 0)
        throw new InvalidOperationException(Strings.ExEndPointOrderMustBeEqual);

      FixedList3<Range<T>> result = new FixedList3<Range<T>>();
      switch (order.First)
      {
      case -1:
      case 0:
        // The first range is inside or equal to the second one
        if (comparisonResult.First.ToFirst >= 0 && comparisonResult.Second.ToSecond <= 0)
          return new FixedList3<Range<T>>(Range<T>.Empty);
        // Left point of the first range lies to the left from the left point of the second one
        if (comparisonResult.First.ToFirst < 0)
          result.Push(new Range<T>(range.EndPoints.First, comparer.GetNearestValue(other.EndPoints.First, Direction.Negative)));
        // Right point of the first range lies to the right from the right point of the second one
        if (comparisonResult.Second.ToSecond > 0)
          result.Push(new Range<T>(comparer.GetNearestValue(other.EndPoints.Second, Direction.Positive), range.EndPoints.Second));
        break;
      case 1:
        // The first range is inside or equal to the second one
        if (comparisonResult.First.ToFirst <= 0 && comparisonResult.Second.ToSecond >= 0)
          return new FixedList3<Range<T>>(Range<T>.Empty);
        // Left point of the first range lies to the left from the left point of the second one
        if (comparisonResult.First.ToFirst > 0)
          result.Push(new Range<T>(range.EndPoints.First, comparer.GetNearestValue(other.EndPoints.First, Direction.Positive)));
        // Right point of the first range lies to the right from the right point of the second one
        if (comparisonResult.Second.ToSecond < 0)
          result.Push(new Range<T>(comparer.GetNearestValue(other.EndPoints.Second, Direction.Negative), range.EndPoints.Second));
        break;

      }
      return result;
    }

    public static Range<T> Intersect(Range<T> range, Range<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");

      if (range.IsEmpty)
        return other;
      if (other.IsEmpty)
        return range;

      // Intersection check
      RangeComparisonResult comparisonResult = Compare(range, other, comparer);
      if (!comparisonResult.HasIntersection)
        throw new ArgumentOutOfRangeException("other", Strings.ExMergeOperationRequireIntersectionOfOperands);

      // Direction check
      Pair<int> order = DetectEndPointsOrder(comparisonResult, range, other, comparer);
      if (order.First != 0 && order.First + order.Second == 0)
        throw new InvalidOperationException(Strings.ExEndPointOrderMustBeEqual);

      // Getting the leftmost from left point and the rightmost from right points dependently on direction
      T first, second;
      switch (order.First)
      {
      case -1:
        first = comparisonResult.First.ToFirst > 0 ? range.EndPoints.First : other.EndPoints.First;
        second = comparisonResult.Second.ToSecond < 0 ? range.EndPoints.Second : other.EndPoints.Second;
        break;
      case 1:
        first = comparisonResult.First.ToFirst < 0 ? range.EndPoints.First : other.EndPoints.First;
        second = comparisonResult.Second.ToSecond > 0 ? range.EndPoints.Second : other.EndPoints.Second;
        break;
      default:
        first = range.EndPoints.First;
        second = range.EndPoints.Second;
        break;
      }

      return new Range<T>(first, second);
    }

    #endregion

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>Standard comparison result.</returns>
    public static int CompareTo(Range<T> range, Range<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (range.IsEmpty ^ other.IsEmpty)
        throw new InvalidOperationException(Strings.ExRangeIsEmpty);
      if (range.IsEmpty && other.IsEmpty)
        return 0;
      int result = comparer.Compare(range.EndPoints.First, other.EndPoints.First);
      return result != 0 ? result : comparer.Compare(range.EndPoints.Second, other.EndPoints.Second);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns><see langword="True"/> if the current object is equal to the <paramref name="other"/> parameter; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool Equals(Range<T> range, Range<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (range.IsEmpty ^ other.IsEmpty)
        return false;
      if (range.IsEmpty && other.IsEmpty)
        return true;
      return comparer.Equals(range.EndPoints.First, other.EndPoints.First) && comparer.Equals(range.EndPoints.Second, other.EndPoints.Second);
    }

    #region Internal \ private methods

    private static PointComparisonResult Compare(T point, Pair<T> points, AdvancedComparer<T> comparer)
    {
      return new PointComparisonResult(
        comparer.Compare(point, points.First),
        comparer.Compare(point, points.Second));
    }

    private static RangeComparisonResult Compare(Range<T> first, Range<T> second, AdvancedComparer<T> comparer)
    {
      return new RangeComparisonResult(
        Compare(first.EndPoints.First, second.EndPoints, comparer),
        Compare(first.EndPoints.Second, second.EndPoints, comparer));
    }

    private static Pair<int> DetectEndPointsOrder(RangeComparisonResult result, Range<T> first, Range<T> second, AdvancedComparer<T> comparer)
    {
      // First range order
      int result1 = Comparer<int>.Default.Compare(result.First.ToFirst, result.Second.ToFirst);
      if (result1 == 0)
        result1 = Comparer<int>.Default.Compare(result.First.ToSecond, result.Second.ToSecond);
      if (result1 == 0)
        result1 = comparer.Compare(first.EndPoints.First, first.EndPoints.Second);

      // Second range order
      int result2 = Comparer<int>.Default.Compare(result.First.ToFirst, result.First.ToSecond);
      if (result2 == 0)
        result2 = Comparer<int>.Default.Compare(result.Second.ToFirst, result.Second.ToSecond);
      if (result2 == 0)
        result2 = comparer.Compare(second.EndPoints.First, second.EndPoints.Second);
      else
        result2 *= -1;

      return new Pair<int>(result1, result2);
    }

    #endregion

    #endregion

    /// <summary>
    /// Check if range contains specified point.
    /// </summary>
    /// <param name="range">Range to check.</param>
    /// <param name="point">Point to check for containment.</param>
    /// <returns><see langword="True"/> if range contains specified point;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Contains(Range<T> range,  T point)
    {
      return Contains(range, point, comparer);
    }

    /// <summary>
    /// Check if range intersects with the specified one.
    /// </summary>
    /// <param name="range">Range to check.</param>
    /// <param name="other">Range to check for intersection.</param>
    /// <returns><see langword="True"/> if range intersects with the specified one;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Intersects(Range<T> range, Range<T> other)
    {
      return Intersects(range, other, comparer);
    }

    #region IHasRange method helpers

    public FixedList3<Range<T>> Merge(Range<T> range ,Range<T> other)
    {
      return Merge(range, other, comparer);
    }

    public FixedList3<Range<T>> Subtract(Range<T> range, Range<T> other)
    {
      return Subtract(range, other, comparer);
    }

    public Range<T> Intersect(Range<T> range, Range<T> other)
    {
      return Intersect(range, other, comparer);
    }

    #endregion

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>Standard comparison result.</returns>
    public int CompareTo(Range<T> range, Range<T> other)
    {
      return CompareTo(range, other, comparer);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="True"/> if the current object is equal to the <paramref name="other"/> parameter; 
    /// otherwise, <see langword="false"/>.</returns>
    public bool Equals(Range<T> range, Range<T> other)
    {
      return Equals(range, other, comparer);
    }


    // Constructors

    public RangeUtils(AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      this.comparer = comparer;
    }
  }
}