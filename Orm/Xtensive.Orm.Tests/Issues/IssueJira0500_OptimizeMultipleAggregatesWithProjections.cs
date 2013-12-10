// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.10

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0500_OptimizeMultipleAggregatesWithProjectionsModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0500_OptimizeMultipleAggregatesWithProjectionsModel
  {
    [HierarchyRoot]
    public class ReferencedEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public decimal Value { get; set; }
    }

    [HierarchyRoot]
    public class AggregatedEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public int Group { get; set; }

      [Field]
      public decimal Value { get; set; }

      [Field]
      public ReferencedEntity Ref { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0500_OptimizeMultipleAggregatesWithProjections : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (AggregatedEntity).Assembly, typeof (AggregatedEntity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ref1 = new ReferencedEntity {Value = 1};
        var ref2 = new ReferencedEntity {Value = 2};
        var ref3 = new ReferencedEntity {Value = 3};
        new AggregatedEntity {Value = 1, Ref = ref1};
        new AggregatedEntity {Value = 2, Ref = ref2};
        new AggregatedEntity {Value = 3, Ref = ref3};
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<AggregatedEntity>()
          .GroupBy(e => e.Group)
          .Select(g => new {
            SumValue = g.Sum(e => e.Value),
            SumRefValue = g.Sum(e => e.Ref.Value),
          });
        query.ToList();
        tx.Complete();
      }
    }
  }
}