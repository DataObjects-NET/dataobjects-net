// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.16

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing.Tests.Index
{
  /// <summary>
  /// Extension methods for BPlusTree test and debug.
  /// </summary>
  public static class IndexTestTools
  {
    public static void DumpIndex<TKey, TItem>(this Index<TKey, TItem> index)
    {
      Log.Info("Dumping index");
      DumpPage(index.GetDescriptorPage().RootPage, 0);
    }

    public static void DumpPage<TKey, TItem>(this DataPage<TKey, TItem> dataPage)
    {
      Log.Info("Dumping page");
      DumpPage(dataPage, 0);
    }

    private static void DumpPage<TKey, TItem>(DataPage<TKey, TItem> dataPage, int level)
    {
      InnerPage<TKey, TItem> innerPage = dataPage.AsInnerPage;
      StringBuilder sb = new StringBuilder();
      sb.Append(' ', level);
      sb.Append(innerPage == null ? "L" : "I");
      sb.Append(dataPage.Key);
      foreach (IMeasure<TItem> measureResult in dataPage.MeasureResults) {
        sb.AppendFormat(" {0}: {1}", measureResult.Name, measureResult.Result);
      }
      Log.Info(sb.ToString());
      if (innerPage != null) {
        for (int i = -1; i < innerPage.CurrentSize; i++) {
          DumpPage(innerPage.GetPage(i), level + 1);
        }
      }
    }

    public static void CheckIntegrity<TKey, TItem>(this Index<TKey, TItem> index)
    {
      Log.Info("Validating index.");
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      List<TItem> itemsList = new List<TItem>(index);
      SetSlim<TItem> itemsSet = new SetSlim<TItem>(index);
      Assert.AreEqual(itemsList.Count, itemsSet.Count);
      long count = index.Count;

      DescriptorPage<TKey, TItem> descriptorPage = GetDescriptorPage(index);
      List<TKey> leafKeys = new List<TKey>();
      LeafPage<TKey, TItem> leafPage = descriptorPage.LeftmostPage;
      while (leafPage!=null) {
        leafKeys.Add(leafPage.Key);
        leafPage = leafPage.RightPage;
      }
      foreach (TKey leafKey in leafKeys) {
        if (!index.ContainsKey(leafKey)) {
          Log.Debug("Leaf page with key {0} is not accessible by ContainsKey", leafKey);
        }
        // Assert.IsTrue(index.ContainsKey(leafKey), "Leaf page with key {0} is not accessible by ContainsKey", leafKey);
      }
      Assert.AreEqual(count, itemsSet.Count);
      foreach (TItem item in index) {
        Assert.IsTrue(itemsSet.Contains(item));
      }
      foreach (TItem item in itemsSet) {
        Assert.IsTrue(index.Contains(item));
      }
      // Check balancing
      InnerPage<TKey, TItem> rootPage = descriptorPage.RootPage as InnerPage<TKey, TItem>;
      int minDepth = int.MaxValue;
      int maxDepth = 0;
      if (rootPage!=null) {
        GetDepth(rootPage, ref minDepth, ref maxDepth, 0);
        Log.Debug("Min depth: {0}, max depth: {1}", minDepth, maxDepth);
        Assert.AreEqual(minDepth, maxDepth, "Index is not balanced.");
      }
      //Check Measures
      CheckMeasures(descriptorPage.RootPage);
    }

    public static DescriptorPage<TKey, TItem> GetDescriptorPage<TItem, TKey>(this Index<TKey, TItem> index)
    {
      return (DescriptorPage<TKey, TItem>)
        typeof (Index<TKey, TItem>)
          .GetProperty("DescriptorPage", BindingFlags.Instance | BindingFlags.NonPublic)
          .GetValue(index, null);
    }

    private static void CheckMeasures<TKey, TItem>(DataPage<TKey, TItem> dataPage)
    {
      MeasureResultSet<TItem> measureResults = new MeasureResultSet<TItem>(dataPage.DescriptorPage.Measures);
      InnerPage<TKey, TItem> innerPage = dataPage.AsInnerPage;
      if (innerPage!=null) {
        for (int i = -1; i < innerPage.CurrentSize; i++) {
          DataPage<TKey, TItem> childPage = innerPage.GetPage(i);
          MeasureUtils<TItem>.BatchAdd(measureResults, childPage.MeasureResults);
          CheckMeasures(childPage);
        }
      }
      LeafPage<TKey, TItem> leafPage = dataPage.AsLeafPage;
      if (leafPage!=null) {
        for (int i = 0; i < leafPage.CurrentSize; i++) {
          measureResults.Add(leafPage[i]);
        }
      }
      for (int i = 0; i < measureResults.Count; i++) {
        if (!Equals(measureResults[i].Result, dataPage.MeasureResults[i].Result)) {
          dataPage.Provider.Index.DumpIndex();
        }
        Assert.AreEqual(measureResults[i].Result, dataPage.MeasureResults[i].Result,
          String.Format("Measure results incorrect. {0} on page {1}{2}", measureResults[i].Name, dataPage.AsInnerPage==null ? "L" : "I", dataPage.Key));
      }
    }

    private static void GetDepth<TKey, TItem>(InnerPage<TKey, TItem> innerPage, ref int minDepth, ref int maxDepth, int level)
    {
      for (int i = -1; i < innerPage.CurrentSize; i++) {
        DataPage<TKey, TItem> dataPage = innerPage.Provider.Resolve(innerPage[i].Value).AsDataPage;
        if (dataPage.AsLeafPage!=null) {
          if (minDepth > level)
            minDepth = level;
          if (maxDepth < level) {
            maxDepth = level;
          }
        }
        if (dataPage.AsInnerPage!=null) {
          GetDepth(dataPage.AsInnerPage, ref minDepth, ref maxDepth, level + 1);
        }
      }
    }
  }
}