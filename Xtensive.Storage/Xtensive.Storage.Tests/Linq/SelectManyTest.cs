// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.01

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SelectManyTest : NorthwindDOModelTest
  {
    [Test]
    public void ParameterTest()
    {
      var result = Query<Customer>.All
        .SelectMany(i => i.Orders.Select(t => i));
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntitySetDefaultIfEmptyTest()
    {
      int expected =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer == c));
      IQueryable<Order> result = Query<Customer>.All.SelectMany(c => c.Orders.DefaultIfEmpty());
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryTest()
    {
      int expected = Query<Order>.All.Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => c.Orders.Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryWithResultSelectorTest()
    {
      int expected = Query<Order>.All
        .Count(o => o.Employee.FirstName.StartsWith("A"));

      IQueryable<DateTime?> result = Query<Customer>.All
        .SelectMany(c => c.Orders.Where(o => o.Employee.FirstName.StartsWith("A")), (c, o) => o.OrderDate);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetTest()
    {
      int expected = Query<Order>.All.Count();
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => c.Orders);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void InnerJoinTest()
    {
      int ordersCount = Query<Order>.All.Count();
      var result = from c in Query<Customer>.All
      from o in Query<Order>.All.Where(o => o.Customer == c)
      select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(ordersCount, list.Count);
    }

    [Test]
    public void NestedTest()
    {
      IQueryable<Product> products = Query<Product>.All;
      int productsCount = products.Count();
      IQueryable<Supplier> suppliers = Query<Supplier>.All;
      IQueryable<Category> categories = Query<Category>.All;
      var result = from p in products
      from s in suppliers
      from c in categories
      where p.Supplier == s && p.Category == c
      select new {p, s, c.CategoryName};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void OuterJoinTest()
    {
      int assertCount =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer == c));
      var result = from c in Query<Customer>.All
        from o in Query<Order>.All.Where(o => o.Customer == c).DefaultIfEmpty()
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
    }


    [Test]
    public void SelectManyAfterSelect1Test()
    {
      IQueryable<string> result = Query<Customer>.All
        .Select(c => c.Orders.Select(o => o.ShipName))
        .SelectMany(orders => orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectManyAfterSelect2Test()
    {
      int expected = Query<Order>.All.Count();
      IQueryable<Order> result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer == c)).SelectMany(o => o);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SimpleTest()
    {
      int expected = Query<Order>.All.Count();
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SimpleWithResultSelectorTest()
    {
      int expected = Query<Order>.All.Count();
      var result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c), (c, o) => new {c, o});
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SubqueryWithEntityReferenceTest()
    {
      int expected = Query<Order>.All.Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c)
          .Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SelectManySelfTest()
    {
      var result =
        from c1 in Query<Customer>.All
        from c2 in Query<Customer>.All
        where c1.Address.City == c2.Address.City
        select new {c1,c2};
      result.ToList();
    }

    [Test]
    public void IntersectBeforeWhere()
    {
      int expected = Query<Order>.All.Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c).Intersect(Query<Order>.All)
          .Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }
  }
}