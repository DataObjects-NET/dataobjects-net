// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.11

using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Testing;

namespace Xtensive.Tests.Caching
{
  internal class TestItem :
    IIdentified<string>,
    IHasSize
  {
    public string Key { get; set; }

    public string Value { get; set; }

    object IIdentified.Identifier
    {
      get { return Identifier; }
    }

    public string Identifier
    {
      get { return Value; }
    }

    public long Size
    {
      get { return Value.Length; }
    }

    public TestItem(string value)
    {
      Key = value;
      Value = value;
    }

    public TestItem(string key, string value)
    {
      Key = key;
      Value = value;
    }
  }


  [TestFixture]
  public class InfiniteCacheTest
  {
    private InfiniteCache<string, TestItem> globalCache;
    private Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);

    [Test]
    public void ConstructorsTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);

      Assert.IsNotNull(cache.KeyExtractor);

      var item = new TestItem("100");
      cache.Add(item);

      Assert.AreEqual(1, cache.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorDenyTest()
    {
      var cache = new InfiniteCache<string, TestClass>(null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorDenyTest2()
    {
      var cache = new InfiniteCache<string, TestClass>(-1, i=> i.Text);
    }

    [Test]
    public void AddRemoveTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);

      TestItem item = new TestItem("1");
      cache.Add(item);
      Assert.AreEqual(1, cache.Count);

      item = new TestItem("2");
      cache.Add(item);
      Assert.AreEqual(2, cache.Count);
      Assert.AreEqual(item, cache[item.Key, false]);

      ICache<string, TestItem> icache = cache;
      Assert.AreEqual(item, icache[item.Value, false]);
      Assert.AreEqual(null, icache["3", false]);

      item = new TestItem("2", "3");
      cache.Add(item, false);
      Assert.AreEqual("2", cache["2", false].Value);
      cache.Add(item, true);
      Assert.AreEqual("3", cache["2", false].Value);

      cache.Remove(item);
      Assert.AreEqual(1, cache.Count);

      cache.Clear();
      Assert.AreEqual(0, cache.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddDenyTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);
      cache.Add(null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveDenyTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);
      cache.Remove((TestItem)null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveDenyTest1()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);
      cache.Remove((TestItem)null);
    }

    [Test]
    public void IEnumerableTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);

      for (int i = 0; i < 100; i++)
        cache.Add(new TestItem("item " + i));

      Assert.AreEqual(cache.Count, 100);

      int itemsCount = 0;
      foreach (TestItem testClass in cache)
      {
        Assert.IsTrue(testClass != null);
        itemsCount++;
      }
      Assert.AreEqual(itemsCount, 100);
    }

    private static bool canFinish = true;

    [Test]
    public void SynchronizationTest()
    {
      globalCache = new InfiniteCache<string, TestItem>(value => value.Key);

      var addThreads = new Thread[10];
      var removeThreads = new Thread[10];
      for (int i = 0; i < 10; i++)
      {
        addThreads[i] = new Thread(AddItem);
        removeThreads[i] = new Thread(RemoveItem);
      }

      try
      {
        for (int i = 0; i < 10; i++)
          addThreads[i].Start();
        Thread.Sleep(10);
        for (int i = 0; i < 10; i++)
          removeThreads[i].Start();
        Thread.Sleep(200);
      }
      finally
      {
        canFinish = true;
        for (int i = 0; i < 10; i++)
        {
          addThreads[i].Join();
          removeThreads[i].Join();
        }
      }
      Assert.IsTrue(globalCache.Count >= 0);

      globalCache = null;
    }

    private void AddItem()
    {
      int count = random.Next(100000);
      while (!canFinish)
      {
        lock (globalCache)
        {
          globalCache.Add(new TestItem("Item " + count));
        }
        count++;
      }
    }

    private void RemoveItem()
    {
      while (!canFinish)
      {
        TestItem test = null;
        lock (globalCache)
        {
          foreach (TestItem testClass in globalCache)
          {
            test = testClass;
            break;
          }
        }
        if (test != null) lock (globalCache)
          {
            globalCache.Remove(test);
          }
      }
    }
  }

}
