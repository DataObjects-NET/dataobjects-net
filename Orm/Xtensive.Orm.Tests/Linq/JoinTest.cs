// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.17

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Region=Xtensive.Orm.Tests.ObjectModel.NorthwindDO.Region;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class JoinTest : NorthwindDOModelTest
  {
    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void JoinWrongKeysTest()
    {
      var result = 
        from c in Session.Query.All<Category>()
        join p in Session.Query.All<Product>() on c.Key equals p.Key // Wrong join
        select p;
      var list = result.ToList();
    }

    [Test]
    [Ignore("Fix later")]
    public void EntityJoinWithNullTest()
    {
      var id = Session.Query.All<Product>().First().Id;
      var result = Session.Query.All<Product>()
        .Select(p=>p.Id==id ? null : p)
        .Select(p=>p==null ? null : p.Category)
        .ToList();
      var expected = Session.Query.All<Product>().ToList().Select(p=>p.Id==id ? null : p).Select(p=>p==null ? null : p.Category).ToList();
      Assert.AreEqual(result.Count, expected.Count);
    }

    [Test]
    public void EntityJoinWithNullModifiedTest()
    {
      var id = Session.Query.All<Product>().First().Id;
      var result = Session.Query.All<Product>()
        .Select(p=>(p.Id==id) && (p==null) ? null : 
          (p.Id==id) && (p!=null) ? p.Category /*exception*/ :
          (p.Id!=id) && (p==null) ? null : p.Category)
        .ToList();
      var expected = Session.Query.All<Product>().ToList()
        .Select(p=>p.Id==id ? null : p)
        .Select(p=>p==null ? null : p.Category)
        .ToList();
      Assert.AreEqual(result.Count, expected.Count);
    }


    [Test]
    public void GroupJoinAggregateTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        join o in Session.Query.All<Order>() on c equals o.Customer into ords
        join e in Session.Query.All<Employee>() on c.Address.City equals e.Address.City into emps
        select new {ords = ords.Count(), emps = emps.Count()};
      var list = result.ToList();
      var expected =
        Session.Query.All<Customer>().Select(c => new {
          ords = (int) c.Orders.Count,
          emps = Session.Query.All<Employee>().Where(e => c.Address.City==e.Address.City).Count()
        }).ToList();

      Assert.IsTrue(expected.Except(list).Count()==0);
    }

    [Test]
    public void GroupJoinAggregate2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .GroupJoin(Session.Query.All<Order>(),
          customer => customer.Id,
          order => order.Customer.Id,
          (customer, orders) => new {customer, orders})
        .GroupJoin(Session.Query.All<Employee>(),
          customerOrders => customerOrders.customer.Address.City,
          employee => employee.Address.City,
          (customerOrders, employees) => new {
            ords = customerOrders.orders.Count(),
            emps = employees.Count(),
            sum = employees.Count() + customerOrders.orders.Count()
          }).OrderBy(t => t.emps).ThenBy(t => t.ords).ThenBy(t => t.sum);

      var list = result.ToList();
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .GroupJoin(Session.Query.All<Order>().AsEnumerable(),
          customer => customer.Id,
          order => order.Customer.Id,
          (customer, orders) => new {customer, orders})
        .GroupJoin(Session.Query.All<Employee>().AsEnumerable(),
          customerOrders => customerOrders.customer.Address.City,
          employee => employee.Address.City,
          (customerOrders, employees) => new {
            ords = customerOrders.orders.Count(),
            emps = employees.Count(),
            sum = employees.Count() + customerOrders.orders.Count()
          }).OrderBy(t => t.emps).ThenBy(t => t.ords).ThenBy(t => t.sum).ToList();

      Assert.IsTrue(expected.SequenceEqual(list));

      QueryDumper.Dump(expected, true);
      QueryDumper.Dump(list, true);
    }

    [Test]
    public void SimpleJoinTest()
    {
      var productsCount = Session.Query.All<Product>().Count();
      var result =
        from product in Session.Query.All<Product>()
        join supplier in Session.Query.All<Supplier>() on product.Supplier.Id equals supplier.Id
        select new {product.ProductName, supplier.ContactName, supplier.Phone};
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void SimpleLeftTest()
    {
      var productsCount = Session.Query.All<Product>().Count();
      var result = Session.Query.All<Product>()
        .LeftJoin(Session.Query.All<Supplier>(),
          product => product.Supplier.Id,
          supplier => supplier.Id,
          (product, supplier) => new {product.ProductName, supplier.ContactName, supplier.Phone});
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
    }

    [Test]
    public void LeftJoin1Test()
    {
      Session.Query.All<Territory>().First().Region = null;
      Session.Current.SaveChanges();
      var territories = Session.Query.All<Territory>();
      var regions = Session.Query.All<ObjectModel.NorthwindDO.Region>();
      var result = territories.LeftJoin(
        regions,
        territory => territory.Region,
        region => region,
        (territory, region) => new {
          territory.TerritoryDescription,
          RegionDescription = region==null ? (string) null : region.RegionDescription
        });
      foreach (var item in result)
        Console.WriteLine("{0} {1}", item.RegionDescription, item.TerritoryDescription);
      QueryDumper.Dump(result);
    }

    public void LeftJoin2Test()
    {
      Session.Query.All<Territory>().First().Region = null;
      Session.Current.SaveChanges();
      var territories = Session.Query.All<Territory>();
      var regions = Session.Query.All<ObjectModel.NorthwindDO.Region>();
      var result = territories.LeftJoin(
        regions,
        territory => territory.Region.Id,
        region => region.Id,
        (territory, region) => new {territory.TerritoryDescription, region.RegionDescription});
      foreach (var item in result)
        Console.WriteLine("{0} {1}", item.RegionDescription, item.TerritoryDescription);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SeveralTest()
    {
      var products = Session.Query.All<Product>();
      var productsCount = products.Count();
      var suppliers = Session.Query.All<Supplier>();
      var categories = Session.Query.All<Category>();
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
      var products = Session.Query.All<Product>();
      var productsCount = products.Count();
      var suppliers = Session.Query.All<Supplier>();
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
        from c in Session.Query.All<Customer>()
        join o in Session.Query.All<Order>() on c equals o.Customer
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
    }

    [Test]
    public void AnonymousEntityJoinTest()
    {
      var result =
        from c in Session.Query.All<Customer>()
        join o in Session.Query.All<Order>()
          on new {Customer = c, Name = c.ContactName} equals new {o.Customer, Name = o.Customer.ContactName}
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
    }

    [Test]
    public void JoinByCalculatedColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var localCustomers = customers.ToList();
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
      var list = result.ToList();
      Assert.AreEqual(expected.Count(), result.Count());
    }

    [Test]
    public void GroupJoinTest()
    {
      var categoryCount = Session.Query.All<Category>().Count();
      var result =
        from category in Session.Query.All<Category>()
        join product in Session.Query.All<Product>()
          on category equals product.Category
          into groups
        select groups;

      var expected =
        from category in Session.Query.All<Category>().AsEnumerable()
        join product in Session.Query.All<Product>().AsEnumerable()
          on category equals product.Category
          into groups
        select groups;
      var list = result.ToList();
      Assert.AreEqual(categoryCount, list.Count);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void GroupJoinWithComparerTest()
    {
      var categories = Session.Query.All<Category>();
      var products = Session.Query.All<Product>();
      var result =
        categories.GroupJoin(
          products,
          c => c.Id,
          p => p.Id,
          (c, pGroup) => pGroup,
          AdvancedComparer<int>.Default.EqualityComparerImplementation);
      AssertEx.Throws<QueryTranslationException>(() => result.ToList());
    }

    [Test]
    public void GroupJoinNestedTest()
    {
      var categories = Session.Query.All<Category>();
      var products = Session.Query.All<Product>();
      var categoryCount = categories.Count();
      var result =
        categories.OrderBy(c => c.CategoryName)
          .GroupJoin(products, c => c, p => p.Category, (c, pGroup) => new {
            Category = c.CategoryName,
            Products = pGroup.OrderBy(ip => ip.ProductName)
          });
      var list = result.ToList();
      Assert.AreEqual(categoryCount, list.Count);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void GroupJoinSelectManyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var categories = Session.Query.All<Category>();
        var products = Session.Query.All<Product>();
        var result = categories
          .OrderBy(c => c.CategoryName)
          .GroupJoin(
          products,
          c => c,
          p => p.Category,
          (c, pGroup) => new {c, pGroup})
          .SelectMany(@t1 => @t1.pGroup, (@t1, gp) => new {@t1, gp})
          .OrderBy(@t1 => @t1.gp.ProductName)
          .Select(@t1 => new {Category = @t1.@t1.c.CategoryName, @t1.gp.ProductName})
          ;
        var list = result.ToList();
        QueryDumper.Dump(result, true);
      }
    }

    [Test]
    [ExpectedException(typeof (QueryTranslationException))]
    public void DefaultIfEmptyTest()
    {
      var categories = Session.Query.All<Category>();
      var products = Session.Query.All<Product>();
      var categoryCount = categories.Count();
      var result = categories.GroupJoin(
        products,
        category => category,
        product => product.Category,
        (c, pGroup) => pGroup.DefaultIfEmpty());
      var list = result.ToList();
      Assert.AreEqual(categoryCount, list.Count);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void LeftOuterTest()
    {
      var categories = Session.Query.All<Category>();
      var products = Session.Query.All<Product>();
      var productsCount = products.Count();
      var result = categories.GroupJoin(
        products,
        c => c,
        p => p.Category,
        (c, pGroup) => new {c, pGroup})
        .SelectMany(@t => @t.pGroup.DefaultIfEmpty(), (@t, p) => new {Name = p==null ? "Nothing!" : p.ProductName, @t.c.CategoryName});
      var list = result.ToList();
      Assert.AreEqual(productsCount, list.Count);
      QueryDumper.Dump(result, true);
    }

    [Test]
    public void GroupJoinAnonymousTest()
    {
      var query = Session.Query.All<Supplier>()
        .GroupJoin(Session.Query.All<Product>(), s => s, p => p.Supplier, (s, products) => new {
          s.CompanyName,
          s.ContactName,
          s.Phone,
          Products = products
        });
      QueryDumper.Dump(query);
    }
  }
}