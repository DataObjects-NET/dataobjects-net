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

    [Test]
    public void GroupByFakeKeyWithProjectionTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<Zames>()
            .Select(z => new {FakeKey = 0, Info = Query.All<ZamesInfo>().Where(info => info.Owner==z).FirstOrDefault()})
            .GroupBy(item => item.FakeKey)
            .Select(grouping => grouping.Min(item => item.Info.Rank))
            .First();
        }
      }
    }

    [Test]
    public void GroupByFakeKeyTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<Zames>()
            .Select(z => new {FakeKey = 0, Info = Query.All<ZamesInfo>().Where(info => info.Owner==z).Select(info => info.Rank).FirstOrDefault()})
            .GroupBy(item => item.FakeKey)
            .Select(grouping => grouping.Min())
            .First();
        }
      }
    }

    [Test]
    public void SimpleAggregateWithProjectionTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<Zames>()
            .Select(z => Query.All<ZamesInfo>().Where(info => info.Owner==z).FirstOrDefault())
            .Min(info => info.Rank);
        }
      }
    }

    [Test]
    public void SimpleAggregateTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var result = Query.All<Zames>()
            .Select(z => Query.All<ZamesInfo>().Where(info => info.Owner==z).Select(info => info.Rank).FirstOrDefault())
            .Min();
        }
      }
    }
  }
}