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
      var result = Query<Category>.FreeText("Dessert candy and coffe seafood").ToList();
      Assert.AreEqual(3, result.Count);
      foreach (var document in result) {
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Key);
        Assert.IsNotNull(document.Target);
      }

      result = Query<Category>.FreeText(c => c.CategoryName, "Dessert candy and coffe").ToList();
      Assert.AreEqual(0, result.Count);

      result = Query<Category>.FreeText(c => c.CategoryName, c => c.Description, "Dessert candy and coffe seafood").ToList();
      Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void SelectTargetTest()
    {
      var result = Query<Category>.FreeText(c => c.Description, "Dessert candy and coffe seafood").Select(d => d.Target).ToList();
      Assert.AreEqual(2, result.Count);
      foreach (var category in result) {
        Assert.IsNotNull(category);
        Assert.IsNotNull(category.Key);
      }
    }

    [Test]
    public void JoinProductsTest()
    {
      var result = 
        from c in Query<Category>.FreeText("Dessert candy and coffe")
        join p in Query<Product>.All on c.Key equals p.Category.Key 
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