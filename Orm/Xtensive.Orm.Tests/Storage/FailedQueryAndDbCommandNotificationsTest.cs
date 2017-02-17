// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.02.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.FailedQueryAndDbCommandNotificationsTestModel;

namespace Xtensive.Orm.Tests.Storage.FailedQueryAndDbCommandNotificationsTestModel
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
      Exception thrownException = null;
      EventHandler<QueryEventArgs> invalidQueryHandler = (sender, args) => {thrownException = args.Exception;};
      using (var session = Domain.OpenSession()) {
        session.Events.QueryExecuted += invalidQueryHandler;
        using (session.OpenTransaction())
          Assert.Throws<InvalidOperationException>(() => session.Query.All<TestModel>().Single(m => m.SomeStringField=="lol"));
        session.Events.QueryExecuted -= invalidQueryHandler;
        Assert.IsNotNull(thrownException);
        Assert.AreSame(typeof(InvalidOperationException), thrownException.GetType());
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
      EventHandler<DbCommandEventArgs> invalidDbCommandHandler = (sender, args) => { commandExecutedException = args.Exception; };
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          new TestModel { SomeStringField = "wat", SomeDateTimeField = DateTime.Now, UniqueValue = 1 };
          var expectedException = GetUniqueConstraintViolationExceptionType();
          session.Events.DbCommandExecuted += invalidDbCommandHandler;
          Assert.Throws(expectedException, () => session.SaveChanges());
          session.Events.DbCommandExecuted -= invalidDbCommandHandler;
          Assert.NotNull(commandExecutedException);
          Assert.AreEqual(expectedException, commandExecutedException.GetType());
        }
      }
    }

    [Test]
    public void ValidDbCommandTest()
    {
      EventHandler<DbCommandEventArgs> validDbCommandHandler = (sender, args) => { Assert.IsNull(args.Exception); };
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          new TestModel { SomeStringField = "wat", SomeDateTimeField = DateTime.Now, UniqueValue = 3 };
          new TestModel { SomeStringField = "dat", SomeDateTimeField = DateTime.Now, UniqueValue = 4 };
          session.Events.DbCommandExecuted += validDbCommandHandler;
          Assert.DoesNotThrow(() => session.SaveChanges());
          session.Events.DbCommandExecuted -= validDbCommandHandler;
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

    private Type GetUniqueConstraintViolationExceptionType()
    {
      var providerName = ProviderInfo.ProviderName;
      switch (providerName) {
        case WellKnown.Provider.MySql:
        case WellKnown.Provider.Sqlite:
        case WellKnown.Provider.Firebird:
          return typeof (StorageException);
        default:
        return typeof (UniqueConstraintViolationException);
      }
    }
  }
}
