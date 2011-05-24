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
using Xtensive.Helpers;
using Xtensive.Testing;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public sealed class FutureTest : NorthwindDOModelTest
  {
    private int searchedFreight = 10;

    [Test]
    public void ExecutionTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureSequenceOrder = session.Query.ExecuteDelayed(
          qe => qe.All<Order>().Where(o => o.Freight > 10));
        var futureScalarUnitPrice = session.Query.ExecuteDelayed(
          qe => qe.All<Product>().Where(p => p.ProductType == ProductType.Active).Count());
        var futureSequenceProduct = session.Query.ExecuteDelayed(
          qe => qe.All<Product>().Where(p => p.ProductName.Contains("c")));
        var futureScalarFreight = session.Query.ExecuteDelayed(
          qe => qe.All<Order>().Average(o => o.Freight));
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
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {
          futureSequenceOrder = session.Query.ExecuteDelayed(qe => qe.All<Order>().Where(o => o.Freight > 10));
          ts.Complete();
        }
        AssertEx.Throws<InvalidOperationException>(() => futureSequenceOrder.GetEnumerator());
        using (session.OpenTransaction())
          AssertEx.Throws<InvalidOperationException>(() => futureSequenceOrder.GetEnumerator());
      }
    }

    [Test]
    public void CachingFutureSequenceTest()
    {
      Func<QueryEndpoint,IQueryable<Order>> futureQueryDelegate = GetFutureSequenceQuery;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureSequenceOrder = session.Query.ExecuteDelayed(futureQueryDelegate);
        Assert.Greater(futureSequenceOrder.Count(), 0);
        ts.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureSequenceOrder = session.Query.ExecuteDelayed(futureQueryDelegate.Method, GetFutureSequenceQueryFake);
        Assert.Greater(futureSequenceOrder.Count(), 0);
        ts.Complete();
      }
    }

    [Test]
    public void CachingFutureScalarTest()
    {
      var cacheKey = new object();
      ProductType searchedType = ProductType.Active;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureScalarUnitPrice = session.Query.ExecuteDelayed(cacheKey,
          qe => qe.All<Product>().Where(p => p.ProductType == searchedType).Count());
        Assert.Greater(futureScalarUnitPrice.Value, 0);
        ts.Complete();
      }

      var t = 0;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var futureScalarUnitPrice = session.Query.ExecuteDelayed(cacheKey, qe => t);
        Assert.Greater(futureScalarUnitPrice.Value, 0);
        ts.Complete();
      }
    }

    private IQueryable<Order> GetFutureSequenceQuery(QueryEndpoint queryEndpoint)
    {
      return Session.Demand().Query.All<Order>().Where(o => o.Freight > searchedFreight);
    }

    private IQueryable<Order> GetFutureSequenceQueryFake(QueryEndpoint queryEndpoint)
    {
      return null;
    }
  }
}