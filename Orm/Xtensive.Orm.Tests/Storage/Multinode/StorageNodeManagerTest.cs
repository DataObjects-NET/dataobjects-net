// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  public class StorageNodeManagerTest : AutoBuildTest
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    private const string DOTestsDb = WellKnownDatabases.MultiDatabase.MainDb;
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;

    private const string dbo = WellKnownSchemas.SqlServerDefaultSchema;
    private const string Schema1 = WellKnownSchemas.Schema1;

    private readonly List<string> NodesToClear = new List<string>();

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer);

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(TestEntity));
      config.DefaultSchema = dbo;
      return config;
    }

    [SetUp]
    public void SetUp()
    {
      NodesToClear.Clear();
    }

    [TearDown]
    public void TearDown()
    {
      foreach (var nodeName in NodesToClear) {
        _ = Domain.StorageNodeManager.RemoveNode(nodeName);
      }
    }

    [Test]
    public void AddUniqueNodeTest()
    {
      var nodeConfiguration = GetBaseNodeConfig();
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      var result = Domain.StorageNodeManager.AddNode(nodeConfiguration);
      Assert.That(result, Is.True);
    }

    [Test]
    public void AddAlreadyExistingTest()
    {
      var nodeConfiguration = GetBaseNodeConfig();
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      _ = Domain.StorageNodeManager.AddNode(nodeConfiguration);

      var sameConfig = (NodeConfiguration)nodeConfiguration.Clone();
      var result = Domain.StorageNodeManager.AddNode(sameConfig);
      Assert.That(result, Is.False);
    }

    [Test]
    public void AddNodeWithNullNameTest()
    {
      var nodeConfiguration = new NodeConfiguration() { UpgradeMode = DomainUpgradeMode.Recreate };
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      _ = Assert.Throws<InvalidOperationException>(() => Domain.StorageNodeManager.AddNode(nodeConfiguration));
    }

    [Test]
    public void AddNodeWithEmptyNameTest()
    {
      var nodeConfiguration = new NodeConfiguration(string.Empty) { UpgradeMode = DomainUpgradeMode.Recreate };
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      _ = Assert.Throws<InvalidOperationException>(() => Domain.StorageNodeManager.AddNode(nodeConfiguration));
    }

    [Test]
    public void AddNodeWithNullConfigTest()
    {
      _ = Assert.Throws<ArgumentNullException>(() => Domain.StorageNodeManager.AddNode(null));
    }

    [Test]
    public void AddNodeForMultidatabaseDomainTest()
    {
      var nodeConfiguration = new NodeConfiguration() { UpgradeMode = DomainUpgradeMode.Recreate };
      nodeConfiguration.DatabaseMapping.Add(DOTestsDb, DOTests1Db);
      _ = Assert.Throws<InvalidOperationException>(() => Domain.StorageNodeManager.AddNode(nodeConfiguration));
    }

    [Test]
    public void GetNodeByNullStringTest()
    {
      _ = Assert.Throws<ArgumentNullException>(() => Domain.StorageNodeManager.GetNode(null));
    }

    [Test]
    public void GetNodeByEmptyStringTest()
    {
      var nodeConfiguration = GetBaseNodeConfig();
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      var result = Domain.StorageNodeManager.AddNode(nodeConfiguration);
      Assert.That(result, Is.True);

      var node = Domain.StorageNodeManager.GetNode(string.Empty);

      Assert.That(node, Is.Not.Null);
      Assert.That(node.Id, Is.Not.EqualTo(nodeConfiguration.NodeId));
    }

    [Test]
    public void GetNodeByExistingNodeNameTest()
    {
      var nodeConfiguration = GetBaseNodeConfig();
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      var result = Domain.StorageNodeManager.AddNode(nodeConfiguration);
      Assert.That(result, Is.True);

      var node = Domain.StorageNodeManager.GetNode(nodeConfiguration.NodeId);

      Assert.That(node, Is.Not.Null);
      Assert.That(node.Id, Is.EqualTo(nodeConfiguration.NodeId));
    }

    [Test]
    public void GetNodeWhichDoesntExistTest()
    {
      var nodeConfiguration = GetBaseNodeConfig();
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      var result = Domain.StorageNodeManager.AddNode(nodeConfiguration);
      Assert.That(result, Is.True);

      var node = Domain.StorageNodeManager.GetNode(nodeConfiguration.NodeId + "dummy");

      Assert.That(node, Is.Null);
    }

    [Test]
    public void RemoveNodeWhichDoesntExistTest()
    {
      var result = Domain.StorageNodeManager.RemoveNode("dummy");
      Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveExistingNode()
    {
      var nodeConfiguration = GetBaseNodeConfig();
      nodeConfiguration.SchemaMapping.Add(dbo, Schema1);
      var result = Domain.StorageNodeManager.AddNode(nodeConfiguration);
      Assert.That(result, Is.True);

      result = Domain.StorageNodeManager.RemoveNode(nodeConfiguration.NodeId);
      Assert.That(result, Is.True);
    }

    private NodeConfiguration GetBaseNodeConfig()
    {
      var nodeConfiguration = new NodeConfiguration(Guid.NewGuid().ToString()) {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      NodesToClear.Add(nodeConfiguration.NodeId);
      return nodeConfiguration;
    }
  }
}
