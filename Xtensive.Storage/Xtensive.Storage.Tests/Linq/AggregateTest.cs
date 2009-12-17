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
using Xtensive.Storage.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  [Category("Linq")]
  public class AggregateTest : NorthwindDOModelTest
  {
    [Test]
    [ExpectedException(typeof(TranslationException))]
    public void EntitySetWithGroupingAggregateTest()
    {
      var query =
        Query.All<Customer>()
          .GroupBy(customer => customer.Address.City)
          .Select(grouping => grouping.Max(g => g.Orders));

      QueryDumper.Dump(query);
    }

    [Test]
    public void SingleAggregateTest()
    {
      var result = Query.All<Order>()
        .Select(o => o.OrderDetails.Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void DualAggregateTest()
    {
      var result = Query.All<Order>()
        .Select(o => new {SUM = o.OrderDetails.Count(), SUM2 = o.OrderDetails.Count()});
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntityNotSupportedTest()
    {
      AssertEx.Throws<TranslationException>(() => Query.All<Order>().Max());
      AssertEx.Throws<TranslationException>(() => Query.All<Order>().Min());
    }

    [Test]
    public void IntAverageTest()
    {
      var avg = Query.All<Order>().Average(o => o.Id);
      var expected = Orders.Average(o => o.Id);
      Assert.AreEqual(expected, avg);
    }

    [Test]
    public void SumWithNoArgTest()
    {
      var result = Query.All<Order>().Select(o => o.Freight).Sum();
      var expected = Orders.Select(o => o.Freight).Sum();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var sum = Query.All<Order>().Sum(o => o.Id);
      var expected = Query.All<Order>().ToList().Sum(o => o.Id);
      Assert.AreEqual(expected, sum);
      Assert.Greater(sum, 0);
    }

    [Test]
    public void CountWithNoPredicateTest()
    {
      var count = Query.All<Order>().Count();
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountWithPredicateTest()
    {
      var count = Query.All<Order>().Count(o => o.Id > 10);
      var expected = Query.All<Order>().ToList().Count(o => o.Id > 10);
      Assert.AreEqual(expected, count);
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountAfterFilterTest()
    {
      var result =
        Query.All<Customer>().Where(c => Query.All<Order>()
          .Where(o => o.Customer==c)
          .Count() > 10);
      var list = result.ToList();
      var expected = from c in Customers
      where Orders
        .Where(o => o.Customer==c)
        .Count() > 10
      select c;
      Assert.AreEqual(0, expected.Except(list).Count());
      QueryDumper.Dump(result);
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void WhereCountTest()
    {
      var result = Query.All<Customer>()
        .Where(c => Query.All<Order>().Count(o => o.Customer==c) > 5);
      var expected = Customers
        .Where(c => Orders
          .Count(o => o.Customer==c) > 5);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereCountWithPredicateTest()
    {
      var result =
        from c in Query.All<Customer>()
        where Query.All<Order>().Count(o => o.Customer==c) > 10
        select c;
      var expected =
        from c in Customers
        where Orders
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
        from c in Query.All<Customer>()
        where Query.All<Order>()
          .Where(o => o.Customer==c)
          .Max(o => o.OrderDate) < new DateTime(1999, 1, 1)
        select c;
      var expected =
        from c in Customers
        where Orders
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
        from c in Query.All<Customer>()
        where Query.All<Order>()
          .Where(o => o.Customer==c)
          .Min(o => o.Freight) > 5
        select c;
      var expected =
        from c in Customers
        where Orders
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
        from c in Query.All<Customer>()
        where Query.All<Order>()
          .Where(o => o.Customer==c)
          .Average(o => o.Freight) < 5
        select c;
      var expected =
        from c in Customers
        where Orders
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
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>().Count());
      var expected = Customers
        .Select(c => Orders.Count());
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectAnonymousCountTest()
    {
      var result =
        from c in Query.All<Customer>()
        select new {
          Customer = c,
          NumberOfOrders = Query.All<Order>()
            .Count(o => o.Customer==c)
        };
      var expected =
        from c in Customers
        select new {
          Customer = c,
          NumberOfOrders = Orders
            .Count(o => o.Customer==c)
        };
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectMaxTest()
    {
      var result = from p in Query.All<Product>()
      select new {
        Product = p,
        MaxID = Query.All<Supplier>()
          .Where(s => s==p.Supplier)
          .Max(s => s.Id)
      };
      var expected = from p in Products
      select new {
        Product = p,
        MaxID = Suppliers
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
      var expected = Query.All<Order>().ToList().Count();
      var count = Query.All<Customer>()
        .Sum(c => Query.All<Order>().Count(o => o.Customer==c));
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void SumMinTest()
    {
      var result = Query.All<Customer>()
        .Where(c => c.Orders.Count > 0)
        .Sum(c => Query.All<Order>().Where(o => o.Customer==c).Min(o => o.Freight));
      var expected = Customers
        .Where(c => c.Orders.Count > 0)
        .Sum(c => Orders.Where(o => o.Customer==c).Min(o => o.Freight));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void MaxCountTest()
    {
      var result = Query.All<Customer>()
        .Max(c => Query.All<Order>().Count(o => o.Customer==c));
      var expected = Customers
        .Max(c => Orders.Count(o => o.Customer==c));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void SelectNullableAggregateTest()
    {
      var result = Query.All<Order>()
        .Select(o => (int?) o.Id)
        .Sum();
      var expected = Orders
        .Select(o => (int?) o.Id)
        .Sum();
      Assert.AreEqual(expected, result);
    }
  }
}