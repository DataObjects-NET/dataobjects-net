// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.04.22

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing.Tests.Index
{
  [TestFixture]
  public class MeasuresTest
  {
    private Random random = RandomManager.CreateRandom();
    private const int count = 1000;
    private const int rangeChecks = count/100;

    [Test]
    public void Measures()
    {
      TestInternal<int, int>();
      TestInternal<long, long>();
      TestInternal<string, long>();
      TestInternal<string, decimal>();
    }

    private void TestInternal<TKey, TItem>()
    {
      Converter<TItem, TKey> keyExtractor = AdvancedConverter<TItem, TKey>.Default.Implementation.Convert;
      AdvancedComparer<TKey> keyComparer = AdvancedComparer<TKey>.Default;
      AdvancedComparer<TItem> itemComparer = AdvancedComparer<TItem>.Default;

      IndexConfiguration<TKey, TItem> indexConfiguration = new IndexConfiguration<TKey, TItem>(keyExtractor, keyComparer);
      IndexConfigurationBase<TKey, TItem> listConfiguration = new IndexConfigurationBase<TKey, TItem>(keyExtractor, keyComparer);

      InitConfiguration(indexConfiguration);
      InitConfiguration(listConfiguration);

      IUniqueOrderedIndex<TKey, TItem> list = new SortedListIndex<TKey, TItem>(listConfiguration);
      IUniqueOrderedIndex<TKey, TItem> index = new Index<TKey, TItem>(indexConfiguration);

      Xtensive.Collections.ISet<TItem> instances = new SetSlim<TItem>(InstanceGeneratorProvider.Default.GetInstanceGenerator<TItem>().GetInstances(random, count));
      Xtensive.Collections.ISet<TItem> missingInstances = new SetSlim<TItem>(InstanceGeneratorProvider.Default.GetInstanceGenerator<TItem>().GetInstances(random, count));
      instances.ExceptWith(missingInstances);

      Process(instances, missingInstances, itemComparer, keyExtractor, list, index);
    }

    private void Process<TKey, TItem>(Xtensive.Collections.ISet<TItem> instances, Xtensive.Collections.ISet<TItem> missingInstances, AdvancedComparer<TItem> itemComparer,
      Converter<TItem, TKey> keyExtractor,
      params IUniqueOrderedIndex<TKey, TItem>[] indexes)
    {
      // Fill 
      foreach (IUniqueOrderedIndex<TKey, TItem> index in indexes) {
        Fill(index, instances);
      }

      // Check data
      foreach (IUniqueOrderedIndex<TKey, TItem> index in indexes) {
        foreach (TItem item in instances) {
          Assert.IsTrue(index.Contains(item));
        }
        foreach (TItem missingInstance in missingInstances) {
          Assert.IsFalse(index.Contains(missingInstance));
        }
        Assert.AreEqual(index.Count, instances.Count);
      }

      // Check correct measures (just in case)
      int measureCount = (int) indexes[0].Measures.Count;
      for (int index = 1; index < indexes.Length; index++) {
        Assert.AreEqual(indexes[index].Measures.Count, measureCount);
      }
      if (measureCount > 0) {
        for (int i = 0; i < measureCount; i++) {
          IMeasure<TItem> measure = indexes[0].Measures[i];
          for (int index = 1; index < indexes.Length; index++) {
            Assert.IsTrue(measure.GetType()==indexes[index].Measures[i].GetType());
            Assert.IsTrue(measure.Name==indexes[index].Measures[i].Name);
          }
        }
      }

      // Check full range
      for (int measureIndex = 0; measureIndex < measureCount; measureIndex++) {
        string measureName = indexes[0].Measures[measureIndex].Name;
        object firstMeasurement = indexes[0].GetMeasureResult(measureName);
        for (int index = 1; index < indexes.Length; index++) {
          object mesasurement = indexes[index].GetMeasureResult(measureName);
          Assert.AreEqual(firstMeasurement, mesasurement);
        }
      }

      List<TItem> instanceList = new List<TItem>(instances);
      for (int i = 0; i < rangeChecks; i++) {
        TItem firstValue = instanceList[random.Next()%instanceList.Count];
        TItem secondValue = instanceList[random.Next()%instanceList.Count];
        int compareResult = itemComparer.Compare(firstValue, secondValue);
        if (compareResult!=0) {
          Entire<TKey> firstEntire = new Entire<TKey>(keyExtractor(firstValue));
          Entire<TKey> secondEntire = new Entire<TKey>(keyExtractor(secondValue));
          Range<Entire<TKey>> range = new Range<Entire<TKey>>(firstEntire, secondEntire);
          for (int measureIndex = 0; measureIndex < measureCount; measureIndex++) {
            string measureName = indexes[0].Measures[measureIndex].Name;
            object firstMeasurement = indexes[0].GetMeasureResult(range, measureName);
            for (int index = 1; index < indexes.Length; index++) {
              object measurement = indexes[index].GetMeasureResult(range, measureName);
              Assert.AreEqual(firstMeasurement, measurement);
            }
          }
        }
      }
    }

    private void InitConfiguration<TKey, TItem>(IndexConfigurationBase<TKey, TItem> configuration)
    {
      configuration.Measures.Add(new MinMeasure<TItem, TItem>("Min", AdvancedConverter<TItem, TItem>.Default.Implementation.Convert));
      configuration.Measures.Add(new MaxMeasure<TItem, TItem>("Max", AdvancedConverter<TItem, TItem>.Default.Implementation.Convert));
    }

    private void Fill<TKey, TItem>(IIndex<TKey, TItem> index, IEnumerable<TItem> items)
    {
      // Fill data
      int count = 0;
      foreach (TItem value in items) {
        index.Add(value);
        count++;
        Assert.IsTrue(index.ContainsKey(index.KeyExtractor(value)));
        Assert.IsTrue(index.Contains(value));
        Assert.AreEqual(index.Count, count);
      }
    }
  }
}