// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.01.29

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
  public class OrderByTest : ChinookDOModelTest
  {
    [Test]
    public void OrderByEnumTest()
    {
      var result = Session.Query.All<Invoice>().OrderBy(i => i.Status).ThenBy(i => i.InvoiceId);
      var list = result.ToList();
      var expected = Invoices.OrderBy(i => i.Status).ThenBy(i => i.InvoiceId);
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(Invoices.Count(), list.Count);
      Assert.IsTrue(expected.SequenceEqual(list));
    }

    [Test]
    public void OrderByTakeTest()
    {
      var result = Session.Query.All<Invoice>().OrderBy(i => i.InvoiceId).Take(10);
      var list = result.ToList();
      var expected = Invoices.OrderBy(i => i.InvoiceId).Take(10);
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(10, list.Count);
      Assert.IsTrue(expected.SequenceEqual(list));
    }

    [Test]
    public void EntityFieldTest()
    {
      var result = Session.Query.All<Invoice>().Select(i => i).OrderBy(g => g.InvoiceId);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByAnonymousEntityTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Select(il => new {InvoiceLine = il, Invoice = il.Invoice})
        .OrderBy(x => new {x, x.Invoice.Customer})
        .Select(x => new {x, x.Invoice.Customer});

      var expected = InvoiceLines
        .Select(il => new {InvoiceLine = il, Invoice = il.Invoice})
        .OrderBy(x => x.InvoiceLine.InvoiceLineId)
        .ThenBy(x => x.Invoice.InvoiceId)
        .ThenBy(x => x.Invoice.Customer.CustomerId)
        .Select(x => new {x, x.Invoice.Customer});

      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByAnonymousTest()
    {
      var result = Session.Query.All<Invoice>()
        .OrderBy(i => new {i.InvoiceDate, i.Commission})
        .Select(i => new {i.InvoiceDate, i.Commission});
      var expected = Invoices
        .OrderBy(i => i.InvoiceDate)
        .ThenBy(i => i.Commission)
        .Select(i => new {i.InvoiceDate, i.Commission});
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByAnonymous2Test()
    {
      IQueryable<Customer> customers = Session.Query.All<Customer>();
      var result = customers
        .Select(c => new {c.SupportRep.EmployeeId, c})
        .OrderBy(x => x);
      var expected = Customers
        .Select(c => new {c.SupportRep.EmployeeId, c})
        .OrderBy(x => x.EmployeeId)
        .ThenBy(x => x.c.CustomerId);
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByDescendingTest()
    {
      var result = Session.Query.All<Customer>().OrderByDescending(c => c.Address.Country)
        .ThenByDescending(c => c.CustomerId).Select(c => c.Address.City);
      var expected = Customers.OrderByDescending(c => c.Address.Country)
        .ThenByDescending(c => c.CustomerId).Select(c => c.Address.City);
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByEntityTest()
    {
      var customers = Session.Query.All<Customer>();
      var result = customers.OrderBy(c => c);
      var expected = customers.ToList().OrderBy(c => c.CustomerId);
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByExpressionTest()
    {
      IQueryable<Customer> customers = Session.Query.All<Customer>();
      List<string> original = customers.Select(c => c.LastName).ToList().Select(n => n.ToUpper()).ToList();
      original.Sort();
      Assert.That(customers, Is.Not.Empty);
      Assert.IsTrue(original.SequenceEqual(
        customers
          .OrderBy(c => c.LastName.ToUpper())
          .ToList()
          .Select(c => c.LastName.ToUpper())));
    }

    [Test]
    public void OrderByJoinTest()
    {
      var result =
        from c in Session.Query.All<Customer>().OrderBy(c => c.LastName)
        join i in Session.Query.All<Invoice>().OrderBy(i => i.InvoiceDate) on c equals i.Customer
        select new {c.LastName, i.InvoiceDate};
      var expected =
        from c in Customers.OrderBy(c => c.LastName)
        join i in Invoices.OrderBy(i => i.InvoiceDate) on c equals i.Customer
        select new {c.LastName, i.InvoiceDate};
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByJoin1Test()
    {
      var result = Session.Query.All<Customer>()
        .OrderBy(c => c.LastName)
        .Join(Session.Query.All<Invoice>()
            .Select(i => new {CustomerID = i.Customer.CustomerId, i.InvoiceDate})
            .Take(1000), c => c.CustomerId, x => x.CustomerID, (c, i) => new {c.LastName, i.InvoiceDate});
      var list = result.ToList();
      Assert.That(result, Is.Not.Empty);
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void OrderByPersistentPropertyTest()
    {
      IQueryable<Customer> customers = Session.Query.All<Customer>();
      List<string> original = customers.Select(c => c.LastName).ToList();
      original.Sort();
      Assert.That(customers, Is.Not.Empty);
      Assert.IsTrue(original.SequenceEqual(
        customers
          .OrderBy(c => c.LastName)
          .ToList()
          .Select(c => c.LastName)));
    }

    [Test]
    public void OrderBySelectAnonymousTest()
    {
      var result = Session.Query.All<Customer>()
        .OrderBy(c => c.CustomerId)
        .Select(c => new {c.Fax, c.Phone});
      var expected = Customers
        .OrderBy(c => c.CustomerId)
        .Select(c => new {c.Fax, c.Phone});
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderBySelectManyTest()
    {
      var result =
        from c in Session.Query.All<Customer>().OrderBy(c => c.LastName)
        from i in Session.Query.All<Invoice>().OrderBy(i => i.InvoiceDate)
        where c==i.Customer
        select new {c.LastName, i.InvoiceDate};
      var expected =
        from c in Customers.OrderBy(c => c.LastName)
        from i in Invoices.OrderBy(i => i.InvoiceDate)
        where c==i.Customer
        select new {c.LastName, i.InvoiceDate};
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderBySelectTest()
    {
      IQueryable<string> result = Session.Query.All<Customer>().OrderBy(c => c.CompanyName)
        .OrderBy(c => c.Address.Country).Select(c => c.Address.City);
      var expected = Customers.OrderBy(c => c.CompanyName)
        .OrderBy(c => c.Address.Country).Select(c => c.Address.City);
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void SequentialOrderByTest()
    {
      IQueryable<string> result = Session.Query.All<Customer>()
        .OrderBy(c => c.CompanyName)
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c)
        .Select(c => c);
      var expected = Customers
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c)
        .Select(c => c);
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void DistinctTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Invoice>().Select(i => i.DesignatedEmployee).Distinct();
      var expected = Invoices.Select(i => i.DesignatedEmployee).Distinct();
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(0, result.ToList().Except(expected).Count());
    }

    [Test]
    public void OrderByTakeSkipTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var original = Session.Query.All<Invoice>().ToList()
        .OrderBy(i => i.InvoiceDate)
        .Skip(100)
        .Take(50)
        .OrderBy(i => i.InvoiceDate)
        .Where(i => i.PaymentDate!=null)
        .Select(i => i.InvoiceDate)
        .Distinct()
        .Skip(10);
      var result = Session.Query.All<Invoice>()
        .OrderBy(i => i.InvoiceDate)
        .Skip(100)
        .Take(50)
        .OrderBy(i => i.InvoiceDate)
        .Where(i => i.PaymentDate!=null)
        .Select(i => i.InvoiceDate)
        .Distinct()
        .Skip(10);
      var originalList = original.ToList();
      var resultList = result.ToList();
      Assert.That(resultList, Is.Not.Empty);
      QueryDumper.Dump(originalList);
      QueryDumper.Dump(resultList);
      Assert.AreEqual(originalList.Count, resultList.Count);
      Assert.IsTrue(originalList.SequenceEqual(resultList));
    }

    [Test]
    public void PredicateTest()
    {
      IQueryable<int> result = Session.Query.All<Invoice>()
        .OrderBy(i => i.Commission > 0.5m)
        .ThenBy(i => i.InvoiceId).Select(i => i.InvoiceId);
      List<int> list = result.ToList();
      var queryNullDate = Session.Query.All<Invoice>().Where(i => i.PaymentDate==null).ToList();
      var invoice = queryNullDate[0];
      var dateTime = invoice.PaymentDate;
      Assert.IsFalse(dateTime.HasValue);
      var listNullDate = queryNullDate.Where(i => i.PaymentDate==null).ToList();
      Assert.That(listNullDate, Is.Not.Empty);
      Assert.AreEqual(queryNullDate.Count, listNullDate.Count);
      List<int> original = Session.Query.All<Invoice>().ToList()
        .OrderBy(i => i.Commission > 0.5m)
        .ThenBy(i => i.InvoiceId).Select(i => i.InvoiceId)
        .ToList();
      Assert.IsTrue(list.SequenceEqual(original));
    }

    [Test]
    public void SelectTest()
    {
      var result = Session.Query.All<Customer>().OrderBy(c => c.Phone)
        .Select(c => c.LastName);
      var expected = Customers.OrderBy(c => c.Phone)
        .Select(c => c.LastName);
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void ThenByTest()
    {
      var result = Session.Query.All<Customer>().OrderBy(c => c.SupportRep.EmployeeId)
        .ThenBy(c => c.Phone).Select(c => c.Address.City);
      var expected = Customers.OrderBy(c => c.SupportRep.EmployeeId)
        .ThenBy(c => c.Phone).Select(c => c.Address.City);

      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByBoolExpression()
    {
      var result = Session.Query.All<Invoice>().OrderBy(c => c.Status == (InvoiceStatus)1)
        .Select(c => c.Status)
        .ToArray();
      Assert.AreEqual(result.Last(), (InvoiceStatus)1);
    }
  }
}
