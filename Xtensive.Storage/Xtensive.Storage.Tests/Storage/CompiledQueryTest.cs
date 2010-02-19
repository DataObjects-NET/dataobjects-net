// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.25

using System;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Storage
{
  [Serializable]
  public class CompiledQueryTest : NorthwindDOModelTest
  {
    [Test]
    public void CachedSequenceTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.Execute(() => Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice));
    }

    [Test]
    public void CachedSubquerySequenceTest()
    {
      var categoryNames = Query.All<Category>()
        .Select(c => c.CategoryName)
        .ToList();
      foreach (var categoryName in categoryNames) {
        var result = Query.Execute(() => Query.All<Category>()
          .Where(c => c.CategoryName == categoryName)
          .Select(c => new {
            Category = c, 
            ProductsCount = Query.All<Product>().Count(p => p.Category.CategoryName == categoryName)})
          ).ToList();
        var expected = new {
          Category = Query.All<Category>().First(c => c.CategoryName == categoryName),
          ProductsCount = Query.All<Product>().Count(p => p.Category.CategoryName == categoryName)
        };
        Assert.AreSame(expected.Category, result.Single().Category);          
        Assert.AreEqual(expected.ProductsCount, result.Single().ProductsCount);          
      }
    }

    [Test]
    public void ScalarLongTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).LongCount();
    }

    [Test]
    public void CachedScalarLongTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.Execute(() => Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).LongCount());
    }

    [Test]
    public void CachedScalarTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.Execute(() => Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).Count());
    }
  }
}