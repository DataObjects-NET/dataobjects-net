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
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  public class FreeTextTest : ChinookDOModelTest
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
      Assert.Throws<QueryTranslationException>(
        () => {
          var result1 = TakeMatchesIncorrect("best greatest hits").Count();
          Assert.AreEqual(3, result1);
          var result2 = TakeMatchesIncorrect("1212erddfr324324rwefrtb43543543").Count();
          Assert.AreEqual(0, result2);
        });
    }

    [Test]
    public void ReuseFreeText2Test()
    {
      var result1 = TakeMatchesCorrect("best greatest hits").Count();
      Assert.AreEqual(24, result1);
      var result2 = TakeMatchesCorrect("1212erddfr324324rwefrtb43543543").Count();
      Assert.AreEqual(0, result2);
    }

    [Test]
    public void Test1()
    {
      var result = Session.Query.FreeText<Album>(() => "best greatest hits").ToList();
      Assert.AreEqual(24, result.Count);
      foreach (var document in result) {
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Entity);
      }
    }

    [Test]
    public void Test2()
    {
      var result = from c in Session.Query.FreeText<Album>("best greatest hits") select c.Rank;
      Assert.AreEqual(24, result.ToList().Count);
    }

    [Test]
    public void TopNByRankTest()
    {
      var allMatchingRecords = Session.Query.FreeText<Album>("best greatest hits");
      Assert.AreEqual(24, allMatchingRecords.Count());
      var topNMatchingRecords = Session.Query.FreeText<Album>("best greatest hits", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count());
      var top2Records = allMatchingRecords.OrderByDescending(record => record.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.Title).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.Title)));
    }

    [Test]
    public void TopNByRankQueryTest()
    {
      var allMatchingRecords = Query.FreeText<Album>("best greatest hits");
      Assert.AreEqual(24, allMatchingRecords.Count());
      var topNMatchingRecords = Query.FreeText<Album>("best greatest hits", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count());
      var top2Records = allMatchingRecords.OrderByDescending(record => record.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.Title).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.Title)));
    }

    [Test]
    public void TopNByRankExpressionTest()
    {
      var allMatchingRecords = Session.Query.FreeText<Album>(() => "best greatest hits");
      Assert.AreEqual(24, allMatchingRecords.Count());
      var topNMatchingRecords = Session.Query.FreeText<Album>(() => "best greatest hits", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count);
      var top2Records = allMatchingRecords.OrderByDescending(rec => rec.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.Title).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.Title)));
    }

    [Test]
    public void TopNByRankQueryExpressionTest()
    {
      var allMatchingRecords = Query.FreeText<Album>(() => "best greatest hits");
      Assert.AreEqual(24, allMatchingRecords.Count());
      var topNMatchingRecords = Query.FreeText<Album>(() => "best greatest hits", 2).ToList();
      Assert.AreEqual(2, topNMatchingRecords.Count);
      var top2Records = allMatchingRecords.OrderByDescending(rec => rec.Rank).Take(2);
      Assert.IsTrue(topNMatchingRecords.Select(rec => rec.Entity.Title).SequenceEqual(top2Records.Select(rec1 => rec1.Entity.Title)));
    }

    [Test]
    public void TopNByrankEmptyResultTest()
    {
      var allMatchingRecords = Session.Query.FreeText<Album>(() => "sdgfgfhghd");
      Assert.IsTrue(!allMatchingRecords.Any());
      var topNMatchingRecords = Session.Query.FreeText<Album>(() => "sdgfgfhghd", 1);
      Assert.IsTrue(!topNMatchingRecords.Any());
    }

    [Test]
    public void NegativeTopNTest()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => Session.Query.FreeText<Album>("sfdgfdhghgf", -1));
    }

    [Test]
    public void ZeroTopNTest()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => Session.Query.FreeText<Album>("sfdgfhgfhhj", 0));
    }

    [Test]
    public void TopNByRankJoinTracksTest()
    {
      var result =
        from c in Session.Query.FreeText<Album>(() => "best greatest hits", 1)
        join p in Session.Query.All<Track>() on c.Entity.Key equals p.Album.Key
        select p;
      Assert.AreEqual(17, result.ToList().Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
        Assert.IsNotNull(track.Key);
      }
    }

    [Test]
    public void JoinTracksTest()
    {
      var result =
        from c in Session.Query.FreeText<Album>(() => "Black Best Greatest")
        join p in Session.Query.All<Track>() on c.Entity.Key equals p.Album.Key
        select p;
      var list = result.ToList();
      Assert.AreEqual(432, list.Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
        Assert.IsNotNull(track.Key);
      }
    }

    [Test]
    public void JoinTracks2Test()
    {
      var result =
        from c in Session.Query.FreeText<Album>(() => "Black Best Greatest")
        join p in Session.Query.All<Track>() on c.Entity equals p.Album
        select p;
      var list = result.ToList();
      Assert.AreEqual(432, list.Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
        Assert.IsNotNull(track.Key);
      }
    }

    [Test]
    public void TopNByRankJoinTracksTest2()
    {
      var result =
        from c in Session.Query.FreeText<Album>(() => "Black Best Greatest", 1)
        join p in Session.Query.All<Track>() on c.Entity equals p.Album
        select p;
      Assert.AreEqual(7, result.ToList().Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
        Assert.IsNotNull(track.Key);
      }
    }

    [Test]
    public void JoinAlbumTest()
    {
      var keywords = "black";
      var result =
        from p in Session.Query.FreeText<Track>(keywords)
        join c in Session.Query.All<Album>() on p.Entity.Album.AlbumId equals c.AlbumId
        orderby p.Rank
        select new {Id = c.AlbumId, Name = c.Title, Rank = p.Rank, Descr = GetTrackDescription(p.Entity)};
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinAlbumTest()
    {
      var keywords = "black";
      var result =
        from p in Session.Query.FreeText<Track>(keywords, 1)
        join c in Session.Query.All<Album>() on p.Entity.Album.AlbumId equals c.AlbumId
        orderby p.Rank
        select new {Id = c.AlbumId, Title = c.Title, Rank = p.Rank, Descr = GetTrackDescription(p.Entity)};
      var list = result.ToList();
    }

    [Test]
    public void JoinAlbum2Test()
    {
      var keywords = "black";
      var result = Session.Query.FreeText<Track>(keywords)
        .Join(Session.Query.All<Album>(), p => p.Entity.Album.AlbumId, c => c.AlbumId, (p, c) => new {p.Rank, c})
        .OrderBy(@t => @t.Rank)
        .Select(@t => new {Id = @t.c.AlbumId, Title = @t.c.Title, Rank = @t.Rank});
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinAlbum2Test()
    {
      var keywords = "black";
      var result = Session.Query.FreeText<Track>(keywords, 1)
        .Join(Session.Query.All<Album>(), p => p.Entity.Album.AlbumId, c => c.AlbumId, (p, c) => new {p.Rank, c})
        .OrderBy(@t => @t.Rank)
        .Select(@t => new {Id = @t.c.AlbumId, Title = @t.c.Title, Rank = @t.Rank});
      var list = result.ToList();
    }

    [Test]
    public void JoinAlbum2PgSqlTest()
    {
      var keywords = "black";
      var result = Session.Query.FreeText<Track>(keywords)
        .Join(Session.Query.All<Album>(), p => p.Entity.Album.AlbumId, c => c.AlbumId, (p, c) => new {p.Rank, c});
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinAlbum2PgSqlTest()
    {
      var keywords = "black";
      var result = Session.Query.FreeText<Track>(keywords, 1)
        .Join(Session.Query.All<Album>(), p => p.Entity.Album.AlbumId, c => c.AlbumId, (p, c) => new {p.Rank, c});
      var list = result.ToList();
    }

    [Test]
    public void JoinAlbum3Test()
    {

      /*[23.07.2010 13:56:17] Alexander Ustinov:         (from ftx in (
                            from ft in Session.Query.FreeText<Data.Common.FullTextRecord>(() => "ìåãàïîëèñ òþìåíü öåíòðàëüíûé ãîðüêîãî êîðï")
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
      var keywords = "black";
      var result = (
        from ft in Session.Query.FreeText<Track>(keywords)
        join c in Session.Query.All<Album>() on ft.Entity.Album equals c
        select new {ID = c.AlbumId, Rank = ft.Rank, Name = ft.Entity.Name}
      ).Take(100).OrderByDescending(i => i.Rank).ThenByDescending(i => i.ID);
//      var result = Session.Query.FreeText<Product>(keywords)
//        .Where(p => p.Entity.ProductName != typeof(Product).Name)
//        .Join(Session.Query.All<Category>(), p => p.Entity.Category.Id, c => c.Id, (p, c) => new { p.Rank, c })
//        .OrderBy(@t => @t.Rank)
//        .Select(@t => new { Id = @t.c.Id, Name = @t.c.CategoryName, Rank = @t.Rank });
      var list = result.ToList();
    }

    [Test]
    public void TopNByRankJoinAlbum3Test()
    {
      var keywords = "black";
      var result = (from ft in Session.Query.FreeText<Track>(keywords, 1)
          join c in Session.Query.All<Album>() on ft.Entity.Album equals c
          select new {ID = c.AlbumId, Rank = ft.Rank, Name = ft.Entity.Name}
        ).Take(100).OrderByDescending(i => i.Rank).ThenByDescending(i => i.ID);
      var list = result.ToList();
    }

    private static string GetTrackDescription(Track p)
    {
      return p.Name + ":" + p.UnitPrice;
    }

    [Test]
    public void JoinTracksWithRanks1Test()
    {
      var result =
        (from c in Session.Query.FreeText<Album>(() => "best greatest Hits")
          orderby c.Rank
          select new {c.Entity, c.Rank});
      var list = result.ToList();
      Assert.AreEqual(24, list.Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
      }
    }

    [Test]
    public void TopNByRankJoinTracksWithRanks1Test()
    {
      var result =
        (from c in Session.Query.FreeText<Album>(() => "best greatest Hits", 2)
          orderby c.Rank
          select new {c.Entity, c.Rank});
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
      }
    }

    [Test]
    public void JoinTracksWithRanks2Test()
    {
      var result = Session.Query.FreeText<Album>(() => "best greatest Hits")
        .OrderBy(c => c.Rank)
        .Select(x => x.Entity);
      var list = result.ToList();
      Assert.AreEqual(24, list.Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
      }
    }

    [Test]
    public void TopNByRankJoinTracksWithRanks2Test()
    {
      var result = Session.Query.FreeText<Album>(() => "best greatest Hits", 2)
        .OrderBy(c => c.Rank)
        .Select(x => x.Entity);
      var list = result.ToList();
      Assert.AreEqual(2, list.Count);
      foreach (var track in result) {
        Assert.IsNotNull(track);
      }
    }

    [Test]
    public void OrderByTest()
    {
      var result = from ft in Session.Query.FreeText<Track>(() => "black")
        where ft.Entity.TrackId > 0 && ft.Entity.UnitPrice > 0.8m
        orderby ft.Entity.Album.Title, ft.Entity.UnitPrice
        select new {Track = ft.Entity, ft.Entity.Name};
      var list = result.ToList();
      Assert.AreEqual(26, list.Count);
    }

    [Test]
    public void TopNByRankOrderByTest()
    {
      var keywords = "best greatestHits";
      var defaultFreeTextQuery = Session.Query.FreeText<Album>(keywords).Take(2).OrderBy(r => r.Rank).ToList();
      Assert.IsTrue(defaultFreeTextQuery.Count()==2);
      var topNByrankOrdered = Session.Query.FreeText<Album>(keywords, 2).OrderBy(r => r.Rank);
      Assert.IsTrue(topNByrankOrdered.Count()==2);
      Assert.IsTrue(
        defaultFreeTextQuery
          .Select(rec => rec.Entity.Title)
          .SequenceEqual(topNByrankOrdered.Select(rec => rec.Entity.Title)));
    }

    [Test]
    public void TopNByRankOrderByExpressionTest()
    {
      var keywords = "best greatestHits";
      var defaultFreeTextQuery = Session.Query.FreeText<Album>(() => keywords).Take(2).OrderBy(r => r.Rank).ToList();
      Assert.IsTrue(defaultFreeTextQuery.Count()==2);
      var topnByRankOrdered = Session.Query.FreeText<Album>(() => keywords, 2).OrderBy(r => r.Rank);
      Assert.IsTrue(topnByRankOrdered.Count()==2);
      Assert.IsTrue(
        defaultFreeTextQuery
          .Select(rec => rec.Entity.Title)
          .SequenceEqual(topnByRankOrdered.Select(rec => rec.Entity.Title)));
    }

    [Test]
    public void TopNByRankTakeMaxTest()
    {
      var keywords = "best greatestHits";
      var topNByRank = Session.Query.FreeText<Album>(keywords, 3).ToList();
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByRankTakeSameAmmount = Session.Query.FreeText<Album>(keywords, 3).Take(3).OrderByDescending(r => r.Rank);
      Assert.IsTrue(topNByRankTakeSameAmmount.Count()==3);
      Assert.IsTrue(
        topNByRank
          .Select(rec => rec.Entity.Title)
          .SequenceEqual(topNByRankTakeSameAmmount.Select(rec => rec.Entity.Title)));
    }

    [Test]
    public void TopNByRankTakeLessThanMaxTest()
    {
      var keywords = "best greatestHits";
      var topNByRank = Session.Query.FreeText<Album>(keywords, 3);
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByRankTakeLess = Session.Query.FreeText<Album>(keywords, 3).Take(2).OrderByDescending(r => r.Rank).ToList();
      Assert.IsTrue(topNByRankTakeLess.Count()==2);
      Assert.IsTrue(
        topNByRankTakeLess
          .Select(r => r.Entity.Title)
          .SequenceEqual(topNByRank.Take(2).OrderByDescending(r => r.Rank).Select(r => r.Entity.Title)));
    }

    [Test]
    public void TopNByRankTakeMoreThanMaxTest()
    {
      var keywords = "best greatestHits";
      var topNByRank = Session.Query.FreeText<Album>(keywords, 3);
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByrankTakeMore = Session.Query.FreeText<Album>(keywords, 3).Take(4).OrderByDescending(r => r.Rank).ToList();
      Assert.IsTrue(topNByrankTakeMore.Count()==3);
      Assert.IsTrue(topNByrankTakeMore.Select(r => r.Entity.Title).SequenceEqual(topNByRank.Select(r => r.Entity.Title)));
    }

    [Test]
    public void TopNByRankOrderByAndTakeSequenceTest()
    {
      var keywords = "best greatestHits";
      var topNByRank = Session.Query.FreeText<Album>(keywords, 3).ToList();
      Assert.IsTrue(topNByRank.Count()==3);
      var topNByrankOrderByAndTake = Session.Query.FreeText<Album>(keywords, 3)
        .OrderBy(r => r.Rank)
        .Take(3)
        .OrderByDescending(r => r.Rank).Take(2).ToList();
      Assert.IsTrue(topNByrankOrderByAndTake.Count()==2);
      Assert.IsTrue(
        topNByRank
          .Take(2)
          .OrderByDescending(r => r.Rank)
          .Select(r => r.Entity.Title)
          .SequenceEqual(topNByrankOrderByAndTake.Select(rec => rec.Entity.Title)));
    }

    [Test]
    public void FreeTextOnFullTextStructureField()
    {
      var keywords = "Boulevard Innere Woodstock Discos";
      var result = Query.FreeText<Customer>(keywords).ToList();
      Assert.AreEqual(2, result.Count);
    }

    [Test]
    public void FreeTextTopNByRankOnFullTextStructureField()
    {
      var keywords = "Rotenturmstraße Innere Stadt Woodstock Boulevard";
      var result = Query.FreeText<Customer>(keywords, 2).ToList();
      var closestMatch = result.First().Entity;
      Assert.AreEqual(2, result.Count);
      Assert.IsTrue(closestMatch.Address.StreetAddress=="Rotenturmstraße 4, 1010 Innere Stadt");
    }

    [Test]
    public void FreeTextOnStructureField()
    {
      var result = Query.FreeText<Customer>("London").ToList();
      Assert.IsTrue(!result.Any());
    }

    private IEnumerable<FullTextMatch<Album>> TakeMatchesIncorrect(string searchCriteria)
    {
      return Session.Query.Execute(qe => qe.FreeText<Album>(searchCriteria));
    }

    private IEnumerable<FullTextMatch<Album>> TakeMatchesCorrect(string searchCriteria)
    {
      return Session.Query.Execute(qe => qe.FreeText<Album>(() => searchCriteria));
    }
  }
}