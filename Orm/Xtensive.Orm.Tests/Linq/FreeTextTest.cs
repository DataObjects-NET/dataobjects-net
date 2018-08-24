// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  public class FreeTextTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    protected override Domain BuildDomain(Xtensive.Orm.Configuration.DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      Thread.Sleep(TimeSpan.FromSeconds(6));
      return domain;
    }

    [Test]
    public void ReuseFreeText1Test()
    {
      Assert.Throws<QueryTranslationException>(() => {
        var result1 = TakeMatchesIncorrect("Dessert candy and coffee seafood").Count();
        Assert.AreEqual(3, result1);
        var result2 = TakeMatchesIncorrect("1212erddfr324324rwefrtb43543543").Count();
        Assert.AreEqual(0, result2);
      });
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
      var result = Session.Query.FreeText<Category>(() => "Dessert candy and coffee seafood").ToList();
      Assert.AreEqual(3, result.Count);
      foreach (var document in result) {
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Entity);
      }
    }

    [Test]
    public void Test2()
    {
      var result = from c in Session.Query.FreeText<Category>("Dessert candy and coffee seafood") select c.Rank;
      Assert.AreEqual(3, result.ToList().Count);
    }

    [Test]
    public void TopNByRankTest()
    {
      var allMatchingRecords = Session.Query.FreeText<Category>("Dessert candy and coffee seafood");
      Assert.AreEqual(3, allMatchingRecords.Count());
      var topNMatchingRecords = Session.Query.FreeText<Category>("Dessert candy and coffee seafood", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count());
      var top2Records = allMatchingRecords.OrderByDescending(record => record.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.CategoryName).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankQueryTest()
    {
      var allMatchingRecords = Query.FreeText<Category>("Dessert candy and coffee seafood");
      Assert.AreEqual(3, allMatchingRecords.Count());
      var topNMatchingRecords = Query.FreeText<Category>("Dessert candy and coffee seafood", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count());
      var top2Records = allMatchingRecords.OrderByDescending(record => record.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.CategoryName).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankExpressionTest()
    {
      var allMatchingRecords = Session.Query.FreeText<Category>(() => "Dessert candy and coffee seafood");
      Assert.AreEqual(3, allMatchingRecords.Count());
      var topNMatchingRecords = Session.Query.FreeText<Category>(() => "Dessert candy and coffee seafood", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count);
      var top2Records = allMatchingRecords.OrderByDescending(rec => rec.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.CategoryName).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankQueryExpressionTest()
    {
      var allMatchingRecords = Query.FreeText<Category>(() => "Dessert candy and coffee seafood");
      Assert.AreEqual(3, allMatchingRecords.Count());
      var topNMatchingRecords = Query.FreeText<Category>(() => "Dessert candy and coffee seafood", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count);
      var top2Records = allMatchingRecords.OrderByDescending(rec => rec.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.CategoryName).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.CategoryName)));
    }

    [Test]
    public void TopNByrankEmptyResultTest()
    {
      var allMatchingRecords = Session.Query.FreeText<Category>(() => "sdgfgfhghd");
      Assert.IsTrue(!allMatchingRecords.Any());
      var topNMatchingRecords = Session.Query.FreeText<Category>(() => "sdgfgfhghd", 1);
      Assert.IsTrue(!topNMatchingRecords.Any());
    }

    [Test]
    public void NegativeTopNTest()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => Session.Query.FreeText<Category>("sfdgfdhghgf", -1));
    }

    [Test]
    public void ZeroTopNTest()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => Session.Query.FreeText<Category>("sfdgfhgfhhj", 0));
    }

    [Test]
    public void TopNByRankJoinProductsTest()
    {
      var result =
        from c in Session.Query.FreeText<Category>(() => "Dessert candy and coffee", 1)
        join p in Session.Query.All<Product>() on c.Entity.Key equals p.Category.Key
        select p;
      Assert.AreEqual(13, result.ToList().Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
        Assert.IsNotNull(product.Key);
      }
    }

    [Test]
    public void JoinProductsTest()
    {
      var result = 
        from c in Session.Query.FreeText<Category>(() => "Dessert candy and coffee")
        join p in Session.Query.All<Product>() on c.Entity.Key equals p.Category.Key 
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
        from c in Session.Query.FreeText<Category>(() => "Dessert candy and coffee")
        join p in Session.Query.All<Product>() on c.Entity equals p.Category 
        select p;
      var list = result.ToList();
      Assert.AreEqual(25, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
        Assert.IsNotNull(product.Key);
      }
    }

    [Test]
    public void TopNByRankJoinProductsTest2()
    {
      var result =
        from c in Session.Query.FreeText<Category>(() => "Dessert candy and coffee", 1)
        join p in Session.Query.All<Product>() on c.Entity equals p.Category
        select p;
      Assert.AreEqual(13, result.ToList().Count);
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
        from p in Session.Query.FreeText<Product>(keywords)
        where p.Entity.ProductName != typeof (Product).Name
        join c in Session.Query.All<Category>() on p.Entity.Category.Id equals c.Id
        orderby p.Rank
        select new {Id = c.Id, Name = c.CategoryName, Rank = p.Rank, Descr = GetProductDescription(p.Entity)};
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinCategoryTest()
    {
      var keywords = "lager";
      var result =
        from p in Session.Query.FreeText<Product>(keywords, 1)
        where p.Entity.ProductName!=typeof (Product).Name
        join c in Session.Query.All<Category>() on p.Entity.Category.Id equals c.Id
        orderby p.Rank
        select new {Id = c.Id, Name = c.CategoryName, Rank = p.Rank, Descr = GetProductDescription(p.Entity)};
      var list = result.ToList();
    }

    [Test]
    public void JoinCategory2Test()
    {
      var keywords = "lager";
      var result = Session.Query.FreeText<Product>(keywords)
        .Where(p => p.Entity.ProductName!=typeof (Product).Name)
        .Join(Session.Query.All<Category>(), p => p.Entity.Category.Id, c => c.Id, (p, c) => new {p.Rank, c})
        .OrderBy(@t => @t.Rank)
        .Select(@t => new {Id = @t.c.Id, Name = @t.c.CategoryName, Rank = @t.Rank});
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinCategory2Test()
    {
      var keywords = "lager";
      var result = Session.Query.FreeText<Product>(keywords, 1)
        .Where(p => p.Entity.ProductName!=typeof (Product).Name)
        .Join(Session.Query.All<Category>(), p => p.Entity.Category.Id, c => c.Id, (p, c) => new {p.Rank, c})
        .OrderBy(@t => @t.Rank)
        .Select(@t => new {Id = @t.c.Id, Name = @t.c.CategoryName, Rank = @t.Rank});
      var list = result.ToList();
    }

    [Test]
    public void JoinCategory2PgSqlTest()
    {
      var keywords = "lager";
      var result = Session.Query.FreeText<Product>(keywords)
        .Join(Session.Query.All<Category>(), p => p.Entity.Category.Id, c => c.Id, (p, c) => new {p.Rank, c});
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinCategory2PgSqlTest()
    {
      var keywords = "lager";
      var result = Session.Query.FreeText<Product>(keywords, 1)
        .Join(Session.Query.All<Category>(), p => p.Entity.Category.Id, c => c.Id, (p, c) => new {p.Rank, c});
      var list = result.ToList();
    }

    [Test]
    public void JoinCategory3Test()
    {

      /*[23.07.2010 13:56:17] Alexander Ustinov:         (from ftx in (
                            from ft in Session.Query.FreeText<Data.Common.FullTextRecord>(() => "мегаполис тюмень центральный горького корп")
                            where ft.Entity.ObjectType==typeof (Data.Rng.Customer).FullName
                            select new {ft.Rank, ft.Entity}
                          )
              join customer in Session.Query.All<Data.Rng.Customer>() on ftx.Entity.ObjectId equals customer.Id
              from gasObject in Session.Query.All<Data.Rng.GasObject>().Where(go => go.Customer==customer).DefaultIfEmpty()
              select new RngCustomerBriefInfo {
                ID = customer.Id,
                Rank = ftx.Rank,
                Number = gasObject==null ? String.Empty : gasObject.AccountNumber,
                Name = customer.Name,
                Address = AbonentMatchDataProvider.GetGasObjectAddress(gasObject)
              }).Take(100).OrderByDescending(i => i.Rank).OrderByDescending(i => i.ID);*/
      var keywords = "lager";
      var result = (
          from ft in Session.Query.FreeText<Product>(keywords)
          where ft.Entity.ProductName != typeof(Product).Name
          join c in Session.Query.All<Category>() on ft.Entity.Category equals c
          select new {ID = c.Id, Rank = ft.Rank, Name = ft.Entity.ProductName}
        ).Take(100).OrderByDescending(i => i.Rank).ThenByDescending(i => i.ID);
//      var result = Session.Query.FreeText<Product>(keywords)
//        .Where(p => p.Entity.ProductName != typeof(Product).Name)
//        .Join(Session.Query.All<Category>(), p => p.Entity.Category.Id, c => c.Id, (p, c) => new { p.Rank, c })
//        .OrderBy(@t => @t.Rank)
//        .Select(@t => new { Id = @t.c.Id, Name = @t.c.CategoryName, Rank = @t.Rank });
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinCategory3Test()
    {
      var keywords = "lager";
      var result = (from ft in Session.Query.FreeText<Product>(keywords, 1)
        where ft.Entity.ProductName!=typeof (Product).Name
        join c in Session.Query.All<Category>() on ft.Entity.Category equals c
        select new {ID = c.Id, Rank = ft.Rank, Name = ft.Entity.ProductName}
        ).Take(100).OrderByDescending(i => i.Rank).ThenByDescending(i => i.ID);
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
        (from c in Session.Query.FreeText<Category>(() => "Dessert candy and coffee")
        orderby c.Rank
        select new {c.Entity, c.Rank});
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
      }
    }

    [Test]
    public void TopNByRankJoinProductsWithRanks1Test()
    {
      var result = 
        (from c in Session.Query.FreeText<Category>(() => "Dessert candy and coffee", 2)
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
      var result = Session.Query.FreeText<Category>(() => "Dessert candy and coffee")
        .OrderBy(c => c.Rank)
        .Select(x => x.Entity);
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
      foreach (var product in result) {
        Assert.IsNotNull(product);
      }
    }

    [Test]
    public void TopNByRankJoinProductsWithRanks2Test()
    {
      var result = Session.Query.FreeText<Category>(() => "Dessert candy and coffee", 2)
        .OrderBy(c => c.Rank)
        .Select(x => x.Entity);
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
      foreach (var product in result){
        Assert.IsNotNull(product);
      }
    }

    [Test]
    public void OrderByTest()
    {
      var result = from ft in Session.Query.FreeText<Product>(() => "lager")
                   where ft.Entity.Id > 0 && ft.Entity.UnitPrice > 10
                   orderby ft.Entity.Category.CategoryName , ft.Entity.ReorderLevel
                   select new {Product = ft.Entity, ft.Entity.Category};
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
    }
    
    [Test]
    public void TopNByRankOrderByTest()
    {
      var keywords = "Dessert candy and coffee seafood";
      var defaultFreeTextQuery = Session.Query.FreeText<Category>(keywords).Take(2).OrderBy(r => r.Rank).ToList();
      Assert.IsTrue(defaultFreeTextQuery.Count()==2);
      var topNByrankOrdered = Session.Query.FreeText<Category>(keywords, 2).OrderBy(r => r.Rank);
      Assert.IsTrue(topNByrankOrdered.Count()==2);
      Assert.IsTrue(defaultFreeTextQuery
        .Select(rec => rec.Entity.CategoryName)
        .SequenceEqual(topNByrankOrdered.Select(rec => rec.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankOrderByExpressionTest()
    {
      var keywords = "Dessert candy and coffee seafood";
      var defaultFreeTextQuery = Session.Query.FreeText<Category>(() => keywords).Take(2).OrderBy(r => r.Rank).ToList();
      Assert.IsTrue(defaultFreeTextQuery.Count()==2);
      var topnByRankOrdered = Session.Query.FreeText<Category>(() => keywords, 2).OrderBy(r => r.Rank);
      Assert.IsTrue(topnByRankOrdered.Count()==2);
      Assert.IsTrue(defaultFreeTextQuery
        .Select(rec => rec.Entity.CategoryName)
        .SequenceEqual(topnByRankOrdered.Select(rec => rec.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankTakeMaxTest()
    {
      var keywords = "Dessert candy and coffee seafood";
      var topNByRank = Session.Query.FreeText<Category>(keywords, 3).ToList();
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByRankTakeSameAmmount = Session.Query.FreeText<Category>(keywords, 3).Take(3).OrderByDescending(r => r.Rank);
      Assert.IsTrue(topNByRankTakeSameAmmount.Count()==3);
      Assert.IsTrue(topNByRank
        .Select(rec => rec.Entity.CategoryName)
        .SequenceEqual(topNByRankTakeSameAmmount.Select(rec => rec.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankTakeLessThanMaxTest()
    {
      var keywords = "Dessert candy and coffee seafood";
      var topNByRank = Session.Query.FreeText<Category>(keywords, 3);
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByRankTakeLess = Session.Query.FreeText<Category>(keywords, 3).Take(2).OrderByDescending(r => r.Rank).ToList();
      Assert.IsTrue(topNByRankTakeLess.Count()==2);
      Assert.IsTrue(topNByRankTakeLess
        .Select(r => r.Entity.CategoryName)
        .SequenceEqual(topNByRank.Take(2).OrderByDescending(r => r.Rank).Select(r => r.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankTakeMoreThanMaxTest()
    {
      var keywords = "Dessert candy and coffee seafood";
      var topNByRank = Session.Query.FreeText<Category>(keywords, 3);
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByrankTakeMore = Session.Query.FreeText<Category>(keywords, 3).Take(4).OrderByDescending(r => r.Rank).ToList();
      Assert.IsTrue(topNByrankTakeMore.Count()==3);
      Assert.IsTrue(topNByrankTakeMore.Select(r => r.Entity.CategoryName).SequenceEqual(topNByRank.Select(r => r.Entity.CategoryName)));
    }

    [Test]
    public void TopNByRankOrderByAndTakeSequenceTest()
    {
      var keywords = "Dessert candy and coffee seafood";
      var topNByRank = Session.Query.FreeText<Category>(keywords, 3).ToList();
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByrankOrderByAndTake = Session.Query.FreeText<Category>(keywords, 3)
        .OrderBy(r => r.Rank)
        .Take(3)
        .OrderByDescending(r => r.Rank).Take(2).ToList();
      Assert.IsTrue(topNByrankOrderByAndTake.Count()==2);
      Assert.IsTrue(topNByRank
        .Take(2)
        .OrderByDescending(r => r.Rank)
        .Select(r => r.Entity.CategoryName)
        .SequenceEqual(topNByrankOrderByAndTake.Select(rec=>rec.Entity.CategoryName)));
    }

    [Test]
    public void FreeTextOnFullTextStructureField()
    {
      var keywords = "Avda. de la Constitucion 2222";
      var result = Query.FreeText<Customer>(keywords).ToList();
      Assert.AreEqual(9, result.Count);
    }

    [Test]
    public void FreeTextTopNByRankOnFullTextStructureField()
    {
      var keywords = "Avda. de la Constitucion 2222";
      var result = Query.FreeText<Customer>("Avda. de la Constitucion 2222", 2).ToList();
      var closestMatch = result.First().Entity;
      Assert.AreEqual(2, result.Count);
      Assert.IsTrue(closestMatch.Address.StreetAddress=="Avda. de la Constitucion 2222");
    }

    [Test]
    public void FreeTextOnStructureField()
    {
      var result = Query.FreeText<Customer>("London").ToList();
      Assert.IsTrue(!result.Any());
    }

    private IEnumerable<FullTextMatch<Category>> TakeMatchesIncorrect(string searchCriteria)
    {
      return Session.Query.Execute(qe => qe.FreeText<Category>(searchCriteria));
    }

    private IEnumerable<FullTextMatch<Category>> TakeMatchesCorrect(string searchCriteria)
    {
      return Session.Query.Execute(qe => qe.FreeText<Category>(() => searchCriteria));
    }
  }
}