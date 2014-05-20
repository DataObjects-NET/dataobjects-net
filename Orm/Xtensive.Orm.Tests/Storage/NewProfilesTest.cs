// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.04.14

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.NewClientProfileTestModel;

namespace Xtensive.Orm.Tests.Storage.NewClientProfileTestModel
{
  [HierarchyRoot]
  [Index("Name", Unique = true)]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }

  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field, Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; private set; }

    [Field, Association(PairTo = "Book")]
    public EntitySet<Comment> Comments { get; private set; }

    [Field]
    public EntitySet<Store> Stores { get; private set; }
    
    [Field, Version(VersionMode.Auto)]
    public int Version { get; set; } 
  }

  [HierarchyRoot]
  public class Comment : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Book Book { get; set; }
  }

  [HierarchyRoot]
  public class Store : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class NewProfilesTest : AutoBuildTest
  {
    private readonly SessionConfiguration clientProfile = new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation);
    private readonly SessionConfiguration serverProfile = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.AutoActivation);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Book).Assembly, typeof (Book).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void SessionAndTransactionIsDisconnected()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        Assert.IsTrue(session.IsDisconnected);
        using (var transaction = session.OpenTransaction()) {
          Assert.IsTrue(transaction.Transaction.IsDisconnected);
        }
      }
    }

    [Test]
    public void ClientProfileSavingChangesInsideNestedTransaction()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile))
      using (session.OpenTransaction()) {
        Key key;
        using (var nestedScope = session.OpenTransaction(TransactionOpenMode.New)) {
          var order = new Author();
          session.SaveChanges();
          key = order.Key;
          Assert.DoesNotThrow(() => session.Query.Single(key));
          var orderFromDB = session.Query.All<Author>().FirstOrDefault();
          Assert.IsNotNull(orderFromDB);
          nestedScope.Complete();
        }
        Assert.DoesNotThrow(() =>
          session.Query.Single(key));
      }
    }

    [Test]
    public void ClientProfileSavingChangesInsideTransaction()
    {
      RebuildDomain();
      var books = new List<Book>();
      using (var session = Domain.OpenSession(clientProfile)) {
        var author = new Author {Name = "Some author"};
        var firstBook = new Book {Authors = {author}, Title = "first book"};
        var secondBook = new Book {Authors = {author}, Title = "second book"};
        session.SaveChanges();
        using (var transaction = session.OpenTransaction()) {
          author.Name = "Author";
          firstBook.Title = "1st Book";
          secondBook.Title = "2nd Book";
          session.SaveChanges();
          Assert.AreEqual("Author", author.Name);
          Assert.AreEqual("1st Book", firstBook.Title);
          Assert.AreEqual("2nd Book", secondBook.Title);
          Assert.AreEqual(1, author.Books.Count(book => book.Title=="1st Book"));
          Assert.AreEqual(1, author.Books.Count(book => book.Title=="2nd Book"));
        }
        Assert.AreEqual("Some author", author.Name);
        Assert.AreEqual("first book", firstBook.Title);
        Assert.AreEqual("second book", secondBook.Title);

        Assert.IsNull(author.Books.FirstOrDefault(book => book.Title=="1st Book"));
        Assert.IsNull(author.Books.FirstOrDefault(book => book.Title=="2nd Book"));
        using (var transaction = session.OpenTransaction()) {
          author.Name = "Another author";
          firstBook.Title = "1st Book";
          secondBook.Title = "2nd Book";
          session.SaveChanges();
          Assert.AreEqual("Another author", author.Name);
          Assert.AreEqual("1st Book", firstBook.Title);
          Assert.AreEqual("2nd Book", secondBook.Title);
          Assert.AreEqual(1, author.Books.Count(book => book.Title=="1st Book"));
          Assert.AreEqual(1, author.Books.Count(book => book.Title=="2nd Book"));
          transaction.Complete();
        }
        Assert.AreEqual("Another author", author.Name);
        Assert.AreEqual("1st Book", firstBook.Title);
        Assert.AreEqual("2nd Book", secondBook.Title);
        Assert.IsNotNull(author.Books.FirstOrDefault(book => book.Title=="1st Book"));
        Assert.IsNotNull(author.Books.FirstOrDefault(book => book.Title=="2nd Book"));
      }
    }

    [Test]
    public void ClientProfileCancelingChangesInsideTransactions()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        var author = new Author {Name = "Some author"};
        session.SaveChanges();
        using (var transaction = session.OpenTransaction()) {
          author.Name = "Author";
          session.CancelChanges();
          Assert.AreEqual("Some author", author.Name);
        }
        Assert.AreEqual("Some author", author.Name);
        using (var transaction = session.OpenTransaction()) {
          author.Name = "Another author";
          session.CancelChanges();
          Assert.AreEqual("Some author", author.Name);
          transaction.Complete();
        }
        Assert.AreEqual("Some author", author.Name);
      }
    }

    [Test]
    public void ClientProfileNestedTransactions()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        var author = new Author {Name = "Some author"};
        session.SaveChanges();
        Assert.AreEqual("Some author", author.Name);
        using (var firstLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
          using (var secondLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.AreEqual("Author", author.Name);
            }
            Assert.AreEqual("Some author", author.Name);
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.AreEqual("Author", author.Name);
              thirdLevelTransaction.Complete();
            }
            Assert.AreEqual("Author", author.Name);
          }
          Assert.AreEqual("Some author", author.Name);

          using (var secondLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.AreEqual("Author", author.Name);
            }
            Assert.AreEqual("Some author", author.Name);
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.AreEqual("Author", author.Name);
              thirdLevelTransaction.Complete();
            }
            Assert.AreEqual("Author", author.Name);
            secondLevelTransaction.Complete();
          }
          Assert.AreEqual("Author", author.Name);
        }
      }
    }

    [Test]
    public void AutoSaveChangesDisabledTest()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        new Book();
        new Book();

        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(0, countOfBooks);

        session.SaveChanges();

        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
      }
    }

    [Test]
    public void ClientProfileNonTransactionTest()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        new Author();
        session.SaveChanges();
        Assert.IsNull(session.Transaction);
      }
    }

    [Test]
    public void ClientProfileCancelingChanges()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        var bookForEditLater = new Book();
        var bookForRemoveLater = new Book();
        var author = new Author {Name = "Author"};
        author.Books.Add(bookForEditLater);
        author.Books.Add(bookForRemoveLater);
        session.SaveChanges();

        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        Assert.AreEqual(2, author.Books.Count);

        author.Books.Add(new Book());
        session.CancelChanges();

        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        Assert.AreEqual(2, author.Books.Count);

        bookForEditLater.Title = "not null title";
        session.CancelChanges();

        Assert.AreNotEqual(bookForEditLater.Title, "not null title");
        Assert.AreEqual(bookForEditLater.PersistenceState, PersistenceState.Synchronized);
        var editedBook = session.Query.Single<Book>(bookForEditLater.Key);
        Assert.AreNotEqual(editedBook.Title, "not null title");

        author.Books.Remove(bookForRemoveLater);
        bookForRemoveLater.Remove();
        session.CancelChanges();
        Assert.IsFalse(bookForRemoveLater.IsRemoved);
        Assert.AreEqual(bookForRemoveLater.PersistenceState, PersistenceState.Synchronized);
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        Assert.AreEqual(2, author.Books.Count);
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
      }
    }

    [Test]
    public void ClientProfileSavingChanges()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        var bookForEditLater = new Book();
        var bookForRemoveLater = new Book();
        session.SaveChanges();
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);

        new Book();
        session.SaveChanges();
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(3, countOfBooks);

        bookForEditLater.Title = "not null title";
        session.SaveChanges();
        Assert.AreEqual(bookForEditLater.Title, "not null title");
        Assert.AreEqual(bookForEditLater.PersistenceState, PersistenceState.Synchronized);
        var editedBook = session.Query.Single<Book>(bookForEditLater.Key);
        Assert.AreEqual(editedBook.Title, "not null title");

        bookForRemoveLater.Remove();
        session.SaveChanges();
        Assert.IsTrue(bookForRemoveLater.IsRemoved);
        Assert.AreEqual(bookForRemoveLater.PersistenceState, PersistenceState.Removed);
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
      }
    }

    [Test]
    public void ServerProfileTest()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookForEditLater = new Book();
        var bookForRemoveLater = new Book();
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);

        new Book();
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(3, countOfBooks);

        bookForEditLater.Title = "not null title";
        Assert.AreEqual(bookForEditLater.Title, "not null title");
        var editedBook = session.Query.Single<Book>(bookForEditLater.Key);
        Assert.AreEqual(editedBook.Title, "not null title");

        bookForRemoveLater.Remove();
        Assert.IsTrue(bookForRemoveLater.IsRemoved);
        Assert.AreEqual(bookForRemoveLater.PersistenceState, PersistenceState.Removed);
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);

        transaction.Complete();
      }
    }

    [Test]
    public void ServerProfileCancelingChanges()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(serverProfile)) {
        Key bookForEditLaterKey;
        Key bookForRemoveLaterKey;
        using (var transaction = session.OpenTransaction()) {
          var bookForEditLater = new Book();
          var bookForRemoveLater = new Book();
          var author = new Author {Name = "Author"};
          author.Books.Add(bookForEditLater);
          author.Books.Add(bookForRemoveLater);
          bookForEditLaterKey = bookForEditLater.Key;
          bookForRemoveLaterKey = bookForRemoveLater.Key;
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().First();
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          Assert.AreEqual(2, author.Books.Count);
          author.Books.Add(new Book());
        }

        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().First();
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          Assert.AreEqual(2, author.Books.Count);
          var bookForEditLater = session.Query.Single<Book>(bookForEditLaterKey);
          bookForEditLater.Title = "not null title";
        }

        using (var transaction = session.OpenTransaction()) {
          var bookForEditLater = session.Query.Single<Book>(bookForEditLaterKey);
          var bookForRemoveLater = session.Query.Single<Book>(bookForRemoveLaterKey);
          Assert.AreNotEqual(bookForEditLater.Title, "not null title");
          Assert.AreEqual(bookForEditLater.PersistenceState, PersistenceState.Synchronized);
          var editedBook = session.Query.Single<Book>(bookForEditLater.Key);
          Assert.AreNotEqual(editedBook.Title, "not null title");
          var author = bookForRemoveLater.Authors.First();
          author.Books.Remove(bookForRemoveLater);
          bookForRemoveLater.Remove();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        var author = session.Query.All<Author>().First();
        Assert.AreEqual(2, countOfBooks);
      }
    }

    [Test]
    public void ClientProfileVersionsTest()
    {
      RebuildDomain();
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation | SessionOptions.ValidateEntityVersions);

      int thread1Version;
      int thread2Version;
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var bookForEditLater = new Book();
        var version = bookForEditLater.Version;
        session.SaveChanges();
      }

      var thread1 = new Thread(
        o => {
          var domain1 = (Domain) o;
          using (var session = domain1.OpenSession(sessionConfiguration)) {
            var book = session.Query.All<Book>().First();
            Thread.Sleep(10);
            book.Title = "first thread";
            session.SaveChanges();
          }
        });
      var thread2 = new Thread(
        o => {
          var domain1 = (Domain) o;
          using (var session = domain1.OpenSession(sessionConfiguration)) {
            var book = session.Query.All<Book>().First();
            Thread.Sleep(10);
            book.Title = "second thread";
            Assert.Throws<VersionConflictException>(session.SaveChanges);
          }
        });

      thread1.Start(Domain);
      thread2.Start(Domain);
    }

    [Test]
    public void ClientProfileSameExceptionWhenDoublePersist()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        var authorWithUniqueName = new Author {Name = "Peter"};
        session.SaveChanges();
        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Peter"));
        var authorWithSameName = new Author {Name = authorWithUniqueName.Name};

        Exception firstException = null;
        Exception secondException = null;
        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          firstException = exception;
        }
        Assert.IsNotNull(firstException);

        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          secondException = exception;
        }
        Assert.IsNotNull(secondException);
        Assert.AreEqual(
            firstException.Message.Substring(0, 20),
            secondException.Message.Substring(0, 20));
        authorWithSameName.Name = "Mathew";
        session.SaveChanges();

        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Mathew"));
      }
    }

    [Test]
    public void ClientProfileSameExceptionWhenDoublePersistInsideTransaction()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var authorWithUniqueName = new Author {Name = "Peter"};
          session.SaveChanges();
          Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Peter"));
          var authorWithSameName = new Author {Name = authorWithUniqueName.Name};

          Exception firstException = null;
          Exception secondException = null;
          try {
            session.SaveChanges();
          }
          catch (Exception exception) {
            firstException = exception;
          }
          Assert.IsNotNull(firstException);

          try {
            session.SaveChanges();
          }
          catch (Exception exception) {
            secondException = exception;
          }
          Assert.IsNotNull(secondException);
          Assert.AreEqual(
            firstException.Message.Substring(0, 20),
            secondException.Message.Substring(0, 20));
          authorWithSameName.Name = "Mathew";
          session.SaveChanges();

          Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Mathew"));
          transaction.Complete();
        }
        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Mathew"));
        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Peter"));
      }
    }

    [Test]
    public void ClientProfileRollbackEntitySetsWhenPersistHasFallen()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession(clientProfile)) {
        var firstAuthor = new Author {Name = "Peter"};
        var firstBook = new Book {Title = "First Book"};
        firstAuthor.Books.Add(firstBook);
        var secondBook = new Book {Title = "Second Book"};
        firstAuthor.Books.Add(secondBook);
        session.SaveChanges();
        Assert.AreEqual(2, session.Query.All<Author>().First().Books.Count);
        Assert.AreEqual(1, session.Query.All<Book>().First(book => book.Title=="First Book").Authors.Count);
        Assert.AreEqual(1, session.Query.All<Book>().First(book => book.Title=="Second Book").Authors.Count);
        var secondAutor = new Author {Name = "Peter"};
        EntitySetState state;
        session.LookupStateInCache(firstAuthor.Key, Domain.Model.Types["Author"].Fields["Books"], out state);
        Assert.IsNotNull(state);
        var booksOfFirstAuthor = new {
          CountOfAddedItems = state.AddedItemsCount,
          CountOfRemovedItems = state.RemovedItemsCount,
          TotalItemsCount = state.TotalItemCount,
          IsLoaded = state.IsLoaded,
          CountOfFetchedItems = state.FetchedItemsCount
        };
        state = null;
        session.LookupStateInCache(firstBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
        Assert.IsNotNull(state);
        var authorsOfFirstBook = new {
          CountOfAddedItems = state.AddedItemsCount,
          CountOfRemovedItems = state.RemovedItemsCount,
          TotalItemsCount = state.TotalItemCount,
          IsLoaded = state.IsLoaded,
          CountOfFetchedItems = state.FetchedItemsCount
        };
        
        state = null;
        session.LookupStateInCache(secondBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
        var authorsOfSecondBook = new {
          CountOfAddedItems = state.AddedItemsCount,
          CountOfRemovedItems = state.RemovedItemsCount,
          TotalItemsCount = state.TotalItemCount,
          IsLoaded = state.IsLoaded,
          CountOfFetchedItems = state.FetchedItemsCount
        };
        try {
          session.SaveChanges();
        }
        catch (Exception) {
          Assert.AreEqual(2, session.Query.All<Author>().First().Books.Count);
          Assert.AreEqual(1, session.Query.All<Book>().First(book => book.Title == "First Book").Authors.Count);
          Assert.AreEqual(1, session.Query.All<Book>().First(book => book.Title == "Second Book").Authors.Count);
          state = null;
          session.LookupStateInCache(firstAuthor.Key, Domain.Model.Types["Author"].Fields["Books"], out state);
          Assert.IsNotNull(state);
          Assert.AreEqual(booksOfFirstAuthor.CountOfAddedItems, state.AddedItemsCount);
          Assert.AreEqual(booksOfFirstAuthor.CountOfRemovedItems, state.RemovedItemsCount);
          Assert.AreEqual(booksOfFirstAuthor.CountOfFetchedItems, state.FetchedItemsCount);
          Assert.AreEqual(booksOfFirstAuthor.IsLoaded, state.IsLoaded);
          Assert.AreEqual(booksOfFirstAuthor.TotalItemsCount, state.TotalItemCount);
          state = null;
          session.LookupStateInCache(firstBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
          Assert.IsNotNull(state);
          Assert.AreEqual(authorsOfFirstBook.CountOfAddedItems, state.AddedItemsCount);
          Assert.AreEqual(authorsOfFirstBook.CountOfRemovedItems, state.RemovedItemsCount);
          Assert.AreEqual(authorsOfFirstBook.CountOfFetchedItems, state.FetchedItemsCount);
          Assert.AreEqual(authorsOfFirstBook.IsLoaded, state.IsLoaded);
          Assert.AreEqual(authorsOfFirstBook.TotalItemsCount, state.TotalItemCount);
          state = null;
          session.LookupStateInCache(secondBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
          Assert.IsNotNull(state);
          Assert.AreEqual(authorsOfSecondBook.CountOfAddedItems, state.AddedItemsCount);
          Assert.AreEqual(authorsOfSecondBook.CountOfRemovedItems, state.RemovedItemsCount);
          Assert.AreEqual(authorsOfSecondBook.CountOfFetchedItems, state.FetchedItemsCount);
          Assert.AreEqual(authorsOfSecondBook.IsLoaded, state.IsLoaded);
          Assert.AreEqual(authorsOfSecondBook.TotalItemsCount, state.TotalItemCount);
        }

        secondAutor.Name = "Mathew";
        session.SaveChanges();
        Assert.AreEqual(2, session.Query.All<Author>().Count());
        Assert.AreEqual(2, session.Query.All<Book>().Count());
        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Peter"));
        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Mathew"));
        Assert.AreEqual(2, session.Query.All<Author>().First(author=>author.Name=="Peter").Books.Count);
        Assert.AreEqual(0, session.Query.All<Author>().First(author => author.Name=="Mathew").Books.Count);
      }
    }
  }
}