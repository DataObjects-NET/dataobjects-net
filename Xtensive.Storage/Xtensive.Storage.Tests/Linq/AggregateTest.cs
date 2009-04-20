// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  [Category("Linq")]
  public class AggregateTest : NorthwindDOModelTest
  {
    [Test]
    public void EntityNotSupportedTest()
    {
      AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Max());
      AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Min());
    }

    [Test]
    public void SumWithNoArgTest()
    {
      var sum = Query<Order>.All.Select(o => o.Freight).Sum();
      Assert.Greater(sum, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var sum = Query<Order>.All.Sum(o => o.Id);
      Assert.Greater(sum, 0);
    }

    [Test]
    public void CountWithNoPredicateTest()
    {
      var count = Query<Order>.All.Count();
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountWithPredicateTest()
    {
      var count = Query<Order>.All.Count(o => o.Id > 10);
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountAfterFilterTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Count() > 10
        select c;
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereCountTest()
    {
      var result = Query<Customer>.All.Where(c => Query<Order>.All.Count(o => o.Customer == c) > 5);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereCountWithPredicateTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Count(o => o.Customer==c) > 10
        select c;
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMaxWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Max(o => o.OrderDate) < new DateTime(1999, 1, 1)
        select c;
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMinWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Min(o => o.Freight) > 5
        select c;
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereAverageWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Average(o => o.Freight) < 5
        select c;
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectCountTest()
    {
      var result =
        from c in Query<Customer>.All
        select new {Customer = c, NumberOfOrders = Query<Order>.All.Count(o => o.Customer==c)};
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectMaxTest()
    {
      var products = Query<Product>.All;
      var suppliers = Query<Supplier>.All;
      var result = from p in products
                   select new { Product = p, MaxID = suppliers.Where(s => s == p.Supplier).Max(s => s.Id) };
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SumCountTest()
    {
      var expected = Query<Order>.All.Count();
      var count = Query<Customer>.All.Sum(c => Query<Order>.All.Count(o => o.Customer == c));
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void SumMinTest()
    {
      var localCustomers = GetEntities<Customer>().ToList().Where(c => c.Orders.Count > 0);
      var localOrders = GetEntities<Order>().ToList();
      var customers = Query<Customer>.All.Where(c => c.Orders.Count > 0);
      var orders = Query<Order>.All;

      var expected = localCustomers.Sum(c => localOrders.Where(o => o.Customer==c).Min(o => o.Freight));
      var result = customers.Sum(c => orders.Where(o => o.Customer==c).Min(o => o.Freight));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void MaxCountTest()
    {
      var localOrders = Query<Order>.All.ToList();
      var localCustomers = Query<Customer>.All.ToList();
      var customers = Query<Customer>.All;
      var orders = Query<Order>.All;

      var expected = localCustomers.Max(c => localOrders.Count(o => o.Customer == c));
      var result = customers.Max(c => orders.Count(o => o.Customer == c));
      Assert.AreEqual(expected, result);
    }
  }
}