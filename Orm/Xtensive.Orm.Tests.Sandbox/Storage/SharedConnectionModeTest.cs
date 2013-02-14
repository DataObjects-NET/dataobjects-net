// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.14

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.SharedConnectionModeTestModel;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Testing;

namespace Xtensive.Orm.Tests.Storage
{
  namespace SharedConnectionModeTestModel
  {
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
  public class SharedConnectionModeTest
  {
    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();

      configuration.Types.Register(typeof (Upgrader).Assembly, typeof (Upgrader).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      configuration.SharedConnection = true;

      var domain = Domain.Build(configuration);

      var sharedConnection = Upgrader.Connection;

      CheckSharedConnection(domain, sharedConnection);

      using (var session = domain.OpenSession()) {
        AssertEx.ThrowsInvalidOperationException(() => domain.OpenSession());
      }

      CheckSharedConnection(domain, sharedConnection);
    }

    [TearDown]
    public void TearDown()
    {
      Upgrader.Connection = null;
    }

    private static void CheckSharedConnection(Domain domain, SqlConnection sharedConnection)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var dbConnection = session.Services.Demand<DirectSqlAccessor>().Connection;
        Assert.That(dbConnection, Is.SameAs(sharedConnection.UnderlyingConnection));
      }
    }
  }
}