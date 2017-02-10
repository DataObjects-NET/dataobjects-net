// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.02.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.NotifyExceptionOnExecutionTestModel;

namespace Xtensive.Orm.Tests.Storage.NotifyExceptionOnExecutionTestModel
{
  [HierarchyRoot]
  [Index("UniqueValue", Unique = true)]
  public class TestModel : Entity
  {
    [Key, Field]
    public long Id { get; set; }

    [Field]
    public string SomeStringField { get; set; }

    [Field]
    public DateTime SomeDateTimeField { get; set; }

    [Field]
    public int UniqueValue { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class FailedQueryAndDbCommandNotificationsTest : AutoBuildTest
  {
    [Test]
    public void InvalidQueryTest()
    {
      EventHandler<QueryEventArgs> invalidQueryHandler = (sender, args) => { Assert.IsNotNull(args.Exception); };
      using(var session = Domain.OpenSession()) {
        session.Events.QueryExecuted += invalidQueryHandler;
        Assert.Throws<InvalidOperationException>(() => session.Query.All<TestModel>().Single(m => m.SomeStringField=="lol"));
        session.Events.QueryExecuted -= invalidQueryHandler;
      }
    }

    [Test]
    public void ValidQueryTest()
    {
      EventHandler<QueryEventArgs> validQueryHandler = (sender, args) => { Assert.IsNull(args.Exception); };
      using (var session = Domain.OpenSession()) {
        session.Events.QueryExecuted += validQueryHandler;
        using (session.OpenTransaction()) 
          Assert.DoesNotThrow(() => session.Query.All<TestModel>().Single(m => m.SomeStringField=="string1"));
        session.Events.QueryExecuted -= validQueryHandler;
      }
    }

    [Test]
    public void InvalidDbCommandTest()
    {
      Exception commandExecutedException = null;
      EventHandler<DbCommandEventArgs> invalidDbCommandHandler = (sender, args) => { Assert.IsNotNull(args.Exception); };
      using (var session = Domain.OpenSession()) {
        session.Events.DbCommandExecuted += invalidDbCommandHandler;
        using (session.OpenTransaction()) {
          new TestModel {SomeStringField = "wat", SomeDateTimeField = DateTime.Now, UniqueValue = 1};
          try {
            session.SaveChanges();
          } 
          catch (Exception ex) {
            commandExecutedException = ex;
          }
          Assert.NotNull(commandExecutedException);
        }
        session.Events.DbCommandExecuted -= invalidDbCommandHandler;
      }
    }

    [Test]
    public void ValidDbCommandTest()
    {
      EventHandler<DbCommandEventArgs> validDbCommandHandler = (sender, args) => { Assert.IsNull(args.Exception); };
      using (var session = Domain.OpenSession()) {
        session.Events.DbCommandExecuted += validDbCommandHandler;
        using (session.OpenTransaction()) {
          new TestModel { SomeStringField = "wat", SomeDateTimeField = DateTime.Now, UniqueValue = 3 };
          new TestModel { SomeStringField = "dat", SomeDateTimeField = DateTime.Now, UniqueValue = 4 };
          session.SaveChanges();
        }
        session.Events.DbCommandExecuted -= validDbCommandHandler;
      }
    }

    protected override void PopulateData()
    {
      using(var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestModel { SomeStringField = "string1", SomeDateTimeField = DateTime.Now, UniqueValue = 1 };
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestModel).Assembly, typeof (TestModel).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
