// Copyright (C) 2014-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2014.04.14

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Storage.NewClientProfileTestModel;

namespace Xtensive.Orm.Tests.Storage.NewClientProfileTestModel
{
  [HierarchyRoot]
  [Index("Name", Unique = true)]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 150)]
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
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [SetUp]
    public void CleanDatabase()
    {
      using(var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        foreach(var author in session.Query.All<Author>()) {
          author.Books.Clear();
          session.SaveChanges();
          author.Remove();
        }
        session.SaveChanges();

        foreach(var book in session.Query.All<Book>()) {
          book.Comments.Clear();
          book.Stores.Clear();
          session.SaveChanges();
          book.Remove();
        }
        session.SaveChanges();

        foreach (var comment in session.Query.All<Comment>()) {
          comment.Remove();
        }
        session.SaveChanges();

        foreach (var store in session.Query.All<Store>()) {
          store.Books.Clear();
          session.SaveChanges();
        }
        tx.Complete();
      }
    }

    [Test]
    public void SessionAndTransactionIsDisconnected()
    {
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
        Assert.DoesNotThrow(() => session.Query.Single(key));
      }
    }

    [Test]
    public void ClientProfileSavingChangesInsideTransaction()
    {
      using (var session = Domain.OpenSession(clientProfile)) {
        var author = new Author { Name = "Some author" };
        var firstBook = new Book { Authors = { author }, Title = "first book" };
        var secondBook = new Book { Authors = { author }, Title = "second book" };
        session.SaveChanges();

        using (var transaction = session.OpenTransaction()) {
          author.Name = "Author";
          firstBook.Title = "1st Book";
          secondBook.Title = "2nd Book";
          session.SaveChanges();

          Assert.AreEqual("Author", author.Name);
          Assert.AreEqual("1st Book", firstBook.Title);
          Assert.AreEqual("2nd Book", secondBook.Title);
          Assert.AreEqual(1, author.Books.Count(book => book.Title == "1st Book"));
          Assert.AreEqual(1, author.Books.Count(book => book.Title == "2nd Book"));
        }
        Assert.AreEqual("Some author", author.Name);
        Assert.AreEqual("first book", firstBook.Title);
        Assert.AreEqual("second book", secondBook.Title);

        Assert.IsNull(author.Books.FirstOrDefault(book => book.Title == "1st Book"));
        Assert.IsNull(author.Books.FirstOrDefault(book => book.Title == "2nd Book"));
        using (var transaction = session.OpenTransaction()) {
          author.Name = "Another author";
          firstBook.Title = "1st Book";
          secondBook.Title = "2nd Book";
          session.SaveChanges();

          Assert.AreEqual("Another author", author.Name);
          Assert.AreEqual("1st Book", firstBook.Title);
          Assert.AreEqual("2nd Book", secondBook.Title);
          Assert.AreEqual(1, author.Books.Count(book => book.Title == "1st Book"));
          Assert.AreEqual(1, author.Books.Count(book => book.Title == "2nd Book"));
          transaction.Complete();
        }
        Assert.AreEqual("Another author", author.Name);
        Assert.AreEqual("1st Book", firstBook.Title);
        Assert.AreEqual("2nd Book", secondBook.Title);
        Assert.IsNotNull(author.Books.FirstOrDefault(book => book.Title == "1st Book"));
        Assert.IsNotNull(author.Books.FirstOrDefault(book => book.Title == "2nd Book"));
      }
    }

    [Test]
    public void ClientProfileCancelingChangesInsideTransactions()
    {
      using (var session = Domain.OpenSession(clientProfile)) {
        var author = new Author { Name = "Some author" };
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
      using (var session = Domain.OpenSession(clientProfile)) {
        var author = new Author { Name = "Some author" };
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
      using (var session = Domain.OpenSession(clientProfile)) {
        _ = new Book();
        _ = new Book();

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
      using (var session = Domain.OpenSession(clientProfile)) {
        _ = new Author();
        session.SaveChanges();
        Assert.IsNull(session.Transaction);
      }
    }

    [Test]
    public void ClientProfileCancelingChanges()
    {
      using (var session = Domain.OpenSession(clientProfile)) {
        var bookToEditLater = new Book();
        var bookToRemoveLater = new Book();
        var author = new Author { Name = "Author" };
        _ = author.Books.Add(bookToEditLater);
        _ = author.Books.Add(bookToRemoveLater);
        session.SaveChanges();

        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        Assert.AreEqual(2, author.Books.Count);

        _ = author.Books.Add(new Book());
        session.CancelChanges();

        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        Assert.AreEqual(2, author.Books.Count);

        bookToEditLater.Title = "not null title";
        session.CancelChanges();

        Assert.AreNotEqual(bookToEditLater.Title, "not null title");
        Assert.AreEqual(bookToEditLater.PersistenceState, PersistenceState.Synchronized);
        var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
        Assert.AreNotEqual(editedBook.Title, "not null title");

        _ = author.Books.Remove(bookToRemoveLater);
        bookToRemoveLater.Remove();
        session.CancelChanges();
        Assert.IsFalse(bookToRemoveLater.IsRemoved);
        Assert.AreEqual(bookToRemoveLater.PersistenceState, PersistenceState.Synchronized);
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
      using (var session = Domain.OpenSession(clientProfile)) {
        var bookToEditLater = new Book();
        var bookToRemoveLater = new Book();
        session.SaveChanges();
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);

        _ = new Book();
        session.SaveChanges();
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(3, countOfBooks);

        bookToEditLater.Title = "not null title";
        session.SaveChanges();
        Assert.AreEqual(bookToEditLater.Title, "not null title");
        Assert.AreEqual(bookToEditLater.PersistenceState, PersistenceState.Synchronized);
        var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
        Assert.AreEqual(editedBook.Title, "not null title");

        bookToRemoveLater.Remove();
        session.SaveChanges();
        Assert.IsTrue(bookToRemoveLater.IsRemoved);
        Assert.AreEqual(bookToRemoveLater.PersistenceState, PersistenceState.Removed);
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
      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookToEditLater = new Book();
        var bookToRemoveLater = new Book();
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);

        _ = new Book();
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(3, countOfBooks);

        bookToEditLater.Title = "not null title";
        Assert.AreEqual(bookToEditLater.Title, "not null title");
        var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
        Assert.AreEqual(editedBook.Title, "not null title");

        bookToRemoveLater.Remove();
        Assert.IsTrue(bookToRemoveLater.IsRemoved);
        Assert.AreEqual(bookToRemoveLater.PersistenceState, PersistenceState.Removed);
        countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);

        transaction.Complete();
      }
    }

    [Test]
    public void ServerProfileCancelingOfEntityChanges()
    {
      Key bookToEditLaterKey;
      Key bookToRemoveLaterKey;
      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookToEditLater = new Book();
          var bookToRemoveLater = new Book();
          bookToEditLaterKey = bookToEditLater.Key;
          bookToRemoveLaterKey = bookToRemoveLater.Key;
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          Assert.IsTrue(string.IsNullOrEmpty(bookToEditLater.Title));
          Assert.IsTrue(string.IsNullOrEmpty(bookToRemoveLater.Title));
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        Assert.IsTrue(string.IsNullOrEmpty(bookToEditLater.Title));
        Assert.IsTrue(string.IsNullOrEmpty(bookToRemoveLater.Title));
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var newBook = new Book();
        }

        using (var transaction = session.OpenTransaction()) {
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var newBook = new Book();
          session.CancelChanges();
          Assert.AreEqual(null, newBook.Tuple);
          Assert.AreEqual(PersistenceState.Removed, newBook.PersistenceState);
          _ = Assert.Throws<InvalidOperationException>(() => newBook.Title = "not null title");
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          bookToEditLater.Title = "not null title";
        }
        using (var transaction = session.OpenTransaction()) {
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          Assert.IsTrue(string.IsNullOrEmpty(bookToEditLater.Title));
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        Assert.IsTrue(string.IsNullOrEmpty(bookToEditLater.Title));
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          bookToEditLater.Title = "not null title";
          session.CancelChanges();
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          Assert.IsTrue(string.IsNullOrEmpty(bookToEditLater.Title));
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        Assert.IsTrue(string.IsNullOrEmpty(bookToEditLater.Title));
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          bookToRemoveLater.Remove();
        }

        using (var transaction = session.OpenTransaction()) {
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          Assert.IsNotNull(bookToRemoveLater);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        Assert.IsNotNull(bookToRemoveLater);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          bookToRemoveLater.Remove();
          session.CancelChanges();
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          Assert.IsNotNull(bookToRemoveLater);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        Assert.IsNotNull(bookToRemoveLater);
        transaction.Complete();
      }
    }

    [Test]
    public void ServerProfileCancelingChanges()
    {
      Key bookToEditLaterKey;
      Key bookToRemoveLaterKey;
      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookForEditLater = new Book();
          var bookForRemoveLater = new Book();
          var author = new Author { Name = "Author" };
          _ = author.Books.Add(bookForEditLater);
          _ = author.Books.Add(bookForRemoveLater);
          bookToEditLaterKey = bookForEditLater.Key;
          bookToRemoveLaterKey = bookForRemoveLater.Key;
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().First();
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          Assert.AreEqual(2, author.Books.Count);
          _ = author.Books.Add(new Book());
        }

        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().First();
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          Assert.AreEqual(2, author.Books.Count);
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          bookToEditLater.Title = "not null title";
        }

        using (var transaction = session.OpenTransaction()) {
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          Assert.AreNotEqual(bookToEditLater.Title, "not null title");
          Assert.AreEqual(bookToEditLater.PersistenceState, PersistenceState.Synchronized);
          var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
          Assert.AreNotEqual(editedBook.Title, "not null title");
          var author = bookToRemoveLater.Authors.First();
          _ = author.Books.Remove(bookToRemoveLater);
          bookToRemoveLater.Remove();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        var author = session.Query.All<Author>().First();
        Assert.AreEqual(2, countOfBooks);
        Assert.AreEqual(2, author.Books.Count);
        var bookForEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        var bookForRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        Assert.NotNull(bookForRemoveLater);
        Assert.IsTrue(string.IsNullOrEmpty(bookForEditLater.Title));
      }
    }

    [Test]
    public void ServerProfileCancelingOfEntitySetChanges()
    {
      Key firstBookKey;
      Key secondBookKey;
      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var firstBook = new Book();
          var secondBook = new Book();
          var author = new Author();
          firstBookKey = firstBook.Key;
          secondBookKey = secondBook.Key;
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var firstBook = session.Query.Single<Book>(firstBookKey);
          var secondBook = session.Query.Single<Book>(secondBookKey);
          var author = session.Query.All<Author>().FirstOrDefault();
          var countOfAuthors = session.Query.All<Author>().Count();
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.AreEqual(2, countOfBooks);
          Assert.AreEqual(1, countOfAuthors);
          Assert.IsNotNull(author);
          Assert.AreEqual(0, author.Books.Count);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var firstBook = session.Query.Single<Book>(firstBookKey);
        var secondBook = session.Query.Single<Book>(secondBookKey);
        var author = session.Query.All<Author>().FirstOrDefault();
        var countOfAuthors = session.Query.All<Author>().Count();
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.AreEqual(2, countOfBooks);
        Assert.AreEqual(1, countOfAuthors);
        Assert.IsNotNull(author);
        Assert.AreEqual(0, author.Books.Count);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var firstBook = session.Query.Single<Book>(firstBookKey);
          var secondBook = session.Query.Single<Book>(secondBookKey);
          var author = session.Query.All<Author>().FirstOrDefault();
          _ = author.Books.Add(firstBook);
          session.CancelChanges();
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var autor = session.Query.All<Author>().FirstOrDefault();
          Assert.AreEqual(0, autor.Books.Count);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.AreEqual(0, author.Books.Count);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var firstBook = session.Query.Single<Book>(firstBookKey);
          var secondBook = session.Query.Single<Book>(secondBookKey);
          var author = session.Query.All<Author>().FirstOrDefault();
          author.Books.AddRange(new[] { firstBook, secondBook });
          session.CancelChanges();
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var autor = session.Query.All<Author>().FirstOrDefault();
          Assert.AreEqual(1, autor.Books.Count);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.AreEqual(1, author.Books.Count);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var firstBook = session.Query.Single<Book>(firstBookKey);
          var secondBook = session.Query.Single<Book>(secondBookKey);
          var author = session.Query.All<Author>().FirstOrDefault();
          author.Books.AddRange(new[] { firstBook, secondBook });
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().FirstOrDefault();
          Assert.AreEqual(2, author.Books.Count);
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.AreEqual(2, author.Books.Count);
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var firstBook = session.Query.Single<Book>(firstBookKey);
          var secondBook = session.Query.Single<Book>(secondBookKey);
          var author = session.Query.All<Author>().FirstOrDefault();
          _ = author.Books.Remove(firstBook);
          session.CancelChanges();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().FirstOrDefault();
          Assert.AreEqual(2, author.Books.Count);
        }
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.AreEqual(2, author.Books.Count);
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var firstBook = session.Query.Single<Book>(firstBookKey);
          var secondBook = session.Query.Single<Book>(secondBookKey);
          var author = session.Query.All<Author>().FirstOrDefault();
          _ = author.Books.Remove(firstBook);
          _ = author.Books.Remove(secondBook);
          session.CancelChanges();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().FirstOrDefault();
          Assert.AreEqual(1, author.Books.Count);
        }
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.AreEqual(1, author.Books.Count);
      }
    }

    [Test]
    public void ClientProfileVersionsTest()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation | SessionOptions.ValidateEntityVersions);

      using (var session = Domain.OpenSession(sessionConfiguration)) {
        var bookForEditLater = new Book();
        var version = bookForEditLater.Version;
        session.SaveChanges();
      }

      using (var session1 = Domain.OpenSession(sessionConfiguration)) {
        var book1 = session1.Query.All<Book>().First();
        book1.Title = "first session";
        using(var session2 = Domain.OpenSession(sessionConfiguration)) {
          var book2 = session2.Query.All<Book>().First();
          book2.Title = "second session";
          session2.SaveChanges();
        }

        _ = Assert.Throws<VersionConflictException>(session1.SaveChanges);
      }
    }

    [Test]
    public void ClientProfileSameExceptionWhenDoublePersist()
    {
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
      Require.ProviderIsNot(StorageProvider.PostgreSql, "UniqueVauleConstraint breaks the outermost transaction.");

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
      using (var session = Domain.OpenSession(clientProfile)) {
        var firstAuthor = new Author { Name = "Peter" };
        var firstBook = new Book { Title = "First Book" };
        _ = firstAuthor.Books.Add(firstBook);
        var secondBook = new Book { Title = "Second Book" };
        _ = firstAuthor.Books.Add(secondBook);
        session.SaveChanges();

        Assert.AreEqual(2, session.Query.All<Author>().First().Books.Count);
        Assert.AreEqual(1, session.Query.All<Book>().First(book => book.Title == "First Book").Authors.Count);
        Assert.AreEqual(1, session.Query.All<Book>().First(book => book.Title == "Second Book").Authors.Count);

        var secondAutor = new Author { Name = "Peter" };
        _ = session.LookupStateInCache(firstAuthor.Key, Domain.Model.Types["Author"].Fields["Books"], out var state);
        Assert.IsNotNull(state);
        var booksOfFirstAuthor = new {
          CountOfAddedItems = state.AddedItemsCount,
          CountOfRemovedItems = state.RemovedItemsCount,
          TotalItemsCount = state.TotalItemCount,
          IsLoaded = state.IsLoaded,
          CountOfFetchedItems = state.FetchedItemsCount
        };

        state = null;
        _ = session.LookupStateInCache(firstBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
        Assert.IsNotNull(state);
        var authorsOfFirstBook = new {
          CountOfAddedItems = state.AddedItemsCount,
          CountOfRemovedItems = state.RemovedItemsCount,
          TotalItemsCount = state.TotalItemCount,
          IsLoaded = state.IsLoaded,
          CountOfFetchedItems = state.FetchedItemsCount
        };

        state = null;
        _ = session.LookupStateInCache(secondBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
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
          _ = session.LookupStateInCache(firstAuthor.Key, Domain.Model.Types["Author"].Fields["Books"], out state);
          Assert.IsNotNull(state);
          Assert.AreEqual(booksOfFirstAuthor.CountOfAddedItems, state.AddedItemsCount);
          Assert.AreEqual(booksOfFirstAuthor.CountOfRemovedItems, state.RemovedItemsCount);
          Assert.AreEqual(booksOfFirstAuthor.CountOfFetchedItems, state.FetchedItemsCount);
          Assert.AreEqual(booksOfFirstAuthor.IsLoaded, state.IsLoaded);
          Assert.AreEqual(booksOfFirstAuthor.TotalItemsCount, state.TotalItemCount);
          state = null;
          _ = session.LookupStateInCache(firstBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
          Assert.IsNotNull(state);
          Assert.AreEqual(authorsOfFirstBook.CountOfAddedItems, state.AddedItemsCount);
          Assert.AreEqual(authorsOfFirstBook.CountOfRemovedItems, state.RemovedItemsCount);
          Assert.AreEqual(authorsOfFirstBook.CountOfFetchedItems, state.FetchedItemsCount);
          Assert.AreEqual(authorsOfFirstBook.IsLoaded, state.IsLoaded);
          Assert.AreEqual(authorsOfFirstBook.TotalItemsCount, state.TotalItemCount);
          state = null;
          _ = session.LookupStateInCache(secondBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
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
        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name == "Peter"));
        Assert.IsNotNull(session.Query.All<Author>().FirstOrDefault(author => author.Name == "Mathew"));
        Assert.AreEqual(2, session.Query.All<Author>().First(author => author.Name == "Peter").Books.Count);
        Assert.AreEqual(0, session.Query.All<Author>().First(author => author.Name == "Mathew").Books.Count);
      }
    }

    [Test]
    public void ClientProfileEntitySetCountTest01()
    {
      long bookId = 0;
      using (var session = Domain.OpenSession(serverProfile))
      using (var transactionScope = session.OpenTransaction()) {
        var book = new Book { Title = "First book" };

        _ = book.Comments.Add(new Comment());
        _ = book.Comments.Add(new Comment());

        bookId = book.Id;
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var book = session.Query.All<Book>().FirstOrDefault(b => b.Id == bookId);

        var dummyCount = book.Comments.Count;
        _ = book.Comments.Add(new Comment());

        Assert.That(book.Comments.Count, Is.EqualTo(3));
      }
    }

    [Test]
    public void ClientProfileEntitySetCount02()
    {
      long bookId = 0;
      using (var session = Domain.OpenSession(serverProfile))
      using (var transactionScope = session.OpenTransaction()) {
        var book = new Book { Title = "First book" };

        _ = book.Comments.Add(new Comment());
        _ = book.Comments.Add(new Comment());

        bookId = book.Id;
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var book = session.Query.All<Book>().FirstOrDefault(b => b.Id == bookId);

        _ = book.Comments.Add(new Comment());

        var enumerableCount = book.Comments.AsEnumerable().Count();

        Assert.That(enumerableCount, Is.EqualTo(3));
        Assert.That(book.Comments.Count, Is.EqualTo(3));
      }
    }

    [Test]
    public void ClientProfileEntitySetCount03()
    {
      long bookId = 0;
      long removingCommentId = 0;
      using (var session = Domain.OpenSession(serverProfile))
      using (var transactionScope = session.OpenTransaction()) {
        var book = new Book { Title = "First book" };
        var commentToBeRemoved = new Comment();

        _ = book.Comments.Add(new Comment());
        _ = book.Comments.Add(new Comment());
        _ = book.Comments.Add(commentToBeRemoved);
        _ = book.Comments.Add(new Comment());

        bookId = book.Id;
        removingCommentId = commentToBeRemoved.Id;
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var book = session.Query.All<Book>().First(b => b.Id == bookId);
        var removingComment = session.Query.All<Comment>().First(c => c.Id == removingCommentId);

        var dummyCount = book.Comments.Count;
        _ = book.Comments.Remove(removingComment);

        Assert.That(book.Comments.Count, Is.EqualTo(3));
      }
    }

    [Test]
    public void ClientProfileEntitySetCount04()
    {
      long bookId = 0;
      long removingCommentId = 0;
      using (var session = Domain.OpenSession(serverProfile))
      using (var transactionScope = session.OpenTransaction()) {
        var book = new Book { Title = "First book" };
        var commentToBeRemoved = new Comment();

        _ = book.Comments.Add(new Comment());
        _ = book.Comments.Add(new Comment());
        _ = book.Comments.Add(commentToBeRemoved);
        _ = book.Comments.Add(new Comment());

        bookId = book.Id;
        removingCommentId = commentToBeRemoved.Id;
        transactionScope.Complete();
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var book = session.Query.All<Book>().First(b => b.Id == bookId);
        var removingComment = session.Query.All<Comment>().First(c => c.Id == removingCommentId);

        _ = book.Comments.Remove(removingComment);

        var enumerableCount = book.Comments.AsEnumerable().Count();

        Assert.That(enumerableCount, Is.EqualTo(3));
        Assert.That(book.Comments.Count, Is.EqualTo(3));
      }
    }
  }
}