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
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;

namespace Xtensive.Orm.Tests.Storage.DiartisBugsTest
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
    [Association(PairTo = "Author", 
      OnTargetRemove = OnRemoveAction.Clear, 
      OnOwnerRemove  = OnRemoveAction.Cascade)]
    public EntitySet<Book> Books { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [TestFixture]
  public class DiartisBugsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      configuration.Sessions.Default.Options |= SessionOptions.AutoTransactionOpenMode;
      return configuration;
    }

    [Test]
    public void PersistOnAttachTest()
    {
      var ds = new DisconnectedState();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var author = new Author {Title = "Author"};
          using (ds.Attach(session))
          using (ds.Connect()) {
            Assert.IsFalse(author.IsRemoved());
          }
          // tx.Complete();
        }
      }
    }

    [Test]
    public void EntityRemoveTest()
    {
      var ds = new DisconnectedState();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var author = new Author {Title = "Author"};
          using (ds.Attach(session))
          using (ds.Connect()) {
            author.Remove();
            Assert.IsTrue(author.IsRemoved);
          }
          // tx.Complete();
        }
      }
    }

    [Test]
    public void SystemOperationRegistrationBugTest1()
    {
      var ds = new DisconnectedState();
      if (ds.OperationLogType!=OperationLogType.SystemOperationLog) {
        Assert.Ignore("SystemOperationLog type is required to run this test");
        return;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
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
        Assert.AreEqual(2, session.Query.All<Author>().Count());
        Assert.AreEqual(1, session.Query.All<Book>().Count());
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

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
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
        Assert.AreEqual(2, session.Query.All<Author>().Count());
        Assert.AreEqual(0, session.Query.All<Book>().Count());
        // tx.Complete();
      }
    }

    [Test]
    public void EntitySetCountBugTest_Regular()
    {
      using (var session = Domain.OpenSession()) {
        var author = new Author {Title = "Author"};
        using (var tx = session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var author = new Author {Title = "Author"};
          session.SaveChanges();
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
      using (var session = Domain.OpenSession()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var author = new Author {Title = "Author"};
          using (var tx = session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          using (var tx = session.OpenTransaction()) {
            var author = new Author {Title = "Author"};
            session.SaveChanges();
            var book = new Book {Title = "Book"};
            book.Author = author;
            Assert.AreEqual(1, author.Books.Count);
            // tx.Complete();
          }
        }
      }
    }
 
    [Test]
    public void EntitySetCountBugTest2()
    {
      using (var session = Domain.OpenSession()) {
        var author = new Author {Title = "Author"};
        var book1 = new Book {Title = "Book 1"};
        var book2 = new Book {Title = "Book 2"};
        book1.Author = author;
        book2.Author = author;
        try {
          using (var tx = session.OpenTransaction()) {
            bool firstTime = true;
            author.Books.CollectionChanged += (s, a) => {
              if (firstTime)
                Assert.AreEqual(1, author.Books.Count);
              firstTime = false;
            };
            book2.Author = null;
            Assert.AreEqual(1, author.Books.Count);
            // tx.Complete();
          }
        }
        finally {
          book1.Remove();
          book2.Remove();
          author.Remove();
        }
      }
    }
  }
}