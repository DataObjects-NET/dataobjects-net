// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.DisconnectedStateTest2
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
    public BookRef BookRef { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class GuidBook : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  public class BookRef : Structure
  {
    [Field]
    public Book Book { get; set; }
  }

  [TestFixture]
  public class DisconnectedStateTest2 : AutoBuildTest
  {
    private const string NewBookTitle = "New Book";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    protected override Domain  BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        new Book() { Title = "Book" };
        tx.Complete();
      }
      return domain;
    }

    [Test]
    public void ConnectInsideTransactionTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = Query.All<Book>().SingleOrDefault();
          var book2 = new Book() { Title = "Book2" };
          Assert.IsNotNull(book);
          Assert.IsNotNull(book2);
          ds.ApplyChanges();
        }
        Assert.AreEqual(2, Query.All<Book>().Count());
        // tx.Complete();
      }
    }

    [Test]
    public void StructureFieldSetTest()
    {
      var ds = new DisconnectedState();
      Book book = null;
      Book book2 = null;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          book = Query.All<Book>().SingleOrDefault();
          book2 = new Book() { Title = "Book2" };
          book.BookRef.Book = book2;
          Assert.AreEqual(book2, book.BookRef.Book);

          var bookRef = new BookRef();
          bookRef.Book = book;
          book.BookRef = bookRef;
          Assert.AreEqual(book, book.BookRef.Book);
          ds.ApplyChanges();
        }
        Assert.AreEqual(2, Query.All<Book>().Count());
        Assert.AreEqual(book, book.BookRef.Book);
        // tx.Complete();
      }
    }

    [Test]
    public void PersistenceStateAndAllMethodTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (ds.Attach(session))
      using (ds.Connect())
      using (var tx = Transaction.Open()) {
        Key bookKey1;
        Key bookKey2;
        using (var tx2 = Transaction.Open(TransactionOpenMode.New)) {
          var book1 = Query.All<Book>().SingleOrDefault();
          bookKey1 = book1.Key;
          var book2 = new Book() {Title = "Book2"};
          bookKey2 = book2.Key;
          session.Persist(); // Necessary to flush the changes to DisconnectedState

          Assert.AreEqual(1, ds.Versions.Count);
          Assert.AreEqual(2, ds.AllPersistenceStates().Count());
          Assert.AreEqual(2, ds.All<Book>().Count());
          Assert.AreEqual(PersistenceState.Synchronized, ds.GetPersistenceState(bookKey1));
          Assert.AreEqual(PersistenceState.New, ds.GetPersistenceState(bookKey2));

          book1.Title = book1.Title + "_";
          book2.Title = book2.Title + "_";
          session.Persist(); // Necessary to flush the changes to DisconnectedState

          Assert.AreEqual(PersistenceState.Modified, ds.GetPersistenceState(bookKey1));
          Assert.AreEqual(PersistenceState.New, ds.GetPersistenceState(bookKey2));
          // tx2.Complete(); // Rollback
        }

        Assert.IsNull(Query.SingleOrDefault(bookKey2));
        Assert.AreEqual(1, ds.Versions.Count);
        Assert.AreEqual(1, ds.AllPersistenceStates().Count());
        Assert.AreEqual(1, ds.All<Book>().Count());
        Assert.AreEqual(PersistenceState.Synchronized, ds.GetPersistenceState(bookKey1));
        Assert.AreEqual(null, ds.GetPersistenceState(bookKey2));
        // tx.Complete();
      }
    }

    [Test]
    public void SequentialApplyChangesTest_NewEntity()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = new Book() {Title = NewBookTitle};
          session.Persist();
          book.Title += " Changed";
          ds.ApplyChanges();
          book.Title += " Changed";
          ds.ApplyChanges();
        }
        // tx.Complete();
      }
    }

    [Test]
    public void SequentialApplyChangesTest_ExistingEntity()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = Query.All<Book>().First();
          book.Title += " Changed";
          ds.ApplyChanges();
          book.Title += " Changed";
          ds.ApplyChanges();
        }
        // tx.Complete();
      }
    }

    [Test]
    public void NewEntityRemapTest1()
    {
      var ds = new DisconnectedState();
      Book book;
      Key bookKey;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session)) {
          using (ds.Connect())
          using (var tx2 = Transaction.Open(TransactionOpenMode.New)) {
            book = new Book() {Title = NewBookTitle};
            bookKey = book.Key;
            tx2.Complete();
          }
          var mapping = ds.ApplyChanges();
          bookKey = mapping.TryRemapKey(bookKey);
        }
        Assert.AreEqual(bookKey, book.Key);
        Assert.AreEqual(book.Title, NewBookTitle);
        Assert.IsFalse(book.IsRemoved);
        Assert.AreSame(book, Query.Single(bookKey));
        // tx.Complete();
      }
    }

    [Test]
    public void NewEntityRemapTest2()
    {
      var ds = new DisconnectedState();
      Book book;
      Key bookKey;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session)) {
          using (ds.Connect()) {
            book = new Book() {Title = NewBookTitle};
            bookKey = book.Key;

            var mapping = ds.ApplyChanges();
            bookKey = mapping.TryRemapKey(bookKey);

            Assert.AreEqual(bookKey, book.Key);
            Assert.AreEqual(book.Title, NewBookTitle);
            Assert.IsFalse(book.IsRemoved);
            Assert.AreSame(book, Query.Single(bookKey));
          }
        }
        // tx.Complete();
      }
    }

    [Test]
    public void NewEntityRemapTest3()
    {
      var ds = new DisconnectedState();
      Book book;
      Key bookKey;
      using (var session = Session.Open(Domain)) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          book = new Book() {Title = NewBookTitle};
          bookKey = book.Key;

          var mapping = ds.ApplyChanges();
          bookKey = mapping.TryRemapKey(bookKey);

          Assert.AreEqual(bookKey, book.Key);
          Assert.AreEqual(book.Title, NewBookTitle);
          Assert.IsFalse(book.IsRemoved);
          Assert.AreSame(book, Query.Single(bookKey));
        }
        book.Remove();
      }
    }


    [Test]
    public void NewGuidEntityRemapTest1()
    {
      var ds = new DisconnectedState();
      GuidBook book;
      Key bookKey;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session)) {
          using (ds.Connect())
          using (var tx2 = Transaction.Open(TransactionOpenMode.New)) {
            book = new GuidBook() {Title = NewBookTitle};
            bookKey = book.Key;
            tx2.Complete();
          }
          var mapping = ds.ApplyChanges();
          bookKey = mapping.TryRemapKey(bookKey);
        }
        Assert.AreEqual(bookKey, book.Key);
        Assert.AreEqual(book.Title, NewBookTitle);
        Assert.IsFalse(book.IsRemoved);
        Assert.AreSame(book, Query.Single(bookKey));
        // tx.Complete();
      }
    }

    [Test]
    public void NewGuidEntityRemapTest2()
    {
      var ds = new DisconnectedState();
      GuidBook book;
      Key bookKey;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session)) {
          using (ds.Connect()) {
            book = new GuidBook() {Title = NewBookTitle};
            bookKey = book.Key;

            var mapping = ds.ApplyChanges();
            bookKey = mapping.TryRemapKey(bookKey);

            Assert.AreEqual(bookKey, book.Key);
            Assert.AreEqual(book.Title, NewBookTitle);
            Assert.IsFalse(book.IsRemoved);
            Assert.AreSame(book, Query.Single(bookKey));
          }
        }
        // tx.Complete();
      }
    }

    [Test]
    public void NewGuidEntityRemapTest3()
    {
      var ds = new DisconnectedState();
      GuidBook book;
      Key bookKey;
      using (var session = Session.Open(Domain)) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          book = new GuidBook() {Title = NewBookTitle};
          bookKey = book.Key;

          var mapping = ds.ApplyChanges();
          bookKey = mapping.TryRemapKey(bookKey);

          Assert.AreEqual(bookKey, book.Key);
          Assert.AreEqual(book.Title, NewBookTitle);
          Assert.IsFalse(book.IsRemoved);
          Assert.AreSame(book, Query.Single(bookKey));
        }
        book.Remove();
      }
    }
  }
}