// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Comparison;
using Xtensive.Core.Conversion;
using Xtensive.Core.Testing;
using Xtensive.Indexing.Statistics;

namespace Xtensive.Indexing.Tests.Statistics
{
  [TestFixture]
  public class IndexAsHistogramProviderTest
  {
    private const int pageSize = 6;

    [Test]
    public void CollectCountFromRootAsLeafTest()
    {
      var index = CreateIndex<string>();
      PopulateIndex(index, pageSize);
      Assert.IsTrue(index.RootPage.AsLeafPage != null);
      Histogram<string, double> histogram = ((IIndexHistogramProvider<string>) index).GetCountHistogram(10);
      Assert.AreEqual(pageSize, histogram.Count());
      Assert.IsTrue(histogram.All(pair => pair.Value == 1));
      Assert.AreEqual(6, histogram.Select(pair => pair.Key)
        .Intersect(index.Select((key, value) => key)).Count());
    }

    [Test]
    public void CollectCountFromRootAsLastInnerPage()
    {
      var index = CreateIndex<string>();
      PopulateIndex(index, pageSize*3);
      Assert.IsTrue(index.RootPage.AsInnerPage != null);
      var maxKeyCount = 16;
      Histogram<string, double> histogram = ((IIndexHistogramProvider<string>)index)
        .GetCountHistogram(maxKeyCount);
      Assert.AreEqual(maxKeyCount, histogram.Count());
      Assert.AreEqual(3, histogram.Where(pair => pair.Value == 2).Count());
    }

    [Test]
    public void CollectCountFromInnerPages()
    {
      var index = CreateIndex<string>();
      PopulateIndex(index, pageSize * pageSize * pageSize);
      Assert.IsTrue(index.RootPage.AsInnerPage != null);
      var maxKeyCount = 16;
      Histogram<string, double> histogram = ((IIndexHistogramProvider<string>)index)
        .GetCountHistogram(maxKeyCount);
      Assert.AreEqual(maxKeyCount, histogram.Count());
      Assert.AreEqual(3, histogram.Where(pair => pair.Value == 2).Count());
    }

    private Index<T, T> CreateIndex<T>()
    {
      var config = new IndexConfiguration<T, T>(AdvancedConverter<T, T>.Default.Implementation.Convert,
        AdvancedComparer<T>.Default) { PageSize = pageSize };
      return new Index<T, T>(config);
    }

    private void PopulateIndex<T>(IIndex<T, T> index, int count)
    {
      index.Clear();
      var rndItems = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>()
        .GetInstances(new Random(), count);
      foreach (var item in rndItems) {
        index.Add(item);
      }
    }
  }
}