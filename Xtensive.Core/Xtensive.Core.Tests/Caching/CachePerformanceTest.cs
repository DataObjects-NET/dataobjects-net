// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.27

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core.Caching;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tests.Caching
{
  [TestFixture]
  public class CachePerformanceTest
  {
    #region Nested type: Item

    [DebuggerDisplay("{Value}")]
    public class Item : IEquatable<Item>, IIdentified<Item>, IHasSize
    {
      public string Value { get; private set; }

      object IIdentified.Identifier
      {
        get { return Identifier; }
      }

      public Item Identifier
      {
        get { return this; }
      }

      public long Size
      {
        get { return 1; }
      }

      public bool Equals(Item obj)
      {
        if (obj==null)
          return false;
        return obj.Value==Value;
      }

      public override bool Equals(object obj)
      {
        return Equals(obj as Item);
      }

      public override int GetHashCode()
      {
        return (Value!=null ? Value.GetHashCode() : 0);
      }

      public override string ToString()
      {
        return string.Format("{0}", Value);
      }

      public Item(string value)
      {
        Value = value;
      }
    }

    #endregion

    public const int LruCapacity = 90000;
    public const int MfuCapacity = 10000;
    public const int Capacity = LruCapacity + MfuCapacity;
    
    public const int BaseCount = 10000000;
    public const int InsertCount = (int) (1.1 * Capacity);

    private static LruCache<Item, Item, Item> lruCache = 
      new LruCache<Item, Item, Item>(Capacity, i => i);
    private static LruCache<Item, Item> lruCache2 = 
      new LruCache<Item, Item>(Capacity, i => i);
    private static MfLruCache<Item, Item> mfLruCache = 
      new MfLruCache<Item, Item>(LruCapacity, MfuCapacity, 5, i => i);
    private static WeakCache<Item, Item> weakCache = 
      new WeakCache<Item, Item>(false, Capacity, i => i);
    private static WeakestCache<Item, Item> weakestCache = 
      new WeakestCache<Item, Item>(false, false, Capacity, i => i);
    private static LruCache<Item, Item> lruWeakestCache = 
      new LruCache<Item, Item>(LruCapacity + MfuCapacity, i => i, 
        new WeakestCache<Item, Item>(false, false, i => i));
    private static ICache<Item, Item>[] caches = new ICache<Item, Item>[] {lruCache, lruCache2, mfLruCache, weakCache, weakestCache, lruWeakestCache};

    private bool warmup = false;

    [Test]
    public void RegularTest()
    {
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
      CombinedTest(BaseCount / 10, InsertCount / 10);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
      CombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      caches = new ICache<Item, Item>[] {lruWeakestCache};
      int insertCount = InsertCount*10;
      InsertTest(insertCount);
      FetchTest(BaseCount);
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      InsertTest(insertCount);
      FetchTest(baseCount);
    }

    private void InsertTest(int count)
    {
      foreach (var cache in caches) {
        TestHelper.CollectGarbage();
        string title = string.Format("Insert ({0})", cache.GetType().GetShortName());
        using (warmup ? null : new Measurement(title, count)) {
          for (int i = 0; i < count; i++) {
            var key = i % InsertCount;
            var item = new Item(key.ToString());
            cache.Add(item);
          }
        }
      }
    }

    private void FetchTest(int count)
    {
      foreach (var cache in caches) {
        TestHelper.CollectGarbage();
        var r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        string title = string.Format("Fetch ({0})", cache.GetType().GetShortName());
        using (warmup ? null : new Measurement(title, count)) {
          int j = 0;
          foreach (var i in InstanceGenerationUtils<int>.GetInstances(r, 0.9)) {
            var o = cache[new Item((i % InsertCount).ToString()), true];
            if (++j > count)
              break;
          }
        }
      }
    }
  }
}