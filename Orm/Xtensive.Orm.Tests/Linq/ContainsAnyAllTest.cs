// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class ContainsAnyAllTest : ChinookDOModelTest
  {
    [Test]
    public void AnyWithSubqueryTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .Any(o => o.Commission > 0));
      var expected = Customers
        .Where(c => Invoices
          .Where(o => o.Customer==c)
          .Any(o => o.Commission > 0));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void AnyWithSubqueryNoPredicateTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .Any());
      var expected = Customers
        .Where(c => Invoices
          .Where(o => o.Customer==c)
          .Any());
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void AnyWithLocalCollectionTest()
    {
      var names = new[] {"ABCDE", "Luis"};
      var result = Session.Query.All<Customer>().Where(c => names.Any(id => c.FirstName==id));
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
      var result = Session.Query.All<Customer>().Where(c=>c.Invoices.Any()).ToList();
      var expected = Invoices.Select(o => o.Customer).Distinct().ToList();
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void AllWithSubqueryTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .All(o => o.Commission > 0));
      var expected = Customers
        .Where(c => Invoices
          .Where(o => o.Customer==c)
          .All(o => o.Commission > 0));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void AllWithLocalCollectionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var patterns = new[] {"a", "e"};
      var result = Session.Query.All<Customer>().Where(c => patterns.All(p => c.FirstName.Contains(p)));
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AllTest()
    {
      var result = Session.Query.All<Customer>().All(c => c.FirstName.StartsWith("a"));
      var expected = Customers.All(c => c.FirstName.StartsWith("a"));
      Assert.AreEqual(expected, result);
      Assert.IsFalse(result);
    }

    [Test]
    public void ContainsWithSubqueryTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Invoice>()
          .Select(o => o.Customer)
          .Contains(c));
      var expected = Customers
        .Where(c => Invoices
          .Select(o => o.Customer)
          .Contains(c));
      var list = result.ToList();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void ContainsWithLocalCollectionTest()
    {
      var customerIDs = new[] {"ALFKI", "Diego", "AROUT", "Luis"};
      var orders = Session.Query.All<Invoice>();
      var order = orders.Where(o => customerIDs.Contains(o.Customer.FirstName)).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void ContainsTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => c.FirstName)
        .Contains("Luis");
      var expected = Customers
        .Select(c => c.FirstName)
        .Contains("Luis");
      Assert.AreEqual(expected, result);
      Assert.IsTrue(result);
    }

    [Test]
    public void SubqueryAllStructureTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .All(o => o.BillingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => Invoices
          .Where(o => o.Customer==c)
          .All(o => o.BillingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryAnyStructureTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .Any(o => o.BillingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => Invoices
          .Where(o => o.Customer==c)
          .Any(o => o.BillingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      result.ToList();
    }

    [Test]
    public void AllAndNotAllTest()
    {
      var result =
        from o in Session.Query.All<Invoice>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .All(c => c.FirstName.StartsWith("A"))
            && !Session.Query.All<Employee>()
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Invoices
        where Customers
          .Where(c => c==o.Customer)
          .All(c => c.FirstName.StartsWith("A"))
            && !Employees
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      var list = result.ToList();
      Assert.AreEqual(14, list.Count);
    }

    [Test]
    public void AllOrAllTest()
    {
      var result =
        from o in Session.Query.All<Invoice>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .All(c => c.FirstName.StartsWith("A"))
            || Session.Query.All<Employee>()
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Invoices
        where Customers
          .Where(c => c==o.Customer)
          .All(c => c.FirstName.StartsWith("A"))
            || Employees
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      var list = result.ToList();
      Assert.AreEqual(128, list.Count);
    }

    [Test]
    public void NotAnyAndAnyTest()
    {
      var result =
        from o in Session.Query.All<Invoice>()
        where !Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            && Session.Query.All<Employee>()
              .Where(e => e==o.DesignatedEmployee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Invoices
        where !Customers
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            && Employees
              .Where(e => e==o.DesignatedEmployee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(111, result.ToList().Count);
    }

    [Test]
    public void AnyOrAnyTest()
    {
      var result =
        from o in Session.Query.All<Invoice>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            || Session.Query.All<Employee>()
              .Where(e => e==o.DesignatedEmployee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Invoices
        where Customers
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            || Employees
              .Where(e => e==o.DesignatedEmployee)
              .Any(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(result.ToList().Count, 118);
    }

    [Test]
    public void AnyAndNotAllTest()
    {
      var result =
        from o in Session.Query.All<Invoice>()
        where Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            && !Session.Query.All<Employee>()
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Invoices
        where Customers
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            && !Employees
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(result.ToList().Count, 4);
    }

    [Test]
    public void NotAnyOrAllTest()
    {
      var result =
        from o in Session.Query.All<Invoice>()
        where !Session.Query.All<Customer>()
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            || Session.Query.All<Employee>()
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      var expected =
        from o in Invoices
        where !Customers
          .Where(c => c==o.Customer)
          .Any(c => c.LastName.StartsWith("A"))
            || Employees
              .Where(e => e==o.DesignatedEmployee)
              .All(e => e.FirstName.EndsWith("t"))
        select o;
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(408, result.ToList().Count);
    }

    [Test]
    public void SelectAnyTest()
    {
      var result =
        (from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          HasOrders = Session.Query.All<Invoice>()
            .Where(o => o.Customer==c)
            .Any()
        }).ToList();
      var expected =
        (from c in Customers
        select new {
          Customer = c,
          HasOrders = Invoices
            .Where(o => o.Customer==c)
            .Any()
        }).ToList();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(1, result.ToList().Count(i => !i.HasOrders));
    }

    [Test]
    public void SelectAllTest()
    {
      var result =
        from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          AllEmployeesAreCool = Session.Query.All<Invoice>()
            .Where(o => o.Customer==c)
            .All(o => o.DesignatedEmployee.FirstName=="Cool")
        };
      var expected =
        from c in Customers
        select new {
          Customer = c,
          AllEmployeesAreCool = Invoices
            .Where(o => o.Customer==c)
            .All(o => o.DesignatedEmployee.FirstName=="Cool")
        };
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(1, result.ToList().Count(i => i.AllEmployeesAreCool));
    }

    [Test]
    public void SelectContainsTest()
    {
      var result =
        from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          HasNewOrders = Session.Query.All<Invoice>()
            .Where(o => o.InvoiceDate > new DateTime(2020, 1, 1))
            .Select(o => o.Customer)
            .Contains(c)
        };
      var expected =
        from c in Customers
        select new {
          Customer = c,
          HasNewOrders = Invoices
            .Where(o => o.InvoiceDate > new DateTime(2020, 1, 1))
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
        .Where(c => c.Invoices.Any(o => o.Commission > 1m));
      var expected = Customers
        .Where(c => c.Invoices.Any(o => o.Commission > 1m));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(2, result.ToList().Count);
    }

    [Test]
    public void EntitySetAllTest()
    {
      Require.ProviderIsNot(StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.All(o => o.DesignatedEmployee.FirstName=="???"));
      var expected = Customers
        .Where(c => c.Invoices.All(o => o.DesignatedEmployee.FirstName=="???"));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(1, result.ToList().Count);
    }

    [Test]
    public void EntitySetContainsTest()
    {
      var bestOrder = Session.Query.All<Invoice>().OrderBy(o => o.Commission).First();
      var result = Session.Query.All<Customer>()
        .Where(c => Queryable.Contains(c.Invoices, bestOrder));
      var expected = Customers
        .Where(c => Queryable.Contains(c.Invoices, bestOrder));
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.AreEqual(bestOrder.Customer.CustomerId, result.ToList().Single().CustomerId);
    }

    [Test]
    public void EntitySetAllStructureTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.All(o => o.BillingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => c.Invoices.All(o => o.BillingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      result.ToList();
    }

    [Test]
    public void EntitySetAnyStructureTest()
    {
      Require.ProviderIsNot(StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.Any(o => o.BillingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => c.Invoices.Any(o => o.BillingAddress.City==c.Address.City));
      Assert.AreEqual(0, expected.Except(result).Count());
      result.ToList();
    }
  }
}