// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.01

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
    public void SimpleTest()
    {
      var expected = Query<Order>.All.Count();
      var result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer==c));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SimpleWithResultSelectorTest()
    {
      var expected = Query<Order>.All.Count();
      var result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer==c), (c, o) => new {c, o});
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SubqueryWithEntityReferenceTest()
    {
      var expected = Query<Order>.All.Count(o => o.Employee.FirstName.StartsWith("A"));
      var result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c).Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void NestedTest()
    {
      var products = Query<Product>.All;
      var productsCount = products.Count();
      var suppliers = Query<Supplier>.All;
      var categories = Query<Category>.All;
      var result = from p in products
      from s in suppliers
      from c in categories
      where p.Supplier==s && p.Category==c
      select new {p, s, c.CategoryName};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void InnerJoinTest()
    {
      var ordersCount = Query<Order>.All.Count();
      var result = from c in Query<Customer>.All
      from o in Query<Order>.All.Where(o => o.Customer==c)
      select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(ordersCount, list.Count);
    }

    [Test]
    public void OuterJoinTest()
    {
      var assertCount =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer==c));
      var result = from c in Query<Customer>.All
      from o in Query<Order>.All.Where(o => o.Customer==c).DefaultIfEmpty()
      select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
    }

    [Test]
    public void EntitySetTest()
    {
      var expected = Query<Order>.All.Count();
      var result = Query<Customer>.All
        .SelectMany(c => c.Orders);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryTest()
    {
      var expected = Query<Order>.All.Count(o => o.Employee.FirstName.StartsWith("A"));
      var result = Query<Customer>.All
        .SelectMany(c => c.Orders.Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryWithResultSelectorTest()
    {
      var expected = Query<Order>.All
        .Count(o => o.Employee.FirstName.StartsWith("A"));

      var result = Query<Customer>.All
        .SelectMany(c => c.Orders.Where(o => o.Employee.FirstName.StartsWith("A")), (c,o) => o.OrderDate);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetDefaultIfEmptyTest()
    {
      var expected = 
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer==c));
      var result = Query<Customer>.All.SelectMany(c => c.Orders.DefaultIfEmpty());
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SelectManyAfterSelectTest()
    {
      var expected = Query<Order>.All.Count();
      var result = Query<Customer>.All.Select(c => Query<Order>.All.Where(o => o.Customer == c)).SelectMany(o => o);
      Assert.AreEqual(expected, result.ToList().Count);
    }
  }
}
