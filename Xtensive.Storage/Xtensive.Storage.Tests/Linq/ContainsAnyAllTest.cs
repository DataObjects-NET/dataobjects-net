// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class ContainsAnyAllTest : NorthwindDOModelTest
  {
    [Test]
    public void AnyWithSubqueryTest()
    {
      var result = Query<Customer>.All.Where(c => Query<Order>.All.Where(o => o.Customer==c).Any(o => o.Freight > 0));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnyWithSubqueryNoPredicateTest()
    {
      var result = Query<Customer>.All.Where(c => Query<Order>.All.Where(o => o.Customer==c).Any());
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnyWithLocalCollectionTest()
    {
      var ids = new[] {"ABCDE", "ALFKI"};
      var result = Query<Customer>.All.Where(c => ids.Any(id => c.Id==id));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnyTest()
    {
      var result = Query<Customer>.All.Any();
      Assert.IsTrue(result);
    }

    [Test]
    public void AllWithSubqueryTest()
    {
      var result = Query<Customer>.All.Where(c => Query<Order>.All.Where(o => o.Customer==c).All(o => o.Freight > 0));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AllWithLocalCollectionTest()
    {
      var patterns = new[] {"a", "e"};
      var result = Query<Customer>.All.Where(c => patterns.All(p => c.ContactName.Contains(p)));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AllTest()
    {
      var result = Query<Customer>.All.All(c => c.ContactName.StartsWith("a"));
      Assert.IsFalse(result);
    }

    [Test]
    public void ContainsWithSubqueryTest()
    {
      var result = Query<Customer>.All.Where(c => Query<Order>.All.Select(o => o.Customer).Contains(c));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void ContainsWithLocalCollectionTest()
    {
      var customerIDs = new[] {"ALFKI", "ANATR", "AROUT", "BERGS"};
      var orders = Query<Order>.All;
      var order = orders.Where(o => customerIDs.Contains(o.Customer.Id)).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void ContainsTest()
    {
      var result = Query<Customer>.All.Select(c => c.Id).Contains("ALFKI");
      Assert.IsTrue(result);
    }

    [Test]
    public void AllAndNotAllTest()
    {
      var result =
        from o in Query<Order>.All
        where Query<Customer>.All.Where(c => c==o.Customer).All(c => c.CompanyName.StartsWith("A"))
          && !Query<Employee>.All.Where(e => e==o.Employee).All(e => e.FirstName.EndsWith("t"))
        select o;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 11);
    }

    [Test]
    public void AllOrAllTest()
    {
      var result =
        from o in Query<Order>.All
        where Query<Customer>.All.Where(c => c==o.Customer).All(c => c.CompanyName.StartsWith("A"))
          || Query<Employee>.All.Where(e => e==o.Employee).All(e => e.FirstName.EndsWith("t"))
        select o;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 366);
    }

    [Test]
    public void NotAnyAndAnyTest()
    {
      var result =
        from o in Query<Order>.All
        where !Query<Customer>.All.Where(c => c==o.Customer).Any(c => c.CompanyName.StartsWith("A"))
          && Query<Employee>.All.Where(e => e==o.Employee).Any(e => e.FirstName.EndsWith("t"))
        select o;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 336);
    }

    [Test]
    public void AnyOrAnyTest()
    {
      var result =
        from o in Query<Order>.All
        where Query<Customer>.All.Where(c => c==o.Customer).Any(c => c.CompanyName.StartsWith("A"))
          || Query<Employee>.All.Where(e => e==o.Employee).Any(e => e.FirstName.EndsWith("t"))
        select o;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 366);
    }

    [Test]
    public void AnyAndNotAllTest()
    {
      var result =
        from o in Query<Order>.All
        where Query<Customer>.All.Where(c => c==o.Customer).Any(c => c.CompanyName.StartsWith("A"))
          && !Query<Employee>.All.Where(e => e==o.Employee).All(e => e.FirstName.EndsWith("t"))
        select o;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 11);
    }

    [Test]
    public void NotAnyOrAllTest()
    {
      var result =
        from o in Query<Order>.All
        where !Query<Customer>.All.Where(c => c==o.Customer).Any(c => c.CompanyName.StartsWith("A"))
          || Query<Employee>.All.Where(e => e==o.Employee).All(e => e.FirstName.EndsWith("t"))
        select o;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 819);
    }

    [Test]
    public void SelectAnyTest()
    {
      var result =
        from c in Query<Customer>.All
        select new {
          Customer = c,
          HasOrders = Query<Order>.All
            .Where(o => o.Customer==c)
            .Any()
        };
      Assert.AreEqual(2, result.ToList().Count(i => !i.HasOrders));
    }

    [Test]
    public void SelectAllTest()
    {
      var result =
        from c in Query<Customer>.All
        select new {
          Customer = c,
          AllEmployeesAreCool = Query<Order>.All
            .Where(o => o.Customer==c)
            .All(o => o.Employee.FirstName=="Cool")
        };
      Assert.AreEqual(2, result.ToList().Count(i => i.AllEmployeesAreCool));
    }

    [Test]
    public void SelectContainsTest()
    {
      var result =
        from c in Query<Customer>.All
        select new {
          Customer = c,
          HasNewOrders = Query<Order>.All
            .Where(o => o.OrderDate > new DateTime(2001, 1, 1))
            .Select(o => o.Customer)
            .Contains(c)
        };
      Assert.AreEqual(0, result.ToList().Count(i => i.HasNewOrders));
    }

    [Test]
    public void EntitySetAnyTest()
    {
      var result = Query<Customer>.All.Where(c => c.Orders.Any(o => o.Freight > 400));
      Assert.AreEqual(10, result.ToList().Count);
    }

    [Test]
    public void EntitySetAllTest()
    {
      var result = Query<Customer>.All.Where(c => c.Orders.All(o => o.Employee.FirstName=="???"));
      Assert.AreEqual(2, result.ToList().Count);
    }

    [Test]
    public void EntitySetContainsTest()
    {
      var bestOrder = Query<Order>.All.OrderBy(o => o.Freight).First();
      var result = Query<Customer>.All.Where(c => c.Orders.Contains(bestOrder));
      Assert.AreEqual("LACOR", result.ToList().Single().Id);
    }
  }
}