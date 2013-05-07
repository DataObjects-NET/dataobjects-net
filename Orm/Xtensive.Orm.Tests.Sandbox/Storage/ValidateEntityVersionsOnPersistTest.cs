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

  public class EntityWithVersion3 : EntityWithVersion2
  {
    [Field]
    public decimal Price1 { get; set; }

    [Field]
    public decimal? Price2 { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithoutVersion : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
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
          Count1 = 2,
        };
        new EntityWithVersion3 {
          Name = "3",
          Amount = 3,
          Count1 = 3,
          Price1 = 3,
        };
        new EntityWithoutVersion();
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var e1 = session.Query.All<EntityWithVersion1>().Single(e => e.Name=="1");
        e1.Name = e1.Name + "changed";
        session.SaveChanges();

        var e2 = session.Query.All<EntityWithVersion2>().Single(e => e.Name=="2");
        e2.Name = e2.Name + "changed";
        e2.Count1++;
        session.SaveChanges();

        var e3 = session.Query.All<EntityWithVersion3>().Single(e => e.Name=="3");
        e3.Name = e3.Name + "changed";
        session.SaveChanges();

        var w = session.Query.All<EntityWithoutVersion>().Single();

        e1.Remove();
        e2.Remove();
        w.Remove();
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var e3 = session.Query.All<EntityWithVersion1>().Single(e => e.Name=="3changed");
        e3.Name = "3changed2";
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var e3 = session.Query.All<EntityWithVersion2>().Single(e => e.Name=="3changed2");
        e3.Count2 = 4;
        session.SaveChanges();
        e3.Remove();
        tx.Complete();
      }
    }
  }
}