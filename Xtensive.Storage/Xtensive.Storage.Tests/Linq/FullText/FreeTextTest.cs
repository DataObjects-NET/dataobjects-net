// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq.FullText
{
  [Serializable]
  [Explicit]
  public class FreeTextTest : NorthwindDOModelTest
  {
    [Test]
    public void Test()
    {
      var result = Query.FreeText<Category>(() => "Dessert candy and coffee seafood").ToList();
      Assert.AreEqual(3, result.Count);
      foreach (var document in result) {
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Entity);
      }
    }

    [Test]
    public void JoinProductsTest()
    {
      var result = 
        from c in Query.FreeText<Category>(() => "Dessert candy and coffee")
        join p in Query.All<Product>() on c.Entity.Key equals p.Category.Key 
        select p;
      var list = result.ToList();
      Assert.AreEqual(25, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
        Assert.IsNotNull(product.Key);
      }
    }

    [Test]
    public void JoinProducts2Test()
    {
      var result = 
        from c in Query.FreeText<Category>(() => "Dessert candy and coffee")
        join p in Query.All<Product>() on c.Entity equals p.Category 
        select p;
      var list = result.ToList();
      Assert.AreEqual(25, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
        Assert.IsNotNull(product.Key);
      }
    }

    [Test]
    public void JoinProducts3Test()
    {
      var result = 
        from c in Query.FreeText<Category>(() => "Dessert candy and coffee")
        join p in Query.All<Product>() on c.Entity.Key equals p.Key // Wrong join
        select p;
      var list = result.ToList();
      Assert.AreEqual(25, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
        Assert.IsNotNull(product.Key);
      }
    }

    [Test]
    public void JoinProductsWithRanks1Test()
    {
      var result =
        (from c in Query.FreeText<Category>(() => "Dessert candy and coffee")
        orderby c.Rank
        select new {c.Entity, c.Rank})
          .Take(10);
      var list = result.ToList();
      Assert.AreEqual(25, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
      }
    }

    [Test]
    public void JoinProductsWithRanks2Test()
    {
      var result = Query.FreeText<Category>(() => "Dessert candy and coffee")
        .OrderBy(c => c.Rank)
        .Take(10)
        .Select(x => x.Entity);
      var list = result.ToList();
      Assert.AreEqual(25, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
      }
    }
  }
}