// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.17

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class JoinTest : ChinookDOModelTest
  {
    [Test]
    public void JoinWrongKeysTest()
    {
      var result = 
        from m in Session.Query.All<MediaType>()
        join t in Session.Query.All<Track>() on m.Key equals t.Key // Wrong join
        select t;
      Assert.Throws<QueryTranslationException>(() => {
        var list = result.ToList();
      });
    }

    [Test]
    [Ignore("Fix later")]
    public void EntityJoinWithNullTest()
    {
      var id = Session.Query.All<Track>().First().TrackId;
      var result = Session.Query.All<Track>()
        .Select(t => t.TrackId==id ? null : t)
        .Select(t => t==null ? null : t.MediaType)
        .ToList();
      var expected = Session.Query.All<Track>().ToList()
        .Select(t => t.TrackId==id ? null : t)
        .Select(t => t==null ? null : t.MediaType).ToList();
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(result.Count, expected.Count);
    }

    [Test]
    public void EntityJoinWithNullModifiedTest()
    {
      var id = Session.Query.All<Track>().First().TrackId;
      var result = Session.Query.All<Track>()
        .Select(t=>(t.TrackId==id) && (t==null) ? null : 
          (t.TrackId==id) && (t!=null) ? t.MediaType /*exception*/ :
          (t.TrackId!=id) && (t==null) ? null : t.MediaType)
        .ToList();
      var expected = Session.Query.All<Track>().ToList()
        .Select(p=>p.TrackId==id ? null : p)
        .Select(p=>p==null ? null : p.MediaType)
        .ToList();
      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(result.Count, expected.Count);
    }


    [Test]
    public void GroupJoinAggregateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        join i in Session.Query.All<Invoice>() on c equals i.Customer into invoices
        join e in Session.Query.All<Employee>() on c.Address.City equals e.Address.City into emps
        select new {invoices = invoices.Count(), emps = emps.Count()};
      var list = result.ToList();
      var expected =
        Session.Query.All<Customer>().Select(c => new {
          invoices = (int) c.Invoices.Count,
          emps = Session.Query.All<Employee>().Where(e => c.Address.City==e.Address.City).Count()
        }).ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.IsTrue(expected.Except(list).Count()==0);
    }

    [Test]
    public void GroupJoinAggregate2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .GroupJoin(Session.Query.All<Invoice>(),
          customer => customer.CustomerId,
          invoice => invoice.Customer.CustomerId,
          (customer, invoices) => new {customer, invoices})
        .GroupJoin(Session.Query.All<Employee>(),
          customerInvoices => customerInvoices.customer.Address.City,
          employee => employee.Address.City,
          (customerInvoices, employees) => new {
            invoices = customerInvoices.invoices.Count(),
            emps = employees.Count(),
            sum = employees.Count() + customerInvoices.invoices.Count()
          }).OrderBy(t => t.emps).ThenBy(t => t.invoices).ThenBy(t => t.sum);

      var list = result.ToList();
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .GroupJoin(Session.Query.All<Invoice>().AsEnumerable(),
          customer => customer.CustomerId,
          invoice => invoice.Customer.CustomerId,
          (customer, invoices) => new {customer, invoices})
        .GroupJoin(Session.Query.All<Employee>().AsEnumerable(),
          customerInvoices => customerInvoices.customer.Address.City,
          employee => employee.Address.City,
          (customerInvoices, employees) => new {
            invoices = customerInvoices.invoices.Count(),
            emps = employees.Count(),
            sum = employees.Count() + customerInvoices.invoices.Count()
          }).OrderBy(t => t.emps).ThenBy(t => t.invoices).ThenBy(t => t.sum).ToList();

      Assert.That(list, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(list));

      QueryDumper.Dump(expected, true);
      QueryDumper.Dump(list, true);
    }

    [Test]
    public void SimpleJoinTest()
    {
      var trackCount = Session.Query.All<Track>().Count();
      var result =
        from track in Session.Query.All<Track>()
        join mediaType in Session.Query.All<MediaType>() on track.MediaType.MediaTypeId equals mediaType.MediaTypeId
        select new {track.Name, mediaTypeName=mediaType.Name, mediaType.MediaTypeId};
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(trackCount, list.Count);
    }

    [Test]
    public void SimpleLeftTest()
    {
      var traclCount = Session.Query.All<Track>().Count();
      var result = Session.Query.All<Track>()
        .LeftJoin(Session.Query.All<Album>(),
          track => track.Album.AlbumId,
          album => album.AlbumId,
          (track, album) => new {track.Name, album.Title, album.AlbumId});
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(traclCount, list.Count);
    }

    [Test]
    public void LeftJoin1Test()
    {
      Session.Query.All<Track>().First().Album = null;
      Session.Current.SaveChanges();
      var tracks = Session.Query.All<Track>();
      var albums = Session.Query.All<Album>();
      var result = tracks.LeftJoin(
        albums,
        track => track.Album,
        album => album,
        (track, album) => new {
          track.Name,
          Title = album==null ? null : album.Title
        });
      Assert.That(result, Is.Not.Empty);
      foreach (var item in result)
        Console.WriteLine("{0} {1}", item.Name, item.Title);
      QueryDumper.Dump(result);
    }

    public void LeftJoin2Test()
    {
      Session.Query.All<Track>().First().Album = null;
      Session.Current.SaveChanges();
      var tracks = Session.Query.All<Track>();
      var albums = Session.Query.All<Album>();
      var result = tracks.LeftJoin(
        albums,
        track => track.Album.AlbumId,
        album => album.AlbumId,
        (track, album) => new {track.Name, album.Title});
      Assert.That(result, Is.Not.Empty);
      foreach (var item in result)
        Console.WriteLine("{0} {1}", item.Name, item.Title);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SeveralTest()
    {
      var tracks = Session.Query.All<Track>();
      var tracksCount = tracks.Count();
      var albums = Session.Query.All<Album>();
      var mediaTypes = Session.Query.All<MediaType>();
      var result = from t in tracks
      join a in albums on t.Album.AlbumId equals a.AlbumId
      join m in mediaTypes on t.MediaType.MediaTypeId equals m.MediaTypeId
      select new {t, a, m.Name};
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(tracksCount, list.Count);
    }

    [Test]
    public void OneToManyTest()
    {
      var invoiceLines = Session.Query.All<InvoiceLine>();
      var invoiceLinesCount = invoiceLines.Count();
      var invoices = Session.Query.All<Invoice>();
      var result = from i in invoices
      join il in invoiceLines on i.InvoiceId equals il.Invoice.InvoiceId
      select new {il.Quantity, i.Total};
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(invoiceLinesCount, list.Count);
    }

    [Test]
    public void EntityJoinTest()
    {
      var result =
        from c in Session.Query.All<Customer>()
        join i in Session.Query.All<Invoice>() on c equals i.Customer
        select new {c.FirstName, i.InvoiceDate};
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
    }

    [Test]
    public void AnonymousEntityJoinTest()
    {
      var result =
        from c in Session.Query.All<Customer>()
        join i in Session.Query.All<Invoice>()
          on new {Customer = c, Name = c.FirstName} equals new {i.Customer, Name = i.Customer.FirstName}
        select new {c.FirstName, i.InvoiceDate};
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
    }

    [Test]
    public void JoinByCalculatedColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var localCustomers = customers.ToList();
      var expected =
        from c1 in localCustomers
        join c2 in localCustomers
          on c1.FirstName.Substring(0, 1).ToUpper() equals c2.FirstName.Substring(0, 1).ToUpper()
        select new {l = c1.FirstName, r = c2.FirstName};
      var result =
        from c1 in customers
        join c2 in customers
          on c1.FirstName.Substring(0, 1).ToUpper() equals c2.FirstName.Substring(0, 1).ToUpper()
        select new {l = c1.FirstName, r = c2.FirstName};
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(expected.Count(), list.Count());
    }

    [Test]
    public void GroupJoinTest()
    {
      var mediaTypeCount = Session.Query.All<MediaType>().Count();
      var result =
        from mediaType in Session.Query.All<MediaType>()
        join track in Session.Query.All<Track>()
          on mediaType equals track.MediaType
          into groups
        select groups;

      var expected =
        from mediaType in Session.Query.All<MediaType>().AsEnumerable()
        join track in Session.Query.All<Track>().AsEnumerable()
          on mediaType equals track.MediaType
          into groups
        select groups;
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(mediaTypeCount, list.Count);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void GroupJoinWithComparerTest()
    {
      var mediaTypes = Session.Query.All<MediaType>();
      var tracks = Session.Query.All<Track>();
      var result =
        mediaTypes.GroupJoin(
          tracks,
          m => m.MediaTypeId,
          t => t.TrackId,
          (m, tGroup) => tGroup,
          EqualityComparer<int>.Default);
      AssertEx.Throws<QueryTranslationException>(() => result.ToList());
    }

    [Test]
    public void GroupJoinNestedTest()
    {
      var mediaTypes = Session.Query.All<MediaType>();
      var tracks = Session.Query.All<Track>();
      var mediaTypesCount = mediaTypes.Count();
      var result =
        mediaTypes.OrderBy(c => c.Name)
          .GroupJoin(tracks, m => m, t => t.MediaType, (m, tGroup) => new {
            MediaType = m.Name,
            Tracks = tGroup.OrderBy(t => t.Name)
          });
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(mediaTypesCount, list.Count);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void GroupJoinSelectManyTest()
    {
      var mediaTypes = Session.Query.All<MediaType>();
      var tracks = Session.Query.All<Track>();
      var result = mediaTypes
          .OrderBy(c => c.Name)
          .GroupJoin(
            tracks,
            t => t,
            p => p.MediaType,
            (m, tGroup) => new {m, tGroup})
          .SelectMany(g1 => g1.tGroup, (g1, gp) => new {g1, gp})
          .OrderBy(g2 => g2.gp.Name)
          .Select(g2 => new {MediaType = g2.g1.m.Name, g2.gp.Name});
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void DefaultIfEmptyTest()
    {
      var mediaTypes = Session.Query.All<MediaType>();
      var tracks = Session.Query.All<Track>();
      var mediaTypeCount = mediaTypes.Count();
      var result = mediaTypes.GroupJoin(
        tracks,
        mediaType => mediaType,
        track => track.MediaType,
        (c, tGroup) => tGroup.DefaultIfEmpty());
      Assert.Throws<QueryTranslationException>(() => {
        var list = result.ToList();
        Assert.AreEqual(mediaTypeCount, list.Count);
        QueryDumper.Dump(result, true);
      });

    }

    [Test]
    public void LeftOuterTest()
    {
      var mediaTypes = Session.Query.All<MediaType>();
      var tracks = Session.Query.All<Track>();
      var tracksCount = tracks.Count();
      var result = mediaTypes.GroupJoin(
        tracks,
        m => m,
        t => t.MediaType,
        (m, tGroup) => new {m, tGroup})
        .SelectMany(g => g.tGroup.DefaultIfEmpty(), (g, t) => new {Name = t==null ? "Nothing!" : t.Name, MediaType = g.m.Name});
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(tracksCount, list.Count);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void GroupJoinAnonymousTest()
    {
      var query = Session.Query.All<Customer>()
        .GroupJoin(Session.Query.All<Invoice>(), c => c, i => i.Customer, (c, invoices) => new {
          c.FirstName,
          c.LastName,
          c.Phone,
          Invoices = invoices
        });
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
    }
  }
}