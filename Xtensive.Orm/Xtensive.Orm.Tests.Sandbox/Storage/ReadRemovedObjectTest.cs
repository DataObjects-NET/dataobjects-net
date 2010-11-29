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
using Xtensive.Testing;
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
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    protected override void PupulateData()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
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
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var book = (CoolBook) Query.All<Book>().Where(b => b.Key == key).Single();
        var otherCoolBooksBackup = book.OtherCoolBooks;
        book.Remove();

        Assert.AreEqual(key, book.Key);
        Assert.AreEqual(key.Type, book.TypeInfo);
        AssertEx.ThrowsInvalidOperationException(
          () => { var id = book.Id; });
        AssertEx.ThrowsInvalidOperationException(
          () => { var title = book.Title; });
        AssertEx.ThrowsInvalidOperationException(
          () => { var otherCoolBoks = book.OtherCoolBooks; });
        Assert.AreEqual(0, otherCoolBooksBackup.Count);
        Assert.AreEqual(null, otherCoolBooksBackup.AsEnumerable().FirstOrDefault());

        // tx.Complete();
      }
    }

    [Test]
    public void ReadRemovedObjectsModeTest()
    {
      var sc = Domain.Configuration.Sessions.Default.Clone();
      sc.Options |= SessionOptions.ReadRemovedObjects;

      using (var session = Session.Open(Domain, sc))
      using (var tx = Transaction.Open()) {
        var book = (CoolBook) Query.All<Book>().Where(b => b.Key == key).Single();
        book.Remove();

        Assert.AreEqual(key.Value.GetValueOrDefault(0), book.Id);
        Assert.AreEqual("Book", book.Title);
        Assert.AreEqual(0, book.OtherCoolBooks.Count);
        Assert.AreEqual(null, book.OtherCoolBooks.AsEnumerable().FirstOrDefault());
        AssertEx.ThrowsInvalidOperationException(
          () => { var p = book.Popularity; });
        AssertEx.ThrowsInvalidOperationException(
          () => { var p = book.LazyPopularity; });

        // tx.Complete();
      }

      using (var session = Session.Open(Domain, sc))
      using (var tx = Transaction.Open()) {
        var book = Query.All<CoolBook>().Where(b => b.Key == key).Single();
        book.Remove();

        Assert.AreEqual(0.0, book.Popularity);
        AssertEx.ThrowsInvalidOperationException(
          () => { var p = book.LazyPopularity; });

        // tx.Complete();
      }

      using (var session = Session.Open(Domain, sc))
      using (var tx = Transaction.Open()) {
        var book = Query.All<CoolBook>().Where(b => b.Key == key).Single();
        var lazyPopulatiry = book.LazyPopularity;
        book.Remove();
        
        Assert.AreEqual(0.0, book.Popularity);
        Assert.AreEqual(0.0, book.LazyPopularity);

        // tx.Complete();
      }
    }

    [Test]
    public void UnexpectedLazyPropertyReadBugTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var book = Query.All<CoolBook>().Where(b => b.Key == key).Single();
        var psa = DirectStateAccessor.Get(book);

        Assert.IsTrue(0 == (psa.GetFieldState("LazyPopularity") & PersistentFieldState.Loaded));

        book.OtherCoolBooks.Add(book); // (1)
        Assert.IsTrue(0 == (psa.GetFieldState("LazyPopularity") & PersistentFieldState.Loaded));

        book.Remove(); // Fails, but only if (1) is enabled!
        Assert.IsTrue(0 == (psa.GetFieldState("LazyPopularity") & PersistentFieldState.Loaded));
      }
    }
  }
}