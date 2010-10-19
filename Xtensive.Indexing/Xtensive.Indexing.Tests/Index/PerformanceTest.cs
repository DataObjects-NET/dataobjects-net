// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.05

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Diagnostics;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;

namespace Xtensive.Indexing.Tests.Index
{
  public class PerformanceTest
  {
    private const int ItemsCount = 100000;

    [TestFixture]
    public abstract class IndexProfiling<TKey,TItem>
    {
      protected List<TItem> list;
      protected List<TItem> orderedList;
      protected Index<TKey, TItem> index;
      protected Index<TKey, TItem> readOnlyIndex;
      protected Converter<TItem, TKey> keyExtractor;

      [TestFixtureSetUp]
      public abstract void SetUp();

      [Test]
      [Explicit]
      [Category("Profile")]
      public void DictionaryAddTest()
      {
        var dictionary = new SortedDictionary<TKey, TItem>();
        TestHelper.CollectGarbage();
        using (new Measurement("Dictionary.Add", MeasurementOptions.Log, ItemsCount))
          foreach (TItem item in list)
            dictionary.Add(keyExtractor(item), item);
      }

      [Test]
      [Explicit]
      [Category("Profile")]
      public void AddTest()
      {
        index.Clear();
        TestHelper.CollectGarbage();
        using (new Measurement("Add", MeasurementOptions.Log, ItemsCount))
          foreach (TItem item in list)
            index.Add(item);
      }

      [Test]
      [Explicit]
      [Category("Profile")]
      public void SeekTest()
      {
        TestHelper.CollectGarbage();
        using (new Measurement("Seek", MeasurementOptions.Log, ItemsCount))
          foreach (TItem item in list)
          {
            var key = keyExtractor(item);
            SeekResult<TItem> result = readOnlyIndex.Seek(new Ray<Entire<TKey>>(new Entire<TKey>(key)));
          }
      }

      [Test]
      [Explicit]
      [Category("Profile")]
      public void EnumerateTest()
      {
        TestHelper.CollectGarbage();
        using (new Measurement("Enumerator.MoveNext+Current", MeasurementOptions.Log, ItemsCount))
          foreach (TItem i in readOnlyIndex)
          {
          }
      }

      [Test]
      [Explicit]
      [Category("Profile")]
      public void GetItemsTest()
      {
        TestHelper.CollectGarbage();
        int count = ItemsCount;
        using (new Measurement("GetItems", MeasurementOptions.Log, count))
          for (int j = 0; j < count; j++)
          {
            var from = keyExtractor(orderedList[j]);
            var to = keyExtractor(orderedList[count - 1]);
            readOnlyIndex.GetItems(new Range<Entire<TKey>>(
              new Entire<TKey>(from),
              new Entire<TKey>(to)));
          }
      }

      [Test]
      [Explicit]
      [Category("Profile")]
      public void GetFirstItemTest()
      {
        TestHelper.CollectGarbage();
        int count = ItemsCount;
        using (new Measurement("GetItems+First", MeasurementOptions.Log, count))
          for (int j = 0; j < count; j++)
          {
            var from = keyExtractor(orderedList[j]);
            var to = keyExtractor(orderedList[count - 1]);
            TItem first = readOnlyIndex.GetItems(new Range<Entire<TKey>>(
              new Entire<TKey>(from),
              new Entire<TKey>(to))).First();
            
          }
      }

      [Test]
      [Explicit]
      [Category("Profile")]
      public void EnumerateGetItemsTest()
      {
        TestHelper.CollectGarbage();
        int count = ItemsCount;
        using (new Measurement("GetItems,Enumerator.MoveNext+Current", MeasurementOptions.Log, count))
          foreach (TItem i in readOnlyIndex.GetItems(new Range<Entire<TKey>>(
            new Entire<TKey>(InfinityType.Negative),
            new Entire<TKey>(InfinityType.Positive))))
          {
          }
      }

      [Test]
      [Explicit]
      [Category("Profile")]
      public void ContainsTest()
      {
        TestHelper.CollectGarbage();
        using (new Measurement("Contains", MeasurementOptions.Log, ItemsCount))
          foreach (TItem item in list)
            Assert.IsTrue(readOnlyIndex.Contains(item));
      }


      [Test]
      [Explicit]
      [Category("Profile")]
      public void RemoveTest()
      {
        if (index.Count != ItemsCount) {
          index.Clear();
          Assert.AreEqual(0, index.Count);
          AddTest();
        }
        TestHelper.CollectGarbage();
        using (new Measurement("Remove", MeasurementOptions.Log, ItemsCount))
          foreach (TItem item in list)
            Assert.IsTrue(index.Remove(item));
        Assert.AreEqual(0, index.Count);
      }
    }

