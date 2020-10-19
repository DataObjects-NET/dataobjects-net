using System;
using System.Collections.Generic;
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

    [OneTimeSetUp]
    public void OneTimeSetUp() => Require.ProviderIs(StorageProvider.SqlServer);

    [Test]
    public void NullNodeInitSql()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = "dbo";
      domainConfig.ConnectionInitializationSql = "USE [DO-Tests]";
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInitializationSql = null;
      nodeConfig.SchemaMapping.Add("dbo", "Model1");

      using (var domain = Domain.Build(domainConfig)) {
        var selectedNode = domain.SelectStorageNode(WellKnown.DefaultNodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo("DO-Tests"));
          }
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        selectedNode = domain.SelectStorageNode("Additional");
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo("DO-Tests"));
          }
        }
      }
    }

    [Test]
    public void EmptyStringIninSql()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = "dbo";
      domainConfig.ConnectionInitializationSql = "USE [DO-Tests]";
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInitializationSql = string.Empty;
      nodeConfig.SchemaMapping.Add("dbo", "Model1");

      using (var domain = Domain.Build(domainConfig)) {
        var selectedNode = domain.SelectStorageNode(WellKnown.DefaultNodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.EqualTo(domainConfig.ConnectionInfo));
          _ = new TestEntity(session);
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo("DO-Tests"));
          }
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        selectedNode = domain.SelectStorageNode("Additional");
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo("DO-Tests"));
          }
        }
      }
    }

    [Test]
    public void ValidInitSql()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = "dbo";
      domainConfig.ConnectionInitializationSql = "USE [DO-Tests]";
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInitializationSql = "USE [DO-Tests-1]";
      nodeConfig.SchemaMapping.Add("dbo", "dbo");

      using (var domain = Domain.Build(domainConfig)) {
        var selectedNode = domain.SelectStorageNode(WellKnown.DefaultNodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.EqualTo(domainConfig.ConnectionInfo));
          _ = new TestEntity(session);
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo("DO-Tests"));
          }
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        selectedNode = domain.SelectStorageNode("Additional");
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var service = session.Services.Get<DirectSqlAccessor>();
          using (var command = service.CreateCommand()) {
            command.CommandText = "SELECT DB_NAME() AS [Current Database];";
            var databaseName = command.ExecuteScalar();
            Assert.That(databaseName, Is.EqualTo("DO-Tests-1"));
          }
        }
      }
    }
  }
}
