// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.25

using System;
using NUnit.Framework;
using Xtensive.Storage.Providers;
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
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var categoryNames = Query.All<Category>()
        .Select(c => c.CategoryName)
        .ToList();
      var expectedItems = Query.All<Category>()
        .Select(c => new {Category = c, ProductsCount = c.Products.Count})
        .ToDictionary(a => a.Category.CategoryName);
      foreach (var categoryName in categoryNames) {
        var result = Query.Execute(() => Query.All<Category>()
          .Where(c => c.CategoryName == categoryName)
          .Select(c => new {
            Category = c, 
            Products = Query.All<Product>().Where(p => p.Category.CategoryName == categoryName)})
          ).ToList();
        var expected = expectedItems[categoryName];
        Assert.AreSame(expected.Category, result.Single().Category);          
        Assert.AreEqual(expected.ProductsCount, result.Single().Products.ToList().Count);          
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