// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.07.16

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0450_SingleOrDefaultInvalidResultModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0450_SingleOrDefaultInvalidResultModel
  {
    [HierarchyRoot]
    public class Entity1 : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }

    public class Entity1Child : Entity1
    {
    }

    [HierarchyRoot]
    public class Entity2 : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }
  }

  [TestFixture]
  public class IssueJira0450_SingleOrDefaultInvalidResult : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Entity1).Assembly, typeof (Entity1).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      long id;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        id = new Entity1().Id;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity2 = session.Query.SingleOrDefault<Entity2>(id);
        Assert.That(entity2, Is.Null);
        var entity1 = session.Query.SingleOrDefault<Entity1>(id);
        Assert.That(entity1, Is.Not.Null);
        tx.Complete();
      }
    }
  }
}