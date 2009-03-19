// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.12

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class SelectTest : NorthwindDOModelTest
  {
    [Test]
    public void SimpleConstantTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from p in products
            select 0;
          var list = result.ToList();
          foreach (var i in list)
            Assert.AreEqual(0, i);
          t.Complete();
        }
      }
    }

    [Test]
    public void NewArrayConstantTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var method = MethodInfo.GetCurrentMethod().Name;
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select new
               {
                 Value = new byte[] {1, 2, 3},
                 Method = method
               }
            where r.Method == method
            select r;
          var list = result.ToList();
          foreach (var i in list)
            Assert.AreEqual(method, i.Method);
          t.Complete();
        }
      }
    }

    [Test]
    public void ConstantTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select 0
            where r == 0
            select r;
          var list = result.ToList();
          foreach (var i in list)
            Assert.AreEqual(0, i);
          t.Complete();
        }
      }
    }

    [Test]
    public void ConstantNullStringTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from p in products
                       select (string)null;
          var list = result.ToList();
          foreach (var s in list)
            Assert.AreEqual(null, s);
          t.Complete();
        }
      }
    }

    [Test]
    public void LocalTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          int x = 10;
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select x
            where r == x
            select r;
          var list = result.ToList();
          foreach (var i in list)
            Assert.AreEqual(10, i);
          x = 20;
          list = result.ToList();
          foreach (var i in list)
            Assert.AreEqual(20, i);
          t.Complete();
        }
      }
    }


    [Test]
    public void ColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select p.ProductName
            where r != null
            select r;
          var list = result.ToList();
          foreach (var s in list)
            Assert.IsNotNull(s);
          t.Complete();
        }
      }
    }

    [Test]
    public void CalculatedColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from r in
              from p in products
              select p.UnitsInStock * p.UnitPrice
            where r > 0
            select r;
          var list = result.ToList();
          var checkList = products.AsEnumerable().Select(p => p.UnitsInStock * p.UnitPrice).ToList();
          list.SequenceEqual(checkList);
          t.Complete();
        }
      }
    }

    [Test]
    public void KeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select p.Key
            where r != null
            select r;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          foreach (var k in list) {
            Assert.IsNotNull(k);
            var p = k.Resolve<Product>();
            Assert.IsNotNull(p);
          }
          t.Complete();
        }
      }
    }

    [Test]
    public void AnonymousTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from p in products
                       select new { p.ProductName, p.UnitPrice, p.UnitsInStock };
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void AnonymousEmptyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from p in products
                       select new {};
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void AnonymousCalculatedTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select new {p.ProductName, TotalPriceInStock = p.UnitPrice * p.UnitsInStock}
            where r.TotalPriceInStock > 0
            select r;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void JoinedEntityColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from p in products
                       select p.Supplier.CompanyName;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }


    [Test]
    public void JoinedEntityTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select p.Supplier
            where r.CompanyName != null
            select r;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void StructureColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from p in products
                       select p.Supplier.Address;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void StructureTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from a in (
                         from p in products
                         select p.Supplier.Address)
                       where a.Region != null
                       select a.StreetAddress;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void EntitySetTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Query<Supplier>.All;
          var result = from s in suppliers
                       select s.Products;
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void AnonymousWithEntityTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select new {p.ProductName, Product = p}
            where r.Product != null
            select r;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }


    [Test]
    public void AnonymousNestedTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result =
            from r in
              from p in products
              select new {p, Desc = new {p.ProductName, p.UnitPrice}}
            where r.Desc.ProductName != null
            select r;
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void NestedQueryTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from pd in 
                         from p in products
                         select new {ProductKey = p.Key, p.ProductName, TotalPrice = p.UnitPrice * p.UnitsInStock}
                       where pd.TotalPrice > 100
                       select new {PKey = pd.ProductKey, pd.ProductName, Total = pd.TotalPrice};
                       
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void NestedQueryWithStructuresTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = 
            from a in 
              from pd in 
                from p in products
                select new {ProductKey = p.Key, SupplierAddress = p.Supplier.Address}
              select new {PKey = pd.ProductKey, pd.SupplierAddress, SupplierCity = pd.SupplierAddress.City}
            select new { a.PKey, a.SupplierAddress, a.SupplierCity };
          var list = result.ToList();
          t.Complete();
        }
      }
    }


    [Test]
    public void NestedQueryWithEntitiesTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from pd in 
                         from p in products
                         select new {ProductKey = p.Key, Product = p}
                       select new {PKey = pd.ProductKey, pd.Product};
                       
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void NestedQueryWithAnonymousTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var result = from pd in 
                         from p in products
                         select new {ProductKey = p.Key, Product = new {Entity = new{p}, Name = p.ProductName}}
                       select new {PKey = pd.ProductKey, pd.Product.Name, A = pd, AProduct = pd.Product, AEntity = pd.Product.Entity};
                       
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void SelectEnumTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var result = from o in orders select o.OrderDate.Value.DayOfWeek;
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void SelectByteArrayLengthTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var categories = Query<Category>.All;
          var result = from c in categories select c.Picture.Length;
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void SelectEqualsTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var result = from c in customers select c.CompanyName.Equals("lalala");
          var list = result.ToList();
          t.Complete();
        }
      }
    }
  }
}