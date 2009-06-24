// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;
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
      var expectedCount = Query<Order>.All.Count();
      var result = Query<Customer>.All
        .SelectMany(i => i.Orders.Select(t => i));
      Assert.AreEqual(expectedCount, result.Count());
      foreach (var customer in result)
        Assert.IsNotNull(customer);
    }

    [Test]
    public void EntitySetDefaultIfEmptyTest()
    {
      int expectedCount =
        Query<Order>.All.Count() + Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer == c));
      IQueryable<Order> result = Query<Customer>.All.SelectMany(c => c.Orders.DefaultIfEmpty());
      Assert.AreEqual(expectedCount, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryTest()
    {
      int expectedCount = Query<Order>.All.Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => c.Orders.Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expectedCount, result.ToList().Count);
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
    public void EntitySetWithCastTest()
    {
      var result = Query<Customer>.All.SelectMany(c => (IEnumerable<Order>) c.Orders).ToList();
      var expected = Query<Order>.All.ToList();
      Assert.IsTrue(result.Except(expected).IsNullOrEmpty());
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
    public void OuterJoinAnonymousTest()
    {
      int assertCount =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer == c));
      var result = from c in Query<Customer>.All
                   from o in Query<Order>.All.Where(o => o.Customer == c).Select(o => new { o.Id, c.CompanyName }).DefaultIfEmpty()
                   select new { c.ContactName, o };
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
      int assertCount =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer == c));
      var result = from c in Query<Customer>.All
                   from o in Query<Order>.All.Where(o => o.Customer == c).Select(o => new {o.Id, c.CompanyName}).DefaultIfEmpty()
                   select new { c.ContactName, o.Id };
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void OuterJoinEntityTest()
    {
      int assertCount =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer == c));
      var result = from c in Query<Customer>.All
                   from o in Query<Order>.All.Where(o => o.Customer == c).DefaultIfEmpty()
                   select new { c.ContactName, o.OrderDate };
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }

    [Test]
    public void OuterJoinValueTest()
    {
      int assertCount =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer == c));
      var result = from c in Query<Customer>.All
                   from o in Query<Order>.All.Where(o => o.Customer == c).Select(o => o.OrderDate).DefaultIfEmpty()
                   select new { c.ContactName, o };
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
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
    public void IntersectBetweenFilterAndApplyTest()
    {
      int expected = Query<Order>.All.Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c).Intersect(Query<Order>.All)
        .Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void DistinctBetweenFilterAndApplyTest()
    {
      int expected = Query<Order>.All.Distinct().Count(o => o.Employee.FirstName.StartsWith("A"));
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c).Distinct()
        .Where(o => o.Employee.FirstName.StartsWith("A")));
      Assert.AreEqual(expected, result.ToList().Count);
    }
    
    [Test]
    public void TakeBetweenFilterAndApplyTest()
    {
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c).Take(10));
      if (Domain.Configuration.ConnectionInfo.Protocol!="memory" 
        && Domain.Configuration.ConnectionInfo.Protocol!="mssql2005")
        AssertEx.ThrowsInvalidOperationException(() => QueryDumper.Dump(result));
      else
        QueryDumper.Dump(result);
    }

    [Test]
    public void SkipBetweenFilterAndApplyTest()
    {
      IQueryable<Order> result = Query<Customer>.All
        .SelectMany(c => Query<Order>.All.Where(o => o.Customer == c).Skip(10));
      if (Domain.Configuration.ConnectionInfo.Protocol!="memory" 
        && Domain.Configuration.ConnectionInfo.Protocol!="mssql2005")
        AssertEx.ThrowsInvalidOperationException(() => QueryDumper.Dump(result));
      else
        QueryDumper.Dump(result);
    }

    [Test]
    public void CalculateWithApply()
    {
      var expected = from c in Query<Customer>.All.ToList()
      from r in (c.Orders.Select(o => c.ContactName + o.ShipName).Where(x => x.StartsWith("a")))
      orderby r
      select r;
      var actual = from c in Query<Customer>.All
      from r in (c.Orders.Select(o => c.ContactName + o.ShipName).Where(x => x.StartsWith("a")))
      orderby r
      select r;
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void TwoCalculateWithApplyTest()
    {
      var actual = from c in Query<Customer>.All
      from r in (c.Orders.Select(o => c.ContactName + o.ShipName))
        .Union(c.Orders.Select(o => c.ContactName + o.ShipName))
      orderby r
      select r;
      if (Domain.Configuration.ConnectionInfo.Protocol!="memory" 
        && Domain.Configuration.ConnectionInfo.Protocol!="mssql2005")
        AssertEx.ThrowsInvalidOperationException(() => QueryDumper.Dump(actual));
      else {
        var expected = from c in Query<Customer>.All.ToList()
        from r in (c.Orders.Select(o => c.ContactName + o.ShipName))
          .Union(c.Orders.Select(o => c.ContactName + o.ShipName))
        orderby r
        select r;
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void TwoFilterWithApplyTest()
    {
      var actual = from c in Query<Customer>.All
      from r in (c.Orders.Where(x => x.ShipName.StartsWith("a"))
        .Intersect(c.Orders.Where(x => x.ShipName.StartsWith("a"))))
      orderby r.Id
      select r.Id;
      if (Domain.Configuration.ConnectionInfo.Protocol!="memory" 
        && Domain.Configuration.ConnectionInfo.Protocol!="mssql2005")
        AssertEx.ThrowsInvalidOperationException(() => QueryDumper.Dump(actual));
      else {
        var expected = from c in Query<Customer>.All.ToList()
        from r in (c.Orders.Where(x => x.ShipName.StartsWith("a"))
          .Intersect(c.Orders))
        orderby r.Id
        select r.Id;
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }
  }
}