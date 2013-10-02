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
using Xtensive.Diagnostics;
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

    private volatile Item item1;

    [Test]
    public void CombinedTest()
    {
      var cache = new WeakestCache<Item, Item>(false, false, i => i);

      item1 = new Item("1");
      cache.Add(item1);

      Assert.AreSame(item1, cache.First());
      Assert.AreSame(item1, cache[new Item("1"), true]);

      cache.Remove(item1);
      Assert.AreEqual(0, cache.Count);

      cache.Add(item1);
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.AreEqual(1, cache.Count);

      item1 = null;
      TestHelper.CollectGarbage(true);
      cache.CollectGarbage();
      Assert.AreEqual(0, cache.Count);

      Assert.IsNull(cache[new Item("1"), true]);
    }

    [Test]
    public void ProfileTest()
    {
      var cache = new WeakestCache<Item, Item>(false, false, i => i);
      var measurement = new Measurement();
      for (int i = 0,j = 0; i < 1000000; i++, j++) {
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