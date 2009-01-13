// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.12

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
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
                         select new {p.Key, p.ProductName, TotalPrice = p.UnitPrice * p.UnitsInStock}
                       )
                       where pd.TotalPrice > 100
                       select new {pd.Key, pd.ProductName};
                       
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void AssociationSingleTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var result = from p in products
                       select p.Supplier;
          var list = result.ToList();
          t.Complete();
        }
      }
    }

    [Test]
    public void AssociationMultipleTest() 
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