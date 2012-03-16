// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.07.29

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0168_RemoveQueryableExtensionFails_Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0168_RemoveQueryableExtensionFails_Model
{
  [HierarchyRoot]
  public class Target : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    public Target(Session session) :
      base(session)
    {}
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0168_RemoveQueryableExtensionFails : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Target).Assembly, typeof (Target).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var sc = new SessionConfiguration(SessionOptions.ServerProfile);
      using (var session = Domain.OpenSession(sc)) {
        using (var t = session.OpenTransaction()) {
          
          new Target(session);
          session.SaveChanges();

          session.Query.All<Target>().Remove();
          // Rollback
        }
      }
    }
  }
}