using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class ContainsTableTest : NorthwindDOModelTest
  {
    [Test]
    public void SearchByTypeWithoutFulltextIndexTest()
    {
      Assert.Throws<QueryTranslationException>(() => Session.Query.ContainsTable<Region>(e => e.SimpleTerm("some text")).Run());
      Assert.Throws<QueryTranslationException>(() => Query.ContainsTable<Region>(e => e.SimpleTerm("some text")).Run());
    }

    [Test]
    public void NullSearchConditionBuilderTest()
    {
      Assert.Throws<ArgumentNullException>(() => Session.Query.ContainsTable<Category>(null).Run());
      Assert.Throws<ArgumentNullException>(() => Query.ContainsTable<Category>(null).Run());
    }

    [Test]
    public void NegativeRank()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => Session.Query.ContainsTable<Category>(e => e.SimpleTerm("some text"), -1).Run());
      Assert.Throws<ArgumentOutOfRangeException>(() => Query.ContainsTable<Category>(e => e.SimpleTerm("some text"), -1).Run());
    }

    [Test]
    public void ReuseTest1()
    {
      var result = Session.Query.Execute(q => q.ContainsTable<Product>(e => e.SimpleTerm("Queso")).Count());
      Assert.That(result, Is.EqualTo(2));
      result = Session.Query.Execute(q => q.ContainsTable<Product>(e => e.SimpleTerm("abcdefghijklmnop")).Count());
      Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void ReuseTest2()
    {
      var result = Query.Execute(() => Query.ContainsTable<Product>(e => e.SimpleTerm("Queso")).Count());
      Assert.That(result, Is.EqualTo(2));
      result = Query.Execute(() => Query.ContainsTable<Product>(e => e.SimpleTerm("abcdefghijklmnop")).Count());
      Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void QueryingEntitesTest()
    {
      var products = Session.Query.ContainsTable<Product>(e => e.SimpleTerm("Queso")).ToList();
      Assert.That(products.Count, Is.EqualTo(2));
      foreach (var product in products) {
        Assert.That(product, Is.Not.Null);
        Assert.That(product.Entity, Is.Not.Null);
        Assert.That(product.Entity.ProductName.Contains("Queso"), Is.True);
      }

      products = Query.ContainsTable<Product>(e => e.SimpleTerm("Queso")).ToList();
      Assert.That(products.Count, Is.EqualTo(2));
      foreach (var product in products) {
        Assert.That(product, Is.Not.Null);
        Assert.That(product.Entity, Is.Not.Null);
        Assert.That(product.Entity.ProductName.Contains("Queso"), Is.True);
      }
    }

    [Test]
    public void QueryingRankTest()
    {
      var ranks = Session.Query.ContainsTable<Product>(e => e.SimpleTerm("Queso")).Select(e=>e.Rank).ToList();
      Assert.That(ranks.Count, Is.EqualTo(2));
      Assert.That(ranks.All(r=> r > 0.0), Is.True);

      ranks = Query.ContainsTable<Product>(e => e.SimpleTerm("Queso")).Select(e => e.Rank).ToList();
      Assert.That(ranks.Count, Is.EqualTo(2));
      Assert.That(ranks.All(r => r > 0.0), Is.True);
    }

    [Test]
    public void LimitResultsByRankTest()
    {
      var actualRanks = Session.Query.ContainsTable<Product>(e => e.PrefixTerm("Cho")).Select(p => p.Rank).ToList();
      Assert.That(actualRanks.Count, Is.EqualTo(3));

      var limitedResults = Session.Query.ContainsTable<Product>(e => e.PrefixTerm("Cho"), 2).Select(p => p.Rank).ToList();
      Assert.That(limitedResults.Count, Is.EqualTo(2));
      var topTwoRanks = actualRanks.OrderBy(r => r).Take(2).ToList();
      Assert.That(limitedResults.All(r => topTwoRanks.Contains(r)), Is.True);

      actualRanks = Query.ContainsTable<Product>(e => e.PrefixTerm("Cho")).Select(p => p.Rank).ToList();
      Assert.That(actualRanks.Count, Is.EqualTo(3));

      limitedResults = Query.ContainsTable<Product>(e => e.PrefixTerm("Cho"), 2).Select(p => p.Rank).ToList();
      Assert.That(limitedResults.Count, Is.EqualTo(2));
      topTwoRanks = actualRanks.OrderBy(r => r).Take(2).ToList();
      Assert.That(limitedResults.All(r => topTwoRanks.Contains(r)), Is.True);
    }

  }
}
