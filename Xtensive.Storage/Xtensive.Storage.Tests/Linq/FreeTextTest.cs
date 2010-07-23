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
    public void JoinCategoryTest()
    {
      var keywords = "lager";
      var result =
        from p in Query.FreeText<Product>(keywords)
        where p.Entity.ProductName != typeof (Product).Name
        join c in Query.All<Category>() on p.Entity.Category.Id equals c.Id
        orderby p.Rank
        select new {Id = c.Id, Name = c.CategoryName, Rank = p.Rank, Descr = GetProductDescription(p.Entity)};
      var list = result.ToList();
    }

    [Test]
    public void JoinCategory2Test()
    {
      var keywords = "lager";
      var result = Query.FreeText<Product>(keywords)
        .Where(p => p.Entity.ProductName!=typeof (Product).Name)
        .Join(Query.All<Category>(), p => p.Entity.Category.Id, c => c.Id, (p, c) => new {p.Rank, c})
        .OrderBy(@t => @t.Rank)
        .Select(@t => new {Id = @t.c.Id, Name = @t.c.CategoryName, Rank = @t.Rank});
      var list = result.ToList();
    }

    private static string GetProductDescription(Product p)
    {
      return p.ProductName + ":" + p.UnitPrice;
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

    [Test]
    public void OrderByTest()
    {
      var result = from ft in Query.FreeText<Product>(() => "lager")
                   where ft.Entity.Id > 0 && ft.Entity.UnitPrice > 10
                   orderby ft.Entity.Category.CategoryName , ft.Entity.ReorderLevel
                   select new {Product = ft.Entity, ft.Entity.Category};
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
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