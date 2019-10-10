// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.16

using System.Collections;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  [Category("Linq")]
  public class NestedCollectionsTest : ChinookDOModelTest
  {
    private int numberOfCustomers;
    private int numberOfInvoices;
    private int numberOfEmployees;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        numberOfCustomers = session.Query.All<Customer>().Count();
        numberOfInvoices = session.Query.All<Invoice>().Count();
        numberOfEmployees = session.Query.All<Employee>().Count();
        t.Complete();
      }
    }

    [Test]
    public void SubqueryTest()
    {
      var result =
        from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          Invoices = c.Invoices
            .Select(i => new {Invoice = i, InvoiceLines = i.InvoiceLines})
        };

      Assert.That(result, Is.Not.Empty);
      foreach (var a in result)
        foreach (var b in a.Invoices)
          foreach (var il in b.InvoiceLines) {
          }
    }

    [Test]
    public void SubqueryWithThisTest()
    {
      var customer = Session.Query.All<Customer>().First();
      var invoices = customer.Invoices;
      Assert.That(invoices, Is.Not.Empty);
      QueryDumper.Dump(invoices);
      var firstInvoice = customer.FirstInvoice;
      Assert.That(firstInvoice, Is.Not.Null);
    }

    [Test]
    public void SelectOtherParameterTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Customer>().Take(5).Select(c => Session.Query.All<Invoice>().Select(i => c.Invoices.Count()));
      Assert.That(result.ToList(), Is.Not.Empty);
    }

    [Test]
    public void SelectNestedTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>())
        .Select(qi => qi);
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(numberOfCustomers * numberOfInvoices, Count(result));
    }

    [Test]
    public void SelectDoubleNestedTest()
    {
      var result = Session.Query.All<Customer>()
        .Take(2)
        .Select(c => Session.Query.All<Invoice>()
          .Take(2)
          .Select(i => Session.Query.All<Employee>().Take(2)))
        .Select(qqe => qqe);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexSubqueryTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Customer>()
        .Take(2)
        .Select(c => Session.Query.All<Invoice>()
          .Select(i => Session.Query.All<Employee>()
            .Take(2)
            .Where(e => e.Invoices.Contains(i)))
          .Where(i => i.Count() > 0))
        .Select(qqe => qqe);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectNestedWithCorrelationTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>().Where(i => i.Customer==c))
        .Select(qi => qi);
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(numberOfInvoices, Count(result));
    }

    [Test]
    public void SelectNestedWithAggregateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>().Where(i => i.Customer==c).Count());
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousTest()
    {
      var result = Session.Query.All<Customer>()
        .Take(10)
        .Select(c => new {Invoices = Session.Query.All<Invoice>().Take(10)});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryAsQuerySourceTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>().Where(i => i.Customer==c));
      Assert.That(result, Is.Not.Empty);
      foreach (var invoices in result) {
        var subQueryCount = invoices.Count();
      }
    }

    [Test]
    public void SelectTwoCollectionsTest()
    {
      var result = Session.Query.All<Invoice>()
        .Select(i => new {
          Customers = Session.Query.All<Customer>().Where(c => c==i.Customer),
          Employees = Session.Query.All<Employee>().Where(e => e==i.DesignatedEmployee)
        })
        .Select(os => os);
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(numberOfInvoices, list.Count);
      foreach (var i in list) {
        Assert.AreEqual(1, i.Customers.Count());
        Assert.AreEqual(1, i.Employees.Count());
        Assert.AreEqual(1, i.Customers.ToList().Count);
        Assert.AreEqual(1, i.Employees.ToList().Count);
      }
    }

    [Test]
    public void SelectNestedSelectManyTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>())
        .SelectMany(i => i);
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(numberOfCustomers * numberOfInvoices, Count(result));
    }

    [Test]
    public void SelectDoubleNestedSelectManyTest()
    {
      var result = Session.Query.All<Customer>()
        .Take(10)
        .Select(c => Session.Query.All<Invoice>()
          .Take(10)
          .Select(i => Session.Query.All<Employee>()))
        .SelectMany(i => i)
        .SelectMany(i => i);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectNestedWithCorrelationSelectManyTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>().Where(i => i.Customer==c))
        .SelectMany(i => i);
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(numberOfInvoices, Count(result));
    }

    [Test]
    public void SelectNestedWithCorrelationSelectMany2Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>().Where(i => i.Customer==c))
        .SelectMany(i => i.Select(x => x));
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(numberOfInvoices, Count(result));
    }

    [Test]
    public void SelectAnonymousSelectMany1Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {Customer = c, Invoices = Session.Query.All<Invoice>().Where(i => i.Customer==c)})
        .SelectMany(i => i.Invoices);
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(numberOfInvoices, result.ToList().Count);
    }

    [Test]
    public void SelectAnonymousSelectMany2Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {Invoices = Session.Query.All<Invoice>().Take(10)})
        .SelectMany(i => i.Invoices);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSubqueryTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {Customer = c, Invoices = Session.Query.All<Invoice>()})
        .Select(i => i.Customer.Invoices);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSelectMany3Test()
    {
      IQueryable<Customer> result = Session.Query.All<Customer>()
        .Select(c => new {Customer = c, Invoices = Session.Query.All<Invoice>().Where(i => i.Customer==c)})
        .SelectMany(a => a.Invoices.Select(i => a.Customer));
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSelectMany4Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {Customer = c, Invoices = Session.Query.All<Invoice>().Where(i => i.Customer==c)})
        .SelectMany(a => a.Invoices.Select(i => new {a.Customer, Invoice = i}));
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(numberOfInvoices, result.ToList().Count);
    }

    [Test]
    public void SelectAnonymousSelectMany5Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {Customer = c, Invoices = Session.Query.All<Invoice>().Where(i => i.Customer==c)})
        .SelectMany(a => a.Invoices.Select(i => a.Customer.FirstName));
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubquerySimpleTest()
    {
      var result = Session.Query.All<Track>()
        .Select(t => Session.Query.All<MediaType>());
      Assert.That(result, Is.Not.Empty);
      foreach (IQueryable<MediaType> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryWhereTest()
    {
      var result = Session.Query.All<Track>()
        .Select(t => Session.Query.All<MediaType>().Where(m => m==t.MediaType));
      Assert.That(result, Is.Not.Empty);
      foreach (IQueryable<MediaType> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryParametrizedFieldTest()
    {
      var result = Session.Query.All<Track>()
        .Select(t => Session.Query.All<MediaType>().Select(m => t.UnitPrice));
      Assert.That(result, Is.Not.Empty);
      foreach (var queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryWithSelectTest()
    {
      var result = Session.Query.All<Track>()
        .Select(t => Session.Query.All<MediaType>().Where(m => m==t.MediaType))
        .Select(m => m);
      Assert.That(result, Is.Not.Empty);
      foreach (IQueryable<MediaType> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryScalarTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Track>()
        .Select(t => Session.Query.All<Playlist>().Count());
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryWhereEntitySetTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Playlist>()
        .Where(p => p.Tracks.Count > 0);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    private static int Count(IEnumerable result)
    {
      var count = 0;
      bool? nested = null;
      foreach (var item in result) {
        if (nested==null)
          nested = item is IEnumerable;
        if (nested.Value)
          count += Count((IEnumerable) item);
        else
          count++;
      }
      return count;
    }
  }
}