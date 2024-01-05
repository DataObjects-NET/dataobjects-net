// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class EntitySetTest : ChinookDOModelTest
  {
    [Test]
    public void EntitySetAnonymousTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new { InvoicesFiled = c.Invoices });
      var expected = Customers
        .Select(c => new { InvoicesFiled = c.Invoices });
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntitySetSelectManyAnonymousTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new { InvoicesFiled = c.Invoices })
        .SelectMany(i => i.InvoicesFiled);
      var expected = Customers
        .Select(c => new { InvoicesFiled = c.Invoices })
        .SelectMany(i => i.InvoicesFiled);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntitySetSelectTest()
    {
      var result = Session.Query.All<Customer>().OrderBy(c=>c.CustomerId).Select(c => c.Invoices).ToList();
      var expected = Customers.OrderBy(c=>c.CustomerId).Select(c => c.Invoices).ToList();
      Assert.Greater(result.Count, 0);
      Assert.AreEqual(expected.Count, result.Count);
      for (var i = 0; i < result.Count; i++) {
        Assert.AreSame(expected[i], result[i]);
      }
    }

    [Test]
    public void QueryTest()
    {
      var customer = GetCustomer();
      var expected = customer
        .Invoices
        .AsEnumerable()
        .OrderBy(i => i.InvoiceId)
        .Select(i => i.InvoiceId)
        .ToList();
      var actual = customer
        .Invoices
        .OrderBy(i => i.InvoiceId)
        .Select(i => i.InvoiceId)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void UnsupportedMethodsTest()
    {
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Customer>().Where(c => c.Invoices.Add(null)).ToList());
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Customer>().Where(c => c.Invoices.Remove(null)).ToList());
    }

    [Test]
    public void CountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var expected = Invoices.Count();
      var count = Session.Query.All<Customer>()
        .Select(c => c.Invoices.Count)
        .ToList()
        .Sum();
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void ContainsTest()
    {
      var bestInvoice = Session.Query.All<Invoice>()
        .OrderBy(i => i.Commission)
        .First();
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.Contains(bestInvoice));
      Assert.AreEqual(bestInvoice.Customer.CustomerId, result.ToList().Single().CustomerId);
    }

    [Test]
    public void OuterEntitySetTest()
    {
      var customer = GetCustomer();
      var result = Session.Query.All<Invoice>().Where(i => customer.Invoices.Contains(i));
      Assert.AreEqual(customer.Invoices.Count, result.ToList().Count);
    }

    [Test]
    public void JoinWithEntitySetTest()
    {
      var customer = GetCustomer();
      var result =
        from i in customer.Invoices
        join e in Session.Query.All<Employee>() on i.DesignatedEmployee equals e
        select e;
      Assert.AreEqual(customer.Invoices.Count, result.ToList().Count);
    }

    private Customer GetCustomer()
    {
      return Session.Query.All<Customer>().Where(c => c.FirstName == "Aaron").Single();
    }
  }
}