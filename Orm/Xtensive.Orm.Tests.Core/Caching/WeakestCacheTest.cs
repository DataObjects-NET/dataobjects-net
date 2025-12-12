// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.29

using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Caching;
using System.Linq;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Caching
{
  [TestFixture]
  public class WeakestCacheTest
  {
    #region Nested type: Item

    [DebuggerDisplay("{Value}")]
    public class Item : IEquatable<Item>
    {
      public string Value { get; private set; }

      public bool Equals(Item obj)
      {
        if (obj == null)
          return false;
        return obj.Value == Value;
      }

      public override bool Equals(object obj)
      {
        return Equals(obj as Item);
      }

      public override int GetHashCode()
      {
        return (Value != null ? Value.GetHashCode() : 0);
      }

      public override string ToString()
      {
        return $"{Value}";
      }

      public Item(string value)
      {
        Value = value;
      }
    }

    #endregion

    private volatile Item fieldItem1;
    private volatile Item fieldItem2;

    [Test]
    public void ClassFieldInTestScopeTest()
    {
      var cache = new WeakestCache<Item, Item>(false, false, i => i);

      fieldItem1 = new Item("1");

      cache.Add(fieldItem1);

      Assert.That(cache.First(), Is.SameAs(fieldItem1));
      Assert.That(cache[new Item("1"), true], Is.SameAs(fieldItem1));

      cache.Remove(fieldItem1);
      Assert.That(cache.Count, Is.EqualTo(0));

      cache.Add(fieldItem1);
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(1));

      fieldItem1 = null;
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(1));

      Assert.That(cache[new Item("1"), true], Is.Not.Null);
    }

    [Test]
    public void LocalVarInTestScopeTest()
    {
      var cache = new WeakestCache<Item, Item>(false, false, i => i);

      var item1 = new Item("1");

      cache.Add(item1);

      Assert.That(cache.First(), Is.SameAs(item1));
      Assert.That(cache[new Item("1"), true], Is.SameAs(item1));

      cache.Remove(item1);
      Assert.That(cache.Count, Is.EqualTo(0));

      cache.Add(item1);
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(1));

      item1 = null;
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(1));

      Assert.That(cache[new Item("1"), true], Is.Not.Null);
    }

    [Test]
    public void LocalVarInCalledMethodScopeTest()
    {
      var cache = new WeakestCache<Item, Item>(false, false, i => i);

      InnerLocalVariableCacheTest(cache);

      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(0));

      Assert.That(cache[new Item("1"), true], Is.Null);
    }

    [Test]
    public void ClassFieldInCalledMethodScopeTest()
    {
      var cache = new WeakestCache<Item, Item>(false, false, i => i);

      InnerClassFieldCacheTest(cache);

      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(0));

      Assert.That(cache[new Item("1"), true], Is.Null);
    }

    private void InnerLocalVariableCacheTest(WeakestCache<Item, Item> cache)
    {
      var item = new Item("1");

      cache.Add(item);

      Assert.That(cache.First(), Is.SameAs(item));
      Assert.That(cache[new Item("1"), true], Is.SameAs(item));

      cache.Remove(item);
      Assert.That(cache.Count, Is.EqualTo(0));

      cache.Add(item);
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(1));

      item = null;
    }

    private void InnerClassFieldCacheTest(WeakestCache<Item, Item> cache)
    {
      fieldItem2 = new Item("1");

      cache.Add(fieldItem2);

      Assert.That(cache.First(), Is.SameAs(fieldItem2));
      Assert.That(cache[new Item("1"), true], Is.SameAs(fieldItem2));

      cache.Remove(fieldItem2);
      Assert.That(cache.Count, Is.EqualTo(0));

      cache.Add(fieldItem2);
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.That(cache.Count, Is.EqualTo(1));

      fieldItem2 = null;
    }

    [Test]
    public void ProfileTest()
    {
      var cache = new WeakestCache<Item, Item>(false, false, i => i);
      var measurement = new Measurement();
      for (int i = 0, j = 0; i < 1000000; i++, j++) {
        var item = new Item(i.ToString());
        cache.Add(item);
        if (j == 100000) {
          j = 0;
          Console.Out.WriteLine(measurement.ToString());
        }
      }
    }
  }
}