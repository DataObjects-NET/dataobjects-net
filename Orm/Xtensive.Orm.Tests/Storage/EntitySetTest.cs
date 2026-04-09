// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Elena Vakhtina
// Created:    2009.03.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Orm.Tests.Storage.EntitySetModel;

namespace Xtensive.Orm.Tests.Storage.EntitySetModel
{
  [HierarchyRoot]
  public class Publisher : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Name { get; set; }

    [Field, Association(PairTo = "Books", OnTargetRemove = OnRemoveAction.Clear)]
    public Author Author { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Name { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }


  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyPublisher : Entity
  {
    public const int SecondKeyValue = 1;

    [Field, Key(0)]
    public Guid Id0 {  get; private set; }

    [Field, Key(1)]
    public int Id1 { get; private set; }

    [Field]
    public EntitySet<CompositeKeyBook> Books { get; private set; }

    public CompositeKeyPublisher(Session session, Guid id0)
      : this(session, id0, SecondKeyValue)
    {
    }

    public CompositeKeyPublisher(Session session, Guid id0, int id1)
      : base(session, id0, id1)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyBook : Entity
  {
    public const int SecondKeyValue = 2;

    [Field, Key(0)]
    public Guid Id0 { get; private set; }

    [Field, Key(1)]
    public int Id1 { get; private set; }

    [Field, Key(2)]
    public int Id2 { get; private set; }

    [Field]
    public int Name { get; set; }

    [Field, Association(PairTo = "Books", OnTargetRemove = OnRemoveAction.Clear)]
    public CompositeKeyAuthor Author { get; private set; }

    public CompositeKeyBook(Session session, Guid id0)
      : this(session, id0, SecondKeyValue, SecondKeyValue)
    {
    }

    public CompositeKeyBook(Session session, Guid id0, int id1, int id2)
      : base(session, id0, id1, id2)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyAuthor : Entity
  {
    public const int SecondKeyValue = 3;

    [Field, Key(0)]
    public Guid Id0 { get; private set; }

    [Field, Key(1)]
    public int Id1 { get; private set; }

    [Field, Key(2)]
    public int Id2 { get; private set; }

    [Field, Key(3)]
    public int Id3 { get; private set; }

    [Field]
    public int Name { get; set; }

    [Field]
    public EntitySet<CompositeKeyBook> Books { get; private set; }

    public CompositeKeyAuthor(Session session, Guid id0)
      : this(session, id0, SecondKeyValue, SecondKeyValue, SecondKeyValue)
    {
    }

    public CompositeKeyAuthor(Session session, Guid id0, int id1, int id2, int id3)
      : base(session, id0, id1, id2, id3)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class EntitySetTest : ChinookDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof (Book).Namespace);
      return config;
    }

    [Test]
    public void SessionActivationTest()
    {
      using (var session = Domain.OpenSession()) {
        Author author;
        TransactionScope transactionScope;

        using (session.Activate()) {
          using (transactionScope = session.OpenTransaction()) {
            author = new Author();
            transactionScope.Complete();
          }

          transactionScope = session.OpenTransaction();
        }

        // Requires manual Session switching, since NorthwindDOModelTest.SetUp 
        // automatically activates a background Session.

        using (Session.Deactivate()) {
          foreach (var book in author.Books) {
          }
        }

        using (session.Activate()) {
          transactionScope.Complete();
          transactionScope.Dispose();
        }
      }
    }

    [Test]
    public void SimpleEntitySetPrefetchTest()
    {
      Key key;
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var a = new Author();
          key = a.Key;
          for (int i = 0; i < 10; i++)
            _ = a.Books.Add(new Book());
          tx.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var a = session.Query.Single<Author>(key);
          a.Books.ToList();
          var b = new Book();
          a.Books.Add(b);
          tx.Complete();
        }
      }
    }

    [Test]
    public void AddNewEntityToEntitySetTest()
    {
      using (var session = Domain.OpenSession()) {
        Author a;
        using (var t = session.OpenTransaction()) {
          a = new Author();
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var books = a.Books; // fetch the author
          var b = new Book();
          _ = books.Add(b);
          Assert.That(b.PersistenceState, Is.EqualTo(PersistenceState.New));
          t.Complete();
        }
      }
    }

