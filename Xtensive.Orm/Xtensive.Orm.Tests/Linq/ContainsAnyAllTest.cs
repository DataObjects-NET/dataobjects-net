// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class ContainsAnyAllTest : NorthwindDOModelTest
  {
    [Test]
    public void AnyWithSubqueryTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Order>()
          .Where(o => o.Customer==c)
          .Any(o => o.Freight > 0));
      var expected = Customers
        .Where(c => Orders
          .Where(o => o.Customer==c)
          .Any(o => o.Freight > 0));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void AnyWithSubqueryNoPredicateTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Order>()
          .Where(o => o.Customer==c)
          .Any());
      var expected = Customers
        .Where(c => Orders
          .Where(o => o.Customer==c)
          .Any());
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void AnyWithLocalCollectionTest()
    {
      var ids = new[] {"ABCDE", "ALFKI"};
      var result = Session.Query.All<Customer>().Where(c => ids.Any(id => c.Id==id));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnyTest()
    {
      var result = Session.Query.All<Customer>().Any();
      var expected = Customers.Any();
      Assert.AreEqual(result, expected);
      Assert.IsTrue(result);
    }

    [Test]
    public void AnySubqueryTest()
    {
      var result = Session.Query.All<Customer>().Where(c=>c.Orders.Any()).ToList();
      var expected = Orders.Select(o => o.Customer).Distinct().ToList();
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void AllWithSubqueryTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Order>()
          .Where(o => o.Customer==c)
          .All(o => o.Freight > 0));
      var expected = Customers
        .Where(c => Orders
          .Where(o => o.Customer==c)
          .All(o => o.Freight > 0));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void AllWithLocalCollectionTest()
    {
      var patterns = new[] {"a", "e"};
      var result = Session.Query.All<Customer>().Where(c => patterns.All(p => c.ContactName.Contains(p)));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AllTest()
    {
      var result = Session.Query.All<Customer>().All(c => c.ContactName.StartsWith("a"));
      var expected = Customers.All(c => c.ContactName.StartsWith("a"));
      Assert.AreEqual(expected, result);
      Assert.IsFalse(result);
    }

    [Test]
    public void ContainsWithSubqueryTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Order>()
          .Select(o => o.Customer)
          .Contains(c));
      var expected = Customers
        .Where(c => Orders
          .Select(o => o.Customer)
          .Contains(c));
      var list = result.ToList();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void ContainsWithLocalCollectionTest()
    {
      var customerIDs = new[] {"ALFKI", "ANATR", "AROUT", "BERGS"};
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => customerIDs.Contains(o.Customer.Id)).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void ContainsTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => c.Id)
        .Contains("ALFKI");
      var expected = Customers
        .Select(c => c.Id)
        .Contains("ALFKI");
      Assert.AreEqual(expected, result);
      Assert.IsTrue(result);
    }

    [Test]
    public void SubqueryAllStructureTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Order>()
          .Where(o => o.Customer==c)
          .All(o => o.ShippingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => Orders
          .Where(o => o.Customer==c)
          .All(o => o.ShippingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryAnyStructureTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Order>()
          .Where(o => o.Customer==c)
          .Any(o => o.ShippingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => Orders
          .Where(o => o.Customer==c)
          .Any(o => o.ShippingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      result.ToList();
    }

    [Test]
    public void AllAndNotAllTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from o in Session.Query.All<Order>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .All(c => c.CompanyName.StartsWith("A"))
            && !Session.Query.All<Employee>()
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Orders
        where Customers
          .Where(c => c==o.Customer)
          .All(c => c.CompanyName.StartsWith("A"))
            && !Employees
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      var list = result.ToList();
      Assert.AreEqual(list.Count, 11);
    }

    [Test]
    public void AllOrAllTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from o in Session.Query.All<Order>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .All(c => c.CompanyName.StartsWith("A"))
            || Session.Query.All<Employee>()
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Orders
        where Customers
          .Where(c => c==o.Customer)
          .All(c => c.CompanyName.StartsWith("A"))
            || Employees
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      var list = result.ToList();
      Assert.AreEqual(list.Count, 366);
    }

    [Test]
    public void NotAnyAndAnyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from o in Session.Query.All<Order>()
        where !Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            && Session.Query.All<Employee>()
              .Where(e => e==o.Employee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Orders
        where !Customers
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            && Employees
              .Where(e => e==o.Employee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(result.ToList().Count, 336);
    }

    [Test]
    public void AnyOrAnyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from o in Session.Query.All<Order>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            || Session.Query.All<Employee>()
              .Where(e => e==o.Employee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Orders
        where Customers
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            || Employees
              .Where(e => e==o.Employee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(result.ToList().Count, 366);
    }

    [Test]
    public void AnyAndNotAllTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from o in Session.Query.All<Order>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            && !Session.Query.All<Employee>()
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Orders
        where Customers
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            && !Employees
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(result.ToList().Count, 11);
    }

    [Test]
    public void NotAnyOrAllTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from o in Session.Query.All<Order>()
        where !Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            || Session.Query.All<Employee>()
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Orders
        where !Customers
          .Where(c => c==o.Customer)
          .Any(c => c.CompanyName.StartsWith("A"))
            || Employees
              .Where(e => e==o.Employee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(result.ToList().Count, 819);
    }

    [Test]
    public void SelectAnyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        (from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          HasOrders = Session.Query.All<Order>()
            .Where(o => o.Customer==c)
            .Any()
        }).ToList();
      var expected =
        (from c in Customers
        select new {
          Customer = c,
          HasOrders = Orders
            .Where(o => o.Customer==c)
            .Any()
        }).ToList();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(2, result.ToList().Count(i => !i.HasOrders));
    }

    [Test]
    public void SelectAllTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          AllEmployeesAreCool = Session.Query.All<Order>()
            .Where(o => o.Customer==c)
            .All(o => o.Employee.FirstName=="Cool")
        };
      var expected =
        from c in Customers
        select new {
          Customer = c,
          AllEmployeesAreCool = Orders
            .Where(o => o.Customer==c)
            .All(o => o.Employee.FirstName=="Cool")
        };
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(2, result.ToList().Count(i => i.AllEmployeesAreCool));
    }

    [Test]
    public void SelectContainsTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result =
        from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          HasNewOrders = Session.Query.All<Order>()
            .Where(o => o.OrderDate > new DateTime(2001, 1, 1))
            .Select(o => o.Customer)
            .Contains(c)
        };
      var expected =
        from c in Customers
        select new {
          Customer = c,
          HasNewOrders = Orders
            .Where(o => o.OrderDate > new DateTime(2001, 1, 1))
            .Select(o => o.Customer)
            .Contains(c)
        };
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(0, result.ToList().Count(i => i.HasNewOrders));
    }

    [Test]
    public void EntitySetAnyTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => c.Orders.Any(o => o.Freight > 400));
      var expected = Customers
        .Where(c => c.Orders.Any(o => o.Freight > 400));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(10, result.ToList().Count);
    }

    [Test]
    public void EntitySetAllTest()
    {
      Require.ProviderIsNot(StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Orders.All(o => o.Employee.FirstName=="???"));
      var expected = Customers
        .Where(c => c.Orders.All(o => o.Employee.FirstName=="???"));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(2, result.ToList().Count);
    }

    [Test]
    public void EntitySetContainsTest()
    {
      var bestOrder = Session.Query.All<Order>().OrderBy(o => o.Freight).First();
      var result = Session.Query.All<Customer>()
        .Where(c => Queryable.Contains(c.Orders, bestOrder));
      var expected = Customers
        .Where(c => Queryable.Contains(c.Orders, bestOrder));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(bestOrder.Customer.Id, result.ToList().Single().Id);
    }

    [Test]
    public void EntitySetAllStructureTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => c.Orders.All(o => o.ShippingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => c.Orders.All(o => o.ShippingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      result.ToList();
    }

    [Test]
    public void EntitySetAnyStructureTest()
    {
      Require.ProviderIsNot(StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Orders.Any(o => o.ShippingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => c.Orders.Any(o => o.ShippingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      result.ToList();
    }
  }
}