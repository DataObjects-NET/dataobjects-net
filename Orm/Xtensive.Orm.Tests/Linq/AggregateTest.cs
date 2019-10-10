// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  [Category("Linq")]
  public class AggregateTest : ChinookDOModelTest
  {
    [Test]
    public void EntitySetWithGroupingAggregateTest()
    {
      var query =
        Session.Query.All<Customer>()
          .GroupBy(customer => customer.Address.City)
          .Select(grouping => grouping.Max(g => g.Invoices));

      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(query));
    }

    [Test]
    public void SingleAggregateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Invoice>()
        .Select(o => o.InvoiceLines.Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void DualAggregateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Invoice>()
        .Select(o => new {SUM = o.InvoiceLines.Count(), SUM2 = o.InvoiceLines.Count()});
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntityNotSupportedTest()
    {
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Invoice>().Max());
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Invoice>().Min());
    }

    [Test]
    public void IntAverageTest()
    {
      var avg = Session.Query.All<Invoice>().Average(o => o.InvoiceId);
      var expected = Invoices.Average(o => o.InvoiceId);
      if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.Firebird) {
        expected = Math.Truncate(expected);
      }
      Assert.AreEqual(expected, avg);
    }

    [Test]
    public void SumWithNoArgTest()
    {
      var result = Session.Query.All<Invoice>().Select(o => o.Commission).Sum();
      var expected = Invoices.Select(o => o.Commission).Sum();

      Assert.That(result.HasValue);
      Assert.That(expected.HasValue);

      var delta = Math.Abs(result.Value - expected.Value);

      // Add some tolerance
      Assert.That(delta, Is.LessThan(0.000000001m));
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var sum = Session.Query.All<Invoice>().Sum(o => o.InvoiceId);
      var expected = Session.Query.All<Invoice>().ToList().Sum(o => o.InvoiceId);
      Assert.AreEqual(expected, sum);
      Assert.Greater(sum, 0);
    }

    [Test]
    public void CountWithNoPredicateTest()
    {
      var count = Session.Query.All<Invoice>().Count();
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountWithPredicateTest()
    {
      var count = Session.Query.All<Invoice>().Count(o => o.InvoiceId > 10);
      var expected = Session.Query.All<Invoice>().ToList().Count(o => o.InvoiceId > 10);
      Assert.AreEqual(expected, count);
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountAfterFilterTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle | StorageProvider.Sqlite);

      var q = Session.Query;

      var result = q.All<Customer>().Where(c => q.All<Invoice>().Where(o => o.Customer==c).Count() > 6);
      var expected = Customers.Where(c => Invoices.Where(o => o.Customer==c).Count() > 6);

      var list = result.ToList();

      Assert.AreEqual(0, expected.Except(list).Count());
      QueryDumper.Dump(result);
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void WhereCountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => Session.Query.All<Invoice>().Count(o => o.Customer==c) > 5);
      var expected = Customers
        .Where(c => Invoices
          .Count(o => o.Customer==c) > 5);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereCountWithPredicateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Invoice>().Count(o => o.Customer==c) > 6
        select c;
      var expected =
        from c in Customers
        where Invoices
          .Count(o => o.Customer==c) > 6
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());

      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMaxWithSelectorTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .Max(o => o.InvoiceDate) < new DateTime(2019, 1, 1)
        select c;
      var expected =
        from c in Customers
        where c.Invoices.Any() && Invoices
          .Where(o => o.Customer==c)
          .Max(o => o.InvoiceDate) < new DateTime(2019, 1, 1)
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMinWithSelectorTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Invoice>()
                .Where(o => o.Customer==c)
                .Min(o => o.Commission) > 0.089m
        select c;
      var expected =
        from c in Customers
        where Invoices
                .Where(o => o.Customer==c)
                .Min(o => o.Commission) > 0.089m
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereAverageWithSelectorTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .Average(o => o.Commission) < 0.3m
        select c;
      var expected =
        from c in Customers
        where Invoices
          .Where(o => o.Customer==c)
          .Average(o => o.Commission) < 0.3m
        select c;
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectCountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>().Count())
        .ToList();
      var expected = Customers
        .Select(c => Invoices.Count());
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.Count, 0);
    }

    [Test]
    public void SelectAnonymousCountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        select new {
          Customer = c,
          NumberOfOrders = Session.Query.All<Invoice>()
            .Count(o => o.Customer==c)
        };
      var expected =
        from c in Customers
        select new {
          Customer = c,
          NumberOfOrders = Invoices
            .Count(o => o.Customer==c)
        };
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectMaxTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = from l in Session.Query.All<InvoiceLine>()
      select new {
        InvoiceLine = l,
        MaxID = Session.Query.All<Invoice>()
          .Where(s => s==l.Invoice)
          .Max(s => s.InvoiceId)
      };
      var expected = from l in InvoiceLines
      select new {
        InvoiceLine = l,
        MaxID = Invoices
          .Where(s => s==l.Invoice)
          .Max(s => s.InvoiceId)
      };
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SumCountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var expected = Session.Query.All<Invoice>().ToList().Count();
      var count = Session.Query.All<Customer>()
        .Sum(c => Session.Query.All<Invoice>().Count(o => o.Customer==c));
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void SumMinTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.Count > 0)
        .Sum(c => Session.Query.All<Invoice>().Where(o => o.Customer==c).Min(o => o.Commission));
      var expected = Customers
        .Where(c => c.Invoices.Count > 0)
        .Sum(c => Invoices.Where(o => o.Customer==c).Min(o => o.Commission));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void MaxCountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Max(c => Session.Query.All<Invoice>().Count(o => o.Customer==c));
      var expected = Customers
        .Max(c => Invoices.Count(o => o.Customer==c));
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void SelectNullableAggregateTest()
    {
      var result = Session.Query.All<Invoice>()
        .Select(o => (int?) o.InvoiceId)
        .Sum();
      var expected = Invoices
        .Select(o => (int?) o.InvoiceId)
        .Sum();
      Assert.AreEqual(expected, result);
    }
  }
}