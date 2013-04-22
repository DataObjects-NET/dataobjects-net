// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.04.22

using System.Linq;
using NUnit.Framework;
using ValidateEntityVersionsOnPersistTestModel;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace ValidateEntityVersionsOnPersistTestModel
{
  [HierarchyRoot]
  public class EntityWithVersion1 : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public decimal Amount { get; set; }
  }

  public class EntityWithVersion2 : EntityWithVersion1
  {
    [Field]
    public int? Count1 { get; set; }

    [Field]
    public int? Count2 { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class ValidateEntityVersionsOnPersistTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      var defaultSession = configuration.Sessions.Default;
      defaultSession.Options = defaultSession.Options | SessionOptions.ValidateEntityVersions;
      configuration.Types.Register(typeof (EntityWithVersion1).Assembly, typeof (EntityWithVersion1).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new EntityWithVersion1 {
          Name = "1",
          Amount = 1,
        };
        new EntityWithVersion2 {
          Name = "2",
          Amount = 2,
          Count1 = 1,
        };
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var e1 = session.Query.All<EntityWithVersion1>().Single(e => e.Name=="1");
        var e2 = session.Query.All<EntityWithVersion2>().Single(e => e.Name=="2");
        e1.Name = e1.Name + "changed";
        e2.Name = e2.Name + "changed";
        e2.Count1++;
        session.SaveChanges();
        tx.Complete();
      }
    }
  }
}