    [TestFixture]
    public class Int32Int32Profiling : IndexProfiling<int, int>
    {
      [TestFixtureSetUp]
      public override void SetUp()
      {
        var dictionary = new SortedDictionary<int, int>();
        list = new List<int>(ItemsCount);

        var random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        IEnumerator<int> generator = InstanceGenerationUtils<int>.GetInstances(random, 0).GetEnumerator();
        int count = 0;
        while (count < ItemsCount && generator.MoveNext()) {
          int i = generator.Current;
          if (!dictionary.ContainsKey(i)) {
            dictionary.Add(i, i);
            list.Add(i);
            count++;
          }
        }

        keyExtractor = input => input;
        orderedList = new List<int>(list);
        orderedList.Sort();

        var configuration = new IndexConfiguration<int, int> {KeyExtractor = keyExtractor, KeyComparer = AdvancedComparer<int>.Default};
        index = new Index<int, int>(configuration);
        AddTest();
        readOnlyIndex = index;
        index = new Index<int, int>(configuration);
      }
    }

    [TestFixture]
    public class TupleTupleProfiling : IndexProfiling<Tuple,Tuple>
    {
      [TestFixtureSetUp]
      public override void SetUp()
      {
        var dictionary = new SortedDictionary<int, int>();
        var descriptor = TupleDescriptor.Create(new[] {typeof (int), typeof (int)});
        list = new List<Tuple>(ItemsCount);
        AdvancedComparer<Tuple> comparer = AdvancedComparer<Tuple>.Default.ApplyRules(
          new ComparisonRules(ComparisonRule.Positive, new[]{ComparisonRules.Positive}, ComparisonRules.None));

        var random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        IEnumerator<int> generator = InstanceGenerationUtils<int>.GetInstances(random, 0).GetEnumerator();
        int count = 0;
        while (count < ItemsCount && generator.MoveNext())
        {
          int i = generator.Current;
          if (!dictionary.ContainsKey(i)) {
            dictionary.Add(i, i);
            list.Add(Tuple.Create(descriptor, i, i));
            count++;
          }
        }

        keyExtractor = input => input;
        orderedList = new List<Tuple>(list);
        orderedList.Sort(comparer.Implementation);

        var configuration = new IndexConfiguration<Tuple,Tuple> {KeyExtractor = keyExtractor, KeyComparer = comparer};
        index = new Index<Tuple, Tuple>(configuration);
        AddTest();
        readOnlyIndex = index;
        index = new Index<Tuple, Tuple>(configuration);
      }
    }

    [TestFixture]
    public class TupleTupleProfilingWithExtractor : IndexProfiling<Tuple,Tuple>
    {
      [TestFixtureSetUp]
      public override void SetUp()
      {
        var dictionary = new SortedDictionary<int, int>();
        var descriptor = TupleDescriptor.Create(new[] {typeof (int), typeof (int)});
        list = new List<Tuple>(ItemsCount);
        AdvancedComparer<Tuple> comparer = AdvancedComparer<Tuple>.Default;

        var random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        IEnumerator<int> generator = InstanceGenerationUtils<int>.GetInstances(random, 0).GetEnumerator();
        int count = 0;
        while (count < ItemsCount && generator.MoveNext()) {
          int i = generator.Current;
          if (!dictionary.ContainsKey(i)) {
            dictionary.Add(i, i);
            list.Add(Tuple.Create(descriptor, i, i));
            count++;
          }
        }

        TupleDescriptor keyDescriptor = TupleDescriptor.Create(new [] {typeof(int)});
        var transform = new MapTransform(true, keyDescriptor, new[] {0});
        keyExtractor = input => transform.Apply(TupleTransformType.TransformedTuple, input);
        orderedList = new List<Tuple>(list);
        orderedList.Sort(comparer.Implementation);

        var configuration = new IndexConfiguration<Tuple, Tuple> {KeyExtractor = keyExtractor, KeyComparer = comparer};
        index = new Index<Tuple, Tuple>(configuration);
        AddTest();
        readOnlyIndex = index;
        index = new Index<Tuple, Tuple>(configuration);
      }
    }

    [TestFixture]
    public class Int32TupleProfilingWithExtractor : IndexProfiling<int, Tuple>
    {
      [TestFixtureSetUp]
      public override void SetUp()
      {
        var dictionary = new SortedDictionary<int, int>();
        var descriptor = TupleDescriptor.Create(new[] { typeof(int), typeof(int) });
        list = new List<Tuple>(ItemsCount);
        AdvancedComparer<int> comparer = AdvancedComparer<int>.Default;

        var random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        IEnumerator<int> generator = InstanceGenerationUtils<int>.GetInstances(random, 0).GetEnumerator();
        int count = 0;
        while (count < ItemsCount && generator.MoveNext())
        {
          int i = generator.Current;
          if (!dictionary.ContainsKey(i))
          {
            dictionary.Add(i, i);
            list.Add(Tuple.Create(descriptor, i, i));
            count++;
          }
        }

        keyExtractor = input => input.GetValueOrDefault<int>(0);
        orderedList = list.OrderBy(input => input.GetValueOrDefault<int>(0), comparer.Implementation).ToList();

        var configuration = new IndexConfiguration<int, Tuple> { KeyExtractor = keyExtractor, KeyComparer = comparer };
        index = new Index<int, Tuple>(configuration);
        AddTest();
        readOnlyIndex = index;
        index = new Index<int, Tuple>(configuration);
      }
    }
  }
}
