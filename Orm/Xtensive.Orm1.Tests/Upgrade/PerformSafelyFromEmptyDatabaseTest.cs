// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.07.05

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class PerformSafelyFromEmptyDatabaseTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "Server-specific SQL is used");
    }

    private Domain BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      return Domain.Build(configuration);
    }

    [Test]
    public void MainTest()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var accessor = session.Services.Demand<DirectSqlAccessor>();
        using (var command = accessor.CreateCommand()) {
          command.CommandText =
            "drop table [Metadata.Assembly]; " +
            "drop table [Metadata.Type]; " +
            "drop table [Metadata.Extension]";
          command.ExecuteNonQuery();
        }
        tx.Complete();
      }

      BuildDomain(DomainUpgradeMode.PerformSafely).Dispose();

      BuildDomain(DomainUpgradeMode.Validate).Dispose();
    }
  }
}