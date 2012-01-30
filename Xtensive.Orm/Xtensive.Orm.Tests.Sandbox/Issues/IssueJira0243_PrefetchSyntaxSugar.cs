// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.30

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0243_PrefetchSyntaxSugarModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0243_PrefetchSyntaxSugarModel
{
  [HierarchyRoot]
  public class Jira0243Owner : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Jira0243Target Target1 { get; set; }

    [Field]
    public Jira0243Target Target2 { get; set; }
  }

  [HierarchyRoot]
  public class Jira0243Target : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0243_PrefetchSyntaxSugar : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Jira0243Owner).Assembly, typeof (Jira0243Target).Namespace);
      return configuration;
    }

    private Key target1Key;
    private Key target2Key;

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var owner = new Jira0243Owner {
          Target1 = new Jira0243Target(),
          Target2 = new Jira0243Target(),
        };
        target1Key = owner.Target1.Key;
        target2Key = owner.Target2.Key;
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // The following statement should prefetch Target1 and Target2
        var result = session.Query.All<Jira0243Owner>()
          .Prefetch(x => new {x.Target1, x.Target2})
          .ToList();

        // Check that Target1 and Target2 are loaded into session.
        AssertEntityIsFetched(session, target1Key);
        AssertEntityIsFetched(session, target2Key);
      }
    }

    private void AssertEntityIsFetched(Session session, Key key)
    {
      EntityState dummy;
      Assert.That(session.EntityStateCache.TryGetItem(key, false, out dummy), Is.True);
    }
  }
}