// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class FirstSingleTest : ChinookDOModelTest
  {
    [Test]
    public void LengthTest()
    {
      var length = Session.Query.All<Customer>()
        .Select(customer => customer.FirstName)
        .FirstOrDefault()
        .Length;
    }

    [Test]
    public void Length2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customers = Session.Query.All<Customer>()
        .Where(cutomer =>
          cutomer
            .Invoices
            .Select(order => order.BillingAddress.City)
            .FirstOrDefault()
            .Length > 0);
      QueryDumper.Dump(customers);
    }

    [Test]
    public void Length3Test()
    {
      var customers = Session.Query.All<Customer>()
        .Where(cutomer =>
          cutomer
            .Invoices
            .Select(order => order.BillingAddress.City)
            .SingleOrDefault()
            .Length > 0);
      _ = Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(customers));
    }

    [Test]
    public void FirstTest()
    {
      var customer = Session.Query.All<Customer>().First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstPredicateTest()
    {
      var customer = Session.Query.All<Customer>().First(c => c.FirstName == "Luis");
      Assert.IsNotNull(customer);
    }

    [Test]
    public void WhereFirstTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.FirstName == "Luis").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      var customer = Session.Query.All<Customer>().FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstOrDefaultPredicateTest()
    {
      var customer = Session.Query.All<Customer>().FirstOrDefault(c => c.FirstName == "Luis");
      Assert.IsNotNull(customer);
      customer = Session.Query.All<Customer>().FirstOrDefault(c => c.FirstName == "Aaron");
      Assert.IsNotNull(customer);
      customer = Session.Query.All<Customer>().FirstOrDefault(c => c.FirstName == "ThereIsNoSuchFirstName");
      Assert.IsNull(customer);
    }

    [Test]
    public void WhereFirstOrDefaultTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.FirstName == "Luis").FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = Session.Query.All<Customer>().Where(c => c.FirstName == "Aaron").FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = Session.Query.All<Customer>().Where(c => c.FirstName == "ThereIsNoSuchFirstName").FirstOrDefault();
      Assert.IsNull(customer);
    }

    [Test]
    public void SingleTest()
    {
      AssertEx.ThrowsInvalidOperationException(() => Session.Query.All<Customer>().Single());
    }

    [Test]
    public void SinglePredicateTest()
    {
      var customer = Session.Query.All<Customer>().Single(c => c.FirstName == "Aaron");
      Assert.IsNotNull(customer);
      _ = Assert.Throws<InvalidOperationException>(() => Session.Query.All<Customer>().Where(c => c.FirstName == "Luis").Single());
      _ = Assert.Throws<InvalidOperationException>(() => Session.Query.All<Customer>().Where(c => c.FirstName == "ThereIsNoSuchFirstName").Single());
    }

    [Test]
    public void WhereSingleTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.FirstName == "Aaron").Single();
      Assert.IsNotNull(customer);
      _ = Assert.Throws<InvalidOperationException>(() => Session.Query.All<Customer>().Where(c => c.FirstName == "Luis").Single());
      _ = Assert.Throws<InvalidOperationException>(() => Session.Query.All<Customer>().Where(c => c.FirstName == "ThereIsNoSuchFirstName").Single());
    }

    [Test]
    public void SingleOrDefaultTest()
    {
      AssertEx.ThrowsInvalidOperationException(() => Session.Query.All<Customer>().SingleOrDefault());
    }

    [Test]
    public void SingleOrDefaultPredicateTest()
    {
      var customer = Session.Query.All<Customer>().SingleOrDefault(c => c.FirstName == "Aaron");
      Assert.IsNotNull(customer);
      _ = Assert.Throws<InvalidOperationException>(() => Session.Query.All<Customer>().SingleOrDefault(c => c.FirstName == "Luis"));
      customer = Session.Query.All<Customer>().SingleOrDefault(c => c.FirstName == "ThereIsNoSuchFirstName");
      Assert.IsNull(customer);
    }

    [Test]
    public void WhereSingleOrDefaultTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.FirstName == "Aaron").SingleOrDefault();
      Assert.IsNotNull(customer);
      _ = Assert.Throws<InvalidOperationException>(() => Session.Query.All<Customer>().Where(c => c.FirstName == "Luis").SingleOrDefault());
      customer = Session.Query.All<Customer>().Where(c => c.FirstName == "ThereIsNoSuchFirstName").SingleOrDefault();
      Assert.IsNull(customer);
    }

    [Test]
    public void SelectFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var invoices = Session.Query.All<Invoice>();
      var invoiceLines = Session.Query.All<InvoiceLine>();
      var result = from i in invoices
        select new {
          Invoice = i,
          MaxOrder = invoiceLines
            .Where(il => il.Invoice == i)
            .OrderByDescending(il => il.UnitPrice * il.Quantity)
            .First()
            .Invoice
        };
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      QueryDumper.Dump(list, true);
    }

    [Test]
    public void SubqueryFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count(c => c.Invoices.Count > 0);
      var result = Session.Query.All<Customer>().Where(c => c.Invoices.Count > 0).Select(c => c.Invoices.First());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
      QueryDumper.Dump(list, true);
    }

    [Test]
    public void SubqueryFirstExpectedExceptionTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>().Select(c => c.Invoices.First());
      AssertEx.ThrowsInvalidOperationException(() => result.ToList());
    }

    [Test]
    public void SubqueryFirstOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count();
      var result = Session.Query.All<Customer>().Select(c => c.Invoices.FirstOrDefault());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void SubquerySingleTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count(c => c.Invoices.Count > 0);
      var result = Session.Query.All<Customer>().Where(c => c.Invoices.Count > 0).Select(c => c.Invoices.Take(1).Single());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void SubquerySingleExpectedException1Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>().Select(c => c.Invoices.Take(1).Single());
      AssertEx.ThrowsInvalidOperationException(() => result.ToList());
    }

    [Test]
    public void SubquerySingleExpectedException2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var exceptionThrown = false;
      var result = Session.Query.All<Customer>().Where(c => c.Invoices.Count > 0).Select(c => c.Invoices.Single());
      try {
        _ = result.ToList();
      }
      catch {
        exceptionThrown = true;
      }
      if (!exceptionThrown) {
        Assert.Fail("Exception was not thrown.");
      }
    }

    [Test]
    public void SubquerySingleOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count();
      var result = Session.Query.All<Customer>().Select(c => c.Invoices.Take(1).SingleOrDefault());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void ComplexSubquerySingleOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var playlistCount = Session.Query.All<Playlist>().Count();
      var result = Session.Query.All<Playlist>().Select(
        p => new {
          Track = p.Tracks.Take(1).SingleOrDefault(),
          p.Tracks.Take(1).SingleOrDefault().Name,
          p.Tracks.Take(1).SingleOrDefault().Album
        });
      var list = result.ToList();
      Assert.AreEqual(playlistCount, list.Count);
    }

    [Test]
    public void ComplexSubquerySelectSingleOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var playlistCount = Session.Query.All<Playlist>().Count();
      var result = Session.Query.All<Playlist>().Select(c => c.Tracks.Take(1).SingleOrDefault()).Select(
        t => new {
          Track = t,
          t.Name,
          t.Album
        });
      var list = result.ToList();
      Assert.AreEqual(playlistCount, list.Count);
    }

    [Test]
    public void ComplexSubqueryFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var playlistCount = Session.Query.All<Playlist>().Count(p => p.Tracks.Any());
      var result = Session.Query.All<Playlist>().Where(p => p.Tracks.Any()).Select(
        p => new {
          Track = p.Tracks.First(),
          p.Tracks.First().Name,
          p.Tracks.First().Album
        });
      var list = result.ToList();
      Assert.AreEqual(playlistCount, list.Count);
    }

    [Test]
    public void ComplexSubquerySelectFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var playlistCount = Session.Query.All<Playlist>().Count(x => x.Tracks.Any());
      var result = Session.Query.All<Playlist>()
        .Where(p => p.Tracks.Any())
        .Select(p => p.Tracks.First())
        .Select(t => new { Track = t, t.Name, t.Album });
      var list = result.ToList();
      Assert.AreEqual(playlistCount, list.Count);
    }
  }
}