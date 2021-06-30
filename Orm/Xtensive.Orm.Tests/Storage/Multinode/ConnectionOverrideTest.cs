// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
  public class ConnectionOverrideTest
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

    private const string dbo = WellKnownSchemas.SqlServerDefaultSchema;
    private const string Schema1 = WellKnownSchemas.Schema1;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Require.ProviderIs(StorageProvider.SqlServer);

    [Test]
    public void NullNodeConnectionTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = dbo;
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInfo = null;
      nodeConfig.SchemaMapping.Add(dbo, Schema1);

      void commandValidator(object sender, DbCommandEventArgs args)
      {
        var session = ((SessionEventAccessor) sender).Session;
        if (session.StorageNodeId == WellKnown.DefaultNodeId) {
          Assert.That(args.Command.CommandText.Contains($"[{dbo}].[ConnectionOverrideTest.TestEntity]"), Is.True);
        }
        else {
          Assert.That(args.Command.CommandText.Contains($"[{Schema1}].[ConnectionOverrideTest.TestEntity]"), Is.True);
        }
      }

      using (var domain = Domain.Build(domainConfig)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.EqualTo(domainConfig.ConnectionInfo));
          _ = new TestEntity(session);
          session.Events.DbCommandExecuted += commandValidator;
          session.SaveChanges();
          session.Events.DbCommandExecuted -= commandValidator;
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        var selectedNode = domain.StorageNodeManager.GetNode(nodeConfig.NodeId);
        using (var session = selectedNode.OpenSession())
          using (var tx = session.OpenTransaction()){
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.EqualTo(domainConfig.ConnectionInfo));

          _ = new TestEntity(session);
          session.Events.DbCommandExecuted += commandValidator;
          session.SaveChanges();
          session.Events.DbCommandExecuted -= commandValidator;
        }
      }
    }

    [Test]
    public void OverwrittenConnectionTest()
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestEntity));
      domainConfig.DefaultSchema = dbo;
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var domainConnectionUrlString = domainConfig.ConnectionInfo.ConnectionUrl.ToString();
      var parametersPosition = domainConnectionUrlString.IndexOf('?');

      var nodeConfig = new NodeConfiguration("Additional");
      nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      nodeConfig.ConnectionInfo = new ConnectionInfo(UrlInfo.Parse(domainConnectionUrlString.Substring(0, parametersPosition)));
      nodeConfig.SchemaMapping.Add(dbo, Schema1);

      void commandValidator(object sender, DbCommandEventArgs args)
      {
        var session = ((SessionEventAccessor) sender).Session;
        if (session.StorageNodeId == WellKnown.DefaultNodeId) {
          Assert.That(args.Command.CommandText.Contains($"[{dbo}].[ConnectionOverrideTest.TestEntity]"), Is.True);
        }
        else {
          Assert.That(args.Command.CommandText.Contains($"[{Schema1}].[ConnectionOverrideTest.TestEntity]"), Is.True);
        }
      }

      using (var domain = Domain.Build(domainConfig)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.EqualTo(domainConfig.ConnectionInfo));
          _ = new TestEntity(session);
          session.Events.DbCommandExecuted += commandValidator;
          session.SaveChanges();
          session.Events.DbCommandExecuted -= commandValidator;
        }

        _ = domain.StorageNodeManager.AddNode(nodeConfig);
        var selectedNode = domain.StorageNodeManager.GetNode(nodeConfig.NodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var connection = ((SqlSessionHandler) session.Handler).Connection;
          Assert.That(connection.ConnectionInfo, Is.Not.EqualTo(domainConfig.ConnectionInfo));

          _ = new TestEntity(session);
          session.Events.DbCommandExecuted += commandValidator;
          session.SaveChanges();
          session.Events.DbCommandExecuted -= commandValidator;
        }
      }
    }
  }
}
