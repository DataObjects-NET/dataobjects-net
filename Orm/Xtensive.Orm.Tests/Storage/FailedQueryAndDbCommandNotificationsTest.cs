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
      using(var session = Domain.OpenSession()){
        session.Events.QueryExecuted += (sender, args) => {
          var exception = args.Exception;
          Assert.IsNotNull(exception);
        };
        Assert.Throws<InvalidOperationException>(() => session.Query.All<TestModel>().Single(m => m.SomeStringField=="lol"));
      }
    }

    [Test]
    public void ValidQueryTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.QueryExecuted += (sender, args) => { Assert.IsNull(args.Exception);};
        using (session.OpenTransaction()) 
          Assert.DoesNotThrow(() => session.Query.All<TestModel>().Single(m => m.SomeStringField=="string1"));
      }
    }

    [Test]
    public void InvalidDbCommandTest()
    {
      Exception commandExecutedException = null;
      using (var session = Domain.OpenSession()) {
        session.Events.DbCommandExecuted += (sender, args) => {
          var exception = args.Exception;
          Assert.IsNotNull(exception);
        };
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
      }
    }

    [Test]
    public void ValidDbCommandTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.DbCommandExecuted += (sender, args) => {Assert.IsNull(args.Exception); };
        using (session.OpenTransaction()) {
          new TestModel { SomeStringField = "wat", SomeDateTimeField = DateTime.Now, UniqueValue = 3 };
          new TestModel { SomeStringField = "dat", SomeDateTimeField = DateTime.Now, UniqueValue = 4 };
          session.SaveChanges();
        }
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
