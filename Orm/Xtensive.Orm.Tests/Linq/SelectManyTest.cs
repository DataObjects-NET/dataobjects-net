// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SelectManyTest : ChinookDOModelTest
  {
    [Test]
    public void GroupJoinTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupJoin(Session.Query.All<Customer>(),
          i => i.Customer,
          c => c,
          (i, ic) => new {i, ic})
        .SelectMany(@t => @t.ic.DefaultIfEmpty(),
          (@t, x) => new {
            CustomerId = x.CustomerId,
            Phone = x.Phone,
            Country = x.Address.Country
          })
          ;
      var expected = Invoices
        .GroupJoin(Customers,
          i => i.Customer,
          c => c,
          (i, ic) => new {i, ic})
        .SelectMany(@t => @t.ic.DefaultIfEmpty(),
          (@t, x) => new {
            CustomerId = x.CustomerId,
            Phone = x.Phone,
            Country = x.Address.Country
          }).OrderBy(t => t.Phone).ThenBy(t => t.Country).ThenBy(t => t.CustomerId);

      var list = result.ToList().OrderBy(t => t.Phone).ThenBy(t => t.Country).ThenBy(t => t.CustomerId).ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(list));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .SelectMany(g => g);
      var list = result.ToList();
      var expected = Invoices;
      Assert.That(list, Is.Not.Empty);
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    [Mute]
    public void GroupBy2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .SelectMany(g => g, (grouping, invoice)=>new {Count = grouping.Count(), Invoice = invoice});
      var expected = Invoices
        .GroupBy(i => i.Customer)
        .SelectMany(g => g, (grouping, invoice)=>new {Count = grouping.Count(), Invoice = invoice});
      var list = result.ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.IsTrue(expected.Except(list).IsNullOrEmpty());
    }

    [Test]
    public void GroupBySelectorTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .SelectMany(g => g.Select(i => i.Customer));
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      var expected = Invoices.Select(i => i.Customer);
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void GroupByCountTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .SelectMany(g => g.Select(i => i.Customer).Where(c => g.Count() > 2));
      var list = result.ToList();
      var expected = Invoices
        .GroupBy(i => i.Customer)
        .SelectMany(g => g.Select(i => i.Customer).Where(c => g.Count() > 2));

      Assert.That(list, Is.Not.Empty);
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void GroupByCount2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Where(g => g.Count() > 2)
        .SelectMany(g => g.Select(i => i.Customer));
      var list = result.ToList();
      var expected = Invoices
        .GroupBy(i => i.Customer)
        .SelectMany(g => g.Select(i => i.Customer).Where(c => g.Count() > 2));

      Assert.That(list, Is.Not.Empty);
      Assert.IsTrue(list.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void ParameterTest()
    {
      var expectedCount = Invoices.Count();
      var result = Session.Query.All<Customer>()
        .SelectMany(i => i.Invoices.Select(t => i));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expectedCount, result.Count());
      foreach (var customer in result)
        Assert.IsNotNull(customer);
    }

    [Test]
    public void EntitySetDefaultIfEmptyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var expectedCount =
        Session.Query.All<Invoice>().Count() + Session.Query.All<Customer>().Count(c => !Session.Query.All<Invoice>().Any(i => i.Customer==c));
      IQueryable<Invoice> result = Session.Query.All<Customer>().SelectMany(c => c.Invoices.DefaultIfEmpty());

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expectedCount, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      int expectedCount = Session.Query.All<Invoice>().Count(i => i.DesignatedEmployee.FirstName.StartsWith("A"));
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => c.Invoices.Where(i => i.DesignatedEmployee.FirstName.StartsWith("A")));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expectedCount, result.ToList().Count);
    }

    [Test]
    public void EntitySetSubqueryWithResultSelectorTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var expected = Session.Query.All<Invoice>()
        .Count(i => i.DesignatedEmployee.FirstName.StartsWith("A"));

      IQueryable<DateTime?> result = Session.Query.All<Customer>()
        .SelectMany(c => c.Invoices.Where(i => i.DesignatedEmployee.FirstName.StartsWith("A")), (c, i) => i.PaymentDate);

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetTest()
    {
      int expected = Invoices.Count();
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => c.Invoices);

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void EntitySetWithCastTest()
    {
      var result = Session.Query.All<Customer>().SelectMany(c => (IEnumerable<Invoice>) c.Invoices).ToList();
      var expected = Invoices;

      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(result.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void SelectManyWithCastTest()
    {
      var result = Session.Query.All<Customer>().SelectMany(c => (IEnumerable<Invoice>) Session.Query.All<Invoice>().Where(i => i.Customer==c)).ToList();
      var expected = Invoices;

      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(result.Except(expected).IsNullOrEmpty());
    }

    [Test]
    public void InnerJoinTest()
    {
      var invoicesCount = Session.Query.All<Invoice>().Count();
      var result = from c in Session.Query.All<Customer>()
      from i in Session.Query.All<Invoice>().Where(i => i.Customer==c)
      select new {c.FirstName, i.PaymentDate};
      var list = result.ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(invoicesCount, list.Count);
    }

    [Test]
    public void NestedTest()
    {
      IQueryable<Track> tracks = Session.Query.All<Track>();
      var tracksCount = tracks.Count();
      IQueryable<Album> albums = Session.Query.All<Album>();
      IQueryable<MediaType> mediaTypes = Session.Query.All<MediaType>();
      var result = from t in tracks
      from a in albums
      from c in mediaTypes
      where t.Album==a && t.MediaType==c
      select new {t, a, c.Name};
      var list = result.ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(tracksCount, list.Count);
    }

    [Test]
    public void OuterJoinAnonymousTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var assertCount =
        Session.Query.All<Invoice>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Invoice>().Any(i => i.Customer==c));
      var result = from c in Session.Query.All<Customer>()
      from i in Session.Query.All<Invoice>().Where(i => i.Customer==c).Select(i => new {i.InvoiceId, c.Phone}).DefaultIfEmpty()
      select new {c.LastName, i};
      var list = result.ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(assertCount, list.Count);
      foreach (var item in result) {
        Assert.IsNotNull(item);
        Assert.IsNotNull(item.LastName);
        Assert.IsNotNull(item.i);
      }
      QueryDumper.Dump(list);
    }

    [Test]
    public void OuterJoinAnonymousFieldTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var assertCount =
        Session.Query.All<Invoice>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Invoice>().Any(i => i.Customer==c));
      var result = from c in Session.Query.All<Customer>()
      from i in Session.Query.All<Invoice>().Where(i => i.Customer==c).Select(i => new {i.InvoiceId, c.Phone}).DefaultIfEmpty()
      select new {c.LastName, i.InvoiceId};
      var list = result.ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }

    [Test]
