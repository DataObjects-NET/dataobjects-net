// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.10

using System.Linq;
using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Core.SizeCalculators;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Statistics;

namespace Xtensive.Indexing
{
  partial class Index<TKey, TItem> : IIndexHistogramProvider<TKey>
  {
    private Histogram<TKey, double> cashedCountHistogram;
    private Histogram<TKey, double> cashedSizeHistogram;
    private int currentMaxKeyCount;
    private Histogram<TKey, double> countHistogram;
    private Histogram<TKey, double> sizeHistogram;
    private readonly SizeCalculator<TItem> sizeCalculator = SizeCalculator<TItem>.Default;

    Histogram<TKey, double> IIndexHistogramProvider<TKey>.GetCountHistogram(int maxKeyCount)
    {
      //TODO: Provide thread safety
      if (!NeedToUpdateHistograms())
        return countHistogram;
      currentMaxKeyCount = maxKeyCount;
      CreateHistograms(maxKeyCount);
      var rootAsLeaf = RootPage as LeafPage<TKey, TItem>;
      if (rootAsLeaf != null) {
        CollectFromLeafPage(rootAsLeaf);
        return countHistogram;
      }
      var rootAsInnerPage = (InnerPage<TKey, TItem>) RootPage;
      LeafPage<TKey, TItem> firstLeafPage = LeftmostPage;
      //CollectFromLeafPage(firstLeafPage);
      AddLastValue();
      if (TryCollectFromInnerPages(new[] { rootAsInnerPage }))
        return countHistogram;
      CollectFromAllLeafPages(firstLeafPage);
      return countHistogram;
    }

    private bool NeedToUpdateHistograms()
    {
      return true;
    }

    private void CreateHistograms(int maxKeyCount)
    {
      countHistogram = new Histogram<TKey, double>(KeyComparer, AdvancedComparer<double>.Default,
        maxKeyCount);
      sizeHistogram = new Histogram<TKey, double>(KeyComparer, AdvancedComparer<double>.Default,
        maxKeyCount);
    }

    private void CollectFromLeafPage(LeafPage<TKey, TItem> page)
    { 
      var size = page.CurrentSize;
      for (int i = 0; i < size; i++) {
        var currentKey = page.GetKey(i);
        countHistogram.AddOrReplace(currentKey, 1);
        sizeHistogram.AddOrReplace(currentKey, sizeCalculator.GetValueSize(page[i]));
      }
    }

    private LeafPage<TKey, TItem> FindFirstLeafPage(InnerPage<TKey, TItem> root)
    {
      var firstChild = root.GetPage(0);
      var leaf = firstChild.AsLeafPage;
      if (leaf == null)
        return FindFirstLeafPage((InnerPage<TKey, TItem>)firstChild);
      return leaf;
    }

    private bool TryCollectFromInnerPages(IEnumerable<InnerPage<TKey, TItem>> pages)
    {
      if (!PagesHasEnoughData(pages)) {
        var firstChild = pages.First().GetPage(-1);
        if (firstChild.AsLeafPage != null)
          return false;
        return TryCollectFromInnerPages(UniteChildrenOfInnerPages(pages));
      }
      foreach (var page in pages)
        CollectFromInnerPage(page);
      return true;
    }

    private bool PagesHasEnoughData(IEnumerable<InnerPage<TKey, TItem>> pages)
    {
      return pages.Sum(page => page.Size) >= (currentMaxKeyCount
        * Histogram<TKey, double>.DefaultMinFillFactor);
    }

    private static IEnumerable<InnerPage<TKey, TItem>> UniteChildrenOfInnerPages(
      IEnumerable<InnerPage<TKey, TItem>> parents)
    {
      foreach (var parent in parents) {
        var childCount = parent.CurrentSize;
        for (int i = -1; i < childCount; i++)
          yield return parent.GetPage(i).AsInnerPage;
      }
    }

    private void CollectFromInnerPage(InnerPage<TKey, TItem> page)
    {
      var size = page.CurrentSize;
      bool isFirst = true;
      for (int i = -1; i < size; i++) {
        var child = page.GetPage(i);
        if (isFirst) {
          AddMeasure(LeftmostPage.Key, child);
          isFirst = false;
          continue;
        }
        AddMeasure(child.Key, child);
      }
    }

    private void CollectFromAllLeafPages(LeafPage<TKey, TItem> firstLeafPage)
    {
      var nextPage = firstLeafPage;
      while (nextPage != null) {
        CollectFromLeafPage(nextPage);
        nextPage = nextPage.RightPage;
      }
    }

    private void AddMeasure(TKey key, DataPage<TKey, TItem> page)
    {
      var measureResults = page.MeasureResults;
      countHistogram.AddOrReplace(key,
        (long)measureResults[CountMeasure<TItem, long>.CommonName].Result);
      sizeHistogram.AddOrReplace(key,
        (long)measureResults[SizeMeasure<TItem>.CommonName].Result);
    }

    private void AddLastValue()
    {
      var currentKey = RightmostPage.GetKey(RightmostPage.CurrentSize - 1);
      countHistogram.AddOrReplace(currentKey, 1);
      sizeHistogram.AddOrReplace(currentKey,
        sizeCalculator.GetValueSize(RightmostPage[RightmostPage.CurrentSize - 1]));
    }
  }
}