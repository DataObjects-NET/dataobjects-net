// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.24

using System;
using NUnit.Framework;
using Xtensive.Collections;

namespace Xtensive.Tests.Collections
{
  [TestFixture]
  public class PoolTest
  {
    [Test]
    public void CombinedTest()
    {
      Pool<object> p = new Pool<object>();
      object o1 = 1;
      object o2 = 2;
      object o3 = 3;
      Assert.AreEqual(p.Count, 0);
      Assert.AreEqual(p.AvailableCount, 0);
      Assert.IsFalse(p.IsPooled(o1));
      Assert.IsFalse(p.IsConsumed(o1));
      
      p.ExecuteConsumer(delegate
        {
          Assert.AreEqual(p.Count, 0);
          Assert.AreEqual(p.AvailableCount, 0);
          return o1;
        },
        delegate(object item)
        {
          Assert.AreEqual(p.Count, 1);
          Assert.AreEqual(p.AvailableCount, 0);
          Assert.IsTrue(p.IsPooled(item));
          Assert.IsTrue(p.IsConsumed(item));
        });
      Assert.AreEqual(p.Count, 1);
      Assert.AreEqual(p.AvailableCount, 1);
      Assert.IsTrue(p.IsPooled(o1));
      Assert.IsFalse(p.IsConsumed(o1));

      Assert.AreEqual(p.Count, 1);
      Assert.AreEqual(p.AvailableCount, 1);
      object co1 = p.Consume();
      Assert.AreEqual(p.Count, 1);
      Assert.AreEqual(p.AvailableCount, 0);
      Assert.AreSame(co1,o1);

      p.ExecuteConsumer(delegate
        {
          Assert.AreEqual(p.Count, 1);
          Assert.AreEqual(p.AvailableCount, 0);
          return o2;
        }, 
        delegate {
          Assert.AreEqual(p.Count, 2);
          Assert.AreEqual(p.AvailableCount, 0);
          Assert.IsTrue(p.IsPooled(o1));
          Assert.IsTrue(p.IsConsumed(o1));
          Assert.IsTrue(p.IsPooled(o2));
          Assert.IsTrue(p.IsConsumed(o2));
        });
      Assert.IsTrue(p.IsPooled(o1));
      Assert.IsTrue(p.IsConsumed(o1));
      Assert.IsTrue(p.IsPooled(o2));
      Assert.IsFalse(p.IsConsumed(o2));

      p.Release(co1);
      Assert.AreEqual(p.Count, 2);
      Assert.AreEqual(p.AvailableCount, 2);
      Assert.IsTrue(p.IsPooled(o1));
      Assert.IsTrue(p.IsPooled(o2));
      Assert.IsFalse(p.IsConsumed(o1));
      Assert.IsFalse(p.IsConsumed(o2));

      co1 = p.Consume(delegate { return o3;});
      Assert.AreEqual(p.Count, 2);
      Assert.AreEqual(p.AvailableCount, 1);
      object co2 = p.Consume(delegate { return o3; });
      Assert.AreEqual(p.Count, 2);
      Assert.AreEqual(p.AvailableCount, 0);
      object co3 = p.Consume(delegate { return o3; });
      Assert.AreEqual(p.Count, 3);
      Assert.AreEqual(p.AvailableCount, 0);

      Assert.IsTrue((co1==o1 && co2==o2) || (co1==o2 && co2==o1));
      Assert.AreSame(co3, o3);

      p.Release(o1);
      Assert.AreEqual(p.Count, 3);
      Assert.AreEqual(p.AvailableCount, 1);
      p.Release(o2);
      Assert.AreEqual(p.Count, 3);
      Assert.AreEqual(p.AvailableCount, 2);
      p.Release(o3);
      Assert.AreEqual(p.Count, 3);
      Assert.AreEqual(p.AvailableCount, 3);

      Assert.IsTrue(p.IsPooled(o1));
      Assert.IsTrue(p.IsPooled(o2));
      Assert.IsTrue(p.IsPooled(o3));
      Assert.IsFalse(p.IsConsumed(o1));
      Assert.IsFalse(p.IsConsumed(o2));
      Assert.IsFalse(p.IsConsumed(o3));
    }
  }
}