// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.01.10

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Indexing;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Tests;

namespace Xtensive.Indexing.Tests.Index
{
  [TestFixture]
  public class IndexTest
  {
    private const int maxCount = 100;
    private Random random = RandomManager.CreateRandom();

    [Test]
    public void Add()
    {
      AddInternal<short>(GetCount());
      AddInternal<int>(GetCount());
      AddInternal<long>(GetCount());
      AddInternal<uint>(GetCount());
      AddInternal<ulong>(GetCount());
      AddInternal<byte>(GetCount());
      AddInternal<string>(GetCount());
      AddInternal<Guid>(GetCount());
    }

    private void AddInternal<T>(int count)
    {
      Index<T, T> index = GetIndex<T>();
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);
      Xtensive.Collections.ISet<T> missingInstances = GetUniqueInstances<T>(count);
      instances.ExceptWith(missingInstances);
      foreach (T instance in instances)
        index.Add(instance);
      int j = 0;
      foreach (T i in instances) {
        Assert.IsTrue(index.ContainsKey(index.KeyExtractor(i)), j++.ToString());
        Assert.IsTrue(index.Contains(i));
      }
      foreach (T i in missingInstances) {
        Assert.IsFalse(index.ContainsKey(index.KeyExtractor(i)));
        Assert.IsFalse(index.Contains(i));
      }
    }

    [Test]
    public void Remove()
    {
      RemoveInternal<short>(GetCount());
      RemoveInternal<long>(GetCount());
      RemoveInternal<int>(GetCount());
      RemoveInternal<string>(GetCount());
      RemoveInternal<Guid>(GetCount());
      RemoveInternal<byte>(GetCount());
    }

    private void RemoveInternal<T>(int count)
    {
      Index<T, T> index = GetIndex<T>();
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);
      Xtensive.Collections.ISet<T> missingInstances = GetUniqueInstances<T>(count);
      instances.ExceptWith(missingInstances);
      foreach (T instance in instances)
        index.Add(instance);

      foreach (T i in instances) {
        Assert.IsTrue(index.Contains(i));
        bool success = index.Remove(i);
        Assert.IsTrue(success);
        Assert.IsFalse(index.Contains(i));
      }

      foreach (T i in instances) {
        Assert.IsFalse(index.Contains(i));
        Assert.IsFalse(index.Remove(i));
      }
      Assert.IsNotNull(index);
      Assert.AreEqual(0, index.Count);

      foreach (T instance in instances)
        index.Add(instance);

      int counter = 0;
      foreach (var i in index) {
        counter++;
        Assert.IsTrue(index.Contains(i));
        bool success = index.Remove(i);
        Assert.IsTrue(success);
        Assert.IsFalse(index.Contains(i));
      }
      Assert.AreEqual(0, index.Count);
      Assert.AreEqual(instances.Count, counter);

      foreach (T instance in instances)
        index.Add(instance);

      counter = 0;
      var removalQueue = new HashSet<T>();
      foreach (var i in index) {
        counter++;
        foreach (var toRemove in removalQueue) {
          Assert.IsTrue(index.Contains(toRemove));
          bool success = index.Remove(toRemove);
          Assert.IsTrue(success);
        } 
        removalQueue.Clear();
        removalQueue.Add(i);
        Assert.IsTrue (index.Contains(i));
      }
      Assert.AreEqual(1, index.Count);
      Assert.AreEqual(instances.Count, counter);

    }

    [Test]
    public void RemoveDebug()
    {
      RemoveDebugInternal<short>(GetCount());
      RemoveDebugInternal<string>(GetCount());
      RemoveDebugInternal<Guid>(GetCount());
      RemoveDebugInternal<long>(GetCount());
      RemoveDebugInternal<int>(GetCount());
    }

    private void RemoveDebugInternal<T>(int count)
    {
      Index<T, T> index = GetIndex<T>();
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);
      foreach (T instance in instances)
        index.Add(instance);

      foreach (T i in instances) {
        Assert.IsTrue(index.Contains(i));
        Assert.IsTrue(index.Remove(i));
        Assert.IsFalse(index.Contains(i));
      }
      Assert.IsNotNull(index);
    }

    [Test]
    public void BehaviorTest()
    {
      BehaviourTestInternal<short>(GetCount());
      BehaviourTestInternal<string>(GetCount());
      BehaviourTestInternal<Guid>(GetCount());
      BehaviourTestInternal<long>(GetCount());
      BehaviourTestInternal<byte>(GetCount());
    }

    private void BehaviourTestInternal<T>(int count)
    {
      Tests.IndexTest.TestIndex(GetIndex<T>(), new Tests.IndexTest.Configuration(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), count));
    }

    [Test]
    public void AddWithMeasures()
    {
      AddWithMeasuresInternal<short>(GetCount());
      AddWithMeasuresInternal<long>(GetCount());
      AddWithMeasuresInternal<Guid>(GetCount());
    }

    private void AddWithMeasuresInternal<T>(int count)
    {
      IndexConfiguration<T, T> testConfiguration = GetConfiguration<T>();
      testConfiguration.PageSize = 2;
      Converter<T, T> converter = AdvancedConverter<T, T>.Default.Implementation.Convert;
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      testConfiguration.Measures.Add(new MinMeasure<T, T>("Min", converter));
      testConfiguration.Measures.Add(new MaxMeasure<T, T>("Max", converter));
      Index<T, T> index = new Index<T, T>(testConfiguration);
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);
      Xtensive.Collections.ISet<T> missingInstances = GetUniqueInstances<T>(count);
      instances.ExceptWith(missingInstances);
      T minValue = AdvancedComparer<T>.Default.ValueRangeInfo.MaxValue;
      T maxValue = AdvancedComparer<T>.Default.ValueRangeInfo.MinValue;
      int insertedCount = 0;
      foreach (T instance in instances) {
        index.Add(instance);
        if (comparer.Compare(instance, maxValue) > 0)
          maxValue = instance;
        if (comparer.Compare(instance, minValue) < 0)
          minValue = instance;

        Assert.AreEqual(++insertedCount, index.Count);
        Assert.AreEqual(minValue, index.GetMeasureResult("Min"));
        Assert.AreEqual(maxValue, index.GetMeasureResult("Max"));
      }
      Assert.AreEqual(instances.Count, index.Count);
      Assert.AreEqual(minValue, index.GetMeasureResult("Min"));
      Assert.AreEqual(maxValue, index.GetMeasureResult("Max"));
      int j = 0;
      foreach (T i in instances) {
        Assert.IsTrue(index.ContainsKey(index.KeyExtractor(i)), j++.ToString());
        Assert.IsTrue(index.Contains(i));
      }
      foreach (T i in missingInstances) {
        Assert.IsFalse(index.ContainsKey(index.KeyExtractor(i)));
        Assert.IsFalse(index.Contains(i));
      }
    }

    private SetSlim<T> GetUniqueInstances<T>(int count)
    {
      return new SetSlim<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, count));
    }

    private IndexConfiguration<T, T> GetConfiguration<T>()
    {
      return new IndexConfiguration<T, T>(AdvancedConverter<T, T>.Default.Implementation.Convert, AdvancedComparer<T>.Default) {
        PageSize = 4
      };
    }

    private int GetCount()
    {
      return random.Next() % maxCount + 1;
    }

    private Index<T, T> GetIndex<T>()
    {
      return new Index<T, T>(GetConfiguration<T>());
    }
  }
}