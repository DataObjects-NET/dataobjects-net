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
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Caching
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

      Assert.That(cache.KeyExtractor, Is.Not.Null);

      var item = new TestItem("100");
      cache.Add(item);

      Assert.That(cache.Count, Is.EqualTo(1));
    }

    [Test]
    public void ConstructorDenyTest()
    {
      Assert.Throws<ArgumentNullException>(() => { var cache = new InfiniteCache<string, TestClass>(null); });
    }

    [Test]
    public void ConstructorDenyTest2()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => { var cache = new InfiniteCache<string, TestClass>(-1, i => i.Text); });
    }

    [Test]
    public void AddRemoveTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);

      TestItem item = new TestItem("1");
      cache.Add(item);
      Assert.That(cache.Count, Is.EqualTo(1));

      item = new TestItem("2");
      cache.Add(item);
      Assert.That(cache.Count, Is.EqualTo(2));
      Assert.That(cache[item.Key, false], Is.EqualTo(item));

      ICache<string, TestItem> icache = cache;
      Assert.That(icache[item.Value, false], Is.EqualTo(item));
      Assert.That(icache["3", false], Is.EqualTo(null));

      item = new TestItem("2", "3");
      cache.Add(item, false);
      Assert.That(cache["2", false].Value, Is.EqualTo("2"));
      cache.Add(item, true);
      Assert.That(cache["2", false].Value, Is.EqualTo("3"));

      cache.Remove(item);
      Assert.That(cache.Count, Is.EqualTo(1));

      cache.Clear();
      Assert.That(cache.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddDenyTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);
      Assert.Throws<ArgumentNullException>(() => cache.Add(null));
    }

    [Test]
    public void RemoveDenyTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);
      Assert.Throws<ArgumentNullException>(() => cache.Remove((TestItem)null));
    }

    [Test]
    public void RemoveDenyTest1()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);
      Assert.Throws<ArgumentNullException>(() => cache.Remove((TestItem)null));
    }

    [Test]
    public void IEnumerableTest()
    {
      var cache = new InfiniteCache<string, TestItem>(value => value.Key);

      for (int i = 0; i < 100; i++)
        cache.Add(new TestItem("item " + i));

      Assert.That(100, Is.EqualTo(cache.Count));

      int itemsCount = 0;
      foreach (TestItem testClass in cache)
      {
        Assert.That(testClass != null, Is.True);
        itemsCount++;
      }
      Assert.That(100, Is.EqualTo(itemsCount));
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
      Assert.That(globalCache.Count >= 0, Is.True);

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
