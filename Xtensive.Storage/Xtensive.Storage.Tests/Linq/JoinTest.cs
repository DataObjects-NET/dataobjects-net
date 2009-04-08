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
  [Category("Linq")]
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
        join o in Query<Order>.All
          on new {Customer = c, Name = c.ContactName} equals new {o.Customer, Name = o.Customer.ContactName}
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
    }

    [Test]
    public void JoinByCalculatedColumnTest()
    {
      var customers = Query<Customer>.All;
      var localCustomers = customers.AsEnumerable();
      var expected =
        from c1 in localCustomers
        join c2 in localCustomers
          on c1.CompanyName.Substring(0, 1).ToUpper() equals c2.CompanyName.Substring(0, 1).ToUpper()
        select new {l = c1.CompanyName, r = c2.CompanyName};
      var result =
        from c1 in customers
        join c2 in customers
          on c1.CompanyName.Substring(0, 1).ToUpper() equals c2.CompanyName.Substring(0, 1).ToUpper()
        select new {l = c1.CompanyName, r = c2.CompanyName};
      Assert.AreEqual(expected.Count(), result.Count());
    }

    [Test]
    public void GroupJoinTest()
    {
      var categories = Query<Category>.All;
      var products = Query<Product>.All;
      var categoryCount = categories.Count();
      var result = 
        from c in categories
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
      var result = 
        from c in categories
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
        var result = 
          from c in categories
          orderby c.CategoryName
          join p in products on c equals p.Category into pGroup
          from gp in pGroup
          orderby gp.ProductName
          select new {Category = c.CategoryName, gp.ProductName};
        var list = result.ToList();
      }
    }

    [Test]
    public void DefaultIfEmptyTest()
    {
      var categories = Query<Category>.All;
      var products = Query<Product>.All;
      var categoryCount = categories.Count();
      var result = 
        from c in categories
        join p in products on c equals p.Category into pGroup
        select pGroup.DefaultIfEmpty();
      var list = result.ToList();
      Assert.AreEqual(categoryCount, list.Count);
    }

    [Test]
    public void LeftOuterTest()
    {
      var categories = Query<Category>.All;
      var products = Query<Product>.All;
      var productsCount = products.Count();
      var result = 
        from c in categories
        join p in products on c equals p.Category into pGroup
        from p in pGroup.DefaultIfEmpty()
        select new {Name = p==null ? "Nothing!" : p.ProductName, c.CategoryName};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }
  }
}