// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  [TestFixture]
  public sealed class InitializationSqlOverrideTest
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      public TestEntity(Session session)
        : base(session)
      {
      }
    }

    private const string DOTestsDb = WellKnownDatabases.MultiDatabase.MainDb;
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;

    private const string dbo = WellKnownSchemas.SqlServerDefaultSchema;
    private const string Schema1 = WellKnownSchemas.Schema1;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Require.ProviderIs(StorageProvider.SqlServer);

    [Test]
    public void NullNodeInitSql()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = dbo;
      domainConfig.ConnectionInitializationSql = $"USE [{DOTestsDb}]";
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInitializationSql = null;
      nodeConfig.SchemaMapping.Add(dbo, Schema1);

      using (var domain = Domain.Build(domainConfig)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo(DOTestsDb));
          }
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        var selectedNode = domain.StorageNodeManager.GetNode(nodeConfig.NodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo(DOTestsDb));
          }
        }
      }
    }

    [Test]
    public void EmptyStringIninSql()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = dbo;
      domainConfig.ConnectionInitializationSql = $"USE [{DOTestsDb}]";
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInitializationSql = string.Empty;
      nodeConfig.SchemaMapping.Add(dbo, Schema1);

      using (var domain = Domain.Build(domainConfig)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.EqualTo(domainConfig.ConnectionInfo));
          _ = new TestEntity(session);
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo(DOTestsDb));
          }
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        var selectedNode = domain.StorageNodeManager.GetNode(nodeConfig.NodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo(DOTestsDb));
          }
        }
      }
    }

    [Test]
    public void ValidInitSql()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = dbo;
      domainConfig.ConnectionInitializationSql = $"USE [{DOTestsDb}]";
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInitializationSql = $"USE [{DOTests1Db}]";
      nodeConfig.SchemaMapping.Add(dbo, dbo);

      using (var domain = Domain.Build(domainConfig)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.EqualTo(domainConfig.ConnectionInfo));
          _ = new TestEntity(session);
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo(DOTestsDb));
          }
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        var selectedNode = domain.StorageNodeManager.GetNode(nodeConfig.NodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo(DOTests1Db));
          }
        }
      }
    }
  }
}
