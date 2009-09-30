// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public sealed class PrefetcherTest : NorthwindDOModelTest
  {
    [Test]
    public void EnumerableOfNonEntityTest()
    {
      List<Key> keys;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open())
        keys = Query<Order>.All.Select(o => o.Key).ToList();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = keys.Prefetch<Order, Key>(key => key).Prefetch(o => o.OrderDate);
        foreach (var key in prefetcher)
          Assert.IsNotNull(session.EntityStateCache[key, true]);
      }
    }

    [Test]
    public void EnumerableOfEntityTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = Query<Order>.All.Prefetch(o => o.OrderDetails);
        var orderDetailsField = typeof (Order).GetTypeInfo().Fields["OrderDetails"];
        foreach (var order in prefetcher) {
          EntitySetState entitySetState;
          Assert.IsTrue(session.Handler.TryGetEntitySetState(order.Key, orderDetailsField, out entitySetState));
          Assert.IsFalse(entitySetState.IsFullyLoaded);
        }
      }
    }
  }
}