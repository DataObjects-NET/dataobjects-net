// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.17

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class JoinTest : NorthwindDOModelTest
  {
    [Test]
    public void SingleTest()
    {
      var products = Query<Product>.All;
      var productsCount = products.Count();
      var suppliers = Query<Supplier>.All;
      var result = from p in products
      join s in suppliers on p.Supplier.Id equals s.Id
      select new {p.ProductName, s.ContactName, s.Phone};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void SeveralTest()
    {
      var products = Query<Product>.All;
      var productsCount = products.Count();
      var suppliers = Query<Supplier>.All;
      var categories = Query<Category>.All;
      var result = from p in products
      join s in suppliers on p.Supplier.Id equals s.Id
      join c in categories on p.Category.Id equals c.Id
      select new {p, s, c.CategoryName};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void OneToManyTest()
    {
      var products = Query<Product>.All;
      var productsCount = products.Count();
      var suppliers = Query<Supplier>.All;
      var result = from s in suppliers
      join p in products on s.Id equals p.Supplier.Id
      select new {p.ProductName, s.ContactName};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void EntityJoinTest()
    {
      var result =
        from c in Query<Customer>.All
        join o in Query<Order>.All on c equals o.Customer
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
    }

    [Test]
    public void AnonymousEntityJoinTest()
    {
      var result =
        from c in Query<Customer>.All
        join o in Query<Order>.All on new {Customer = c, Name = c.ContactName} equals new {Customer = o.Customer, Name = o.Customer.ContactName}
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
    }

    [Test]
    public void GroupJoinTest()
    {
      var categories = Query<Category>.All;
      var products = Query<Product>.All;
      var categoryCount = categories.Count();
      var result = from c in categories
      join p in products on c equals p.Category into pGroup
      select pGroup;
      var list = result.ToList();
      Assert.AreEqual(categoryCount, list.Count);
    }

    [Test]
    public void GroupJoinNestedTest()
    {
      var categories = Query<Category>.All;
      var products = Query<Product>.All;
      var categoryCount = categories.Count();
      var result = from c in categories
      orderby c.CategoryName
      join p in products on c equals p.Category into pGroup
      select new
             {
               Category = c.CategoryName,
               Products = from ip in pGroup
               orderby ip.ProductName
               select ip
             };
      var list = result.ToList();
      Assert.AreEqual(categoryCount, list.Count);
    }

    [Test]
    public void GroupJoinSelectManyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var categories = Query<Category>.All;
        var products = Query<Product>.All;
        var productsCount = products.Count();
        var result = from c in categories
        orderby c.CategoryName
        join p in products on c equals p.Category into pGroup
        from gp in pGroup
        orderby gp.ProductName
        select new {Category = c.CategoryName, gp.ProductName};
        var list = result.ToList();
      }
    }

    [Test]
    public void LeftOuterTest()
    {
      var categories = Query<Category>.All;
      var products = Query<Product>.All;
      var categoryCount = categories.Count();
      var result = from c in categories
      join p in products on c equals p.Category into pGroup
      select pGroup.DefaultIfEmpty(new ActiveProduct() {ProductName = "Nothing!", Category = c});
      var list = result.ToList();
      Assert.AreEqual(categoryCount, list.Count);
    }

    [Test]
    public void LeftOuterNestedTest()
    {
      var categories = Query<Category>.All;
      var products = Query<Product>.All;
      var productsCount = products.Count();
      var result = from c in categories
      join p in products on c equals p.Category into pGroup
      from pg in pGroup.DefaultIfEmpty()
      select new {Name = pg==null ? "Nothing!" : pg.ProductName, CategoryID = c.Id};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void SelectManyTest()
    {
      var products = Query<Product>.All;
      var productsCount = products.Count();
      var suppliers = Query<Supplier>.All;
      var categories = Query<Category>.All;
      var result = from p in products
      from s in suppliers
      from c in categories
      where p.Supplier==s && p.Category==c
      select new {p, s, c.CategoryName};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void SelectManyJoinedTest()
    {
      var ordersCount = Query<Order>.All.Count();
      var result = from c in Query<Customer>.All
      from o in Query<Order>.All.Where(o => o.Customer==c)
      select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(ordersCount, list.Count);
    }

    [Test]
    public void SelectManyJoinedDefaultIfEmptyTest()
    {
      var assertCount =
        Query<Order>.All.Count() +
          Query<Customer>.All.Count(c => !Query<Order>.All.Any(o => o.Customer==c));
      var result = from c in Query<Customer>.All
      from o in Query<Order>.All.Where(o => o.Customer==c).DefaultIfEmpty()
      select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
      Assert.AreEqual(assertCount, list.Count);
    }
  }
}