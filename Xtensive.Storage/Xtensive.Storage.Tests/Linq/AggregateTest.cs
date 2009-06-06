// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
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
    public void DualAggregateTest()
    {
      var result = Query<Order>.All
        .Select(o => new {SUM = o.OrderDetails.Count(), SUM2 = o.OrderDetails.Count()});
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntityNotSupportedTest()
    {
      AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Max());
      AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Min());
    }

    [Test]
    public void IntAverageTest()
    {
      var avg = Query<Order>.All.Average(o => o.Id);
      var expected = Query<Order>.All
        .ToList()
        .Average(o => o.Id);
      Assert.AreEqual(expected, avg);
    }

    [Test]
    public void SumWithNoArgTest()
    {
      var sum = Query<Order>.All.Select(o => o.Freight).Sum();
      var expected = Query<Order>.All.ToList().Select(o => o.Freight).Sum();
      Assert.AreEqual(expected, sum);
      Assert.Greater(sum, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var sum = Query<Order>.All.Sum(o => o.Id);
      var expected = Query<Order>.All.ToList().Sum(o => o.Id);
      Assert.AreEqual(expected, sum);
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
      var expected = Query<Order>.All.ToList().Count(o => o.Id > 10);
      Assert.AreEqual(expected, count);
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountAfterFilterTest()
    {
      var result =
        Query<Customer>.All.Where(c => Query<Order>.All
          .Where(o => o.Customer==c)
          .Count() > 10);
      var expected = from c in Query<Customer>.All.ToList()
      where Query<Order>.All
        .ToList()
        .Where(o => o.Customer==c)
        .Count() > 10
      select c;
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereCountTest()
    {
      var result = Query<Customer>.All
        .Where(c => Query<Order>.All.Count(o => o.Customer==c) > 5);
      var expected = Query<Customer>.All
        .ToList()
        .Where(c => Query<Order>.All
          .ToList()
          .Count(o => o.Customer==c) > 5);
      Assert.AreEqual(0, expected.Except(result).Count());
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
      var expected =
        from c in Query<Customer>.All.ToList()
        where Query<Order>.All
          .ToList()
          .Count(o => o.Customer==c) > 10
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());

      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMaxWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All
          .Where(o => o.Customer==c)
          .Max(o => o.OrderDate) < new DateTime(1999, 1, 1)
        select c;
      var expected =
        from c in Query<Customer>.All.ToList()
        where Query<Order>.All
          .ToList()
          .Where(o => o.Customer==c)
          .Max(o => o.OrderDate) < new DateTime(1999, 1, 1)
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMinWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All
          .Where(o => o.Customer==c)
          .Min(o => o.Freight) > 5
        select c;
      var expected =
        from c in Query<Customer>.All.ToList()
        where Query<Order>.All
          .ToList()
          .Where(o => o.Customer==c)
          .Min(o => o.Freight) > 5
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereAverageWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All
          .Where(o => o.Customer==c)
          .Average(o => o.Freight) < 5
        select c;
      var expected =
        from c in Query<Customer>.All.ToList()
        where Query<Order>.All
          .ToList()
          .Where(o => o.Customer==c)
          .Average(o => o.Freight) < 5
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectCountTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Count());
      var expected = Query<Customer>.All
        .ToList()
        .Select(c => Query<Order>.All.ToList().Count());
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectAnonimousCountTest()
    {
      var result =
        from c in Query<Customer>.All
        select new {
          Customer = c,
          NumberOfOrders = Query<Order>.All
            .Count(o => o.Customer==c)
        };
      var expected =
        from c in Query<Customer>.All.ToList()
        select new {
          Customer = c,
          NumberOfOrders = Query<Order>.All
            .ToList()
            .Count(o => o.Customer==c)
        };
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectMaxTest()
    {
      var result = from p in Query<Product>.All
      select new {
        Product = p,
        MaxID = Query<Supplier>.All
          .Where(s => s==p.Supplier)
          .Max(s => s.Id)
      };
      var expected = from p in Query<Product>.All.ToList()
      select new {
        Product = p,
        MaxID = Query<Supplier>.All
          .ToList()
          .Where(s => s==p.Supplier)
          .Max(s => s.Id)
      };
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SumCountTest()
    {
      var expected = Query<Order>.All.ToList().Count();
      var count = Query<Customer>.All
        .Sum(c => Query<Order>.All.Count(o => o.Customer==c));
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

      var expected = localCustomers.Max(c => localOrders.Count(o => o.Customer==c));
      var result = customers.Max(c => orders.Count(o => o.Customer==c));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void SelectNullableAggregateTest()
    {
      var result = Query<Order>.All
        .Select(o => (int?) o.Id)
        .Sum();
      var expected = Query<Order>.All
        .ToList()
        .Select(o => (int?) o.Id)
        .Sum();
      Assert.AreEqual(expected, result);
    }
  }
}