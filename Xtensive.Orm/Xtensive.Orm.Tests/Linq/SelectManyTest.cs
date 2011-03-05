// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SelectManyTest : NorthwindDOModelTest
  {
    [Test]
    public void GroupJoinTest()
    {
      var result = Session.Query.All<Order>()
        .GroupJoin(Session.Query.All<Customer>(),
          o => o.Customer,
          c => c,
          (o, oc) => new {o, oc})
        .SelectMany(@t => @t.oc.DefaultIfEmpty(),
          (@t, x) => new {
            CustomerId = x.Id,
            CompanyName = x.CompanyName,
            Country = x.Address.Country
          })
          ;
      var expected = Session.Query.All<Order>().AsEnumerable()
        .GroupJoin(Session.Query.All<Customer>().AsEnumerable(),
          o => o.Customer,
          c => c,
          (o, oc) => new {o, oc})
        .SelectMany(@t => @t.oc.DefaultIfEmpty(),
          (@t, x) => new {
            CustomerId = x.Id,
            CompanyName = x.CompanyName,
            Country = x.Address.Country
          }).OrderBy(t => t.CompanyName).ThenBy(t => t.Country).ThenBy(t => t.CustomerId)
          ;
      Assert.IsTrue(expected.SequenceEqual(
        result.ToList().OrderBy(t => t.CompanyName).ThenBy(t => t.Country).ThenBy(t => t.CustomerId)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByTest()
    {
      var result = Session.Query.All<Order>()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g);
      var list = result.ToList();
      var expected = Session.Query.All<Order>().ToList();
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void GroupBy2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Order>()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g, (grouping, order)=>new {Count = grouping.Count(), order});
      var expected = Session.Query.All<Order>()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g, (grouping, order)=>new {Count = grouping.Count(), order});
      Assert.IsTrue(expected.Except(result).IsNullOrEmpty());
    }

    [Test]
    public void GroupBySelectorTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var result = Session.Query.All<Order>()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g.Select(o => o.Customer));
      var list = result.ToList();
      var expected = Session.Query.All<Order>().Select(o => o.Customer).ToList();
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void GroupByCountTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var result = Session.Query.All<Order>()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g.Select(o => o.Customer).Where(c => g.Count() > 2));
      var list = result.ToList();
      var expected = Session.Query.All<Order>().ToList()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g.Select(o => o.Customer).Where(c => g.Count() > 2))
        .ToList();
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void GroupByCount2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var result = Session.Query.All<Order>()
        .GroupBy(o => o.Customer)
        .Where(g => g.Count() > 2)
        .SelectMany(g => g.Select(o => o.Customer));
      var list = result.ToList();
      var expected = Session.Query.All<Order>().ToList()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g.Select(o => o.Customer).Where(c => g.Count() > 2))
        .ToList();
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void ParameterTest()
    {
      var expectedCount = Session.Query.All<Order>().Count();
      var result = Session.Query.All<Customer>()
        .SelectMany(i => i.Orders.Select(t => i));
      Assert.AreEqual(expectedCount, result.Count());
      foreach (var customer in result)
        Assert.IsNotNull(customer);
    }

    [Test]
    public void EntitySetDefaultIfEmptyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      int expectedCount =
        Session.Query.All<Order>().Count() + Session.Query.All<Customer>().Count(c => !Session.Query.All<Order>().Any(o => o.Customer==c));
      IQueryable<Order> result = Session.Query.All<Customer>().SelectMany(c => c.Orders.DefaultIfEmpty());
      Assert.AreEqual(expectedCount, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      int expectedCount = Session.Query.All<Order>().Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => c.Orders.Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expectedCount, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryWithResultSelectorTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      int expected = Session.Query.All<Order>()
        .Count(o => o.Employee.FirstName.StartsWith("A"));

      IQueryable<DateTime?> result = Session.Query.All<Customer>()
        .SelectMany(c => c.Orders.Where(o => o.Employee.FirstName.StartsWith("A")), (c, o) => o.OrderDate);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetTest()
    {
      int expected = Session.Query.All<Order>().Count();
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => c.Orders);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetWithCastTest()
    {
      var result = Session.Query.All<Customer>().SelectMany(c => (IEnumerable<Order>) c.Orders).ToList();
      var expected = Session.Query.All<Order>().ToList();
      Assert.IsTrue(result.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void SelectManyWithCastTest()
    {
      var result = Session.Query.All<Customer>().SelectMany(c => (IEnumerable<Order>) Session.Query.All<Order>().Where(o => o.Customer==c)).ToList();
      var expected = Session.Query.All<Order>().ToList();
      Assert.IsTrue(result.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void InnerJoinTest()
    {
      int ordersCount = Session.Query.All<Order>().Count();
      var result = from c in Session.Query.All<Customer>()
      from o in Session.Query.All<Order>().Where(o => o.Customer==c)
      select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(ordersCount, list.Count);
    }

    [Test]
    public void NestedTest()
    {
      IQueryable<Product> products = Session.Query.All<Product>();
      int productsCount = products.Count();
      IQueryable<Supplier> suppliers = Session.Query.All<Supplier>();
      IQueryable<Category> categories = Session.Query.All<Category>();
      var result = from p in products
      from s in suppliers
      from c in categories
      where p.Supplier==s && p.Category==c
      select new {p, s, c.CategoryName};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void OuterJoinAnonymousTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      int assertCount =
        Session.Query.All<Order>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Order>().Any(o => o.Customer==c));
      var result = from c in Session.Query.All<Customer>()
      from o in Session.Query.All<Order>().Where(o => o.Customer==c).Select(o => new {o.Id, c.CompanyName}).DefaultIfEmpty()
      select new {c.ContactName, o};
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
      foreach (var item in result) {
        Assert.IsNotNull(item);
        Assert.IsNotNull(item.ContactName);
        Assert.IsNotNull(item.o);
      }
      QueryDumper.Dump(list);
    }

    [Test]
    public void OuterJoinAnonymousFieldTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      int assertCount =
        Session.Query.All<Order>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Order>().Any(o => o.Customer==c));
      var result = from c in Session.Query.All<Customer>()
      from o in Session.Query.All<Order>().Where(o => o.Customer==c).Select(o => new {o.Id, c.CompanyName}).DefaultIfEmpty()
      select new {c.ContactName, o.Id};
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }

    [Test]
//    [ExpectedException(typeof (NullReferenceException))]
    public void OuterJoinEntityTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      int assertCount =
        Session.Query.All<Order>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Order>().Any(o => o.Customer==c));
      var result = 
        from c in Session.Query.All<Customer>()
        from o in Session.Query.All<Order>().Where(o => o.Customer==c).DefaultIfEmpty()
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }

    [Test]
    public void OuterJoinValueTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      int assertCount =
        Session.Query.All<Order>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Order>().Any(o => o.Customer==c));
      var result = from c in Session.Query.All<Customer>()
      from o in Session.Query.All<Order>().Where(o => o.Customer==c).Select(o => o.OrderDate).DefaultIfEmpty()
      select new {c.ContactName, o};
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }


    [Test]
    public void SelectManyAfterSelect1Test()
    {
      IQueryable<string> result = Session.Query.All<Customer>()
        .Select(c => c.Orders.Select(o => o.ShipName))
        .SelectMany(orders => orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectManyAfterSelect2Test()
    {
      int expected = Session.Query.All<Order>().Count();
      IQueryable<Order> result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Order>().Where(o => o.Customer==c)).SelectMany(o => o);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SimpleTest()
    {
      int expected = Session.Query.All<Order>().Count();
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Order>().Where(o => o.Customer==c));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SimpleWithResultSelectorTest()
    {
      int expected = Session.Query.All<Order>().Count();
      var result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Order>().Where(o => o.Customer==c), (c, o) => new {c, o});
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SubqueryWithEntityReferenceTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      int expected = Session.Query.All<Order>().Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Order>().Where(o => o.Customer==c)
          .Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SelectManySelfTest()
    {
      var result =
        from c1 in Session.Query.All<Customer>()
        from c2 in Session.Query.All<Customer>()
        where c1.Address.City==c2.Address.City
        select new {c1, c2};
      result.ToList();
    }

    [Test]
    public void IntersectBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      int expected = Session.Query.All<Order>().Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Order>().Where(o => o.Customer==c).Intersect(Session.Query.All<Order>())
          .Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void DistinctBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      int expected = Session.Query.All<Order>().Distinct().Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Order>().Where(o => o.Customer==c).Distinct()
          .Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void TakeBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Order>().Where(o => o.Customer==c).Take(10));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SkipBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      IQueryable<Order> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Order>().Where(o => o.Customer==c).Skip(10));
      QueryDumper.Dump(result);
    }

    [Test]
    public void CalculateWithApply()
    {
      var expected = from c in Session.Query.All<Customer>().ToList()
      from r in (c.Orders.Select(o => c.ContactName + o.ShipName).Where(x => x.StartsWith("a")))
      orderby r
      select r;
      var actual = from c in Session.Query.All<Customer>()
      from r in (c.Orders.Select(o => c.ContactName + o.ShipName).Where(x => x.StartsWith("a")))
      orderby r
      select r;
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void TwoCalculateWithApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var actual = from c in Session.Query.All<Customer>()
      from r in (c.Orders.Select(o => c.ContactName + o.ShipName))
        .Union(c.Orders.Select(o => c.ContactName + o.ShipName))
      orderby r
      select r;
      var expected = from c in Session.Query.All<Customer>().ToList()
      from r in (c.Orders.Select(o => c.ContactName + o.ShipName))
        .Union(c.Orders.Select(o => c.ContactName + o.ShipName))
      orderby r
      select r;
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void TwoFilterWithApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var actual = from c in Session.Query.All<Customer>()
      from r in (c.Orders.Where(x => x.ShipName.StartsWith("a"))
        .Intersect(c.Orders.Where(x => x.ShipName.StartsWith("a"))))
      orderby r.Id
      select r.Id;
      var expected = from c in Session.Query.All<Customer>().ToList()
      from r in (c.Orders.Where(x => x.ShipName.StartsWith("a"))
        .Intersect(c.Orders))
      orderby r.Id
      select r.Id;
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void TwoSelectManyTest()
    {
      var q =
        from o in Session.Query.All<Order>().Take(10)
        from d in Session.Query.All<OrderDetails>().Take(10)
        select new {OrderId = o.Id, d.UnitPrice};

      var count = q.Count();
      Log.Info("Records count: {0}", count);
      QueryDumper.Dump(q);
    }
  }
}