// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;

namespace Xtensive.Storage.Tests.Storage.DartisBugsTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {

    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public Author Author { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Author")]
    public EntitySet<Book> Books { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [TestFixture]
  public class DartisBugsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    [Test]
    public void SystemOperationRegistrationBugTest1()
    {
      var ds = new DisconnectedState();
      if (ds.OperationLogType!=OperationLogType.SystemOperationLog) {
        Assert.Ignore("SystemOperationLog type is required to run this test");
        return;
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var author = new Author {Title = "Author"};
          var book = new Book {Title = "Book"};
          Author author2 = null;
          author.Books.CollectionChanged += (sender, e) => {
            author2 = new Author();
          };
          book.Author = author;

          Assert.IsNotNull(author2);
          author2.Title = "Author 2";

          ds.ApplyChanges();
        }
        Assert.AreEqual(2, Query.All<Author>().Count());
        Assert.AreEqual(1, Query.All<Book>().Count());
        // tx.Complete();
      }
    }

    [Test]
    public void SystemOperationRegistrationBugTest2()
    {
      var ds = new DisconnectedState();
      if (ds.OperationLogType!=OperationLogType.SystemOperationLog) {
        Assert.Ignore("SystemOperationLog type is required to run this test");
        return;
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var author = new Author {Title = "Author"};
          var book = new Book {Title = "Book"};
          Author author2 = null;
          book.PropertyChanged += (sender, e) => {
            author2 = new Author();
          };
          book.Remove(); // Must raise "PersistenceState" property changed event

          Assert.IsNotNull(author2);
          author2.Title = "Author 2";

          ds.ApplyChanges();
        }
        Assert.AreEqual(2, Query.All<Author>().Count());
        Assert.AreEqual(0, Query.All<Book>().Count());
        // tx.Complete();
      }
    }

    [Test]
    public void EntitySetCountBugTest_Regular()
    {
      using (var session = Session.Open(Domain)) {
        var author = new Author {Title = "Author"};
        using (var tx = Transaction.Open()) {
          var book = new Book {Title = "Book"};
          book.Author = author;
          Assert.AreEqual(1, author.Books.Count);
          // tx.Complete();
        }
        author.Remove();
      }
    }

    [Test]
    public void EntitySetCountBugTest_RegularWithPersist()
    {
      using (var session = Session.Open(Domain)) {
        using (var tx = Transaction.Open()) {
          var author = new Author {Title = "Author"};
          session.Persist();
          var book = new Book {Title = "Book"};
          book.Author = author;
          Assert.AreEqual(1, author.Books.Count);
          // tx.Complete();
        }
      }
    }

    [Test]
    public void EntitySetCountBugTest_Disconnected()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var author = new Author {Title = "Author"};
          using (var tx = Transaction.Open()) {
            var book = new Book {Title = "Book"};
            book.Author = author;
            Assert.AreEqual(1, author.Books.Count);
            // tx.Complete();
          }
        }
      }
    }

    [Test]
    public void EntitySetCountBugTest_DisconnectedWithPersist()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          using (var tx = Transaction.Open()) {
            var author = new Author {Title = "Author"};
            session.Persist();
            var book = new Book {Title = "Book"};
            book.Author = author;
            Assert.AreEqual(1, author.Books.Count);
            // tx.Complete();
          }
        }
      }
    }
  }
}