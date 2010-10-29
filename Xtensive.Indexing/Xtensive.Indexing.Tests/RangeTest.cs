// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.08

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Testing;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class RangeTest
  {
    private static void TestFullRange<T>()
    {
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      if (!(comparer.ValueRangeInfo.HasMinValue && comparer.ValueRangeInfo.HasMaxValue)) {
        Assert.AreEqual(Range<T>.Empty, Range<T>.Full);
        return;
      }
      Assert.IsNotNull(Range<T>.Full);
      Assert.AreEqual(comparer.ValueRangeInfo.MinValue, Range<T>.Full.EndPoints.First);
      Assert.AreEqual(comparer.ValueRangeInfo.MaxValue, Range<T>.Full.EndPoints.Second);
    }

    private static void TestContains<T>(Range<T> range)
    {
      Assert.IsTrue(range.Contains(range.EndPoints.First));
      Assert.IsTrue(range.Contains(range.EndPoints.Second));

      PointMap<T>? mapCandidate = GetPointMap(range);
      if (mapCandidate==null)
        return;
      PointMap<T> map = mapCandidate.Value;
      Assert.IsFalse(range.Contains(map.LeftOuter));
      Assert.IsTrue(range.Contains(map.LeftInner));
      Assert.IsTrue(range.Contains(map.RightInner));
      Assert.IsFalse(range.Contains(map.RightOuter));
    }

    private static void TestEquals<T>(Range<T> range)
    {
      Assert.IsTrue(range.Equals(range));
      Assert.IsFalse(range.Equals(Range<T>.Empty));
      Assert.IsTrue(range.Equals(new Range<T>(range.EndPoints.First, range.EndPoints.Second)));
      if (EqualityComparer<T>.Default.Equals(range.EndPoints.First, range.EndPoints.Second))
        Assert.IsTrue(range.Equals(new Range<T>(range.EndPoints.Second, range.EndPoints.First)));
      else {
        Assert.IsFalse(range.Equals(new Range<T>(range.EndPoints.Second, range.EndPoints.First)));
        Assert.IsFalse(range.Equals(new Range<T>(range.EndPoints.First, range.EndPoints.First)));
        Assert.IsFalse(range.Equals(new Range<T>(range.EndPoints.Second, range.EndPoints.Second)));
      }

      PointMap<T>? mapCandidate = GetPointMap(range);
      if (mapCandidate==null)
        return;
      PointMap<T> map = mapCandidate.Value;

      Assert.IsFalse(range.Equals(new Range<T>(map.Left, map.RightOuter)));
      Assert.IsFalse(range.Equals(new Range<T>(map.LeftOuter, map.Right)));
      Assert.IsFalse(range.Equals(new Range<T>(map.LeftOuter, map.RightOuter)));
    }

    private static void TestIntersects<T>(Range<T> range)
    {
      Assert.IsTrue(range.Intersects(range));
      Assert.IsTrue(range.Intersects(new Range<T>(range.EndPoints.First, range.EndPoints.Second)));
      Assert.IsTrue(range.Intersects(new Range<T>(range.EndPoints.Second, range.EndPoints.First)));
      Assert.IsTrue(range.Intersects(new Range<T>(range.EndPoints.First, range.EndPoints.First)));
      Assert.IsTrue(range.Intersects(new Range<T>(range.EndPoints.Second, range.EndPoints.Second)));
      Assert.IsFalse(range.Intersects(Range<T>.Empty));

      PointMap<T>? mapCandidate = GetPointMap(range);
      if (mapCandidate==null)
        return;
      PointMap<T> map = mapCandidate.Value;

      Assert.IsFalse(range.Intersects(new Range<T>(map.LeftOuter, map.LeftOuter)));
      Assert.IsFalse(range.Intersects(new Range<T>(map.RightOuter, map.RightOuter)));

      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftOuter, map.Left)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftOuter, map.LeftInner)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftOuter, map.RightInner)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftOuter, map.Right)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftOuter, map.RightOuter)));

      Assert.IsTrue(range.Intersects(new Range<T>(map.Left, map.LeftInner)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.Left, map.RightInner)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.Left, map.RightOuter)));

      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftInner, map.LeftInner)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftInner, map.RightInner)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftInner, map.Right)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.LeftInner, map.RightOuter)));

      Assert.IsTrue(range.Intersects(new Range<T>(map.RightInner, map.RightInner)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.RightInner, map.Right)));
      Assert.IsTrue(range.Intersects(new Range<T>(map.RightInner, map.RightOuter)));

      Assert.IsTrue(range.Intersects(new Range<T>(map.Right, map.RightOuter)));
    }

    private static void TestCompareTo<T>(Range<T> range)
    {
      AssertEx.ThrowsInvalidOperationException(delegate { range.CompareTo(Range<T>.Empty); });
      Assert.IsTrue(range.CompareTo(range) == 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(range.EndPoints.First, range.EndPoints.Second)) == 0);

      PointMap<T>? mapCandidate = GetPointMap(range);
      if (mapCandidate==null)
        return;
      PointMap<T> map = mapCandidate.Value;

      range = new Range<T>(map.Left, map.Right);

      Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftOuter, map.LeftOuter)) > 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftOuter, map.Left)) > 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftOuter, map.LeftInner)) > 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftOuter, map.RightInner)) > 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftOuter, map.Right)) > 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftOuter, map.RightOuter)) > 0);

      if (!EqualityComparer<T>.Default.Equals(map.Left, map.Right)) {
        Assert.IsTrue(range.CompareTo(new Range<T>(map.Left, map.Left)) > 0);
        Assert.IsTrue(range.CompareTo(new Range<T>(map.Left, map.LeftInner)) > 0);
        Assert.IsTrue(range.CompareTo(new Range<T>(map.Left, map.RightInner)) > 0);
        Assert.IsTrue(range.CompareTo(new Range<T>(map.Left, map.Right))==0);

        Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftInner, map.LeftInner)) < 0);
        Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftInner, map.RightInner)) < 0);
        Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftInner, map.Right)) < 0);

        Assert.IsTrue(range.CompareTo(new Range<T>(map.RightInner, map.RightInner)) < 0);
        Assert.IsTrue(range.CompareTo(new Range<T>(map.RightInner, map.Right)) < 0);

        Assert.IsTrue(range.CompareTo(new Range<T>(map.Right, map.Right)) < 0);
      }
      Assert.IsTrue(range.CompareTo(new Range<T>(map.Left, map.RightOuter)) < 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.LeftInner, map.RightOuter)) < 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.RightInner, map.RightOuter)) < 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.Right, map.RightOuter)) < 0);
      Assert.IsTrue(range.CompareTo(new Range<T>(map.RightOuter, map.RightOuter)) < 0);
    }

    private static void TestMerge<T>(Range<T> range)
    {
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      Assert.IsTrue(range.Merge(range, comparer).Pop().Equals(range));
      Assert.IsTrue(range.Merge(new Range<T>(range.EndPoints.First, range.EndPoints.Second), comparer).Pop().Equals(range));
      Assert.IsTrue(range.Merge(Range<T>.Empty, comparer).Pop().Equals(range));

      PointMap<T>? mapCandidate = GetPointMap(range);
      if (mapCandidate==null)
        return;
      PointMap<T> map = mapCandidate.Value;

      range = new Range<T>(map.Left, map.Right);

      AssertEx.ThrowsArgumentOutOfRangeException(delegate { range.Merge(new Range<T>(map.LeftOuter, map.LeftOuter), comparer); });
      AssertEx.ThrowsArgumentOutOfRangeException(delegate { range.Merge(new Range<T>(map.RightOuter, map.RightOuter), comparer); });

      Assert.IsTrue(range.Merge(new Range<T>(map.LeftOuter, map.Left), comparer).Pop().Equals(new Range<T>(map.LeftOuter, map.Right)));
      Assert.IsTrue(range.Merge(new Range<T>(map.LeftOuter, map.LeftInner), comparer).Pop().Equals(new Range<T>(map.LeftOuter, map.Right)));
      Assert.IsTrue(range.Merge(new Range<T>(map.LeftOuter, map.RightInner), comparer).Pop().Equals(new Range<T>(map.LeftOuter, map.Right)));
      Assert.IsTrue(range.Merge(new Range<T>(map.LeftOuter, map.Right), comparer).Pop().Equals(new Range<T>(map.LeftOuter, map.Right)));
      Assert.IsTrue(range.Merge(new Range<T>(map.LeftOuter, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.LeftOuter, map.RightOuter)));

      if (!EqualityComparer<T>.Default.Equals(map.Left, map.Right)) {
        Assert.IsTrue(range.Merge(new Range<T>(map.Left, map.Left), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));
        Assert.IsTrue(range.Merge(new Range<T>(map.Left, map.LeftInner), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));
        Assert.IsTrue(range.Merge(new Range<T>(map.Left, map.RightInner), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));
        Assert.IsTrue(range.Merge(new Range<T>(map.Left, map.Right), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));

        Assert.IsTrue(range.Merge(new Range<T>(map.LeftInner, map.LeftInner), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));
        Assert.IsTrue(range.Merge(new Range<T>(map.LeftInner, map.RightInner), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));
        Assert.IsTrue(range.Merge(new Range<T>(map.LeftInner, map.Right), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));

        Assert.IsTrue(range.Merge(new Range<T>(map.RightInner, map.RightInner), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));
        Assert.IsTrue(range.Merge(new Range<T>(map.RightInner, map.Right), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));

        Assert.IsTrue(range.Merge(new Range<T>(map.Right, map.Right), comparer).Pop().Equals(new Range<T>(map.Left, map.Right)));
      }
      Assert.IsTrue(range.Merge(new Range<T>(map.Left, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.Left, map.RightOuter)));
      Assert.IsTrue(range.Merge(new Range<T>(map.LeftInner, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.Left, map.RightOuter)));
      Assert.IsTrue(range.Merge(new Range<T>(map.RightInner, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.Left, map.RightOuter)));
      Assert.IsTrue(range.Merge(new Range<T>(map.Right, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.Left, map.RightOuter)));
    }

    private static void TestSubtract<T>(Range<T> range)
    {
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      Assert.IsTrue(range.Subtract(range, comparer).Pop().Equals(Range<int>.Empty));
      Assert.IsTrue(range.Subtract(new Range<T>(range.EndPoints.First, range.EndPoints.Second), comparer).Pop().Equals(Range<int>.Empty));

      PointMap<T>? mapCandidate = GetPointMap(range);
      if (mapCandidate==null)
        return;
      PointMap<T> map = mapCandidate.Value;

      range = new Range<T>(map.Left, map.Right);

      AssertEx.ThrowsArgumentOutOfRangeException(delegate { range.Subtract(new Range<T>(map.LeftOuter, map.LeftOuter), comparer); });
      AssertEx.ThrowsArgumentOutOfRangeException(delegate { range.Subtract(new Range<T>(map.RightOuter, map.RightOuter), comparer); });

      if (EqualityComparer<T>.Default.Equals(map.Left, map.Right)) {
        Assert.IsTrue(range.Subtract(new Range<T>(map.LeftOuter, map.Left), comparer).Pop().Equals(Range<T>.Empty));
        Assert.IsTrue(range.Subtract(new Range<T>(map.Right, map.RightOuter), comparer).Pop().Equals(Range<T>.Empty));
        return;
      }

      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftOuter, map.Left), comparer).Pop().Equals(new Range<T>(map.LeftInner, map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftOuter, map.LeftInner), comparer).Pop().Equals(new Range<T>(comparer.GetNearestValue(map.LeftInner, Direction.Positive), map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftOuter, map.RightInner), comparer).Pop().Equals(new Range<T>(map.Right, map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftOuter, map.Right), comparer).Pop().Equals(Range<T>.Empty));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftOuter, map.RightOuter), comparer).Pop().Equals(Range<T>.Empty));

      Assert.IsTrue(range.Subtract(new Range<T>(map.Left, map.Left), comparer).Pop().Equals(new Range<T>(map.LeftInner, map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.Left, map.LeftInner), comparer).Pop().Equals(new Range<T>(comparer.GetNearestValue(map.LeftInner, Direction.Positive), map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.Left, map.RightInner), comparer).Pop().Equals(new Range<T>(map.Right, map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.Left, map.Right), comparer).Pop().Equals(Range<T>.Empty));
      Assert.IsTrue(range.Subtract(new Range<T>(map.Left, map.RightOuter), comparer).Pop().Equals(Range<T>.Empty));

      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftInner, map.LeftInner), comparer)[1].Equals(new Range<T>(comparer.GetNearestValue(map.LeftInner, Direction.Positive), map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftInner, map.LeftInner), comparer)[0].Equals(new Range<T>(map.Left, map.Left)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftInner, map.RightInner), comparer)[1].Equals(new Range<T>(map.Right, map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftInner, map.RightInner), comparer)[0].Equals(new Range<T>(map.Left, map.Left)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftInner, map.Right), comparer).Pop().Equals(new Range<T>(map.Left, map.Left)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.LeftInner, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.Left, map.Left)));

      Assert.IsTrue(range.Subtract(new Range<T>(map.RightInner, map.RightInner), comparer)[1].Equals(new Range<T>(map.Right, map.Right)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.RightInner, map.RightInner), comparer)[0].Equals(new Range<T>(map.Left, comparer.GetNearestValue(map.RightInner, Direction.Negative))));
      Assert.IsTrue(range.Subtract(new Range<T>(map.RightInner, map.Right), comparer).Pop().Equals(new Range<T>(map.Left, comparer.GetNearestValue(map.RightInner, Direction.Negative))));
      Assert.IsTrue(range.Subtract(new Range<T>(map.RightInner, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.Left, comparer.GetNearestValue(map.RightInner, Direction.Negative))));

      Assert.IsTrue(range.Subtract(new Range<T>(map.Right, map.Right), comparer).Pop().Equals(new Range<T>(map.Left, map.RightInner)));
      Assert.IsTrue(range.Subtract(new Range<T>(map.Right, map.RightOuter), comparer).Pop().Equals(new Range<T>(map.Left, map.RightInner)));
    }

    private static void TestIntersect<T>(Range<T> range)
    {
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      Assert.IsTrue(range.Intersect(range, comparer).Equals(range));
      Assert.IsTrue(range.Intersect(new Range<T>(range.EndPoints.First, range.EndPoints.Second), comparer).Equals(range));
      Assert.IsTrue(range.Intersect(Range<T>.Empty, comparer).Equals(range));

      PointMap<T>? mapCandidate = GetPointMap(range);
      if (mapCandidate==null)
        return;
      PointMap<T> map = mapCandidate.Value;

      range = new Range<T>(map.Left, map.Right);

      AssertEx.ThrowsArgumentOutOfRangeException(delegate { range.Intersect(new Range<T>(map.LeftOuter, map.LeftOuter), comparer); });
      AssertEx.ThrowsArgumentOutOfRangeException(delegate { range.Intersect(new Range<T>(map.RightOuter, map.RightOuter), comparer); });

      Assert.IsTrue(range.Intersect(new Range<T>(map.LeftOuter, map.Left), comparer).Equals(new Range<T>(map.Left, map.Left)));
      Assert.IsTrue(range.Intersect(new Range<T>(map.LeftOuter, map.LeftInner), comparer).Equals(new Range<T>(map.Left, map.LeftInner)));
      Assert.IsTrue(range.Intersect(new Range<T>(map.LeftOuter, map.RightInner), comparer).Equals(new Range<T>(map.Left, map.RightInner)));
      Assert.IsTrue(range.Intersect(new Range<T>(map.LeftOuter, map.Right), comparer).Equals(new Range<T>(map.Left, map.Right)));
      Assert.IsTrue(range.Intersect(new Range<T>(map.LeftOuter, map.RightOuter), comparer).Equals(new Range<T>(map.Left, map.Right)));

      if (!EqualityComparer<T>.Default.Equals(map.Left, map.Right)) {
        Assert.IsTrue(range.Intersect(new Range<T>(map.Left, map.Left), comparer).Equals(new Range<T>(map.Left, map.Left)));
        Assert.IsTrue(range.Intersect(new Range<T>(map.Left, map.LeftInner), comparer).Equals(new Range<T>(map.Left, map.LeftInner)));
        Assert.IsTrue(range.Intersect(new Range<T>(map.Left, map.RightInner), comparer).Equals(new Range<T>(map.Left, map.RightInner)));
        Assert.IsTrue(range.Intersect(new Range<T>(map.Left, map.Right), comparer).Equals(new Range<T>(map.Left, map.Right)));

        Assert.IsTrue(range.Intersect(new Range<T>(map.LeftInner, map.LeftInner), comparer).Equals(new Range<T>(map.LeftInner, map.LeftInner)));
        Assert.IsTrue(range.Intersect(new Range<T>(map.LeftInner, map.RightInner), comparer).Equals(new Range<T>(map.LeftInner, map.RightInner)));
        Assert.IsTrue(range.Intersect(new Range<T>(map.LeftInner, map.Right), comparer).Equals(new Range<T>(map.LeftInner, map.Right)));

        Assert.IsTrue(range.Intersect(new Range<T>(map.RightInner, map.RightInner), comparer).Equals(new Range<T>(map.RightInner, map.RightInner)));
        Assert.IsTrue(range.Intersect(new Range<T>(map.RightInner, map.Right), comparer).Equals(new Range<T>(map.RightInner, map.Right)));

        Assert.IsTrue(range.Intersect(new Range<T>(map.Right, map.Right), comparer).Equals(new Range<T>(map.Right, map.Right)));
      }
      Assert.IsTrue(range.Intersect(new Range<T>(map.Left, map.RightOuter), comparer).Equals(new Range<T>(map.Left, map.Right)));
      Assert.IsTrue(range.Intersect(new Range<T>(map.LeftInner, map.RightOuter), comparer).Equals(new Range<T>(map.LeftInner, map.Right)));
      Assert.IsTrue(range.Intersect(new Range<T>(map.RightInner, map.RightOuter), comparer).Equals(new Range<T>(map.RightInner, map.Right)));
      Assert.IsTrue(range.Intersect(new Range<T>(map.Right, map.RightOuter), comparer).Equals(new Range<T>(map.Right, map.Right)));
    }

    private static PointMap<T>? GetPointMap<T>(Range<T> range)
    {
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      if (!comparer.ValueRangeInfo.HasDeltaValue)
        return null;

      int pointComparisonResult = Comparer<T>.Default.Compare(range.EndPoints.First, range.EndPoints.Second);
      PointMap<T> map = new PointMap<T>();

      switch (pointComparisonResult) {
        case 1:
          map.Left = range.EndPoints.Second;
          map.LeftInner = comparer.GetNearestValue(map.Left, Direction.Positive);
          map.Right = range.EndPoints.First;
          map.RightInner = comparer.GetNearestValue(map.Right, Direction.Negative);
          break;
        case -1:
          map.Left = range.EndPoints.First;
          map.LeftInner = comparer.GetNearestValue(map.Left, Direction.Positive);
          map.Right = range.EndPoints.Second;
          map.RightInner = comparer.GetNearestValue(map.Right, Direction.Negative);
          break;
        default:
          map.Left = range.EndPoints.First;
          map.LeftInner = map.Left;
          map.Right = range.EndPoints.Second;
          map.RightInner = map.Right;
          break;
      }
      map.LeftOuter = comparer.GetNearestValue(map.Left, Direction.Negative);
      map.RightOuter = comparer.GetNearestValue(map.Right, Direction.Positive);

      return map;
    }

    private struct PointMap<T>
    {
      public T Left;
      public T LeftInner;
      public T LeftOuter;
      public T Right;
      public T RightInner;
      public T RightOuter;
    }

    [Test]
    public void GenericTest()
    {
      TestFullRange<int>();
      TestFullRange<string>();

      TestContains(new Range<int>(0, 10));
      TestContains(new Range<int>(0, 0));
      TestContains(new Range<int>(0, 1));
      TestContains(new Range<int>(1, 0));
      TestContains(new Range<int>(10, 0));

      TestEquals(new Range<int>(0, 10));
      TestEquals(new Range<int>(0, 0));
      TestEquals(new Range<int>(0, 1));
      TestEquals(new Range<int>(1, 0));
      TestEquals(new Range<int>(10, 0));
      Assert.IsTrue(Range<int>.Empty.Equals(Range<int>.Empty));

      TestIntersects(new Range<int>(0, 10));
      TestIntersects(new Range<int>(0, 0));
      TestIntersects(new Range<int>(0, 1));
      TestIntersects(new Range<int>(1, 0));
      TestIntersects(new Range<int>(10, 0));

      TestCompareTo(new Range<int>(0, 10));
      TestCompareTo(new Range<int>(0, 0));
      TestCompareTo(new Range<int>(10, 10));
      Assert.IsTrue(Range<int>.Empty.CompareTo(Range<int>.Empty) == 0);

      TestMerge(new Range<int>(0, 10));
      TestMerge(new Range<int>(0, 0));
      TestMerge(new Range<int>(10, 10));

      TestSubtract(new Range<int>(0, 10));
      TestSubtract(new Range<int>(0, 0));
      TestSubtract(new Range<int>(10, 10));

      TestIntersect(new Range<int>(0, 10));
      TestIntersect(new Range<int>(0, 0));
      TestIntersect(new Range<int>(10, 10));
    }
  }
}