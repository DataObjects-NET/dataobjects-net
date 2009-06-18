// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.11

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Tests.Storage.EntitySetModel;

namespace Xtensive.Storage.Tests.Storage.EntitySetModel
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public int Name { get; set; }

    [Field, Association(PairTo = "Books")]
    public Author Author { get; private set; }
  }

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
    public void OneToManyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var categories = Query<Category>.All;
        var resultCount = categories.First().Products.Count();
        var queryResult = categories.First().Products.ToList().Count();
        Assert.AreEqual(queryResult, resultCount);
        t.Complete();
      }
    }

    [Test]
    public void ManyToManyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var employees = Query<Employee>.All;
        var territories = Query<Territory>.All;
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
      using (Domain.OpenSession())
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
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var category = Query<Category>.All.First();
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
          var category = Query<Category>.All.First();
          Assert.AreEqual(category.Products.Count, 0);
          var product = new ActiveProduct();
          category.Products.Add(product);
          Session.Current.Persist();
          t.Complete();
        }

        using (var t = Transaction.Open()) {
          var category = Query<Category>.All.First();
          Assert.AreEqual(category.Products.Count, 1);
          t.Complete();
        }
      }
    }

    [Test]
    public void SetOperationsTest()
    {
      using (Domain.OpenSession())
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

    private List<Order> GenerateOrders(int count)
    {
      var result = new List<Order>();
      for (int i = 0; i < count; i++)
        result.Add(new Order());
      return result;
    }
  }
}