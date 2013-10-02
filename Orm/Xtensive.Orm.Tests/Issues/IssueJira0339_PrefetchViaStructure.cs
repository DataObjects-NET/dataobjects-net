// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.22

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0339_PrefetchViaStructureModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0339_PrefetchViaStructureModel
  {
    public class Jira0339Struct : Structure
    {
      [Field]
      public Jira0339Target Target { get; set; }
    }

    [HierarchyRoot]
    public class Jira0339Owner : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Jira0339Struct Targets { get; set; }
    }

    [HierarchyRoot]
    public class Jira0339Target : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }
  }

  public class IssueJira0339_PrefetchViaStructure : AutoBuildTest
  {
    private Key targetKey;

    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Jira0339Owner).Assembly, typeof (Jira0339Owner).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var target = new Jira0339Target();
        var owner = new Jira0339Owner();
        targetKey = target.Key;
        owner.Targets.Target = target;
        tx.Complete();
      }
    }

    [Test]
    public void PrefetchViaPersistentType()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = Query.All<Jira0339Owner>()
          .Prefetch(o => o.Targets.Target)
          .ToList();
        StorageTestHelper.IsFetched(session, targetKey);
      }
    }

    [Test]
    public void PrefetchViaNonPersistentType()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = Query.All<Jira0339Owner>()
          .GroupBy(o => new {Owner = o})
          .Prefetch(g => g.Key.Owner.Targets.Target)
          .ToList();
        StorageTestHelper.IsFetched(session, targetKey);
      }
    }
  }
}