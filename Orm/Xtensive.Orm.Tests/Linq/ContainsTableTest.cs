// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.12.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class ContainsTableTest : ChinookDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.SingleKeyRankTableFullText);
    }

    protected override Domain BuildDomain(Xtensive.Orm.Configuration.DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      StorageTestHelper.WaitFullTextIndexesPopulated(domain, TimeSpan.FromSeconds(20));
      return domain;
    }

    [Test]
    public void SearchByTypeWithoutFulltextIndexTest()
    {
      _ = Assert.Throws<QueryTranslationException>(() => Session.Query.ContainsTable<Artist>(e => e.SimpleTerm("some text")).Run());
      _ = Assert.Throws<QueryTranslationException>(() => Query.ContainsTable<Artist> (e => e.SimpleTerm("some text")).Run());
    }

    [Test]
    public void NullSearchConditionBuilderTest()
    {
      _ = Assert.Throws<ArgumentNullException>(() => Session.Query.ContainsTable<Artist>(null).Run());
      _ = Assert.Throws<ArgumentNullException>(() => Query.ContainsTable<Artist>(null).Run());
    }

    [Test]
    public void NegativeRank()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => Session.Query.ContainsTable<Artist>(e => e.SimpleTerm("some text"), -1).Run());
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => Query.ContainsTable<Artist>(e => e.SimpleTerm("some text"), -1).Run());
    }

    [Test]
    public void ReuseTest1()
    {
      var result = Session.Query.Execute(q => q.ContainsTable<Track>(e => e.SimpleTerm("Letterbomb")).Count());
      Assert.That(result, Is.EqualTo(1));
      result = Session.Query.Execute(q => q.ContainsTable<Track>(e => e.SimpleTerm("abcdefghijklmnop")).Count());
      Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void ReuseTest2()
    {
      var result = Query.Execute(() => Query.ContainsTable<Track>(e => e.SimpleTerm("Letterbomb")).Count());
      Assert.That(result, Is.EqualTo(1));
      result = Query.Execute(() => Query.ContainsTable<Track>(e => e.SimpleTerm("abcdefghijklmnop")).Count());
      Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void QueryingEntitesTest()
    {
      var tracks = Session.Query.ContainsTable<Track>(e => e.SimpleTerm("Letterbomb")).ToList();
      Assert.That(tracks.Count, Is.EqualTo(1));
      foreach (var track in tracks) {
        Assert.That(track, Is.Not.Null);
        Assert.That(track.Entity, Is.Not.Null);
        Assert.That(track.Entity.Name.Contains("Letterbomb"), Is.True);
      }

      tracks = Query.ContainsTable<Track>(e => e.SimpleTerm("Letterbomb")).ToList();
      Assert.That(tracks.Count, Is.EqualTo(1));
      foreach (var track in tracks) {
        Assert.That(track, Is.Not.Null);
        Assert.That(track.Entity, Is.Not.Null);
        Assert.That(track.Entity.Name.Contains("Letterbomb"), Is.True);
      }
    }

    [Test]
    public void QueryingRankTest()
    {
      var ranks = Session.Query.ContainsTable<Track>(e => e.SimpleTerm("Letterbomb")).Select(e=>e.Rank).ToList();
      Assert.That(ranks.Count, Is.EqualTo(1));
      Assert.That(ranks.All(r=> r > 0.0), Is.True);

      ranks = Query.ContainsTable<Track>(e => e.SimpleTerm("Letterbomb")).Select(e => e.Rank).ToList();
      Assert.That(ranks.Count, Is.EqualTo(1));
      Assert.That(ranks.All(r => r > 0.0), Is.True);
    }

    [Test]
    public void LimitResultsByRankTest()
    {
      var actualRanks = Session.Query.ContainsTable<Track>(e => e.PrefixTerm("Com")).Select(p => p.Rank).ToList();
      Assert.That(actualRanks.Count, Is.EqualTo(65));

      var limitedResults = Session.Query.ContainsTable<Track>(e => e.PrefixTerm("Com"), 2).Select(p => p.Rank).ToList();
      Assert.That(limitedResults.Count, Is.EqualTo(2));
      var topTwoRanks = actualRanks.OrderByDescending(r => r).Take(2).ToList();
      Assert.That(limitedResults.All(r => topTwoRanks.Contains(r)), Is.True);

      actualRanks = Query.ContainsTable<Track>(e => e.PrefixTerm("Com")).Select(p => p.Rank).ToList();
      Assert.That(actualRanks.Count, Is.EqualTo(65));

      limitedResults = Query.ContainsTable<Track>(e => e.PrefixTerm("Com"), 2).Select(p => p.Rank).ToList();
      Assert.That(limitedResults.Count, Is.EqualTo(2));
      topTwoRanks = actualRanks.OrderByDescending(r => r).Take(2).ToList();
      Assert.That(limitedResults.All(r => topTwoRanks.Contains(r)), Is.True);
    }
  }
}
