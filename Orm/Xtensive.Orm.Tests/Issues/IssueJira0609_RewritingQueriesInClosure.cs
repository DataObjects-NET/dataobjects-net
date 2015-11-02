// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2009.11.02

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Xtensive.Orm.Tests.Issues.IssueJira0609_RewritingQueriesInClosureModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0609_RewritingQueriesInClosure : AutoBuildTest
  {
    [Test]
    public void DirectInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var q = Query.All<StoredMaterialLot>().Where(c => c.IsAvailable);
        var pallets = Query.All<BegroPallet>().Where(p => p.UniqueIdentifier.In("dfjghkdfjhg", "jdfhgkjfhg"));
        var storedContainers = Query.All<StoredContainer>().Where(sc => sc.Container.Id.In(pallets.Select(p => p.Id)));
        q = q.Where(sml => sml.StoredContainer.Id.In(storedContainers.Select(sc => sc.Id)));
        Assert.DoesNotThrow(()=>q.Count());
      }
    }

    [Test]
    public void IndirectInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var q = Query.All<StoredMaterialLot>().Where(c => c.IsAvailable);
        var a = new string[] {"dfjhgkjdfgj", "jdfgkjfd"};
        var pallets = Query.All<BegroPallet>().Where(p => p.UniqueIdentifier.In(a));
        var storedContainers = Query.All<StoredContainer>().Where(sc => sc.Container.Id.In(pallets.Select(p => p.Id)));
        q = q.Where(sml => sml.StoredContainer.Id.In(storedContainers.Select(sc => sc.Id)));
        Assert.DoesNotThrow(() => q.Count());
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(StoredMaterialLot));
      return configuration;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.Xtensive.Orm.Tests.Issues.IssueJira0609_RewritingQueriesInClosureModel
{
  [HierarchyRoot]
  public class StoredMaterialLot : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public bool IsAvailable { get; set; }

    [Field]
    public StoredContainer StoredContainer { get; set; }
  }

  [HierarchyRoot]
  public class BegroPallet : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string UniqueIdentifier { get; set; }
  }

  [HierarchyRoot]
  public class StoredContainer : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public BegroPallet Container { get; set; }
  }
}