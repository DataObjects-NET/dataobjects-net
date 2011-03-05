// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.15

using System.Threading;
using NUnit.Framework;
using Xtensive.Collections;
using System;
using Xtensive.Comparison;

namespace Xtensive.Tests.Collections
{
  [TestFixture]
  public class PoolTKeyTValueTest
  {

    [Test]
    public void Consume()
    {
      Pool<int, string> pool = new Pool<int, string>(2000, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(-10));
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          pool.Consume(i, string.Format("item{0}_{1}", i, j));
        }
      }
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConsumeDuplicates()
    {
      Pool<int, string> pool = new Pool<int, string>(2000, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(-10));
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          pool.Consume(i, string.Format("item{0}", j));
        }
      }
    }

    [Test]
    public void ConsumeAndAdd()
    {
      Pool<int, string> pool = new Pool<int, string>(2000, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(-10));
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          Assert.AreEqual(0, pool.AvailableCount);
          pool.Add(i, string.Format("item{0}_{1}", i, j));
          Assert.AreEqual(1, pool.AvailableCount);
          pool.Consume(i, string.Format("item{0}_{1}", i, j));
        }
      }
      Assert.AreEqual(100, pool.Count);
      Assert.AreEqual(0, pool.AvailableCount);
    }

    [Test]
    public void IsPooledIsConsumedIsAvailable()
    {
      Pool<int, string> pool = new Pool<int, string>(2000, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(-10));
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          string item = string.Format("item{0}_{1}", i, j);
          Assert.AreEqual(false, pool.IsPooled(item));
          Assert.AreEqual(false, pool.IsConsumed(item));
          Assert.AreEqual(false, pool.IsAvailable(item));

          pool.Add(i, item);
          Assert.AreEqual(true, pool.IsPooled(item));
          Assert.AreEqual(false, pool.IsConsumed(item));
          Assert.AreEqual(true, pool.IsAvailable(item));

          pool.Consume(i, item);
          Assert.AreEqual(true, pool.IsPooled(item));
          Assert.AreEqual(true, pool.IsConsumed(item));
          Assert.AreEqual(false, pool.IsAvailable(item));

          pool.Release(item);
          Assert.AreEqual(true, pool.IsPooled(item));
          Assert.AreEqual(false, pool.IsConsumed(item));
          Assert.AreEqual(true, pool.IsAvailable(item));

          pool.Remove(item);
          Assert.AreEqual(false, pool.IsPooled(item));
          Assert.AreEqual(false, pool.IsConsumed(item));
          Assert.AreEqual(false, pool.IsAvailable(item));
        }
      }
    }

    [Test]
    public void ItemExpiration()
    {
      Pool<int, string> pool = new Pool<int, string>(2000, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
      pool.ItemExpires += PoolItemExpiresHandler;
      pool.ItemRemoved += PoolItemRemvoedHandler;
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          string item = string.Format("item{0}_{1}", i, j);

          pool.Add(i, item);
          Assert.AreEqual(true, pool.IsPooled(item));
          Assert.AreEqual(false, pool.IsConsumed(item));
          Assert.AreEqual(true, pool.IsAvailable(item));

        }
      }
      Thread.Sleep(TimeSpan.FromSeconds(4));
      Assert.AreEqual(100, expiresCount);
      Assert.AreEqual(100, expiredCount);

    }

    private int expiresCount = 0;
    private int expiredCount = 0;

    void PoolItemExpiresHandler(object sender, ItemExpiresEventArgs<string> e)
    {
      // e.Cancel = true;
      Interlocked.Increment(ref expiresCount);
    }

    void PoolItemRemvoedHandler(object sender, ItemRemovedEventArgs<string> e)
    {
      Interlocked.Increment(ref expiredCount);
    }
  }


}