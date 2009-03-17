// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class RangeSetTest
  {
    [Test]
    public void UniteTest()
    {
      var rangeSetX = new RangeSet<Entire<Int32>>(new Range<Entire<Int32>>(20, 30), AdvancedComparer<Entire<Int32>>.Default);
      rangeSetX.Unite(new Range<Entire<Int32>>(50, 30));
      rangeSetX.Unite(new Range<Entire<Int32>>(-10, 2));
      rangeSetX.Unite(new Range<Entire<Int32>>(0, Entire<Int32>.MinValue));

      Assert.AreEqual(2, rangeSetX.Count());

      var rangeSetY = new RangeSet<Entire<Int32>>(new Range<Entire<Int32>>(-20, -30), AdvancedComparer<Entire<Int32>>.Default);
      rangeSetY.Unite(new Range<Entire<Int32>>(new Entire<int>(-50, Direction.Positive), -15));
      rangeSetY.Unite(new Range<Entire<Int32>>(-50, -60));
      rangeSetY.Unite(new Range<Entire<Int32>>(-50, -100));

      Assert.AreEqual(2, rangeSetY.Count());

      rangeSetX.Unite(rangeSetY);

      Assert.AreEqual(2, rangeSetX.Count());
    }

    [Test]
    public void IntersectTest()
    {
      var rangeSetX = new RangeSet<Entire<Int32>>(new Range<Entire<Int32>>(20, 30), AdvancedComparer<Entire<Int32>>.Default);
      rangeSetX.Unite(new Range<Entire<Int32>>(100, 10));
      rangeSetX.Unite(new Range<Entire<Int32>>(0, 200));
      rangeSetX.Unite(new Range<Entire<Int32>>(50, 90));
      rangeSetX.Intersect(new Range<Entire<Int32>>(30, 130));
      Assert.AreEqual(1, rangeSetX.Count());

      var rangeSetY = new RangeSet<Entire<Int32>>(new Range<Entire<Int32>>(20, 30), AdvancedComparer<Entire<Int32>>.Default);
      rangeSetY.Unite(new Range<Entire<Int32>>(100, 140));
      rangeSetY.Unite(new Range<Entire<Int32>>(500, 600));
      rangeSetY.Unite(new Range<Entire<Int32>>(50, 90));
      rangeSetY.Intersect(new Range<Entire<Int32>>(510, 130));
      Assert.AreEqual(2, rangeSetY.Count());

      rangeSetX.Intersect(rangeSetY);
      Assert.AreEqual(1, rangeSetX.Count());
    }

    [Test]
    public void InvertTest()
    {
      var rangeSet = new RangeSet<Entire<Int32>>(new Range<Entire<Int32>>(20, 30), AdvancedComparer<Entire<Int32>>.Default);
      rangeSet.Unite(new Range<Entire<Int32>>(100, 140));
      rangeSet.Unite(new Range<Entire<Int32>>(500, 600));
      rangeSet.Unite(new Range<Entire<Int32>>(50, 90));
      rangeSet.Invert();

      Assert.AreEqual(5, rangeSet.Count());
    }
  }
}
