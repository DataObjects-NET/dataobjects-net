// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class FirstSingleTest : NorthwindDOModelTest
  {
    [Test]
    public void LengthTest()
    {
      var length = Session.Query.All<Customer>()
        .Select(customer => customer.ContactName)
        .FirstOrDefault()
        .Length;
    }

    [Test]
    public void Length2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customers = Session.Query.All<Customer>()
        .Where(cutomer =>
          cutomer
            .Orders
            .Select(order => order.ShipName)
            .FirstOrDefault()
            .Length > 0);
      QueryDumper.Dump(customers);
    }

    [Test]
    public void Length3Test()
    {
      var customers = Session.Query.All<Customer>()
        .Where(cutomer =>
          cutomer
            .Orders
            .Select(order => order.ShipName)
            .SingleOrDefault()
            .Length > 0);
      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(customers));
    }

    [Test]
    public void FirstTest()
    {
      var customer = Session.Query.All<Customer>().First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstPredicateTest()
    {
      var customer = Session.Query.All<Customer>().First(c => c.Id=="ALFKI");
      Assert.IsNotNull(customer);
    }

    [Test]
    public void WhereFirstTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.Id=="ALFKI").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      var customer = Session.Query.All<Customer>().FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstOrDefaultPredicateTest()
    {
      Session.Query.All<Customer>().FirstOrDefault(c => c.Id=="ALFKI");
    }

    [Test]
    public void WhereFirstOrDefaultTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.Id=="ALFKI").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void SingleTest()
    {
      AssertEx.ThrowsInvalidOperationException(() => Session.Query.All<Customer>().Single());
    }

    [Test]
    public void SinglePredicateTest()
    {
      var customer = Session.Query.All<Customer>().Single(c => c.Id=="ALFKI");
      Assert.IsNotNull(customer);
    }

    [Test]
    public void WhereSingleTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.Id=="ALFKI").Single();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void SingleOrDefaultTest()
    {
      AssertEx.ThrowsInvalidOperationException(() => Session.Query.All<Customer>().SingleOrDefault());
    }

    [Test]
    public void SingleOrDefaultPredicateTest()
    {
      var customer = Session.Query.All<Customer>().SingleOrDefault(c => c.Id=="ALFKI");
      Assert.IsNotNull(customer);
    }

    [Test]
    public void WhereSingleOrDefaultTest()
    {
      var customer = Session.Query.All<Customer>().Where(c => c.Id=="ALFKI").SingleOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void SelectFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var products = Session.Query.All<Product>();
      var orderDetails = Session.Query.All<OrderDetails>();
      var result = from p in products
      select new {
        Product = p,
        MaxOrder = orderDetails
          .Where(od => od.Product==p)
          .OrderByDescending(od => od.UnitPrice * od.Quantity)
          .First()
          .Order
      };
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      QueryDumper.Dump(list, true);
    }

    [Test]
    public void SubqueryFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count(c => c.Orders.Count > 0);
      var result = Session.Query.All<Customer>().Where(c => c.Orders.Count > 0).Select(c => c.Orders.First());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
      QueryDumper.Dump(list, true);
    }

    [Test]
    public void SubqueryFirstExpectedExceptionTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>().Select(c => c.Orders.First());
      AssertEx.ThrowsInvalidOperationException(() => result.ToList());
    }

    [Test]
    public void SubqueryFirstOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count();
      var result = Session.Query.All<Customer>().Select(c => c.Orders.FirstOrDefault());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void SubquerySingleTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count(c => c.Orders.Count > 0);
      var result = Session.Query.All<Customer>().Where(c => c.Orders.Count > 0).Select(c => c.Orders.Take(1).Single());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void SubquerySingleExpectedException1Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>().Select(c => c.Orders.Take(1).Single());
      AssertEx.ThrowsInvalidOperationException(() => result.ToList());
    }

    [Test]
    public void SubquerySingleExpectedException2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      bool exceptionThrown = false;
      var result = Session.Query.All<Customer>().Where(c => c.Orders.Count > 0).Select(c => c.Orders.Single());
      try {
        result.ToList();
      }
      catch {
        exceptionThrown = true;
      }
      if (!exceptionThrown)
        Assert.Fail("Exception was not thrown.");
    }


    [Test]
    public void SubquerySingleOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customersCount = Session.Query.All<Customer>().Count();
      var result = Session.Query.All<Customer>().Select(c => c.Orders.Take(1).SingleOrDefault());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void ComplexSubquerySingleOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var categoriesCount = Session.Query.All<Category>().Count();
      var result = Session.Query.All<Category>().Select(
        c => new {
          Product = c.Products.Take(1).SingleOrDefault(),
          c.Products.Take(1).SingleOrDefault().ProductName,
          c.Products.Take(1).SingleOrDefault().Supplier
        });
      var list = result.ToList();
      Assert.AreEqual(categoriesCount, list.Count);
    }

    [Test]
    public void ComplexSubquerySelectSingleOrDefaultTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var categoriesCount = Session.Query.All<Category>().Count();
      var result = Session.Query.All<Category>().Select(c => c.Products.Take(1).SingleOrDefault()).Select(
        p => new {
          Product = p,
          p.ProductName,
          p.Supplier
        });
      var list = result.ToList();
      Assert.AreEqual(categoriesCount, list.Count);
    }


    [Test]
    public void ComplexSubqueryFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var categoriesCount = Session.Query.All<Category>().Count();
      var result = Session.Query.All<Category>().Select(
        c => new {
          Product = c.Products.First(),
          c.Products.First().ProductName,
          c.Products.First().Supplier
        });
      var list = result.ToList();
      Assert.AreEqual(categoriesCount, list.Count);
    }

    [Test]
    public void ComplexSubquerySelectFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var categoriesCount = Session.Query.All<Category>().Count();
      var result = Session.Query.All<Category>().Select(c => c.Products.First()).Select(p => new { Product = p, p.ProductName, p.Supplier });
      var list = result.ToList();
      Assert.AreEqual(categoriesCount, list.Count);
    }
  }
}