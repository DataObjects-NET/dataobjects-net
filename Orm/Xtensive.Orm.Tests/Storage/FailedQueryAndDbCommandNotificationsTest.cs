// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      using (var session = Domain.OpenSession()) {
        session.Events.QueryExecuted += InvalidQueryHandler;
        using (session.OpenTransaction()) {
          _ = Assert.Throws<InvalidOperationException>(() => session.Query.All<TestModel>().Single(m => m.SomeStringField == "lol"));
        }
        session.Events.QueryExecuted -= InvalidQueryHandler;
        Assert.IsNotNull(thrownException);
        Assert.AreSame(typeof (InvalidOperationException), thrownException.GetType());
      }

      void InvalidQueryHandler(object sender, QueryEventArgs args)
      {
        thrownException = args.Exception;
      }
    }

    [Test]
    public void ValidQueryTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.QueryExecuted += ValidQueryHandler;
        using (session.OpenTransaction()) 
          Assert.DoesNotThrow(() => session.Query.All<TestModel>().Single(m => m.SomeStringField=="string1"));
        session.Events.QueryExecuted -= ValidQueryHandler;
      }

      static void ValidQueryHandler(object sender, QueryEventArgs args)
      {
        Assert.IsNull(args.Exception);
      }
    }

    [Test]
    public void InvalidDbCommandTest()
    {
      Exception commandExecutedException = null;
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          _ = new TestModel { SomeStringField = "wat", SomeDateTimeField = DateTime.Now, UniqueValue = 1 };
          var expectedException = GetUniqueConstraintViolationExceptionType();
          session.Events.DbCommandExecuted += InvalidDbCommandHandler;
          _ = Assert.Throws(expectedException, () => session.SaveChanges());
          session.Events.DbCommandExecuted -= InvalidDbCommandHandler;
          Assert.NotNull(commandExecutedException);
          Assert.AreEqual(expectedException, commandExecutedException.GetType());
        }
      }

      void InvalidDbCommandHandler(object sender, DbCommandEventArgs args)
      {
        commandExecutedException = args.Exception;
      }
    }

    [Test]
    public void ValidDbCommandTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          _ = new TestModel { SomeStringField = "wat", SomeDateTimeField = DateTime.Now, UniqueValue = 3 };
          _ = new TestModel { SomeStringField = "dat", SomeDateTimeField = DateTime.Now, UniqueValue = 4 };
          session.Events.DbCommandExecuted += ValidDbCommandHandler;
          Assert.DoesNotThrow(() => session.SaveChanges());
          session.Events.DbCommandExecuted -= ValidDbCommandHandler;
        }
      }

      static void ValidDbCommandHandler(object sender, DbCommandEventArgs args)
      {
        Assert.IsNull(args.Exception);
      }
    }

    protected override void PopulateData()
    {
      using(var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestModel { SomeStringField = "string1", SomeDateTimeField = DateTime.Now, UniqueValue = 1 };
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof(TestModel).Assembly, typeof(TestModel).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private Type GetUniqueConstraintViolationExceptionType()
    {
      var providerName = ProviderInfo.ProviderName;
      switch (providerName) {
        case WellKnown.Provider.Sqlite:
          return typeof(StorageException);
        default:
          return typeof(UniqueConstraintViolationException);
      }
    }
  }
}