//    [ExpectedException(typeof (NullReferenceException))]
    public void OuterJoinEntityTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var assertCount =
        Session.Query.All<Invoice>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Invoice>().Any(i => i.Customer==c));
      var result = 
        from c in Session.Query.All<Customer>()
        from i in Session.Query.All<Invoice>().Where(i => i.Customer==c).DefaultIfEmpty()
        select new {c.LastName, i.PaymentDate};
      var list = result.ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }

    [Test]
    public void OuterJoinValueTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var assertCount =
        Session.Query.All<Invoice>().Count() +
          Session.Query.All<Customer>().Count(c => !Session.Query.All<Invoice>().Any(i => i.Customer==c));
      var result = from c in Session.Query.All<Customer>()
      from i in Session.Query.All<Invoice>().Where(i => i.Customer==c).Select(i => i.PaymentDate).DefaultIfEmpty()
      select new {c.LastName, i};
      var list = result.ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(assertCount, list.Count);
      QueryDumper.Dump(list);
    }


    [Test]
    public void SelectManyAfterSelect1Test()
    {
      IQueryable<string> result = Session.Query.All<Track>()
        .Select(t => t.Playlists.Select(p => p.Name))
        .SelectMany(p => p);

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectManyAfterSelect2Test()
    {
      int expected = Invoices.Count();
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .Select(c => Session.Query.All<Invoice>().Where(i => i.Customer==c)).SelectMany(i => i);

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SimpleTest()
    {
      int expected = Invoices.Count();
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Invoice>().Where(i => i.Customer==c));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SimpleWithResultSelectorTest()
    {
      var expected = Invoices.Count();
      var result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Invoice>().Where(i => i.Customer==c), (c, i) => new {c, i});

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SubqueryWithEntityReferenceTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      int expected = Session.Query.All<Invoice>().Count(i => i.DesignatedEmployee.FirstName.StartsWith("A"));
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Invoice>().Where(i => i.Customer==c)
          .Where(i => i.DesignatedEmployee.FirstName.StartsWith("A")));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void SelectManySelfTest()
    {
      var result =
        from c1 in Session.Query.All<Customer>()
        from c2 in Session.Query.All<Customer>()
        where c1.Address.City==c2.Address.City
        select new {c1, c2};

      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
    }

    [Test]
    public void IntersectBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      Require.ProviderIsNot(StorageProvider.Firebird);

      var expected = Session.Query.All<Invoice>().Count(i => i.DesignatedEmployee.FirstName.StartsWith("A"));
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Invoice>().Where(i => i.Customer==c).Intersect(Session.Query.All<Invoice>())
          .Where(i => i.DesignatedEmployee.FirstName.StartsWith("A")));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void DistinctBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var expected = Session.Query.All<Invoice>().Distinct().Count(i => i.DesignatedEmployee.FirstName.StartsWith("A"));
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Invoice>().Where(i => i.Customer==c).Distinct()
          .Where(i => i.DesignatedEmployee.FirstName.StartsWith("A")));
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(expected, result.ToList().Count);
    }

    [Test]
    public void TakeBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Invoice>().Where(i => i.Customer==c).Take(5));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SkipBetweenFilterAndApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      IQueryable<Invoice> result = Session.Query.All<Customer>()
        .SelectMany(c => Session.Query.All<Invoice>().Where(i => i.Customer==c).Skip(5));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void CalculateWithApply()
    {
      var actual = from c in Session.Query.All<Customer>()
        from i in (c.Invoices.Select(x => c.FirstName + x.BillingAddress.StreetAddress).Where(x => x.StartsWith("M")))
        orderby i
        select i;

      var expected = from c in Customers
        from i in (c.Invoices.Select(x => c.FirstName + x.BillingAddress.StreetAddress).Where(x => x.StartsWith("M")))
        orderby i
        select i;

      Assert.That(actual, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void TwoCalculateWithApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);

      var actual = from c in Session.Query.All<Customer>()
        from n in (c.Invoices.Select(i => c.FirstName + i.BillingAddress.StreetAddress).Where(x => x.StartsWith("M")))
          .Union(c.Invoices.Select(i => c.FirstName + i.BillingAddress.StreetAddress).Where(x => x.StartsWith("N")))
        orderby n
        select n;

      var expected = from c in Customers
        from n in (c.Invoices.Select(i => c.FirstName + i.BillingAddress.StreetAddress).Where(x => x.StartsWith("M")))
          .Union(c.Invoices.Select(i => c.FirstName + i.BillingAddress.StreetAddress).Where(x => x.StartsWith("N")))
        orderby n
        select n;

      Assert.That(actual, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void TwoFilterWithApplyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      Require.ProviderIsNot(StorageProvider.Firebird);

      var actual = from c in Session.Query.All<Customer>()
        from i in (c.Invoices.Where(x => x.BillingAddress.StreetAddress.StartsWith("A"))
          .Intersect(c.Invoices.Where(x => x.BillingAddress.StreetAddress.StartsWith("A"))))
        orderby i.InvoiceId
        select i.InvoiceId;
      var expected = from c in Customers
        from i in (c.Invoices.Where(x => x.BillingAddress.StreetAddress.StartsWith("A")).Intersect(c.Invoices))
        orderby i.InvoiceId
        select i.InvoiceId;

      Assert.That(actual, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void TwoSelectManyTest()
    {
      var query =
        from i in Session.Query.All<Invoice>().Take(10)
        from il in Session.Query.All<InvoiceLine>().Take(10)
        select new {InvoiceId = i.InvoiceId, il.UnitPrice};

      Assert.That(query, Is.Not.Empty);
      var count = query.Count();
      TestLog.Info("Records count: {0}", count);
      QueryDumper.Dump(query);
    }
  }
}