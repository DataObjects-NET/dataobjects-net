// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.08

using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class RayTest
  {
    [Test]
    public void MainTest()
    {
      GenericTest(new Ray<int>(0, Direction.Positive));
    }

    public static void GenericTest<T>(Ray<T> ray)
    {
      Direction reversedDirection = ray.Direction==Direction.Positive ? Direction.Negative : Direction.Positive;

      Assert.IsTrue(ray.Contains(ray.Point));
      Assert.IsTrue(ray.Intersects(new Ray<T>(ray.Point)));
      Assert.IsFalse(ray.Equals(new Ray<T>(ray.Point, reversedDirection)));
      Assert.IsTrue(ray.CompareTo(ray)==0);
      Assert.IsTrue(ray.CompareTo(new Ray<T>(ray.Point, reversedDirection)) > 0);

      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      if (!comparer.ValueRangeInfo.HasDeltaValue)
        return;
      T pre, post;
      pre = comparer.GetNearestValue(ray.Point, reversedDirection);
      post = comparer.GetNearestValue(ray.Point, ray.Direction);

      Assert.IsTrue(ray.Contains(post));
      Assert.IsFalse(ray.Contains(pre));
      Assert.IsTrue(ray.Intersects(new Ray<T>(pre)));
      Assert.IsTrue(ray.Intersects(new Ray<T>(post, reversedDirection)));
      Assert.IsTrue(ray.Intersects(new Ray<T>(ray.Point, reversedDirection)));
      Assert.IsTrue(ray.Equals(ray));
      Assert.IsTrue(ray.CompareTo(new Ray<T>(post, reversedDirection)) < 0);
    }
  }
}