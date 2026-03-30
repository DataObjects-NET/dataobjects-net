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
using Xtensive.Orm.Providers;
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
      configuration.Types.RegisterCaching(typeof(Book).Assembly, typeof(Book).Namespace);
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
        Assert.That(session.IsDisconnected, Is.True);
        using (var transaction = session.OpenTransaction()) {
          Assert.That(transaction.Transaction.IsDisconnected, Is.True);
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
          Assert.That(orderFromDB, Is.Not.Null);
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

          Assert.That(author.Name, Is.EqualTo("Author"));
          Assert.That(firstBook.Title, Is.EqualTo("1st Book"));
          Assert.That(secondBook.Title, Is.EqualTo("2nd Book"));
          Assert.That(author.Books.Count(book => book.Title == "1st Book"), Is.EqualTo(1));
          Assert.That(author.Books.Count(book => book.Title == "2nd Book"), Is.EqualTo(1));
        }
        Assert.That(author.Name, Is.EqualTo("Some author"));
        Assert.That(firstBook.Title, Is.EqualTo("first book"));
        Assert.That(secondBook.Title, Is.EqualTo("second book"));

        Assert.That(author.Books.FirstOrDefault(book => book.Title == "1st Book"), Is.Null);
        Assert.That(author.Books.FirstOrDefault(book => book.Title == "2nd Book"), Is.Null);
        using (var transaction = session.OpenTransaction()) {
          author.Name = "Another author";
          firstBook.Title = "1st Book";
          secondBook.Title = "2nd Book";
          session.SaveChanges();

          Assert.That(author.Name, Is.EqualTo("Another author"));
          Assert.That(firstBook.Title, Is.EqualTo("1st Book"));
          Assert.That(secondBook.Title, Is.EqualTo("2nd Book"));
          Assert.That(author.Books.Count(book => book.Title == "1st Book"), Is.EqualTo(1));
          Assert.That(author.Books.Count(book => book.Title == "2nd Book"), Is.EqualTo(1));
          transaction.Complete();
        }
        Assert.That(author.Name, Is.EqualTo("Another author"));
        Assert.That(firstBook.Title, Is.EqualTo("1st Book"));
        Assert.That(secondBook.Title, Is.EqualTo("2nd Book"));
        Assert.That(author.Books.FirstOrDefault(book => book.Title == "1st Book"), Is.Not.Null);
        Assert.That(author.Books.FirstOrDefault(book => book.Title == "2nd Book"), Is.Not.Null);
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
          Assert.That(author.Name, Is.EqualTo("Some author"));
        }
        Assert.That(author.Name, Is.EqualTo("Some author"));
        using (var transaction = session.OpenTransaction()) {
          author.Name = "Another author";
          session.CancelChanges();
          Assert.That(author.Name, Is.EqualTo("Some author"));
          transaction.Complete();
        }
        Assert.That(author.Name, Is.EqualTo("Some author"));
      }
    }

    [Test]
    public void ClientProfileNestedTransactions()
    {
      using (var session = Domain.OpenSession(clientProfile)) {
        var author = new Author { Name = "Some author" };
        session.SaveChanges();
        Assert.That(author.Name, Is.EqualTo("Some author"));
        using (var firstLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
          using (var secondLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.That(author.Name, Is.EqualTo("Author"));
            }
            Assert.That(author.Name, Is.EqualTo("Some author"));
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.That(author.Name, Is.EqualTo("Author"));
              thirdLevelTransaction.Complete();
            }
            Assert.That(author.Name, Is.EqualTo("Author"));
          }
          Assert.That(author.Name, Is.EqualTo("Some author"));

          using (var secondLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.That(author.Name, Is.EqualTo("Author"));
            }
            Assert.That(author.Name, Is.EqualTo("Some author"));
            using (var thirdLevelTransaction = session.OpenTransaction(TransactionOpenMode.New)) {
              author.Name = "Author";
              Assert.That(author.Name, Is.EqualTo("Author"));
              thirdLevelTransaction.Complete();
            }
            Assert.That(author.Name, Is.EqualTo("Author"));
            secondLevelTransaction.Complete();
          }
          Assert.That(author.Name, Is.EqualTo("Author"));
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
        Assert.That(countOfBooks, Is.EqualTo(0));

        session.SaveChanges();

        countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
      }
    }

    [Test]
    public void ClientProfileNonTransactionTest()
    {
      using (var session = Domain.OpenSession(clientProfile)) {
        _ = new Author();
        session.SaveChanges();
        Assert.That(session.Transaction, Is.Null);
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
        Assert.That(countOfBooks, Is.EqualTo(2));
        Assert.That(author.Books.Count, Is.EqualTo(2));

        _ = author.Books.Add(new Book());
        session.CancelChanges();

        countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
        Assert.That(author.Books.Count, Is.EqualTo(2));

        bookToEditLater.Title = "not null title";
        session.CancelChanges();

        Assert.That("not null title", Is.Not.EqualTo(bookToEditLater.Title));
        Assert.That(PersistenceState.Synchronized, Is.EqualTo(bookToEditLater.PersistenceState));
        var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
        Assert.That("not null title", Is.Not.EqualTo(editedBook.Title));

        _ = author.Books.Remove(bookToRemoveLater);
        bookToRemoveLater.Remove();
        session.CancelChanges();
        Assert.That(bookToRemoveLater.IsRemoved, Is.False);
        Assert.That(PersistenceState.Synchronized, Is.EqualTo(bookToRemoveLater.PersistenceState));
        countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
        Assert.That(author.Books.Count, Is.EqualTo(2));
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
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
        Assert.That(countOfBooks, Is.EqualTo(2));

        _ = new Book();
        session.SaveChanges();
        countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(3));

        bookToEditLater.Title = "not null title";
        session.SaveChanges();
        Assert.That("not null title", Is.EqualTo(bookToEditLater.Title));
        Assert.That(PersistenceState.Synchronized, Is.EqualTo(bookToEditLater.PersistenceState));
        var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
        Assert.That("not null title", Is.EqualTo(editedBook.Title));

        bookToRemoveLater.Remove();
        session.SaveChanges();
        Assert.That(bookToRemoveLater.IsRemoved, Is.True);
        Assert.That(PersistenceState.Removed, Is.EqualTo(bookToRemoveLater.PersistenceState));
        countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
      }

      using (var session = Domain.OpenSession(clientProfile)) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
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
        Assert.That(countOfBooks, Is.EqualTo(2));

        _ = new Book();
        countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(3));

        bookToEditLater.Title = "not null title";
        Assert.That("not null title", Is.EqualTo(bookToEditLater.Title));
        var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
        Assert.That("not null title", Is.EqualTo(editedBook.Title));

        bookToRemoveLater.Remove();
        Assert.That(bookToRemoveLater.IsRemoved, Is.True);
        Assert.That(PersistenceState.Removed, Is.EqualTo(bookToRemoveLater.PersistenceState));
        countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));

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
          Assert.That(countOfBooks, Is.EqualTo(2));
          Assert.That(string.IsNullOrEmpty(bookToEditLater.Title), Is.True);
          Assert.That(string.IsNullOrEmpty(bookToRemoveLater.Title), Is.True);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
        Assert.That(string.IsNullOrEmpty(bookToEditLater.Title), Is.True);
        Assert.That(string.IsNullOrEmpty(bookToRemoveLater.Title), Is.True);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var newBook = new Book();
        }

        using (var transaction = session.OpenTransaction()) {
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.That(countOfBooks, Is.EqualTo(2));
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var newBook = new Book();
          session.CancelChanges();
          Assert.That(newBook.Tuple, Is.EqualTo(null));
          Assert.That(newBook.PersistenceState, Is.EqualTo(PersistenceState.Removed));
          _ = Assert.Throws<InvalidOperationException>(() => newBook.Title = "not null title");
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.That(countOfBooks, Is.EqualTo(2));
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          bookToEditLater.Title = "not null title";
        }
        using (var transaction = session.OpenTransaction()) {
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.That(countOfBooks, Is.EqualTo(2));
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          Assert.That(string.IsNullOrEmpty(bookToEditLater.Title), Is.True);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
        var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        Assert.That(string.IsNullOrEmpty(bookToEditLater.Title), Is.True);
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
          Assert.That(countOfBooks, Is.EqualTo(2));
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          Assert.That(string.IsNullOrEmpty(bookToEditLater.Title), Is.True);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        Assert.That(countOfBooks, Is.EqualTo(2));
        var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        Assert.That(string.IsNullOrEmpty(bookToEditLater.Title), Is.True);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(serverProfile)) {
        using (var transaction = session.OpenTransaction()) {
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          bookToRemoveLater.Remove();
        }

        using (var transaction = session.OpenTransaction()) {
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          Assert.That(bookToRemoveLater, Is.Not.Null);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        Assert.That(bookToRemoveLater, Is.Not.Null);
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
          Assert.That(bookToRemoveLater, Is.Not.Null);
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        Assert.That(bookToRemoveLater, Is.Not.Null);
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
          Assert.That(countOfBooks, Is.EqualTo(2));
          Assert.That(author.Books.Count, Is.EqualTo(2));
          _ = author.Books.Add(new Book());
        }

        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().First();
          var countOfBooks = session.Query.All<Book>().Count();
          Assert.That(countOfBooks, Is.EqualTo(2));
          Assert.That(author.Books.Count, Is.EqualTo(2));
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          bookToEditLater.Title = "not null title";
        }

        using (var transaction = session.OpenTransaction()) {
          var bookToEditLater = session.Query.Single<Book>(bookToEditLaterKey);
          var bookToRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
          Assert.That("not null title", Is.Not.EqualTo(bookToEditLater.Title));
          Assert.That(PersistenceState.Synchronized, Is.EqualTo(bookToEditLater.PersistenceState));
          var editedBook = session.Query.Single<Book>(bookToEditLater.Key);
          Assert.That("not null title", Is.Not.EqualTo(editedBook.Title));
          var author = bookToRemoveLater.Authors.First();
          _ = author.Books.Remove(bookToRemoveLater);
          bookToRemoveLater.Remove();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var countOfBooks = session.Query.All<Book>().Count();
        var author = session.Query.All<Author>().First();
        Assert.That(countOfBooks, Is.EqualTo(2));
        Assert.That(author.Books.Count, Is.EqualTo(2));
        var bookForEditLater = session.Query.Single<Book>(bookToEditLaterKey);
        var bookForRemoveLater = session.Query.Single<Book>(bookToRemoveLaterKey);
        Assert.That(bookForRemoveLater, Is.Not.Null);
        Assert.That(string.IsNullOrEmpty(bookForEditLater.Title), Is.True);
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
          Assert.That(countOfBooks, Is.EqualTo(2));
          Assert.That(countOfAuthors, Is.EqualTo(1));
          Assert.That(author, Is.Not.Null);
          Assert.That(author.Books.Count, Is.EqualTo(0));
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
        Assert.That(countOfBooks, Is.EqualTo(2));
        Assert.That(countOfAuthors, Is.EqualTo(1));
        Assert.That(author, Is.Not.Null);
        Assert.That(author.Books.Count, Is.EqualTo(0));
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
          Assert.That(autor.Books.Count, Is.EqualTo(0));
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.That(author.Books.Count, Is.EqualTo(0));
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
          Assert.That(autor.Books.Count, Is.EqualTo(1));
          transaction.Complete();
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.That(author.Books.Count, Is.EqualTo(1));
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
          Assert.That(author.Books.Count, Is.EqualTo(2));
        }
      }

      using (var session = Domain.OpenSession(serverProfile))
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.That(author.Books.Count, Is.EqualTo(2));
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
          Assert.That(author.Books.Count, Is.EqualTo(2));
        }
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.That(author.Books.Count, Is.EqualTo(2));
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
          Assert.That(author.Books.Count, Is.EqualTo(1));
        }
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = session.Query.All<Author>().FirstOrDefault();
        Assert.That(author.Books.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void ClientProfileVersionsTest()
    {
      Require.ProviderIsNot(StorageProvider.Oracle, "ExecuteNonQuery returns -1 all the time so versioning is not working");

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
        Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Peter"), Is.Not.Null);
        var authorWithSameName = new Author {Name = authorWithUniqueName.Name};

        Exception firstException = null;
        Exception secondException = null;
        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          firstException = exception;
        }
        Assert.That(firstException, Is.Not.Null);

        try {
          session.SaveChanges();
        }
        catch (Exception exception) {
          secondException = exception;
        }
        Assert.That(secondException, Is.Not.Null);
        Assert.That(
            secondException.Message.Substring(0, 20), Is.EqualTo(firstException.Message.Substring(0, 20)));
        authorWithSameName.Name = "Mathew";
        session.SaveChanges();

        Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Mathew"), Is.Not.Null);
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
          Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Peter"), Is.Not.Null);
          var authorWithSameName = new Author {Name = authorWithUniqueName.Name};

          Exception firstException = null;
          Exception secondException = null;
          try {
            session.SaveChanges();
          }
          catch (Exception exception) {
            firstException = exception;
          }
          Assert.That(firstException, Is.Not.Null);

          try {
            session.SaveChanges();
          }
          catch (Exception exception) {
            secondException = exception;
          }
          Assert.That(secondException, Is.Not.Null);
          Assert.That(
            secondException.Message.Substring(0, 20), Is.EqualTo(firstException.Message.Substring(0, 20)));
          authorWithSameName.Name = "Mathew";
          session.SaveChanges();

          Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Mathew"), Is.Not.Null);
          transaction.Complete();
        }
        Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Mathew"), Is.Not.Null);
        Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name=="Peter"), Is.Not.Null);
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

        Assert.That(session.Query.All<Author>().First().Books.Count, Is.EqualTo(2));
        Assert.That(session.Query.All<Book>().First(book => book.Title == "First Book").Authors.Count, Is.EqualTo(1));
        Assert.That(session.Query.All<Book>().First(book => book.Title == "Second Book").Authors.Count, Is.EqualTo(1));

        var secondAutor = new Author { Name = "Peter" };
        _ = session.LookupStateInCache(firstAuthor.Key, Domain.Model.Types["Author"].Fields["Books"], out var state);
        Assert.That(state, Is.Not.Null);
        var booksOfFirstAuthor = new {
          CountOfAddedItems = state.AddedItemsCount,
          CountOfRemovedItems = state.RemovedItemsCount,
          TotalItemsCount = state.TotalItemCount,
          IsLoaded = state.IsLoaded,
          CountOfFetchedItems = state.FetchedItemsCount
        };

        state = null;
        _ = session.LookupStateInCache(firstBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
        Assert.That(state, Is.Not.Null);
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
          Assert.That(session.Query.All<Author>().First().Books.Count, Is.EqualTo(2));
          Assert.That(session.Query.All<Book>().First(book => book.Title == "First Book").Authors.Count, Is.EqualTo(1));
          Assert.That(session.Query.All<Book>().First(book => book.Title == "Second Book").Authors.Count, Is.EqualTo(1));
          state = null;
          _ = session.LookupStateInCache(firstAuthor.Key, Domain.Model.Types["Author"].Fields["Books"], out state);
          Assert.That(state, Is.Not.Null);
          Assert.That(state.AddedItemsCount, Is.EqualTo(booksOfFirstAuthor.CountOfAddedItems));
          Assert.That(state.RemovedItemsCount, Is.EqualTo(booksOfFirstAuthor.CountOfRemovedItems));
          Assert.That(state.FetchedItemsCount, Is.EqualTo(booksOfFirstAuthor.CountOfFetchedItems));
          Assert.That(state.IsLoaded, Is.EqualTo(booksOfFirstAuthor.IsLoaded));
          Assert.That(state.TotalItemCount, Is.EqualTo(booksOfFirstAuthor.TotalItemsCount));
          state = null;
          _ = session.LookupStateInCache(firstBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
          Assert.That(state, Is.Not.Null);
          Assert.That(state.AddedItemsCount, Is.EqualTo(authorsOfFirstBook.CountOfAddedItems));
          Assert.That(state.RemovedItemsCount, Is.EqualTo(authorsOfFirstBook.CountOfRemovedItems));
          Assert.That(state.FetchedItemsCount, Is.EqualTo(authorsOfFirstBook.CountOfFetchedItems));
          Assert.That(state.IsLoaded, Is.EqualTo(authorsOfFirstBook.IsLoaded));
          Assert.That(state.TotalItemCount, Is.EqualTo(authorsOfFirstBook.TotalItemsCount));
          state = null;
          _ = session.LookupStateInCache(secondBook.Key, Domain.Model.Types["Book"].Fields["Authors"], out state);
          Assert.That(state, Is.Not.Null);
          Assert.That(state.AddedItemsCount, Is.EqualTo(authorsOfSecondBook.CountOfAddedItems));
          Assert.That(state.RemovedItemsCount, Is.EqualTo(authorsOfSecondBook.CountOfRemovedItems));
          Assert.That(state.FetchedItemsCount, Is.EqualTo(authorsOfSecondBook.CountOfFetchedItems));
          Assert.That(state.IsLoaded, Is.EqualTo(authorsOfSecondBook.IsLoaded));
          Assert.That(state.TotalItemCount, Is.EqualTo(authorsOfSecondBook.TotalItemsCount));
        }

        secondAutor.Name = "Mathew";
        session.SaveChanges();
        Assert.That(session.Query.All<Author>().Count(), Is.EqualTo(2));
        Assert.That(session.Query.All<Book>().Count(), Is.EqualTo(2));
        Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name == "Peter"), Is.Not.Null);
        Assert.That(session.Query.All<Author>().FirstOrDefault(author => author.Name == "Mathew"), Is.Not.Null);
        Assert.That(session.Query.All<Author>().First(author => author.Name == "Peter").Books.Count, Is.EqualTo(2));
        Assert.That(session.Query.All<Author>().First(author => author.Name == "Mathew").Books.Count, Is.EqualTo(0));
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