// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.12

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class SelectTest : AutoBuildTest
  {
    private Supplier supplier20;
    private Category category;

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.ObjectModel.NorthwindDO");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Xtensive.Storage.Domain result = base.BuildDomain(configuration);
      return result;
    }

    [TestFixtureSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Domain.OpenSession())
      {
        using (var t = Transaction.Open())
        {
          category = new Category();
          category.CategoryName = "Category";
          category.Description = "Description of category";
          for (int i = 0; i < 100; i++)
          {
            var supplier = new Supplier();
            supplier.Address = new Address();
            supplier.Address.City = "City" + i;
            supplier.Address.Country = "Country" + i;
            supplier.Address.PostalCode = "Code" + i;
            supplier.Address.StreetAddress = "Address" + i;
            supplier.Address.Region = "Region" + i;
            supplier.CompanyName = "Company" + i;
            supplier.ContactName = "Contact" + i;
            supplier.ContactTitle = "Title" + i;
            supplier.Phone = "Phone" + i;
            supplier.Fax = "Fax" + i;
            supplier.HomePage = "www.homepage.com" + i;
            if (supplier.Id == 20)
              supplier20 = supplier;
            for (int j = 0; j < 10; j++)
            {
              Product product = new Product();
              product.ProductName = string.Format("Product_{0}_{1}", i, j);
              product.UnitPrice = j;
              product.UnitsInStock = 10;
              product.UnitsOnOrder = 1;
              product.Category = category;
              product.Supplier = supplier;
            }

          }
          t.Complete();
        }
      }
    }

    [Test]
    public void ConstantTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select 0;
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
          var products = Session.Current.All<Product>();
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
// ReSharper disable ConvertToConstant
          int x = 10;
// ReSharper restore ConvertToConstant
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select x;
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select p.ProductName;
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select p.UnitsInStock * p.UnitPrice;
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select p.Key;
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
          var products = Session.Current.All<Product>();
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
          var products = Session.Current.All<Product>();
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select new { p.ProductName, TotalPriceInStock = p.UnitPrice * p.UnitsInStock };
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
          var products = Session.Current.All<Product>();
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select p.Supplier;
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select p.Supplier.Address;
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
          var suppliers = Session.Current.All<Supplier>();
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select new { p.ProductName, Product = p };
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
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select new { p, Desc = new {p.ProductName, p.UnitPrice} };
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void CorrelatedQueryTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var result = from p in products
                       select (
                         from s in suppliers
                         where s.Id == p.Supplier.Id
                         select s.CompanyName
                       );
          var list = result.ToList();
          Assert.Greater(list.Count , 0);
          foreach (var strings in list) {
            foreach (var s in strings) {
              Assert.IsNotNull(s);
            }
          }
          t.Complete();
        }
      }
    }

    [Test]
    public void CorrelatedQueryAnonymousTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var result = from p in products
                       select new {
                         Suppliers = from s in suppliers
                                     where s.Id == p.Supplier.Id
                                     select s.CompanyName
                       };
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          foreach (var p in list) {
            foreach (var companyName in p.Suppliers) {
              Assert.IsNotNull(companyName);
            }
          }
          t.Complete();
        }
      }
    }

    [Test]
    public void NestedQueryTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var result = from pd in (
                         from p in products
                         select new {ProductKey = p.Key, p.ProductName, TotalPrice = p.UnitPrice * p.UnitsInStock}
                       )
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
          var products = Session.Current.All<Product>();
          var result = from pd in (
                         from p in products
                         select new {ProductKey = p.Key, SupplierAddress = p.Supplier.Address}
                       )
                       select new {PKey = pd.ProductKey, pd.SupplierAddress, SupplierCity = pd.SupplierAddress.City};
                       
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
          var products = Session.Current.All<Product>();
          var result = from pd in (
                         from p in products
                         select new {ProductKey = p.Key, Product = p}
                       )
                       select new {PKey = pd.ProductKey, pd.Product};
                       
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void NestedQueryWithAnonimousTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var result = from pd in (
                         from p in products
                         select new {ProductKey = p.Key, Product = new {Entity = p, Name = p.ProductName}}
                       )
                       select new {PKey = pd.ProductKey, pd.Product.Name, Anonimous = pd.Product, Product = pd.Product.Entity};
                       
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void MultipleTest()
    {
      using (Domain.OpenSession())
      {
        using (var t = Transaction.Open())
        {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var categories = Session.Current.All<Category>();
          var result = from p in products
                       from s in suppliers
                       from c in categories
                       where p.Supplier == s && p.Category == c
                       select new { p, s, c.CategoryName };
          var list = result.ToList();
          Assert.AreEqual(1000, list.Count);
          t.Complete();
        }
      }
    }


  }
}