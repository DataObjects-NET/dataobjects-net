// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.14

using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.MemoryProviderTestModel;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Testing;

namespace Xtensive.Orm.Tests.Storage
{
  namespace MemoryProviderTestModel
  {
    [HierarchyRoot]
    public class TheEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }

    public class Upgrader : UpgradeHandler
    {
      public static SqlConnection Connection;

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnPrepare()
      {
        Connection = UpgradeContext.Services.Connection;
      }
    }
  }

  [TestFixture]
  public class MemoryProviderTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.Sqlite, "Only sqlite supports memory data source");
    }

    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TheEntity).Assembly, typeof (TheEntity).Namespace);
      configuration.ConnectionInfo = new ConnectionInfo(WellKnown.Provider.Sqlite, "Data Source=:memory:");
      configuration.UpgradeMode = DomainUpgradeMode.Perform;

      var domain = Domain.Build(configuration);
      var singleConnection = Upgrader.Connection;

      Assert.That(domain.StorageProviderInfo.Supports(ProviderFeatures.SingleConnection));

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new TheEntity {Value = "in-memory"};
        CheckSingleConnection(session, singleConnection);
        tx.Complete();
      }

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var theEntity = session.Query.All<TheEntity>().Single();
        Assert.That(theEntity.Value, Is.EqualTo("in-memory"));
        CheckSingleConnection(session, singleConnection);
        tx.Complete();
      }

      using (var session = domain.OpenSession()) {
        AssertEx.ThrowsInvalidOperationException(() => domain.OpenSession());
      }

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        CheckSingleConnection(session, singleConnection);
      }

      domain.Dispose();

      AssertEx.Throws<ObjectDisposedException>(() => singleConnection.State.ToString());
    }

    [TearDown]
    public void TearDown()
    {
      Upgrader.Connection = null;
    }

    private static void CheckSingleConnection(Session session, SqlConnection singleConnection)
    {
      var dbConnection = session.Services.Demand<DirectSqlAccessor>().Connection;
      Assert.That(dbConnection, Is.SameAs(singleConnection.UnderlyingConnection));
    }
  }
}