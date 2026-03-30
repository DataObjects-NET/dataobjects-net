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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count, Is.GreaterThan(0));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count, Is.GreaterThan(0));
    }

    [Test]
    public void AnyWithLocalCollectionTest()
    {
      var names = new[] {"ABCDE", "Luis"};
      var result = Session.Query.All<Customer>().Where(c => names.Any(id => c.FirstName==id));
      var list = result.ToList();
      Assert.That(list.Count, Is.GreaterThan(0));
    }

    [Test]
    public void AnyTest()
    {
      var result = Session.Query.All<Customer>().Any();
      var expected = Customers.Any();
      Assert.That(expected, Is.EqualTo(result));
      Assert.That(result, Is.True);
    }

    [Test]
    public void AnySubqueryTest()
    {
      var result = Session.Query.All<Customer>().Where(c=>c.Invoices.Any()).ToList();
      var expected = Invoices.Select(o => o.Customer).Distinct().ToList();
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count, Is.GreaterThan(0));
    }

    [Test]
    public void AllWithLocalCollectionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var patterns = new[] {"a", "e"};
      var result = Session.Query.All<Customer>().Where(c => patterns.All(p => c.FirstName.Contains(p)));
      var list = result.ToList();
      Assert.That(list.Count, Is.GreaterThan(0));
    }

    [Test]
    public void AllTest()
    {
      var result = Session.Query.All<Customer>().All(c => c.FirstName.StartsWith("a"));
      var expected = Customers.All(c => c.FirstName.StartsWith("a"));
      Assert.That(result, Is.EqualTo(expected));
      Assert.That(result, Is.False);
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(list.Count, Is.GreaterThan(0));
    }

    [Test]
    public void ContainsWithLocalCollectionTest()
    {
      var customerIDs = new[] {"ALFKI", "Diego", "AROUT", "Luis"};
      var orders = Session.Query.All<Invoice>();
      var order = orders.Where(o => customerIDs.Contains(o.Customer.FirstName)).First();
      Assert.That(order, Is.Not.Null);
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
      Assert.That(result, Is.EqualTo(expected));
      Assert.That(result, Is.True);
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      var list = result.ToList();
      Assert.That(list.Count, Is.EqualTo(14));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      var list = result.ToList();
      Assert.That(list.Count, Is.EqualTo(128));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count, Is.EqualTo(111));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(118, Is.EqualTo(result.ToList().Count));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(4, Is.EqualTo(result.ToList().Count));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count, Is.EqualTo(408));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count(i => !i.HasOrders), Is.EqualTo(1));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count(i => i.AllEmployeesAreCool), Is.EqualTo(1));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count(i => i.HasNewOrders), Is.EqualTo(0));
    }

    [Test]
    public void EntitySetAnyTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.Any(o => o.Commission > 1m));
      var expected = Customers
        .Where(c => c.Invoices.Any(o => o.Commission > 1m));
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count, Is.EqualTo(2));
    }

    [Test]
    public void EntitySetAllTest()
    {
      Require.ProviderIsNot(StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.All(o => o.DesignatedEmployee.FirstName=="???"));
      var expected = Customers
        .Where(c => c.Invoices.All(o => o.DesignatedEmployee.FirstName=="???"));
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Count, Is.EqualTo(1));
    }

    [Test]
    public void EntitySetContainsTest()
    {
      var bestOrder = Session.Query.All<Invoice>().OrderBy(o => o.Commission).First();
      var result = Session.Query.All<Customer>()
        .Where(c => Queryable.Contains(c.Invoices, bestOrder));
      var expected = Customers
        .Where(c => Queryable.Contains(c.Invoices, bestOrder));
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      Assert.That(result.ToList().Single().CustomerId, Is.EqualTo(bestOrder.Customer.CustomerId));
    }

    [Test]
    public void EntitySetAllStructureTest()
    {
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.All(o => o.BillingAddress.City==c.Address.City));
      var expected = Customers
        .Where(c => c.Invoices.All(o => o.BillingAddress.City==c.Address.City));
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
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
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      result.ToList();
    }
  }
}