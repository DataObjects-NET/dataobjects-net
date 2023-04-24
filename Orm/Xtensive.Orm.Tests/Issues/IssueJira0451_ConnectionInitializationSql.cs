// Copyright (C) 2013-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.07.02

using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0451_ConnectionInitializationSqlModel;
using Xtensive.Orm.Tests.Storage.Multimapping;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0451_ConnectionInitializationSqlModel
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0451_ConnectionInitializationSql
  {
    [Mute]
    [Test]
    public void MainTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "uses native SQL");
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);

      const string message = "Hello custom initialized world!";
      const string db1 = MultidatabaseTest.Database1Name;
      const string db2 = MultidatabaseTest.Database2Name;

      var configuration = BuildConfiguration(db1, DomainUpgradeMode.Recreate);
      configuration.ConnectionInitializationSql = string.Format("use [{0}]", db2);

      long entityId;

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = new TestEntity {Value = message};
        entityId = entity.Id;
        tx.Complete();
      }

      configuration = BuildConfiguration(db2, DomainUpgradeMode.Validate);
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = session.Query.Single<TestEntity>(entityId);
        Assert.That(entity.Value, Is.EqualTo(message));
        tx.Complete();
      }
    }

    private static DomainConfiguration BuildConfiguration(string database, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.CreateForConnectionStringTest();
      configuration.ConnectionInfo = OverrideDatabase(configuration.ConnectionInfo, database);
      configuration.Types.Register(typeof (TestEntity));
      configuration.UpgradeMode = upgradeMode;
      return configuration;
    }

    private static ConnectionInfo OverrideDatabase(ConnectionInfo connectionInfo, string database)
    {
      var builder = new SqlConnectionStringBuilder(connectionInfo.ConnectionString) {
        InitialCatalog = database
      };
      return new ConnectionInfo(WellKnown.Provider.SqlServer, builder.ToString());
    }
  }
}