// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public sealed class FutureTest : NorthwindDOModelTest
  {
    private int searchedFreight = 10;

    [Test]
    public void ExecutionTest()
    {
      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var futureSequenceOrder = Query.ExecuteFuture(
          () => Query.All<Order>().Where(o => o.Freight > 10));
        var futureScalarUnitPrice = Query.ExecuteFutureScalar(
          () => Query.All<Product>().Where(p => p.ProductType == ProductType.Active).Count());
        var futureSequenceProduct = Query.ExecuteFuture(
          () => Query.All<Product>().Where(p => p.ProductName.GreaterThan("c")));
        var futureScalarFreight = Query.ExecuteFutureScalar(
          () => Query.All<Order>().Average(o => o.Freight));
        Assert.Greater(futureSequenceOrder.Count(), 0); // Count() here is IEnumerable.Count()
        Assert.Greater(futureScalarUnitPrice.Value, 0);
        Assert.Greater(futureSequenceProduct.Count(), 0); // Count() here is IEnumerable.Count()
        Assert.Greater(futureScalarFreight.Value, 0);
        ts.Complete();
      }
    }

    [Test]
    public void TransactionChangingTest()
    {
      IEnumerable<Order> futureSequenceOrder;
      using (Session.Open(Domain)) {
        using (var ts = Transaction.Open()) {
          futureSequenceOrder = Query.ExecuteFuture(() => Query.All<Order>().Where(o => o.Freight > 10));
          ts.Complete();
        }
        AssertEx.Throws<InvalidOperationException>(() => futureSequenceOrder.GetEnumerator());
        using (Transaction.Open())
          AssertEx.Throws<InvalidOperationException>(() => futureSequenceOrder.GetEnumerator());
      }
    }

    [Test]
    public void CachingFutureSequenceTest()
    {
      var futureQueryDelegate = (Func<IQueryable<Order>>) GetFutureSequenceQuery;
      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var futureSequenceOrder = Query.ExecuteFuture(futureQueryDelegate);
        Assert.Greater(futureSequenceOrder.Count(), 0);
        ts.Complete();
      }

      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var futureSequenceOrder = Query.ExecuteFuture<Order>(futureQueryDelegate.Method,
          GetFutureSequenceQueryFake);
        Assert.Greater(futureSequenceOrder.Count(), 0);
        ts.Complete();
      }
    }

    [Test]
    public void CachingFutureScalarTest()
    {
      var cacheKey = new object();
      ProductType searchedType = ProductType.Active;
      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var futureScalarUnitPrice = Query.ExecuteFutureScalar(cacheKey,
          () => Query.All<Product>().Where(p => p.ProductType == searchedType).Count());
        Assert.Greater(futureScalarUnitPrice.Value, 0);
        ts.Complete();
      }

      var t = 0;
      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var futureScalarUnitPrice = Query.ExecuteFutureScalar(cacheKey, () => t);
        Assert.Greater(futureScalarUnitPrice.Value, 0);
        ts.Complete();
      }
    }

    private IQueryable<Order> GetFutureSequenceQuery()
    {
      return Query.All<Order>().Where(o => o.Freight > searchedFreight);
    }

    private IQueryable<Order> GetFutureSequenceQueryFake()
    {
      return null;
    }
  }
}