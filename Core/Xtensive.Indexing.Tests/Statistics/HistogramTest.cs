// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing.Optimization;

namespace Xtensive.Indexing.Tests.Statistics
{
  [TestFixture]
  [Ignore]
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
      var histogram = new Histogram<Tuple, double>(AdvancedComparer<Tuple>.Default,
        AdvancedComparer<double>.Default, 10);
      TestReplaceAndShrink(histogram, tupleDescriptor);
    }

    [Test]
    public void MergeTest()
    {
      var firstHistogram = new Histogram<Tuple, double>(AdvancedComparer<Tuple>.Default,
        AdvancedComparer<double>.Default, 10);
      var secondHistogram = new Histogram<Tuple, double>(AdvancedComparer<Tuple>.Default,
        AdvancedComparer<double>.Default, 10);
      var rnd = new Random();
      Fill(firstHistogram, tupleDescriptor, rnd);
      Fill(secondHistogram, tupleDescriptor, rnd);
      firstHistogram.Merge(secondHistogram);
      Assert.AreEqual(10, firstHistogram.Count());
    }

    [Test]
    public void InvalidTest()
    {
      AssertEx.ThrowsArgumentOutOfRangeException(() => new Histogram<Tuple, double>(
        AdvancedComparer<Tuple>.Default, AdvancedComparer<double>.Default, 1));
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
      for (int i = 0; i < 7; i++)
        AddRandom(tupleDescriptor, rnd, histogram);
      Assert.AreEqual(10, histogram.Count());
      var newValue = value + value + 1;
      histogram.AddOrReplace(key, value + 1);
      Assert.AreEqual(newValue, histogram.Single(pair => pair.Key == key).Value);
      Assert.AreEqual(1, histogram.Count(pair => pair.Key == minimalSecond));
      AddRandom(tupleDescriptor, rnd, histogram);
      Assert.AreEqual(0, histogram.Count(pair => pair.Key == minimalSecond));
      var updatedMinimalFirst = histogram.Single(pair => pair.Key == minimalFirst);
      Assert.AreEqual(minimalFirstValue + minimalSecondValue, updatedMinimalFirst.Value);
    }

    private static void Fill(Histogram<Tuple, double> histogram, TupleDescriptor tupleDescriptor, Random rnd)
    {
      for (int i = 0; i < histogram.MaxKeyCount; i++)
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