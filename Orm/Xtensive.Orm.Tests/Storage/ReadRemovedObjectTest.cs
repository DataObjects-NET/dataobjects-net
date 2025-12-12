// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Storage.ReadRemovedObjectTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }
  }

  [Serializable]
  public class CoolBook : Book
  {
    [Field]
    public double Popularity { get; set; }

    [Field(LazyLoad = true)]
    public double LazyPopularity { get; set; }

    [Field]
    public EntitySet<Book> OtherCoolBooks { get; private set; }
  }

  [TestFixture]
  public class ReadRemovedObjectTest : AutoBuildTest
  {
    private Key key;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var book = new CoolBook { Title = "Book" };
        var otherBook = new Book {Title = "OtherBook" };
        book.OtherCoolBooks.Add(otherBook);

        key = book.Key;
        tx.Complete();
      }
    }

    [Test]
    public void StandardModeTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var book = (CoolBook) Query.All<Book>().Where(b => b.Key == key).Single();
        var otherCoolBooksBackup = book.OtherCoolBooks;
        book.Remove();

        Assert.That(book.Key, Is.EqualTo(key));
        Assert.That(book.TypeInfo, Is.EqualTo(key.TypeInfo));
        AssertEx.ThrowsInvalidOperationException(
          () => { var id = book.Id; });
        AssertEx.ThrowsInvalidOperationException(
          () => { var title = book.Title; });
        AssertEx.ThrowsInvalidOperationException(
          () => { var otherCoolBoks = book.OtherCoolBooks; });
        Assert.That(otherCoolBooksBackup.Count, Is.EqualTo(0));
        Assert.That(otherCoolBooksBackup.AsEnumerable().FirstOrDefault(), Is.EqualTo(null));

        // tx.Complete();
      }
    }

    [Test]
    public void ReadRemovedObjectsModeTest()
    {
      var sc = Domain.Configuration.Sessions.Default.Clone();
      sc.Options |= SessionOptions.ReadRemovedObjects;

      using (var session = Domain.OpenSession(sc))
      using (var tx = session.OpenTransaction()) {
        var book = (CoolBook) Query.All<Book>().Where(b => b.Key == key).Single();
        book.Remove();

        Assert.That(book.Id, Is.EqualTo(key.Value.GetValueOrDefault(0)));
        Assert.That(book.Title, Is.EqualTo("Book"));
        Assert.That(book.OtherCoolBooks.Count, Is.EqualTo(0));
        Assert.That(book.OtherCoolBooks.AsEnumerable().FirstOrDefault(), Is.EqualTo(null));
        AssertEx.ThrowsInvalidOperationException(
          () => { var p = book.Popularity; });
        AssertEx.ThrowsInvalidOperationException(
          () => { var p = book.LazyPopularity; });

        // tx.Complete();
      }

      using (var session = Domain.OpenSession(sc))
      using (var tx = session.OpenTransaction()) {
        var book = Query.All<CoolBook>().Where(b => b.Key == key).Single();
        book.Remove();

        Assert.That(book.Popularity, Is.EqualTo(0.0));
        AssertEx.ThrowsInvalidOperationException(
          () => { var p = book.LazyPopularity; });

        // tx.Complete();
      }

      using (var session = Domain.OpenSession(sc))
      using (var tx = session.OpenTransaction()) {
        var book = Query.All<CoolBook>().Where(b => b.Key == key).Single();
        var lazyPopulatiry = book.LazyPopularity;
        book.Remove();

        Assert.That(book.Popularity, Is.EqualTo(0.0));
        Assert.That(book.LazyPopularity, Is.EqualTo(0.0));

        // tx.Complete();
      }
    }

    [Test]
    public void UnexpectedLazyPropertyReadBugTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var book = Query.All<CoolBook>().Where(b => b.Key == key).Single();
        var psa = DirectStateAccessor.Get(book);

        Assert.That(0 == (psa.GetFieldState("LazyPopularity") & PersistentFieldState.Loaded), Is.True);

        book.OtherCoolBooks.Add(book); // (1)
        Assert.That(0 == (psa.GetFieldState("LazyPopularity") & PersistentFieldState.Loaded), Is.True);

        session.SaveChanges();
        book.Remove(); // Fails, but only if (1) is enabled!
        Assert.That(0 == (psa.GetFieldState("LazyPopularity") & PersistentFieldState.Loaded), Is.True);
      }
    }
  }
}