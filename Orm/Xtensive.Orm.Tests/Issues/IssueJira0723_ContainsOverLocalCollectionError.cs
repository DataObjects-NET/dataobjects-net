// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2018.01.31

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0723_IncludeFilterMappingGathererHandlesMemberExpressionBadlyModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0723_IncludeFilterMappingGathererHandlesMemberExpressionBadlyModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public int Id { get; private set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0723_IncludeFilterMappingGathererHandlesMemberExpressionBadly : AutoBuildTest
  {
    private static int ItemId = 1;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var trasaction = session.OpenTransaction()) {
        var ids = new[] {1, 2};
        Assert.DoesNotThrow(()=>session.Query.All<TestEntity>().Count(e => ItemId.In(ids)));
        Assert.DoesNotThrow(()=>session.Query.All<TestEntity>().Count(e => ids.Contains(ItemId)));
      }
    }
  }
}
