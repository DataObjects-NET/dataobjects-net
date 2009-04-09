// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Comparison;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Indexing.Statistics;

namespace Xtensive.Indexing.Tests.Statistics
{
  [TestFixture]
  public class HistogramTest
  {
    private TupleDescriptor tupleDescriptor;

    [SetUp]
    public void SetUp()
    {
      tupleDescriptor = TupleDescriptor.Create(new[] { typeof(int), typeof(string) });
    }

    [Test]
    public void AddTest()
    {
      var firstKey = Tuple.Create(tupleDescriptor, 1, "aaa");
      var histogram = new Histogram<Tuple, double>(AdvancedComparer<Tuple>.Default,
        AdvancedComparer<double>.Default, 10, firstKey);
      Assert.AreEqual(firstKey, histogram.First().Key);
      TestInsertionBeforeFirst(histogram, tupleDescriptor);
      TestReplaceAndShrink(histogram, tupleDescriptor);
    }

    [Test]
    public void MergeTest()
    {
      var firstKey0 = Tuple.Create(tupleDescriptor, 1, "aaa");
      var firstHistogram = new Histogram<Tuple, double>(AdvancedComparer<Tuple>.Default,
        AdvancedComparer<double>.Default, 10, firstKey0);
      var firstKey1 = Tuple.Create(tupleDescriptor, 2, "aaa");
      var secondHistogram = new Histogram<Tuple, double>(AdvancedComparer<Tuple>.Default,
        AdvancedComparer<double>.Default, 10, firstKey1);
      var rnd = new Random();
      Fill(firstHistogram, tupleDescriptor, rnd);
      Fill(secondHistogram, tupleDescriptor, rnd);
      firstHistogram.Merge(secondHistogram);
      Assert.AreEqual(10, firstHistogram.Count());
      Assert.AreEqual(firstKey0, firstHistogram.First().Key);
    }

    [Test]
    public void InvalidTest()
    {
      var firstKey0 = Tuple.Create(tupleDescriptor, 1, "aaa");
      AssertEx.ThrowsArgumentOutOfRangeException(() => new Histogram<Tuple, double>(
        AdvancedComparer<Tuple>.Default, AdvancedComparer<double>.Default, 1, firstKey0));
    }

    private static void TestInsertionBeforeFirst(Histogram<Tuple, double> histogram,
      TupleDescriptor tupleDescriptor)
    {
      var rnd = new Random();
      Tuple key = Tuple.Create(tupleDescriptor, -1, "aaa" + rnd.Next());
      double value = rnd.NextDouble() * 100;
      Assert.AreEqual(0, histogram.First().Value);
      histogram.AddOrReplace(key, value);
      Assert.AreEqual(0, histogram.First().Value);
      Assert.AreEqual(key, histogram.First().Key);
    }

    private static void TestReplaceAndShrink(Histogram<Tuple, double> histogram,
      TupleDescriptor tupleDescriptor)
    {
      var rnd = new Random();
      Tuple key = Tuple.Create(tupleDescriptor, 8, "aaa" + rnd.Next());
      double value = rnd.NextDouble() * 100;
      histogram.AddOrReplace(key, value);
      var minimalFirst = Tuple.Create(tupleDescriptor, 4, "aaa");
      const double minimalFirstValue = 0.000001;
      histogram.AddOrReplace(minimalFirst, minimalFirstValue);
      var minimalSecond = Tuple.Create(tupleDescriptor, 5, "aaa");
      const double minimalSecondValue = 0.000001;
      histogram.AddOrReplace(minimalSecond, minimalSecondValue);
      for (int i = 0; i < 5; i++)
        AddRandom(tupleDescriptor, rnd, histogram);
      Assert.AreEqual(10, histogram.Count());
      var newValue = value + 1;
      histogram.AddOrReplace(key, value + 1);
      Assert.AreEqual(newValue, histogram.Single(pair => pair.Key == key).Value);
      Assert.AreEqual(1, histogram.Count(pair => pair.Key == minimalFirst));
      AddRandom(tupleDescriptor, rnd, histogram);
      Assert.AreEqual(0, histogram.Count(pair => pair.Key == minimalFirst));
      var updatedMinimalSecond = histogram.Single(pair => pair.Key == minimalSecond);
      Assert.AreEqual((minimalFirstValue + minimalSecondValue) / 2, updatedMinimalSecond.Value);
    }

    private static void Fill(Histogram<Tuple, double> histogram, TupleDescriptor tupleDescriptor, Random rnd)
    {
      for (int i = 0; i < histogram.MaxKeyCount - 1; i++)
        AddRandom(tupleDescriptor, rnd, histogram);
    }

    private static void AddRandom(TupleDescriptor tupleDescriptor, Random rnd,
      Histogram<Tuple, double> histogram)
    {
      histogram.AddOrReplace(Tuple.Create(tupleDescriptor, rnd.Next(10, 100), "aaa" + rnd.Next()),
        rnd.NextDouble() * 100);
    }
  }
}