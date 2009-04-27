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
    [ExpectedException(typeof(NotImplementedException))]
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
    }
  }
}