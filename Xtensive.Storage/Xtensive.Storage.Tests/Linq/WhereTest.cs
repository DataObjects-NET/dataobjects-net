// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class WhereTest : AutoBuildTest
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
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          category = new Category();
          category.CategoryName = "Category";
          category.Description = "Description of category";
          for (int i = 0; i < 100; i++) {
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
            for (int j = 0; j < 10; j++) {
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
    public void ColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s.CompanyName=="Company20").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Company20", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void StructureTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s.Address.Region == "Region30").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Region30", supplier.Address.Region);
          t.Complete();
        }
      }
    }

    [Test]
    public void IdTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s.Id == 20 ).First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Company19", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void KeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var key = Key.Create<Supplier>(Tuple.Create(20));
          var supplier = suppliers.Where(s => s.Key == key).First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Company19", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void InstanceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s == supplier20).First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Company19", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignKeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var product = products.Where(p => p.Supplier.Key == supplier20.Key).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Product_19_0", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignIDTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var product = products.Where(p => p.Supplier.Id == supplier20.Id).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Product_19_0", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignInstanceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var product = products.Where(p => p.Supplier == supplier20).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Product_19_0", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignPropertyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var product = products.Where(p => p.Supplier.CompanyName == "Company20").First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Company20", product.Supplier.CompanyName);
          t.Complete();
        }
      }
    }
  }
}