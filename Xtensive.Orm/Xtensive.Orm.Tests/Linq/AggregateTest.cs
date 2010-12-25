// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  [Category("Linq")]
  public class AggregateTest : NorthwindDOModelTest
  {
    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void EntitySetWithGroupingAggregateTest()
    {
      var query =
        Session.Query.All<Customer>()
          .GroupBy(customer => customer.Address.City)
          .Select(grouping => grouping.Max(g => g.Orders));

      QueryDumper.Dump(query);
    }

    [Test]
    public void SingleAggregateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Order>()
        .Select(o => o.OrderDetails.Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void DualAggregateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Order>()
        .Select(o => new {SUM = o.OrderDetails.Count(), SUM2 = o.OrderDetails.Count()});
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntityNotSupportedTest()
    {
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Order>().Max());
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Order>().Min());
    }

    [Test]
    public void IntAverageTest()
    {
      var avg = Session.Query.All<Order>().Average(o => o.Id);
      var expected = Orders.Average(o => o.Id);
      Assert.AreEqual(expected, avg);
    }

    [Test]
    public void SumWithNoArgTest()
    {
      var result = Session.Query.All<Order>().Select(o => o.Freight).Sum();
      var expected = Orders.Select(o => o.Freight).Sum();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var sum = Session.Query.All<Order>().Sum(o => o.Id);
      var expected = Session.Query.All<Order>().ToList().Sum(o => o.Id);
      Assert.AreEqual(expected, sum);
      Assert.Greater(sum, 0);
    }

    [Test]
    public void CountWithNoPredicateTest()
    {
      var count = Session.Query.All<Order>().Count();
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountWithPredicateTest()
    {
      var count = Session.Query.All<Order>().Count(o => o.Id > 10);
      var expected = Session.Query.All<Order>().ToList().Count(o => o.Id > 10);
      Assert.AreEqual(expected, count);
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountAfterFilterTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        Session.Query.All<Customer>().Where(c => Session.Query.All<Order>()
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Order>().Count(o => o.Customer==c) > 5);
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Order>().Count(o => o.Customer==c) > 10
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Order>()
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Order>()
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Order>()
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Order>().Count())
        .ToList();
      var expected = Customers
        .Select(c => Orders.Count());
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.Count, 0);
    }

    [Test]
    public void SelectAnonymousCountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          NumberOfOrders = Session.Query.All<Order>()
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = from p in Session.Query.All<Product>()
      select new {
        Product = p,
        MaxID = Session.Query.All<Supplier>()
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var expected = Session.Query.All<Order>().ToList().Count();
      var count = Session.Query.All<Customer>()
        .Sum(c => Session.Query.All<Order>().Count(o => o.Customer==c));
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void SumMinTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Orders.Count > 0)
        .Sum(c => Session.Query.All<Order>().Where(o => o.Customer==c).Min(o => o.Freight));
      var expected = Customers
        .Where(c => c.Orders.Count > 0)
        .Sum(c => Orders.Where(o => o.Customer==c).Min(o => o.Freight));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void MaxCountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Max(c => Session.Query.All<Order>().Count(o => o.Customer==c));
      var expected = Customers
        .Max(c => Orders.Count(o => o.Customer==c));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void SelectNullableAggregateTest()
    {
      var result = Session.Query.All<Order>()
        .Select(o => (int?) o.Id)
        .Sum();
      var expected = Orders
        .Select(o => (int?) o.Id)
        .Sum();
      Assert.AreEqual(expected, result);
    }
  }
}