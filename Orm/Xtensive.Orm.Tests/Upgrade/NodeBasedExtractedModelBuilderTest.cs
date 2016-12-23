// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.13

using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using part1 = Xtensive.Orm.Tests.Upgrade.NodeBasedExtractedModelBuilderTestModel.Part1;
using part2 = Xtensive.Orm.Tests.Upgrade.NodeBasedExtractedModelBuilderTestModel.Part2;

namespace Xtensive.Orm.Tests.Upgrade.NodeBasedExtractedModelBuilderTestModel
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Text { get; set; }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Text { get; set; }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  public class NodeBasedExtractedModelBuilderTest
  {
    [Test]
    public void MultischemaWithDatabaseSwitchingTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var masterConnection = BuildConnectionToMaster(DomainConfigurationFactory.Create().ConnectionInfo);
      var configuration = new DomainConfiguration(masterConnection);
      configuration.Types.Register(typeof (part1.TestEntity1));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = "dbo";
      configuration.ConnectionInitializationSql = "USE [DO-Tests-1]";
      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.ConnectionInfo = masterConnection;
        domainCopyNode.ConnectionInitializationSql = "USE [DO-Tests-1]";
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.ConnectionInfo = masterConnection;
        anotherDatabaseNode.ConnectionInitializationSql = "USE [DO-Tests-2]";
        anotherDatabaseNode.SchemaMapping.Add("dbo", "dbo");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);

        var thirdDatabaseNode = new NodeConfiguration("3");
        thirdDatabaseNode.ConnectionInfo = masterConnection;
        thirdDatabaseNode.ConnectionInitializationSql = "USE [DO-Tests-3]";
        thirdDatabaseNode.SchemaMapping.Add("dbo", "Model1");
        thirdDatabaseNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(thirdDatabaseNode);
      }

      configuration = new DomainConfiguration(masterConnection);
      configuration.Types.Register(typeof (part1.TestEntity1));
      configuration.DefaultSchema = "dbo";
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.ConnectionInitializationSql = "USE [DO-Tests-1]";
      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.ConnectionInfo = masterConnection;
        domainCopyNode.ConnectionInitializationSql = "USE [DO-Tests-1]";
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.ConnectionInfo = masterConnection;
        anotherDatabaseNode.ConnectionInitializationSql = "USE [DO-Tests-2]";
        anotherDatabaseNode.SchemaMapping.Add("dbo", "dbo");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);

        var thirdDatabaseNode = new NodeConfiguration("3");
        thirdDatabaseNode.ConnectionInfo = masterConnection;
        thirdDatabaseNode.ConnectionInitializationSql = "USE [DO-Tests-3]";
        thirdDatabaseNode.SchemaMapping.Add("dbo", "Model1");
        thirdDatabaseNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(thirdDatabaseNode);

        var testEntity1Type = domain.Model.Types[typeof(part1.TestEntity1)];
        var defaultNode = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        var table = defaultNode.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));

        var node1 = domain.StorageNodeManager.GetNode("1");
        table = node1.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));

        var node2 = domain.StorageNodeManager.GetNode("2");
        table = node2.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node3 = domain.StorageNodeManager.GetNode("3");
        table = node3.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("Model1"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-3"));
      }
    }

    [Test]
    public void MultischemaWithoutDatabaseSwitchingTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(part1.TestEntity1));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = "dbo";

      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.SchemaMapping.Add("dbo", "Model1");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(part1.TestEntity1));
      configuration.DefaultSchema = "dbo";
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.SchemaMapping.Add("dbo", "Model1");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);

        var testEntity1Type = domain.Model.Types[typeof (part1.TestEntity1)];
        var defaultNode = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        var table = defaultNode.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests"));

        var node1 = domain.StorageNodeManager.GetNode("1");
        table = node1.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests"));

        var node2 = domain.StorageNodeManager.GetNode("2");
        table = node2.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("Model1"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests"));
      }
    }

    [Test]
    public void MultidatabaseNodesToOneDatabaseSetTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var masterConnectionInfo = BuildConnectionToMaster(DomainConfigurationFactory.Create().ConnectionInfo);
      var configuration = new DomainConfiguration(masterConnectionInfo);
      configuration.Types.Register(typeof (part1.TestEntity1));
      configuration.Types.Register(typeof (part2.TestEntity2));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = "dbo";
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.MappingRules.Map(typeof (part1.TestEntity1).Namespace).ToDatabase("DO-Tests-1");
      configuration.MappingRules.Map(typeof (part2.TestEntity2).Namespace).ToDatabase("DO-Tests-2");

      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.ConnectionInfo = masterConnectionInfo;
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.ConnectionInfo = masterConnectionInfo;
        anotherDatabaseNode.SchemaMapping.Add("dbo", "dbo");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);

        var testEntity1Type = domain.Model.Types[typeof(part1.TestEntity1)];
        var testEntity2Type = domain.Model.Types[typeof(part2.TestEntity2)];

        var defaultNode = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        var table = defaultNode.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = defaultNode.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node1 = domain.StorageNodeManager.GetNode("1");
        table = node1.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = node1.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node2 = domain.StorageNodeManager.GetNode("2");
        table = node2.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = node2.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));
      }

      configuration = new DomainConfiguration(masterConnectionInfo);
      configuration.Types.Register(typeof(part1.TestEntity1));
      configuration.Types.Register(typeof(part2.TestEntity2));
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.DefaultSchema = "dbo";
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.MappingRules.Map(typeof(part1.TestEntity1).Namespace).ToDatabase("DO-Tests-1");
      configuration.MappingRules.Map(typeof(part2.TestEntity2).Namespace).ToDatabase("DO-Tests-2");

      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.ConnectionInfo = masterConnectionInfo;
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.ConnectionInfo = masterConnectionInfo;
        anotherDatabaseNode.SchemaMapping.Add("dbo", "dbo");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);

        var testEntity1Type = domain.Model.Types[typeof(part1.TestEntity1)];
        var testEntity2Type = domain.Model.Types[typeof(part2.TestEntity2)];

        var defaultNode = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        var table = defaultNode.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = defaultNode.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node1 = domain.StorageNodeManager.GetNode("1");
        table = node1.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = node1.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node2 = domain.StorageNodeManager.GetNode("2");
        table = node2.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = node2.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));
      }
    }

    [Test]
    public void MultidatabaseNodesToDifferentDatabaseSetTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var masterConnectionInfo = BuildConnectionToMaster(DomainConfigurationFactory.Create().ConnectionInfo);
      var configuration = new DomainConfiguration(masterConnectionInfo);
      configuration.Types.Register(typeof(part1.TestEntity1));
      configuration.Types.Register(typeof(part2.TestEntity2));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = "dbo";
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.MappingRules.Map(typeof(part1.TestEntity1).Namespace).ToDatabase("DO-Tests-1");
      configuration.MappingRules.Map(typeof(part2.TestEntity2).Namespace).ToDatabase("DO-Tests-2");

      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.ConnectionInfo = masterConnectionInfo;
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-1");
        domainCopyNode.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-2");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.ConnectionInfo = masterConnectionInfo;
        anotherDatabaseNode.SchemaMapping.Add("dbo", "dbo");
        anotherDatabaseNode.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-3");
        anotherDatabaseNode.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-4");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Recreate;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);

        var testEntity1Type = domain.Model.Types[typeof(part1.TestEntity1)];
        var testEntity2Type = domain.Model.Types[typeof(part2.TestEntity2)];

        var defaultNode = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        var table = defaultNode.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = defaultNode.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node1 = domain.StorageNodeManager.GetNode("1");
        table = node1.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = node1.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node2 = domain.StorageNodeManager.GetNode("2");
        table = node2.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-3"));
        table = node2.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-4"));
      }

      configuration = new DomainConfiguration(masterConnectionInfo);
      configuration.Types.Register(typeof(part1.TestEntity1));
      configuration.Types.Register(typeof(part2.TestEntity2));
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.DefaultSchema = "dbo";
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.MappingRules.Map(typeof(part1.TestEntity1).Namespace).ToDatabase("DO-Tests-1");
      configuration.MappingRules.Map(typeof(part2.TestEntity2).Namespace).ToDatabase("DO-Tests-2");

      using (var domain = Domain.Build(configuration)) {
        var domainCopyNode = new NodeConfiguration("1");
        domainCopyNode.ConnectionInfo = masterConnectionInfo;
        domainCopyNode.SchemaMapping.Add("dbo", "dbo");
        domainCopyNode.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-1");
        domainCopyNode.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-2");
        domainCopyNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(domainCopyNode);

        var anotherDatabaseNode = new NodeConfiguration("2");
        anotherDatabaseNode.ConnectionInfo = masterConnectionInfo;
        anotherDatabaseNode.SchemaMapping.Add("dbo", "dbo");
        anotherDatabaseNode.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-3");
        anotherDatabaseNode.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-4");
        anotherDatabaseNode.UpgradeMode = DomainUpgradeMode.Skip;
        domain.StorageNodeManager.AddNode(anotherDatabaseNode);

        var testEntity1Type = domain.Model.Types[typeof(part1.TestEntity1)];
        var testEntity2Type = domain.Model.Types[typeof(part2.TestEntity2)];

        var defaultNode = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        var table = defaultNode.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = defaultNode.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node1 = domain.StorageNodeManager.GetNode("1");
        table = node1.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-1"));
        table = node1.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-2"));

        var node2 = domain.StorageNodeManager.GetNode("2");
        table = node2.Mapping[testEntity1Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-3"));
        table = node2.Mapping[testEntity2Type];
        Assert.That(table.Schema.Name, Is.EqualTo("dbo"));
        Assert.That(table.Schema.Catalog.Name, Is.EqualTo("DO-Tests-4"));
      }
    }

    private ConnectionInfo BuildConnectionToMaster(ConnectionInfo connectionInfo)
    {
      var connectionStringTemplate = "{0}://{1}{2}/{3}{4}";
      var loginInfoTemplate = "{0}:{1}@";
      var hostTemplate = "{0}:{1}";
      var parameterTemplate = "{0}={1}";

      var protocol = connectionInfo.ConnectionUrl.Protocol;
      var loginInfo = (!connectionInfo.ConnectionUrl.User.IsNullOrEmpty()) 
        ? string.Format(loginInfoTemplate, connectionInfo.ConnectionUrl.User, connectionInfo.ConnectionUrl.Password)
        : string.Empty;
      var server = (connectionInfo.ConnectionUrl.Port > 0)
        ? string.Format(hostTemplate, connectionInfo.ConnectionUrl.Host, connectionInfo.ConnectionUrl.Port)
        : connectionInfo.ConnectionUrl.Host;
      var database = "master";

      var parameters = string.Empty;
      if (connectionInfo.ConnectionUrl.Params.Count > 0) {
        var stringBuilder = new StringBuilder("?");
        foreach (var parameter in connectionInfo.ConnectionUrl.Params) {
          stringBuilder.Append(string.Format(parameterTemplate, parameter.Key, parameter.Value));
        }
      }
      return new ConnectionInfo(string.Format(connectionStringTemplate, protocol, loginInfo, server, database, parameters));
    }
  }
}
