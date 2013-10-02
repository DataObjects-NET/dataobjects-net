// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.26

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.DoubleAggregateToStructTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace DoubleAggregateToStructTestModel
  {
    public struct GroupInfo
    {
      public int Key { get; set; }

      public int Sum1 { get; set; }

      public int Sum2 { get; set; }
    }

    [HierarchyRoot]
    public class Aggregated : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public int GroupId { get; private set; }

      [Field]
      public int Value1 { get; private set; }

      [Field]
      public int Value2 { get; private set; }

      public Aggregated(int value1, int value2)
      {
        GroupId = 0;
        Value1 = value1;
        Value2 = value2;
      }
    }
  }

  public class DoubleAggregateToStructTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Aggregated));
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Aggregated(1, 2);
        new Aggregated(2, 3);
        tx.Complete();
      }
    }

    [Test]
    public void SumTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var q = session.Query.All<Aggregated>().GroupBy(a => a.GroupId)
          .Select(g => new GroupInfo {
            Key = g.Key,
            Sum1 = g.Sum(x => x.Value1),
            Sum2 = g.Sum(x => x.Value2)
          });
        var result = q.ToList().Single();
        Assert.That(result.Sum1, Is.EqualTo(3));
        Assert.That(result.Sum2, Is.EqualTo(5));
        tx.Complete();
      }
    }
  }
}