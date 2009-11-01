// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.17

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Indexing;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class RangeSetTest
  {
    [Test]
    public void UnionTest()
    {
      RangeSet<Range<int>, int> rangeSet = new RangeSet<Range<int>, int>(new Range<int>[] {new Range<int>(20, 30)}, ComparerProvider.GetComparer<int>());
      rangeSet.Union(new Range<int>(10, 100));
      rangeSet.Union(new Range<int>(0, 200));
      rangeSet.Union(new Range<int>(50, 90));
      rangeSet.Union(new Range<int>(500, 600));
      rangeSet.Union(new Range<int>(5000, 6000));
      List<Range<int>> result = new List<Range<int>>(rangeSet);
      Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void SubtractTest()
    {
      RangeSet<Range<int>, int> rangeSet = new RangeSet<Range<int>, int>(new Range<int>[] { new Range<int>(20, 30) }, Comparer<int>.Default);
      rangeSet.Union(new Range<int>(10, 100));
      rangeSet.Union(new Range<int>(0, 200));
      rangeSet.Union(new Range<int>(50, 90));
      rangeSet.Subtract(new Range<int>(30,130));

      List<Range<int>> result = new List<Range<int>>(rangeSet);
      Assert.AreEqual(2, result.Count);

      Assert.AreEqual(new Range<int>(new Ray<int>(0, RayProperties.PositiveDirection), new Ray<int>(30, RayProperties.NegativeDirectionExclusive)), result[0]);
      Assert.AreEqual(new Range<int>(new Ray<int>(130, RayProperties.PositiveDirectionExclusive), new Ray<int>(200, RayProperties.NegativeDirection)), result[1]);
    }

    [Test]
    public void IntersectionTest()
    {
      RangeSet<Range<int>, int> rangeSet = new RangeSet<Range<int>, int>(new Range<int>[] { new Range<int>(20, 30) }, Comparer<int>.Default);
      rangeSet.Union(new Range<int>(10, 100));
      rangeSet.Union(new Range<int>(0, 200));
      rangeSet.Union(new Range<int>(50, 90));
      rangeSet.Intersect(new Range<int>(30, 130));

      List<Range<int>> result = new List<Range<int>>(rangeSet);
      Assert.AreEqual(1, result.Count);

      Assert.AreEqual(new Range<int>(new Ray<int>(30, RayProperties.PositiveDirection), new Ray<int>(130, RayProperties.NegativeDirection)), result[0]);
    }

    [Test]
    [Ignore("This part does not work yet.")]
    public void RSUnionTest()
    {
      RangeSet<Range<int>, int> rangeSetX = new RangeSet<Range<int>, int>(new Range<int>[] { new Range<int>(20, 30) }, Comparer<int>.Default);
      rangeSetX.Union(new Range<int>(10, 100));
      rangeSetX.Union(new Range<int>(0, 200));
      rangeSetX.Union(new Range<int>(50, 90));

      RangeSet<Range<int>, int> rangeSetY = new RangeSet<Range<int>, int>(new Range<int>[] { new Range<int>(20, 30) }, Comparer<int>.Default);
      rangeSetY.Union(new Range<int>(100, 140));
      rangeSetY.Union(new Range<int>(500, 600));
      rangeSetY.Union(new Range<int>(50, 90));

      rangeSetX.Union(rangeSetY);

      List<Range<int>> result = new List<Range<int>>(rangeSetX);
      Assert.AreEqual(2, result.Count);

    }

    [Test]
    public void RSSubtractTest()
    {

    }

  }
}
