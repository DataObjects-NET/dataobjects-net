// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public void TestFixtureSetUp() => Require.ProviderIs(StorageProvider.SqlServer, "Server-specific SQL is used");

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
          _ = command.ExecuteNonQuery();
        }
        tx.Complete();
      }

      BuildDomain(DomainUpgradeMode.PerformSafely).Dispose();

      BuildDomain(DomainUpgradeMode.Validate).Dispose();
    }

    [Test]
    public async Task MainAsyncTest()
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
          _ = command.ExecuteNonQuery();
        }
        tx.Complete();
      }

      (await BuildDomainAsync(DomainUpgradeMode.PerformSafely)).Dispose();

      (await BuildDomainAsync(DomainUpgradeMode.Validate)).Dispose();
    }

    private Domain BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      return Domain.BuildAsync(configuration);
    }
  }
}