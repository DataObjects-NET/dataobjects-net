// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Tests.Storage.EntitySetModel;

namespace Xtensive.Storage.Tests.Storage.EntitySetModel
{
  [HierarchyRoot]
  public class Publisher : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

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
    public int ID { get; private set; }

    [Field]
    public int Name { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class EntitySetTest : NorthwindDOModelTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (Book).Namespace);
      return config;
    }

    [Test]
    public void SessionActivationTest()
    {
      var session = Session.Open(Domain);

      Author author;

      TransactionScope transactionScope;

      using (session.Activate()) {
        using (transactionScope = Transaction.Open()) {
          author = new Author();
          transactionScope.Complete();
        }

        transactionScope = Transaction.Open();
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

    [Test]
    public void SimpleEntitySetPrefetchTest()
    {
      Key key;
      using (Session.Open(Domain)) {
        using (var tx = Transaction.Open()) {
          var a = new Author();
          key = a.Key;
          for (int i = 0; i < 10; i++)
            a.Books.Add(new Book());
          tx.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var tx = Transaction.Open()) {
          var a = Query.Single<Author>(key);
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
      using (Session.Open(Domain)) {
        Author a;
        using (var t = Transaction.Open()) {
          a = new Author();
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          var books = a.Books; // fetch the author
          var b = new Book();
          books.Add(b);
          Assert.AreEqual(PersistenceState.New, b.PersistenceState);
          t.Complete();
        }
      }
    }

    [Test]
    public void PairedEntitySetTest()
    {
      Author author;
      using (var session = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          author = new Author();
          for (int i = 0; i < 100; i++) {
            var book = new Book() { Name = i };
            author.Books.Add(book);
          }
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          var list = author.Books.ToList();
          foreach (var book in list)
          {
            Assert.IsNotNull(book);
          }
        }
      }
    }

    [Test]
    public void NonPairedEntitySetTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var publisher = new Publisher();
        for (int i = 0; i < 100; i++) {
          var book = new Book() {Name = i};
          publisher.Books.Add(book);
        }
        t.Complete();
      }
      using (var t = Transaction.Open()) {
        var publisher = Query.All<Publisher>().First();
        var list = publisher.Books.ToList();
        foreach (var book in list) {
          Assert.IsNotNull(book);
        }
      }
    }

    [Test]
    public void OneToManyTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var categories = Query.All<Category>();
        Assert.AreSame(categories.First().Products, categories.First().Products);
        var resultCount = categories.First().Products.Count();
        var set = categories.First().Products;
        var list = set.ToList();
        var queryResult = list.Count;
        var setCount = categories.First().Products.Count;
        Assert.AreEqual(setCount, queryResult);
        Assert.AreEqual(queryResult, resultCount);
        t.Complete();
      }
    }

    [Test]
    public void ManyToManyTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var employees = Query.All<Employee>();
        var territories = Query.All<Territory>();
        var resultCount = employees.First().Territories.Count();
        var queryResult = employees.First().Territories.ToList().Count();
        Assert.AreEqual(queryResult, resultCount);
        resultCount = territories.First().Employees.Count();
        queryResult = territories.First().Employees.ToList().Count();
        Assert.AreEqual(queryResult, resultCount);
        t.Complete();
      }
    }

    [Test]
    public void NewObjectTest()
    {
      const int bookCount = 10;
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var author = new Author();
        for (int i = 0; i < bookCount; i++)
          author.Books.Add(new Book {Name = i});
        var book = new Book {Name = bookCount};
        author.Books.Add(book);
        Assert.AreEqual(author.Books.Count, bookCount + 1);
        author.Books.Contains(book);
        author.Books.Remove(book);
        Assert.AreEqual(author.Books.Count, bookCount);
        var enumerator = author.Books.GetEnumerator();
        var list = new List<Book>();
        while (enumerator.MoveNext()) 
          list.Add(enumerator.Current);
        Assert.AreEqual(list.Count, author.Books.Count);
        author.Books.Clear();
        Assert.AreEqual(author.Books.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void PersistentObjectTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var category = Query.All<Category>().First();
          var prodsuctCount = category.Products.Count;
          var product = new ActiveProduct();
          category.Products.Add(product);
          Assert.AreEqual(category.Products.Count, prodsuctCount + 1);
          category.Products.Contains(product);
          category.Products.Remove(product);
          Assert.AreEqual(category.Products.Count, prodsuctCount);
          var enumerator = category.Products.GetEnumerator();
          var list = new List<Product>();
          while (enumerator.MoveNext()) 
            list.Add(enumerator.Current);
          Assert.AreEqual(list.Count, category.Products.Count);
          category.Products.Clear();
          Assert.AreEqual(category.Products.Count, 0);
          Session.Current.Persist();
          t.Complete();
        }

        using (var t = Transaction.Open()) {
          var category = Query.All<Category>().First();
          Assert.AreEqual(category.Products.Count, 0);
          var product = new ActiveProduct();
          category.Products.Add(product);
          Session.Current.Persist();
          t.Complete();
        }

        using (var t = Transaction.Open()) {
          var category = Query.All<Category>().First();
          Assert.AreEqual(category.Products.Count, 1);
          t.Complete();
        }
      }
    }

    [Test]
    public void SetOperationsTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var customer = new Customer("QQQ77");
        var orders1 = GenerateOrders(2);
        var orders2 = GenerateOrders(3);
        var orders3 = GenerateOrders(4);
        // UnionWith
        customer.Orders.UnionWith(orders1);
        customer.Orders.UnionWith(orders2);
        var orders = customer.Orders.ToList();
        Assert.IsTrue(orders.ContainsAll(orders1));
        Assert.IsTrue(orders.ContainsAll(orders2));
        // IntersectWith
        customer.Orders.IntersectWith(orders1);
        orders = customer.Orders.ToList();
        Assert.IsTrue(orders.ContainsAll(orders1));
        Assert.IsFalse(orders.ContainsAny(orders2));
        // ExceptWith
        customer.Orders.ExceptWith(orders1);
        orders = customer.Orders.ToList();
        Assert.AreEqual(0, orders.Count);
        // Check all operations with self.
        customer.Orders.UnionWith(orders3);
        customer.Orders.UnionWith(customer.Orders);
        customer.Orders.IntersectWith(customer.Orders);
        customer.Orders.ExceptWith(customer.Orders);
        Assert.AreEqual(0, customer.Orders.Count);
        // rolling back
      }
    }

    [Test]
    public void EnumerateFullyLoadedEntitySetWhenItsOwnerIsRemovedTest()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet(out author0Key, out author1Key);

      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var author0 = Query.Single<Author>(author0Key);
        LoadEntitySetThenRemoveOwnerAndEnumerateIt(author0, author0.Books);

        var author1 = Query.Single<Author>(author1Key);
        LoadEntitySetThenRemoveOwnerAndEnumerateIt(author1, author1.Books);
      }
    }

    [Test]
    public void EnumerateNotLoadedEntitySetWhenItsOwnerIsRemovedTest()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet(out author0Key, out author1Key);

      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var author0 = Query.Single<Author>(author0Key);
        RemoveOwnerAndEnumerateEntitySet(author0, author0.Books);

        var author1 = Query.Single<Author>(author1Key);
        RemoveOwnerAndEnumerateEntitySet(author1, author1.Books);
      }
    }

    [Test]
    public void ExecutingFutureQueryOnEntitySetWhenItsOwnerHasBeenRemovedTest()
    {
      Key author0Key;
      Key author1Key;
      CreateTwoAuthorsAndTheirBooksSet(out author0Key, out author1Key);

      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var author0 = Query.Single<Author>(author0Key);
        var books0 = author0.Books;
        author0.Remove();
        AssertEx.Throws<InvalidOperationException>(() =>
          Query.ExecuteFuture(() => books0.Where(b => b.Name==0)));
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
          a.Books.Add(new Book());
      };
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var bigAuthor = new Author();
        bigKey = bigAuthor.Key;
        generator.Invoke(bigAuthor, itemCountOfBigEntitySet);
        var smallAuthor = new Author();
        smallKey = smallAuthor.Key;
        generator.Invoke(smallAuthor, itemCountOfSmallEntitySet);
        t.Complete();
      }

      var booksField = typeof (Author).GetTypeInfo(Domain).Fields["Books"];
      TestAdd(bigKey, itemCountOfBigEntitySet, booksField);
      TestRemove(bigKey, itemCountOfBigEntitySet + 2, booksField);
      TestSmallEntitySet(smallKey, itemCountOfSmallEntitySet, booksField);
    }

    private void TestAdd(Key key, int itemCount, Xtensive.Storage.Model.FieldInfo booksField)
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var author = Query.Single<Author>(key);
        FetchEntitySet(author.Books);
        author.Books.Add(new Book());
        EntitySetState setState;
        session.Handler.TryGetEntitySetState(key, booksField, out setState);
        Assert.IsNull(setState.TotalItemCount);
        Assert.AreEqual(itemCount + 1, author.Books.Count);
        Assert.AreEqual(itemCount + 1, setState.TotalItemCount);
        author.Books.Add(new Book());
        Assert.AreEqual(itemCount + 2, setState.TotalItemCount);
        t.Complete();
      }
    }

    private void TestRemove(Key key, int itemCount, Xtensive.Storage.Model.FieldInfo booksField)
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var author = Query.Single<Author>(key);
        FetchEntitySet(author.Books);
        var booksToBeRemoved = Query.All<Book>().Where(b => b.Author.Key == key).Take(2).ToList();
        var bookToBeRemoved0 = booksToBeRemoved[0];
        var bookToBeRemoved1 = booksToBeRemoved[1];
        author.Books.Remove(bookToBeRemoved0);
        EntitySetState setState;
        session.Handler.TryGetEntitySetState(key, booksField, out setState);
        Assert.IsNull(setState.TotalItemCount);
        Assert.AreEqual(itemCount - 1, author.Books.Count);
        Assert.AreEqual(itemCount - 1, setState.TotalItemCount);
        author.Books.Remove(bookToBeRemoved1);
        Assert.AreEqual(itemCount - 2, setState.TotalItemCount);
        t.Complete();
      }
    }

    private void TestSmallEntitySet(Key key, int itemCount, Xtensive.Storage.Model.FieldInfo booksField)
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var author = Query.Single<Author>(key);
        FetchEntitySet(author.Books);
        author.Books.Add(new Book());
        EntitySetState setState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(key, booksField, out setState));
        Assert.AreEqual(itemCount+1, setState.TotalItemCount);
      }
    }

    private static void LoadEntitySetThenRemoveOwnerAndEnumerateIt(Author owner, EntitySet<Book> entitySet)
    {
      foreach (var book in entitySet) {}
      RemoveOwnerAndEnumerateEntitySet(owner, entitySet);
    }

    private static void RemoveOwnerAndEnumerateEntitySet(Author owner, EntitySet<Book> entitySet)
    {
      var expectedCount = entitySet.Count;
      owner.Remove();
      var actualCount = 0;
      AssertEx.Throws<InvalidOperationException>(() => entitySet.GetEnumerator().MoveNext());
    }

    private void CreateTwoAuthorsAndTheirBooksSet(out Key author0Key, out Key author1Key)
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        Action<Author, int> bookGenerator = (author, count) => {
          for (var i = 0; i < count; i++)
            author.Books.Add(new Book());
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
    
    private List<Order> GenerateOrders(int count)
    {
      var result = new List<Order>();
      for (int i = 0; i < count; i++)
        result.Add(new Order());
      return result;
    }

    private void FetchEntitySet<T>(EntitySet<T> books) where T : IEntity
    {
      // fancy trick to force loading at most N items (currently N = 32)
      books.Contains(Key.Create(typeof (T), -77));
    }
  }
}