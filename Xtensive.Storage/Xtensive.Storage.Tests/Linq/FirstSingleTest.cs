// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class FirstSingleTest : NorthwindDOModelTest
  {
    [Test]
    public void FirstTest()
    {
      var customer = Query<Customer>.All.First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstPredicateTest()
    {
      var customer = Query<Customer>.All.First(c => c.Id=="ALFKI");
      Assert.IsNotNull(customer);
    }

    [Test]
    public void WhereFirstTest()
    {
      var customer = Query<Customer>.All.Where(c => c.Id=="ALFKI").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      var customer = Query<Customer>.All.FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void FirstOrDefaultPredicateTest()
    {
      Query<Customer>.All.FirstOrDefault(c => c.Id=="ALFKI");
    }

    [Test]
    public void WhereFirstOrDefaultTest()
    {
      var customer = Query<Customer>.All.Where(c => c.Id=="ALFKI").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void SingleTest()
    {
      AssertEx.ThrowsInvalidOperationException(() => Query<Customer>.All.Single());
    }

    [Test]
    public void SinglePredicateTest()
    {
      var customer = Query<Customer>.All.Single(c => c.Id=="ALFKI");
      Assert.IsNotNull(customer);
    }

    [Test]
    public void WhereSingleTest()
    {
      var customer = Query<Customer>.All.Where(c => c.Id=="ALFKI").Single();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void SingleOrDefaultTest()
    {
      AssertEx.ThrowsInvalidOperationException(() => Query<Customer>.All.SingleOrDefault());
    }

    [Test]
    public void SingleOrDefaultPredicateTest()
    {
      var customer = Query<Customer>.All.SingleOrDefault(c => c.Id=="ALFKI");
      Assert.IsNotNull(customer);
    }

    [Test]
    public void WhereSingleOrDefaultTest()
    {
      var customer = Query<Customer>.All.Where(c => c.Id=="ALFKI").SingleOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void SelectFirstTest()
    {
      var products = Query<Product>.All;
      var orderDetails = Query<OrderDetails>.All;
      var result = from p in products
                   select new
                     {
                       Product = p,
                       MaxOrder = orderDetails
                         .Where(od => od.Product == p)
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
      var customersCount = Query<Customer>.All.Count(c => c.Orders.Count > 0);
      var result = Query<Customer>.All.Where(c => c.Orders.Count > 0).Select(c => c.Orders.First());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
      QueryDumper.Dump(list, true);
    }

    [Test]
    public void SubqueryFirstExpectedExceptionTest()
    {
      var result = Query<Customer>.All.Select(c => c.Orders.First());
      AssertEx.ThrowsInvalidOperationException(() => result.ToList());
    }

    [Test]
    public void SubqueryFirstOrDefaultTest()
    {
      var customersCount = Query<Customer>.All.Count();
      var result = Query<Customer>.All.Select(c => c.Orders.FirstOrDefault());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void SubquerySingleTest()
    {
      var customersCount = Query<Customer>.All.Count(c => c.Orders.Count > 0);
      var result = Query<Customer>.All.Where(c => c.Orders.Count > 0).Select(c => c.Orders.Take(1).Single());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void SubquerySingleExpectedException1Test()
    {
      var result = Query<Customer>.All.Select(c => c.Orders.Take(1).Single());
      AssertEx.ThrowsInvalidOperationException(() => result.ToList());
    }

    [Test]
    public void SubquerySingleExpectedException2Test()
    {
      bool exceptionThrown = false;
      var result = Query<Customer>.All.Where(c => c.Orders.Count > 0).Select(c => c.Orders.Single());
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
      var customersCount = Query<Customer>.All.Count();
      var result = Query<Customer>.All.Select(c => c.Orders.Take(1).SingleOrDefault());
      var list = result.ToList();
      Assert.AreEqual(customersCount, list.Count);
    }

    [Test]
    public void ComplexSubquerySingleOrDefaultTest()
    {
      var categoriesCount = Query<Category>.All.Count();
      var result = Query<Category>.All.Select(
        c => new
             {
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
      var categoriesCount = Query<Category>.All.Count();
      var result = Query<Category>.All.Select(c => c.Products.Take(1).SingleOrDefault()).Select(
        p => new
        {
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
      var categoriesCount = Query<Category>.All.Count();
      var result = Query<Category>.All.Select(
        c => new
             {
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
      var categoriesCount = Query<Category>.All.Count();
      var result = Query<Category>.All.Select(c => c.Products.First()).Select(p => new { Product = p, p.ProductName, p.Supplier });
      var list = result.ToList();
      Assert.AreEqual(categoriesCount, list.Count);
    }
  
  }
}