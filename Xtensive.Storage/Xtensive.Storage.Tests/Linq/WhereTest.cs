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
  public class WhereTest : NorthwindDOModelTest
  {
    private Key supplier20Key;
    private Key category1Key;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Domain.OpenSession()) {
        supplier20Key = Session.Current.All<Supplier>().Single(s => s.Id == 20).Key;
        category1Key = Session.Current.All<Category>().Single(c => c.Id == 1).Key;
      }
    }


    [Test]
    public void ColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s.CompanyName == "Tokyo Traders").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Tokyo Traders", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void CalculatedTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var list = products.Where(p => p.UnitPrice * p.UnitsInStock >= 100).ToList();
          Assert.AreEqual(67, list.Count);
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
          var supplier = suppliers.Where(s => s.Address.Region == "Victoria").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Victoria", supplier.Address.Region);
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
          Assert.AreEqual("Leka Trading", supplier.CompanyName);
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
          Assert.AreEqual("Leka Trading", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void InstanceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplier20 = supplier20Key.Resolve<Supplier>();
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s == supplier20).First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Leka Trading", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignKeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplier20 = supplier20Key.Resolve<Supplier>();
          var products = Session.Current.All<Product>();
          var product = products.Where(p => p.Supplier.Key == supplier20.Key).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Singaporean Hokkien Fried Mee", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignIDTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplier20 = supplier20Key.Resolve<Supplier>();
          var products = Session.Current.All<Product>();
          var product = products.Where(p => p.Supplier.Id == supplier20.Id).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Singaporean Hokkien Fried Mee", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignInstanceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplier20 = supplier20Key.Resolve<Supplier>();
          var products = Session.Current.All<Product>();
          var product = products.Where(p => p.Supplier == supplier20).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Singaporean Hokkien Fried Mee", product.ProductName);
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
          var product = products.Where(p => p.Supplier.CompanyName == "Leka Trading").First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
          product = products.Where(p => p.Supplier.CompanyName == "Leka Trading" && p.Category.Key == category1Key && p.Supplier.ContactTitle == "Owner").First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
          t.Complete();
        }
      }
    }
  }
}