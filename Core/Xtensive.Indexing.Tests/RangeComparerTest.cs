// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.17

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Indexing;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class RangeComparerTest
  {
    [Test]
    public void Test()
    {
      using (new Measurement()) {
        RangeComparer<int> comparer = new RangeComparer<int>(ComparerProvider.GetComparer<int>());
        Assert.IsTrue(comparer.Compare(new Range<int>(0, 10), new Range<int>(20, 30)) < 0);
        Assert.IsTrue(comparer.Compare(new Range<int>(-100, 10), new Range<int>(-200, -150)) > 0);
        Assert.IsTrue(comparer.Compare(new Range<int>(0, 10), new Range<int>(5, 20)) == 0);
        Assert.IsTrue(comparer.Compare(new Range<int>(0, 10), new Range<int>(0, 20)) == 0);
        Assert.IsTrue(comparer.Compare(new Range<int>(0, 10), new Range<int>(-5, 20)) == 0);
        Assert.IsTrue(comparer.Compare(new Range<int>(0, 10), new Range<int>(2, 5)) == 0);
        Assert.IsTrue(comparer.Compare(new Range<int>(0, 10), new Range<int>(10, 20)) == 0);
      }
    }
  }
}