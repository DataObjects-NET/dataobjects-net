// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.17

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class JoinTest : AutoBuildTest
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
    public void OneToOneTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var result = from p in products
                       join s in suppliers on p.Supplier.Id equals s.Id
                       select new { p, s };
          var list = result.ToList();
          Assert.AreEqual(1000, list.Count);
          t.Complete();
        }
      }
    }

    [Test]
    public void MultipleTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var categories = Session.Current.All<Category>();
          var result = from p in products
                       join s in suppliers on p.Supplier.Id equals s.Id
                       join c in categories on p.Category.Id equals c.Id
                       select new { p, s, c.CategoryName };
          var list = result.ToList();
          Assert.AreEqual(1000, list.Count);
          t.Complete();
        }
      }
    }


  }
}