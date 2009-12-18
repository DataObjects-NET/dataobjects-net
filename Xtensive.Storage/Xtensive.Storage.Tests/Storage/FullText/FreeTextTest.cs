// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Storage.FullText
{
  [Serializable]
  public class FreeTextTest : NorthwindDOModelTest
  {
    [Test]
    public void Test()
    {
      var result = Query.AllText<Category>().FreeText(() => "Dessert candy and coffe seafood").ToList();
      Assert.AreEqual(3, result.Count);
      foreach (var document in result) {
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Key);
        Assert.IsNotNull(document.Entity);
      }
    }

    [Test]
    public void JoinProductsTest()
    {
      var result = 
        from c in Query.AllText<Category>().FreeText(() => "Dessert candy and coffe")
        join p in Query.All<Product>() on c.Key equals p.Category.Key 
        select p;
      var list = result.ToList();
      Assert.AreEqual(25, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
        Assert.IsNotNull(product.Key);
      }
    }
  }
}