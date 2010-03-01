// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  public class FreeTextTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    protected override Domain BuildDomain(Xtensive.Storage.Configuration.DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      Thread.Sleep(TimeSpan.FromSeconds(3));
      return domain;
    }

    [Test]
    [ExpectedException(typeof (TranslationException))]
    public void ReuseFreeText1Test()
    {
      var result1 = TakeMatchesIncorrect("Dessert candy and coffee seafood").Count();
      Assert.AreEqual(3, result1);
      var result2 = TakeMatchesIncorrect("1212erddfr324324rwefrtb43543543").Count();
      Assert.AreEqual(0, result2);
    }

    [Test]
    public void ReuseFreeText2Test()
    {
      var result1 = TakeMatchesCorrect("Dessert candy and coffee seafood").Count();
      Assert.AreEqual(3, result1);
      var result2 = TakeMatchesCorrect("1212erddfr324324rwefrtb43543543").Count();
      Assert.AreEqual(0, result2);
    }

    [Test]
    public void Test1()
    {
      var result = Query.FreeText<Category>(() => "Dessert candy and coffee seafood").ToList();
      Assert.AreEqual(3, result.Count);
      foreach (var document in result) {
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Entity);
      }
    }

    [Test]
    public void Test2()
    {
      var result = from c in Query.FreeText<Category>("Dessert candy and coffee seafood") select c.Rank;
      Assert.AreEqual(3, result.ToList().Count);
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
    public void JoinProductsWithRanks1Test()
    {
      var result =
        (from c in Query.FreeText<Category>(() => "Dessert candy and coffee")
        orderby c.Rank
        select new {c.Entity, c.Rank});
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
      }
    }

    [Test]
    public void JoinProductsWithRanks2Test()
    {
      var result = Query.FreeText<Category>(() => "Dessert candy and coffee")
        .OrderBy(c => c.Rank)
        .Select(x => x.Entity);
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
      }
    }
    private IEnumerable<FullTextMatch<Category>> TakeMatchesIncorrect(string searchCriteria)
    {
      return Query.Execute(() => Query.FreeText<Category>(searchCriteria));
    }

    private IEnumerable<FullTextMatch<Category>> TakeMatchesCorrect(string searchCriteria)
    {
      return Query.Execute(() => Query.FreeText<Category>(() => searchCriteria));
    }
  }
}