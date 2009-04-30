// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SelectTest : NorthwindDOModelTest
  {
    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void OutOfHierarchy()
    {
      Assert.Greater(Query<Person>.All.Count(), 0);
    }

    [Test]
    public void SimpleSelectTest()
    {
      var result = Query<Order>.All;
      QueryDumper.Dump(result);
    }

    [Test]
    public void SimpleConstantTest()
    {
      var products = Query<Product>.All;
      var result =
        from p in products
        select 0;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(0, i);
    }

    [Test]
    public void AnonymousColumn()
    {
      var products = Query<Product>.All.Select(p => new {p.ProductName}.ProductName);
      var list = products.ToList();
    }

    [Test]
    public void AnonymousWithCalculatedColumnsTest()
    {
      var result = Query<Customer>.All.Select(c =>
        new {
          n1 = c.ContactTitle.Length + c.ContactName.Length,
          n2 = c.ContactName.Length + c.ContactTitle.Length
        });
      result = result.Where(i => i.n1 > 10);
      result.ToList();
    }

    [Test]
    public void AnonymousParameterColumn()
    {
      var param = new {ProductName = "name"};
      var products = Query<Product>.All.Select(p => param.ProductName);
      var list = products.ToList();
    }

    [Test]
    public void NewIntArrayTest()
    {
      var result = Query<Customer>.All.Select(x => new []{1, 2});
      QueryDumper.Dump(result);
    }

    [Test]
    public void NewArrayConstantTest()
    {
      var method = MethodInfo.GetCurrentMethod().Name;
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new
                 {
                   Value = new byte[] {1, 2, 3},
                   Method = method,
                   p.ProductName
                 }
        orderby r.ProductName
        where r.Method==method
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(method, i.Method);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void NewPairTest()
    {
      var method = MethodInfo.GetCurrentMethod().Name;
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new
                 {
                   Value = new Pair<string>(p.ProductName, method),
                   Method = method,
                   p.ProductName
                 }
        orderby r.ProductName
        where r.Method==method
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(method, i.Method);
    }

    [Test]
    public void ConstantTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select 0
        where r==0
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(0, i);
    }

    [Test]
    public void ConstantNullStringTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select (string) null;
      var list = result.ToList();
      foreach (var s in list)
        Assert.AreEqual(null, s);
    }

    [Test]
    public void LocalTest()
    {
      int x = 10;
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select x
        where r==x
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(10, i);
      x = 20;
      list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(20, i);
    }


    [Test]
    public void ColumnTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select p.ProductName
        where r!=null
        select r;
      var list = result.ToList();
      foreach (var s in list)
        Assert.IsNotNull(s);
    }

    [Test]
    public void CalculatedColumnTest()
    {
      var products = Query<Product>.All;
      var result = from r in
        from p in products
        select p.UnitsInStock * p.UnitPrice
      where r > 0
      select r;
      var list = result.ToList();
      var checkList = products.AsEnumerable().Select(p => p.UnitsInStock * p.UnitPrice).ToList();
      list.SequenceEqual(checkList);
    }

    [Test]
    public void KeyTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select p.Key
        where r!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var k in list) {
        Assert.IsNotNull(k);
        var p = k.Resolve<Product>();
        Assert.IsNotNull(p);
      }
    }

    [Test]
    public void AnonymousTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select new {p.ProductName, p.UnitPrice, p.UnitsInStock};
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnonymousEmptyTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select new {};
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnonymousCalculatedTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new {p.ProductName, TotalPriceInStock = p.UnitPrice * p.UnitsInStock}
        where r.TotalPriceInStock > 0
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void JoinedEntityColumnTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select p.Supplier.CompanyName;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void JoinedEntityTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select p.Supplier
        where r.CompanyName!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void StructureColumnTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select p.Supplier.Address;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void StructureTest()
    {
      var products = Query<Product>.All;
      var result = from a in (
        from p in products
        select p.Supplier.Address)
      where a.Region!=null
      select a.StreetAddress;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void EntitySetTest()
    {
      var suppliers = Query<Supplier>.All;
      var result = from s in suppliers
      select s.Products;
      var list = result.ToList();
    }

    [Test]
    public void AnonymousWithEntityTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new {p.ProductName, Product = p}
        where r.Product!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void AnonymousNestedTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new {p, Desc = new {p.ProductName, p.UnitPrice}}
        where r.Desc.ProductName!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void NestedQueryTest()
    {
      var products = Query<Product>.All;
      var result = from pd in
        from p in products
        select new {ProductKey = p.Key, p.ProductName, TotalPrice = p.UnitPrice * p.UnitsInStock}
      where pd.TotalPrice > 100
      select new {PKey = pd.ProductKey, pd.ProductName, Total = pd.TotalPrice};

      var list = result.ToList();
    }

    [Test]
    public void NestedQueryWithStructuresTest()
    {
      var products = Query<Product>.All;
      var result =
        from a in
          from pd in
            from p in products
            select new {ProductKey = p.Key, SupplierAddress = p.Supplier.Address}
          select new {PKey = pd.ProductKey, pd.SupplierAddress, SupplierCity = pd.SupplierAddress.City}
        select new {a.PKey, a.SupplierAddress, a.SupplierCity};
      var list = result.ToList();
    }


    [Test]
    public void NestedQueryWithEntitiesTest()
    {
      var products = Query<Product>.All;
      var result = from pd in
        from p in products
        select new {ProductKey = p.Key, Product = p}
      select new {PKey = pd.ProductKey, pd.Product};

      var list = result.ToList();
    }

    [Test]
    public void NestedQueryWithAnonymousTest()
    {
      var products = Query<Product>.All;
      var result = from pd in
        from p in products
        select new {ProductKey = p.Key, Product = new {Entity = new {p}, Name = p.ProductName}}
      select new {PKey = pd.ProductKey, pd.Product.Name, A = pd, AProduct = pd.Product, AEntity = pd.Product.Entity};

      var list = result.ToList();
    }

    [Test]
    public void SelectEnumTest()
    {
      var result = from o in Query<Order>.All select o.OrderDate.Value.DayOfWeek;
      result.ToList();
    }

    [Test]
    public void SelectAnonymousEnumTest()
    {
      var result = from o in Query<Order>.All select new {o.OrderDate.Value.DayOfWeek};
      result.ToList();
    }

    [Test]
    public void SelectEnumFieldTest()
    {
      var result = from p in Query<ActiveProduct>.All select p.ProductType;
      foreach (var p in result)
        Assert.AreEqual(p, ProductType.Active);
    }

    [Test]
    public void SelectAnonymousEnumFieldTest()
    {
      var result = from p in Query<ActiveProduct>.All select new {p.ProductType};
      foreach (var p in result)
        Assert.AreEqual(p.ProductType, ProductType.Active);      
    }

    [Test]
    public void SelectCharTest()
    {
      var result = from c in Query<Customer>.All select c.CompanyName[0];
      var list = result.ToList();
    }

    [Test]
    public void SelectByteArrayLengthTest()
    {
      var categories = Query<Category>.All;
      var result = from c in categories select c.Picture.Length;
      var list = result.ToList();
    }

    [Test]
    public void SelectEqualsTest()
    {
      var customers = Query<Customer>.All;
      var result = from c in customers select c.CompanyName.Equals("lalala");
      var list = result.ToList();
    }

    [Test]
    public void DoubleSelectEntitySet1Test()
    {
      IQueryable<EntitySet<Order>> query = Query<Customer>.All.Select(c => c.Orders).Select(c => c);
      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void DoubleSelectEntitySet2Test()
    {
      IQueryable<EntitySet<Order>> query = Query<Customer>.All.Select(c => c).Select(c => c.Orders);
      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void DoubleSelectEntitySet3Test()
    {
      var query = Query<Customer>.All.Select(c => c.Orders.Select(o => o));
//          var query = Query<Customer>.All.Select(c => c.Orders);

      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void NestedAnonymousTest()
    {
      var result = Query<Customer>.All
        .Select(c => new {c})
        .Select(a1 => new {a1})
        .Select(a2 => a2.a1.c.CompanyName);
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntityWithLazyLoadFieldTest()
    {
      var category = Query<Category>.All.Where(c => c.Picture != null).First();
      int columnIndex = Domain.Model.Types[typeof (Category)].Fields["Picture"].MappingInfo.Offset;
      Assert.IsFalse(category.State.Tuple.IsAvailable(columnIndex));
    }

    [Test]
    public void AnonymousSelectTest()
    {
      var result = Query<Order>.All
        .Select(o => new {o.OrderDate, o.Freight})
        .Select(g => g.OrderDate);
      QueryDumper.Dump(result);
    }
  }
}