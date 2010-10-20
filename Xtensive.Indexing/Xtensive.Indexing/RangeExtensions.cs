// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// <see cref="Range{T}"/> extension methods.
  /// </summary>
  public static class RangeExtensions
  {
    #region Nested type: PointComparisonResult

    private struct PointComparisonResult
    {
      public readonly int ToFirst;
      public readonly int ToSecond;
      public readonly bool HasInclusion;

      public PointComparisonResult(int first, int second)
      {
        ToFirst = first > 0 ? 1 : first < 0 ? -1 : 0;
        ToSecond = second > 0 ? 1 : second < 0 ? -1 : 0;
        int sum = ToFirst + ToSecond;
        if (sum < 0)
          sum = -sum;
        HasInclusion = sum <= 1; // Either different signs or one is 0
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

    #region GetDirection, GetXxxEndPoint, Redirect, Invert methods

    /// <summary>
    /// Gets the <see cref="Direction"/> of the range 
    /// relatively to specified <paramref name="comparer"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">Range to get the direction of.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>The <see cref="Direction"/> of the range 
    /// relatively to specified <paramref name="comparer"/>.</returns>
    public static Direction GetDirection<T>(this Range<T> range, AdvancedComparer<T> comparer)
    {
      return comparer.Compare(range.EndPoints.First, range.EndPoints.Second) <= 0 ? Direction.Positive : Direction.Negative;
    }

    /// <summary>
    /// Gets lower endpoint of the range
    /// relatively to specified <paramref name="comparer"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">Range to get the lower endpoint of.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>The the lower endpoint of the range
    /// relatively to specified <paramref name="comparer"/>.</returns>
    public static T GetLowerEndpoint<T>(this Range<T> range, AdvancedComparer<T> comparer)
    {
      return comparer.Compare(range.EndPoints.First, range.EndPoints.Second) <= 0 ? range.EndPoints.First : range.EndPoints.Second;
    }

    /// <summary>
    /// Gets lower endpoint of the range
    /// relatively to specified <paramref name="direction"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">Range to get the lower endpoint of.</param>\
    /// <param name="direction">Assumed range direction.</param>
    /// <returns>The the lower endpoint of the range
    /// relatively to specified <paramref name="direction"/>.</returns>
    public static T GetLowerEndpoint<T>(this Range<T> range, Direction direction)
    {
      return direction!=Direction.Negative ? range.EndPoints.First : range.EndPoints.Second;
    }

    /// <summary>
    /// Gets higher endpoint of the range
    /// relatively to specified <paramref name="comparer"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">Range to get the higher endpoint of.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>The the higher endpoint of the range
    /// relatively to specified <paramref name="comparer"/>.</returns>
    public static T GetHigherEndpoint<T>(this Range<T> range, AdvancedComparer<T> comparer)
    {
      return comparer.Compare(range.EndPoints.First, range.EndPoints.Second) <= 0 ? range.EndPoints.Second : range.EndPoints.First;
    }

    /// <summary>
    /// Gets higher endpoint of the range
    /// relatively to specified <paramref name="direction"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">Range to get the higher endpoint of.</param>
    /// <param name="direction">Assumed range direction.</param>
    /// <returns>The the higher endpoint of the range
    /// relatively to specified <paramref name="direction"/>.</returns>
    public static T GetHigherEndpoint<T>(this Range<T> range, Direction direction)
    {
      return direction!=Direction.Negative ? range.EndPoints.Second : range.EndPoints.First;
    }

    /// <summary>
    /// Converts the specified <paramref name="range"/> to positively directed
    /// relatively to specified <paramref name="comparer"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">The range to convert.</param>
    /// <param name="newDirection">The new direction of the range.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>Positively directed range (relatively to specified <paramref name="comparer"/>), 
    /// which <see cref="Range{T}.EndPoints"/> are the same as of specified <paramref name="range"/>.
    /// </returns>
    public static Range<T> Redirect<T>(this Range<T> range, Direction newDirection, AdvancedComparer<T> comparer)
    {
      if (GetDirection(range, comparer)==newDirection)
        return range;
      else
        return range.Invert();
    }

    /// <summary>
    /// Inverts the specified range - i.e. exchanges its endpoints.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">The range to invert.</param>
    /// <returns>Inverted range.</returns>
    public static Range<T> Invert<T>(this Range<T> range)
    {
      return new Range<T>(range.EndPoints.Second, range.EndPoints.First);
    }

    #endregion

    #region Contains, Intersects methods

    /// <summary>
    /// Check if range contains specified point.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">The range to check.</param>
    /// <param name="point">The point to check for containment.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns><see langword="True"/> if range contains specified point;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool Contains<T>(this Range<T> range,  T point, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (range.IsEmpty)
        return false;
      return Compare(point, range.EndPoints, comparer).HasInclusion;
    }

    /// <summary>
    /// Check if range contains specified point.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">The range to check.</param>
    /// <param name="point">The point to check for containment.</param>
    /// <param name="asymmetricCompare">The comparer to use.</param>
    /// <returns><see langword="True"/> if range contains specified point;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool Contains<T>(this Range<Entire<T>> range, T point, Func<Entire<T>,T,int> asymmetricCompare)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (range.IsEmpty)
        return false;
      return Compare(point, range.EndPoints, asymmetricCompare).HasInclusion;
    }

    /// <summary>
    /// Check if range intersects with the specified one.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="first">The first range to check.</param>
    /// <param name="second">The second range to check.</param>
    /// <param name="comparer">Comparer to use.</param>
    /// <returns><see langword="True"/> if ranges intersect with each other;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool Intersects<T>(this Range<T> first, Range<T> second, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (first.IsEmpty || second.IsEmpty)
        return false;
      return Compare(first, second, comparer).HasIntersection;
    }

    #endregion

    #region CompareTo, Equals methods

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="first">The first range to compare.</param>
    /// <param name="second">The range to compare with the <paramref name="first"/> one.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>Standard comparison result.</returns>
    public static int CompareTo<T>(this Range<T> first, Range<T> second, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (first.IsEmpty ^ second.IsEmpty)
        throw new InvalidOperationException(Strings.ExRangeIsEmpty);
      if (first.IsEmpty && second.IsEmpty)
        return 0;
      int result = comparer.Compare(first.EndPoints.First, second.EndPoints.First);
      return result != 0 ? result : comparer.Compare(first.EndPoints.Second, second.EndPoints.Second);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="first">The first range to compare.</param>
    /// <param name="second">The range to compare with the <paramref name="first"/> one.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns><see langword="True"/> if the current object is equal to the <paramref name="second"/> parameter; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool EqualTo<T>(this Range<T> first, Range<T> second, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (first.IsEmpty ^ second.IsEmpty)
        return false;
      if (first.IsEmpty && second.IsEmpty)
        return true;
      return comparer.Equals(first.EndPoints.First, second.EndPoints.First) && comparer.Equals(first.EndPoints.Second, second.EndPoints.Second);
    }

    #endregion

    #region Merge, Subtract, Intersect, IsSimilar methods

    /// <summary>
    /// Merges two specified ranges.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">The first range.</param>
    /// <param name="other">The second range.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>Merge operation result - up to 2 ranges.</returns>
    public static FixedList3<Range<T>> Merge<T>(this Range<T> range, Range<T> other, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");

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

    /// <summary>
    /// Subtracts the <paramref name="subtracted"/> range from the specified <paramref name="range"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">The range to subtract from.</param>
    /// <param name="subtracted">The range to subtract.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>Subtraction result - up to 2 ranges.</returns>
    public static FixedList3<Range<T>> Subtract<T>(this Range<T> range, Range<T> subtracted, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");

      if (range.IsEmpty)
        return new FixedList3<Range<T>>(Range<T>.Empty);
      if (subtracted.IsEmpty)
        return new FixedList3<Range<T>>(range);

      // Intersection check
      RangeComparisonResult comparisonResult = Compare(range, subtracted, comparer);
      if (!comparisonResult.HasIntersection)
        throw new ArgumentOutOfRangeException("subtracted", Strings.ExMergeOperationRequireIntersectionOfOperands);

      // Direction check
      Pair<int> order = DetectEndPointsOrder(comparisonResult, range, subtracted, comparer);
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
          result.Push(new Range<T>(range.EndPoints.First, comparer.GetNearestValue(subtracted.EndPoints.First, Direction.Negative)));
        // Right point of the first range lies to the right from the right point of the second one
        if (comparisonResult.Second.ToSecond > 0)
          result.Push(new Range<T>(comparer.GetNearestValue(subtracted.EndPoints.Second, Direction.Positive), range.EndPoints.Second));
        break;
      case 1:
        // The first range is inside or equal to the second one
        if (comparisonResult.First.ToFirst <= 0 && comparisonResult.Second.ToSecond >= 0)
          return new FixedList3<Range<T>>(Range<T>.Empty);
        // Left point of the first range lies to the left from the left point of the second one
        if (comparisonResult.First.ToFirst > 0)
          result.Push(new Range<T>(range.EndPoints.First, comparer.GetNearestValue(subtracted.EndPoints.First, Direction.Positive)));
        // Right point of the first range lies to the right from the right point of the second one
        if (comparisonResult.Second.ToSecond < 0)
          result.Push(new Range<T>(comparer.GetNearestValue(subtracted.EndPoints.Second, Direction.Negative), range.EndPoints.Second));
        break;

      }
      return result;
    }

    /// <summary>
    /// Intersects the specified <paramref name="range"/> with the <paramref name="other"/> one.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Range{T}"/> endpoints.</typeparam>
    /// <param name="range">The range.</param>
    /// <param name="other">The other.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns></returns>
    public static Range<T> Intersect<T>(this Range<T> range, Range<T> other, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");

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

    /// <summary>
    /// Determines whether the specified ranges are similar. 
    /// I.e., range' endpoints should have same structure (infinities and shifts on the same places).
    /// </summary>
    /// <param name="range">The range.</param>
    /// <param name="other">The other range.</param>
    /// <returns>
    ///   <see langword="true"/> if the specified range is similar; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsSimilar<T>(this Range<Entire<T>> range, Range<Entire<T>> other)
    {
      if (typeof (Tuple).IsAssignableFrom(typeof (T))) {
        var x1 = range.EndPoints.First.Value as Tuple;
        var x2 = other.EndPoints.First.Value as Tuple;
        var y1 = range.EndPoints.Second.Value as Tuple;
        var y2 = other.EndPoints.Second.Value as Tuple;

        bool result = x1.Descriptor.Equals(x2.Descriptor);
        if (!result)
          return false;
        result = y1.Descriptor.Equals(y2.Descriptor);
        if (!result)
          return false;

        result = range.EndPoints.First.ValueType == other.EndPoints.First.ValueType;
        if (!result)
          return false;

        result = range.EndPoints.Second.ValueType == other.EndPoints.Second.ValueType;
        if (!result)
          return false;

        var indexes = Enumerable.Range(0, x1.Count).ToList();
        result = indexes.Select(i => x1.GetFieldState(i).HasValue()).SequenceEqual(indexes.Select(i => y1.GetFieldState(i).HasValue()));

        return result;
      }
      return
        range.EndPoints.First.ValueType==other.EndPoints.First.ValueType &&
        range.EndPoints.Second.ValueType==other.EndPoints.Second.ValueType;
    }

    /// <summary>
    /// Determines whether specified <paramref name="range"/> has equals endpoints 
    /// and does not contains infinities or shifts for <see cref="Entire{T}"/>.
    /// </summary>
    /// <typeparam name="T">Endpoint type.</typeparam>
    /// <param name="range">The range.</param>
    /// <param name="comparer">Endpoint comparer.</param>
    /// <returns>
    ///   <see langword="true" /> if specified range has equals endpoints; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsEqualityRange<T>(this Range<Entire<T>> range, AdvancedComparer<T> comparer)
    {
      var endPoints = range.EndPoints;
      if (endPoints.First.ValueType == EntireValueType.Exact && endPoints.Second.ValueType == EntireValueType.Exact)
        return comparer.Equals(endPoints.First.Value, endPoints.Second.Value);
      if (((sbyte) endPoints.First.ValueType*(sbyte) endPoints.Second.ValueType) == (sbyte) EntireValueType.NegativeInfinitesimal)
        return comparer.Equals(endPoints.First.Value, endPoints.Second.Value);
      return false;
    }

    #endregion

    #region Static internal \ private methods

    private static PointComparisonResult Compare<T>(T point, Pair<Entire<T>> points, Func<Entire<T>, T, int> asymmetricCompare)
    {
      return new PointComparisonResult(
        asymmetricCompare(points.First, point),
        asymmetricCompare(points.Second, point));
    }


    private static PointComparisonResult Compare<T>(T point, Pair<T> points, AdvancedComparer<T> comparer)
    {
      return new PointComparisonResult(
        comparer.Compare(point, points.First),
        comparer.Compare(point, points.Second));
    }

    private static RangeComparisonResult Compare<T>(Range<T> first, Range<T> second, AdvancedComparer<T> comparer)
    {
      return new RangeComparisonResult(
        Compare(first.EndPoints.First, second.EndPoints, comparer),
        Compare(first.EndPoints.Second, second.EndPoints, comparer));
    }

    private static Pair<int> DetectEndPointsOrder<T>(RangeComparisonResult result, Range<T> first, Range<T> second, AdvancedComparer<T> comparer)
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
  }
}