    [Test]
    public void PairedEntitySetTest()
    {
      Author author;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          author = new Author();
          for (int i = 0; i < 100; i++) {
            var book = new Book() { Name = i };
            _ = author.Books.Add(book);
          }
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var list = author.Books.ToList();
          foreach (var book in list)
          {
            Assert.That(book, Is.Not.Null);
          }
        }
      }
    }

    [Test]
    public void PairedEntitySetTest2()
    {
      CompositeKeyAuthor author;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          author = new CompositeKeyAuthor(session, Guid.NewGuid());
          for (int i = 0; i < 100; i++) {
            var book = new CompositeKeyBook(session, Guid.NewGuid()) { Name = i };
            _ = author.Books.Add(book);
          }
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var list = author.Books.ToList();
          foreach (var book in list) {
            Assert.That(book, Is.Not.Null);
          }
        }
      }
    }

    [Test]
    public void NonPairedEntitySetTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var publisher = new Publisher();
          for (int i = 0; i < 100; i++) {
            var book = new Book() { Name = i };
            _ = publisher.Books.Add(book);
          }
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var publisher = session.Query.All<Publisher>().First();
          var list = publisher.Books.ToList();
          foreach (var book in list) {
            Assert.That(book, Is.Not.Null);
          }
        }
      }
    }

    [Test]
    public void NonPairedEntitySetTest2()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var publisher = new CompositeKeyPublisher(session, Guid.NewGuid());
          for (int i = 0; i < 100; i++) {
            var book = new CompositeKeyBook(session, Guid.NewGuid()) { Name = i };
            _ = publisher.Books.Add(book);
          }
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var publisher = session.Query.All<CompositeKeyPublisher>().First();
          var list = publisher.Books.ToList();
          foreach (var book in list) {
            Assert.That(book, Is.Not.Null);
          }
        }
      }
    }

    [Test]
    public void OneToManyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var categories = session.Query.All<Playlist>();
        Assert.That(categories.First().Tracks, Is.SameAs(categories.First().Tracks));
        var resultCount = categories.First().Tracks.Count();
        var set = categories.First().Tracks;
        var list = set.ToList();
        var queryResult = list.Count;
        var setCount = categories.First().Tracks.Count;
        Assert.That(queryResult, Is.EqualTo(setCount));
        Assert.That(resultCount, Is.EqualTo(queryResult));
        t.Complete();
      }
    }

    [Test]
    public void ManyToManyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var playlists = session.Query.All<Playlist>();
        var tracks = session.Query.All<Track>();
        var resultCount = playlists.First().Tracks.Count();
        var queryResult = playlists.First().Tracks.ToList().Count();
        Assert.That(resultCount, Is.EqualTo(queryResult));
        resultCount = tracks.First().Playlists.Count();
        queryResult = tracks.First().Playlists.ToList().Count();
        Assert.That(resultCount, Is.EqualTo(queryResult));
        t.Complete();
      }
    }

    [Test]
    public void NewObjectTest()
    {
      const int bookCount = 10;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = new Author();
        for (int i = 0; i < bookCount; i++)
          _ = author.Books.Add(new Book {Name = i});
        var book = new Book {Name = bookCount};
        _ = author.Books.Add(book);
        Assert.That(bookCount + 1, Is.EqualTo(author.Books.Count));
        _ = author.Books.Contains(book);
        _ = author.Books.Remove(book);
        Assert.That(bookCount, Is.EqualTo(author.Books.Count));
        var enumerator = author.Books.GetEnumerator();
        var list = new List<Book>();
        while (enumerator.MoveNext()) 
          list.Add(enumerator.Current);
        Assert.That(author.Books.Count, Is.EqualTo(list.Count));
        author.Books.Clear();
        Assert.That(0, Is.EqualTo(author.Books.Count));
        t.Complete();
      }
    }

    [Test]
    public void NewObjectTest2()
    {
      const int bookCount = 10;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = new CompositeKeyAuthor(session, Guid.NewGuid());
        for (int i = 0; i < bookCount; i++)
          _ = author.Books.Add(new CompositeKeyBook(session, Guid.NewGuid()) { Name = i });
        var book = new CompositeKeyBook(session, Guid.NewGuid()) { Name = bookCount };
        _ = author.Books.Add(book);
        Assert.That(bookCount + 1, Is.EqualTo(author.Books.Count));
        _ = author.Books.Contains(book);
        _ = author.Books.Remove(book);
        Assert.That(bookCount, Is.EqualTo(author.Books.Count));
        var enumerator = author.Books.GetEnumerator();
        var list = new List<CompositeKeyBook>();
        while (enumerator.MoveNext())
          list.Add(enumerator.Current);
        Assert.That(author.Books.Count, Is.EqualTo(list.Count));
        author.Books.Clear();
        Assert.That(0, Is.EqualTo(author.Books.Count));
        t.Complete();
      }
    }

    [Test]
    public void PersistentObjectTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var playlist = session.Query.All<Playlist>().First();
          var trackCount = playlist.Tracks.Count;
          var track = new AudioTrack {Name = "Temp1"};
          _ = playlist.Tracks.Add(track);
          Assert.That(trackCount + 1, Is.EqualTo(playlist.Tracks.Count));
          _ = playlist.Tracks.Contains(track);
          _ = playlist.Tracks.Remove(track);
          Assert.That(trackCount, Is.EqualTo(playlist.Tracks.Count));
          var enumerator = playlist.Tracks.GetEnumerator();
          var list = new List<Track>();
          while (enumerator.MoveNext()) 
            list.Add(enumerator.Current);
          Assert.That(playlist.Tracks.Count, Is.EqualTo(list.Count));
          playlist.Tracks.Clear();
          Assert.That(0, Is.EqualTo(playlist.Tracks.Count));
          Session.Current.SaveChanges();
          t.Complete();
        }

        using (var t = session.OpenTransaction()) {
          var category = session.Query.All<Playlist>().First();
          Assert.That(0, Is.EqualTo(category.Tracks.Count));
          var track = new VideoTrack() {Name = "Temp2"};
          _ = category.Tracks.Add(track);
          Session.Current.SaveChanges();
          t.Complete();
        }

        using (var t = session.OpenTransaction()) {
          var playlist = session.Query.All<Playlist>().First();
          Assert.That(1, Is.EqualTo(playlist.Tracks.Count));
          t.Complete();
        }
      }
    }

    [Test]
    public void SetOperationsTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var customer = new Customer{FirstName = "test", LastName = "test2"};
        var invoices1 = GenerateInvoices(2);
        var invoices2 = GenerateInvoices(3);
        var invoices3 = GenerateInvoices(4);
        // UnionWith
        customer.Invoices.UnionWith(invoices1);
        customer.Invoices.UnionWith(invoices2);
        var invoices = customer.Invoices.ToList();
        Assert.That(invoices.ContainsAll(invoices1), Is.True);
        Assert.That(invoices.ContainsAll(invoices2), Is.True);
        // IntersectWith
        customer.Invoices.IntersectWith(invoices1);
        invoices = customer.Invoices.ToList();
        Assert.That(invoices.ContainsAll(invoices1), Is.True);
        Assert.That(invoices.ContainsAny(invoices2), Is.False);
        // ExceptWith
        customer.Invoices.ExceptWith(invoices1);
        invoices = customer.Invoices.ToList();
        Assert.That(invoices.Count, Is.EqualTo(0));
        // Check all operations with self.
        customer.Invoices.UnionWith(invoices3);
        customer.Invoices.UnionWith(customer.Invoices);
        customer.Invoices.IntersectWith(customer.Invoices);
        customer.Invoices.ExceptWith(customer.Invoices);
        Assert.That(customer.Invoices.Count, Is.EqualTo(0));
        // rolling back
      }
    }

    [Test]
    public void EnumerateFullyLoadedEntitySetWhenItsOwnerIsRemovedTest()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet(out author0Key, out author1Key);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author0 = session.Query.Single<Author>(author0Key);
        LoadEntitySetThenRemoveOwnerAndEnumerateIt(author0, author0.Books);

        var author1 = session.Query.Single<Author>(author1Key);
        LoadEntitySetThenRemoveOwnerAndEnumerateIt(author1, author1.Books);
      }
    }

    [Test]
    public void EnumerateFullyLoadedEntitySetWhenItsOwnerIsRemovedTest2()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet2(out author0Key, out author1Key);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author0 = session.Query.Single<CompositeKeyAuthor>(author0Key);
        LoadEntitySetThenRemoveOwnerAndEnumerateIt2(author0, author0.Books);

        var author1 = session.Query.Single<CompositeKeyAuthor>(author1Key);
        LoadEntitySetThenRemoveOwnerAndEnumerateIt2(author1, author1.Books);
      }
    }

    [Test]
    public void EnumerateNotLoadedEntitySetWhenItsOwnerIsRemovedTest()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet(out author0Key, out author1Key);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author0 = session.Query.Single<Author>(author0Key);
        RemoveOwnerAndEnumerateEntitySet(author0, author0.Books);

        var author1 = session.Query.Single<Author>(author1Key);
        RemoveOwnerAndEnumerateEntitySet(author1, author1.Books);
      }
    }

    [Test]
    public void EnumerateNotLoadedEntitySetWhenItsOwnerIsRemovedTest2()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet2(out author0Key, out author1Key);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author0 = session.Query.Single<CompositeKeyAuthor>(author0Key);
        RemoveOwnerAndEnumerateEntitySet2(author0, author0.Books);

        var author1 = session.Query.Single<CompositeKeyAuthor>(author1Key);
        RemoveOwnerAndEnumerateEntitySet2(author1, author1.Books);
      }
    }

    [Test]
    public void ExecutingFutureQueryOnEntitySetWhenItsOwnerHasBeenRemovedTest()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet(out author0Key, out author1Key);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author0 = session.Query.Single<Author>(author0Key);
        var books0 = author0.Books;
        author0.Remove();
        AssertEx.Throws<InvalidOperationException>(() =>
          session.Query.CreateDelayedQuery(_ => books0.Where(b => b.Name==0)));
      }
    }

    [Test]
    public void CountPropertyBehaviorTest()
    {
      const int itemCountOfBigEntitySet = 50;
      const int itemCountOfSmallEntitySet = 30;
      Key bigKey;
      Key smallKey;
      Action<Author, int> generator = (a, count) => {
        for (var i = 0; i < count; i++)
          _ = a.Books.Add(new Book());
      };
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var bigAuthor = new Author();
        bigKey = bigAuthor.Key;
        generator.Invoke(bigAuthor, itemCountOfBigEntitySet);
        var smallAuthor = new Author();
        smallKey = smallAuthor.Key;
        generator.Invoke(smallAuthor, itemCountOfSmallEntitySet);
        t.Complete();
      }

      var booksField = Domain.Model.Types[typeof (Author)].Fields["Books"];
      TestAdd1(bigKey, itemCountOfBigEntitySet, booksField);
      TestRemove1(bigKey, itemCountOfBigEntitySet + 2, booksField);
      TestSmallEntitySet1(smallKey, itemCountOfSmallEntitySet, booksField);
    }

    [Test]
    public void CountPropertyBehaviorTest2()
    {
      const int itemCountOfBigEntitySet = 50;
      const int itemCountOfSmallEntitySet = 30;
      Key bigKey;
      Key smallKey;
      Action<CompositeKeyAuthor, int> generator = (a, count) => {
        for (var i = 0; i < count; i++)
          _ = a.Books.Add(new CompositeKeyBook(a.Session, Guid.NewGuid()));
      };
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var bigAuthor = new CompositeKeyAuthor(session, Guid.NewGuid());
        bigKey = bigAuthor.Key;
        generator.Invoke(bigAuthor, itemCountOfBigEntitySet);
        var smallAuthor = new CompositeKeyAuthor(session, Guid.NewGuid());
        smallKey = smallAuthor.Key;
        generator.Invoke(smallAuthor, itemCountOfSmallEntitySet);
        t.Complete();
      }

      var booksField = Domain.Model.Types[typeof(CompositeKeyAuthor)].Fields["Books"];
      TestAdd2(bigKey, itemCountOfBigEntitySet, booksField);
      TestRemove2(bigKey, itemCountOfBigEntitySet + 2, booksField);
      TestSmallEntitySet2(smallKey, itemCountOfSmallEntitySet, booksField);
    }

    private void TestAdd1(Key key, int itemCount, Orm.Model.FieldInfo booksField)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = session.Query.Single<Author>(key);
        FetchEntitySet(author.Books);
        _ = author.Books.Add(new Book());
        _ = session.Handler.LookupState(key, booksField, out var setState);
        Assert.That(setState.TotalItemCount, Is.Null);
        Assert.That(author.Books.Count, Is.EqualTo(itemCount + 1));
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount + 1));
        _ = author.Books.Add(new Book());
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount + 2));
        t.Complete();
      }
    }

    private void TestAdd2(Key key, int itemCount, Orm.Model.FieldInfo booksField)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = session.Query.Single<CompositeKeyAuthor>(key);
        FetchEntitySet(author.Books);
        _ = author.Books.Add(new CompositeKeyBook(session, Guid.NewGuid()));
        _ = session.Handler.LookupState(key, booksField, out var setState);
        Assert.That(setState.TotalItemCount, Is.Null);
        Assert.That(author.Books.Count, Is.EqualTo(itemCount + 1));
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount + 1));
        _ = author.Books.Add(new CompositeKeyBook(session, Guid.NewGuid()));
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount + 2));
        t.Complete();
      }
    }

    private void TestRemove1(Key key, int itemCount, Xtensive.Orm.Model.FieldInfo booksField)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = session.Query.Single<Author>(key);
        FetchEntitySet(author.Books);
        var booksToBeRemoved = session.Query.All<Book>().Where(b => b.Author.Key == key).Take(2).ToList();
        var bookToBeRemoved0 = booksToBeRemoved[0];
        var bookToBeRemoved1 = booksToBeRemoved[1];
        _ = author.Books.Remove(bookToBeRemoved0);
        _ = session.Handler.LookupState(key, booksField, out var setState);
        Assert.That(setState.TotalItemCount, Is.Null);
        Assert.That(author.Books.Count, Is.EqualTo(itemCount - 1));
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount - 1));
        _ = author.Books.Remove(bookToBeRemoved1);
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount - 2));
        t.Complete();
      }
    }

    private void TestRemove2(Key key, int itemCount, Xtensive.Orm.Model.FieldInfo booksField)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = session.Query.Single<CompositeKeyAuthor>(key);
        FetchEntitySet(author.Books);
        var booksToBeRemoved = session.Query.All<CompositeKeyBook>().Where(b => b.Author.Key == key).Take(2).ToList();
        var bookToBeRemoved0 = booksToBeRemoved[0];
        var bookToBeRemoved1 = booksToBeRemoved[1];
        _ = author.Books.Remove(bookToBeRemoved0);
        _ = session.Handler.LookupState(key, booksField, out var setState);
        Assert.That(setState.TotalItemCount, Is.Null);
        Assert.That(author.Books.Count, Is.EqualTo(itemCount - 1));
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount - 1));
        _ = author.Books.Remove(bookToBeRemoved1);
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount - 2));
        t.Complete();
      }
    }

    private void TestSmallEntitySet1(Key key, int itemCount, Xtensive.Orm.Model.FieldInfo booksField)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = session.Query.Single<Author>(key);
        FetchEntitySet(author.Books);
        _ = author.Books.Add(new Book());
        Assert.That(session.Handler.LookupState(key, booksField, out var setState), Is.True);
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount + 1));
      }
    }

    private void TestSmallEntitySet2(Key searchKey, int itemCount, Xtensive.Orm.Model.FieldInfo booksField)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var author = session.Query.Single<CompositeKeyAuthor>(searchKey);
        FetchEntitySet(author.Books);
        _ = author.Books.Add(new CompositeKeyBook(session, Guid.NewGuid()));
        EntitySetState setState;
        Assert.That(session.Handler.LookupState(searchKey, booksField, out setState), Is.True);
        Assert.That(setState.TotalItemCount, Is.EqualTo(itemCount + 1));
      }
    }

    private static void LoadEntitySetThenRemoveOwnerAndEnumerateIt(Author owner, EntitySet<Book> entitySet)
    {
      foreach (var book in entitySet) {}
      RemoveOwnerAndEnumerateEntitySet(owner, entitySet);
    }

    private static void LoadEntitySetThenRemoveOwnerAndEnumerateIt2(CompositeKeyAuthor owner, EntitySet<CompositeKeyBook> entitySet)
    {
      foreach (var book in entitySet) { }
      RemoveOwnerAndEnumerateEntitySet2(owner, entitySet);
    }

    private static void RemoveOwnerAndEnumerateEntitySet(Author owner, EntitySet<Book> entitySet)
    {
      var expectedCount = entitySet.Count;
      owner.Remove();
      _ = entitySet.GetEnumerator().MoveNext();
    }

    private static void RemoveOwnerAndEnumerateEntitySet2(CompositeKeyAuthor owner, EntitySet<CompositeKeyBook> entitySet)
    {
      var expectedCount = entitySet.Count;
      owner.Remove();
      _ = entitySet.GetEnumerator().MoveNext();
    }

    private void CreateTwoAuthorsAndTheirBooksSet(out Key author0Key, out Key author1Key)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Action<Author, int> bookGenerator = (author, count) => {
          for (var i = 0; i < count; i++)
            _ = author.Books.Add(new Book());
        };
        var author0 = new Author();
        author0Key = author0.Key;
        bookGenerator.Invoke(author0, 5);
        var author1 = new Author();
        author1Key = author1.Key;
        bookGenerator.Invoke(author1, 50);
        t.Complete();
      }
    }

    private void CreateTwoAuthorsAndTheirBooksSet2(out Key author0Key, out Key author1Key)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Action<CompositeKeyAuthor, int> bookGenerator = (author, count) => {
          for (var i = 0; i < count; i++)
            _ = author.Books.Add(new CompositeKeyBook(session, Guid.NewGuid()));
        };
        var author0 = new CompositeKeyAuthor(session, Guid.NewGuid());
        author0Key = author0.Key;
        bookGenerator.Invoke(author0, 5);
        var author1 = new CompositeKeyAuthor(session, Guid.NewGuid());
        author1Key = author1.Key;
        bookGenerator.Invoke(author1, 50);
        t.Complete();
      }
    }

    private List<Invoice> GenerateInvoices(int count)
    {
      var result = new List<Invoice>();
      for (var i = 0; i < count; i++)
        result.Add(new Invoice());
      return result;
    }

    private void FetchEntitySet<T>(EntitySet<T> books) where T : IEntity
    {
      var keyFieldCount = books.Session.Domain.Model.Types[typeof(T)].Key.TupleDescriptor.Count;
      if (keyFieldCount == 1) {
        // fancy trick to force loading at most N items (currently N = 32)
        _ = books.Contains(Key.Create(Domain, typeof(T), -77));
      }
      else if (keyFieldCount == 2) {
        // fancy trick to force loading at most N items (currently N = 32)
        _ = books.Contains(Key.Create(Domain, typeof(T), Guid.NewGuid(), -77));
      }
      else if (keyFieldCount == 3) {
        // fancy trick to force loading at most N items (currently N = 32)
        _ = books.Contains(Key.Create(Domain, typeof(T), Guid.NewGuid(), -77, -66));
      }
      else if (keyFieldCount == 4) {
        // fancy trick to force loading at most N items (currently N = 32)
        _ = books.Contains(Key.Create(Domain, typeof(T), Guid.NewGuid(), -77, -66, -55));
      }
    }
  }
}