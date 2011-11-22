// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.21

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueJira0221_UnableToTranslateAggregateModel;

namespace Xtensive.Storage.Tests.Issues.IssueJira0221_UnableToTranslateAggregateModel
{
  public class RowTuple<TValue>
  {
    public int FakeKey { get; set; }
    public TValue Value { get; set; }
  }

  [HierarchyRoot]
  public class Zames : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class ZamesInfo : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Zames Owner { get; set; }

    [Field]
    public int Rank { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0221_UnableToTranslateAggregate : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Zames).Assembly, typeof (Zames).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      CreateSessionAndTransaction();

      new ZamesInfo {Owner = new Zames(), Rank = 1};
      new ZamesInfo {Owner = new Zames(), Rank = 3};
    }

    #region Min()

    [Test]
    public void MinOnGroupingsTest()
    {
      var result = GetGroupingsQuery()
        .Select(grouping => grouping.Min(item => item.Value.Rank))
        .FirstOrDefault();
      Assert.AreEqual(1, result);
    }

    [Test]
    public void MinOnEntitiesTest()
    {
      var result = GetEntitiesQuery().Min(info => info.Rank);
      Assert.AreEqual(1, result);
    }

    [Test]
    public void MinOnValuesTest()
    {
      var result = GetValuesQuery().Min();
      Assert.AreEqual(1, result);
    }

    #endregion

    #region Max()

    [Test]
    public void MaxOnGroupingsTest()
    {
      var result = GetGroupingsQuery()
        .Select(grouping => grouping.Max(item => item.Value.Rank))
        .FirstOrDefault();
      Assert.AreEqual(3, result);
    }

    [Test]
    public void MaxOnEntitiesTest()
    {
      var result = GetEntitiesQuery().Max(info => info.Rank);
      Assert.AreEqual(3, result);
    }

    [Test]
    public void MaxOnValuesTest()
    {
      var result = GetValuesQuery().Max();
      Assert.AreEqual(3, result);
    }

    #endregion

    #region Average()

    [Test]
    public void AverageOnGroupingsTest()
    {
      var result = GetGroupingsQuery()
        .Select(grouping => grouping.Average(item => item.Value.Rank))
        .FirstOrDefault();
      Assert.AreEqual(2, result);
    }

    [Test]
    public void AverageOnEntitiesTest()
    {
      var result = GetEntitiesQuery().Average(info => info.Rank);
      Assert.AreEqual(2, result);
    }

    [Test]
    public void AverageOnValuesTest()
    {
      var result = GetValuesQuery().Average();
      Assert.AreEqual(2, result);
    }

    #endregion

    #region Sum()

    [Test]
    public void SumOnGroupingsTest()
    {
      var result = GetGroupingsQuery()
        .Select(grouping => grouping.Sum(item => item.Value.Rank))
        .FirstOrDefault();
      Assert.AreEqual(4, result);
    }

    [Test]
    public void SumOnEntitiesTest()
    {
      var result = GetEntitiesQuery().Sum(info => info.Rank);
      Assert.AreEqual(4, result);
    }

    [Test]
    public void SumOnValuesTest()
    {
      var result = GetValuesQuery().Sum();
      Assert.AreEqual(4, result);
    }

    #endregion

    #region Count()

    [Test]
    public void CountOnGroupingsTest()
    {
      var result = GetGroupingsQuery()
        .Select(grouping => grouping.Count())
        .FirstOrDefault();
      Assert.AreEqual(2, result);
    }

    [Test]
    public void CountOnEntitiesTest()
    {
      var result = GetEntitiesQuery().Count();
      Assert.AreEqual(2, result);
    }

    [Test]
    public void CountOnValuesTest()
    {
      var result = GetValuesQuery().Count();
      Assert.AreEqual(2, result);
    }

    #endregion

    #region LongCount()

    [Test]
    public void LongCountOnGroupingsTest()
    {
      var result = GetGroupingsQuery()
        .Select(grouping => grouping.LongCount())
        .FirstOrDefault();
      Assert.AreEqual(2, result);
    }

    [Test]
    public void LongCountOnEntitiesTest()
    {
      var result = GetEntitiesQuery().LongCount();
      Assert.AreEqual(2, result);
    }

    [Test]
    public void LongCountOnValuesTest()
    {
      var result = GetValuesQuery().LongCount();
      Assert.AreEqual(2, result);
    }

    #endregion

    #region Helpers

    private static IQueryable<int> GetValuesQuery()
    {
      return Query.All<Zames>()
        .Select(z => Query.All<ZamesInfo>().Where(info => info.Owner==z).Select(info => info.Rank).FirstOrDefault());
    }

    private static IQueryable<ZamesInfo> GetEntitiesQuery()
    {
      return Query.All<Zames>()
        .Select(z => Query.All<ZamesInfo>().Where(info => info.Owner==z).FirstOrDefault());
    }

    private static IQueryable<IGrouping<int, RowTuple<ZamesInfo>>> GetGroupingsQuery()
    {
      return Query.All<Zames>()
        .Select(z => new RowTuple<ZamesInfo> {
          FakeKey = 0,
          Value = Query.All<ZamesInfo>().Where(info => info.Owner==z).FirstOrDefault()
        })
        .GroupBy(item => item.FakeKey);
    }

    #endregion
  }
}