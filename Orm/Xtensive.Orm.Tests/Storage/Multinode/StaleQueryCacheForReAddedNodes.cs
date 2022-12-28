// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.Multinode.QueryCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  public sealed class StaleQueryCacheForReAddedNodes : MultinodeTest
  {
    private const string DefaultSchema = WellKnownSchemas.Schema1;
    private const string Schema1 = WellKnownSchemas.Schema2;
    private const string Schema2 = WellKnownSchemas.Schema3;

    private readonly object SimpleQueryKey = new object();

    protected override void CheckRequirements() =>
      Require.AllFeaturesSupported(Orm.Providers.ProviderFeatures.Multischema);

    protected override DomainConfiguration BuildConfiguration()
    {
      CustomUpgradeHandler.TypeIdPerNode.Add(TestNodeId2, 100);
      CustomUpgradeHandler.TypeIdPerNode.Add(TestNodeId3, 100);

      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(BaseTestEntity).Assembly, typeof(BaseTestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = DefaultSchema;
      return configuration;
    }

    protected override void PopulateNodes()
    {
      CustomUpgradeHandler.CurrentNodeId = TestNodeId2;
      var nodeConfiguration = new NodeConfiguration(TestNodeId2);
      nodeConfiguration.SchemaMapping.Add(DefaultSchema, Schema1);
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      _ = Domain.StorageNodeManager.AddNode(nodeConfiguration);

      CustomUpgradeHandler.CurrentNodeId = TestNodeId3;
      nodeConfiguration = new NodeConfiguration(TestNodeId3);
      nodeConfiguration.SchemaMapping.Add(DefaultSchema, Schema2);
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      _ = Domain.StorageNodeManager.AddNode(nodeConfiguration);
    }

    protected override void PopulateData()
    {
      var nodes = new[] { WellKnown.DefaultNodeId, TestNodeId2, TestNodeId3 };

      foreach (var nodeId in nodes) {
        var selectedNode = Domain.StorageNodeManager.GetNode(nodeId);
        using (var session = selectedNode.OpenSession())
        using (var tx = session.OpenTransaction()) {

          var nodeIdName = string.IsNullOrEmpty(nodeId) ? "<default>" : nodeId;

          _ = new BaseTestEntity(session) { BaseName = "A", BaseOwnerNodeId = nodeIdName };
          _ = new MiddleTestEntity(session) {
            BaseName = "AA",
            MiddleName = "AAM",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName
          };
          _ = new LeafTestEntity(session) {
            BaseName = "AAA",
            MiddleName = "AAAM",
            LeafName = "AAAL",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName,
            LeafOwnerNodeId = nodeIdName
          };

          _ = new BaseTestEntity(session) { BaseName = "B", BaseOwnerNodeId = nodeIdName };
          _ = new MiddleTestEntity(session) {
            BaseName = "BB",
            MiddleName = "BBM",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName
          };
          _ = new LeafTestEntity(session) {
            BaseName = "BBB",
            MiddleName = "BBBM",
            LeafName = "BBBL",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName,
            LeafOwnerNodeId = nodeIdName
          };

          _ = new BaseTestEntity(session) { BaseName = "C", BaseOwnerNodeId = nodeIdName };
          _ = new MiddleTestEntity(session) {
            BaseName = "CC",
            MiddleName = "CCM",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName
          };
          _ = new LeafTestEntity(session) {
            BaseName = "CCC",
            MiddleName = "CCCM",
            LeafName = "CCCL",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName,
            LeafOwnerNodeId = nodeIdName
          };

          _ = new BaseTestEntity(session) { BaseName = "D", BaseOwnerNodeId = nodeIdName };
          _ = new MiddleTestEntity(session) {
            BaseName = "DD",
            MiddleName = "DDM",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName
          };
          _ = new LeafTestEntity(session) {
            BaseName = "DDD",
            MiddleName = "DDDM",
            LeafName = "DDDL",
            BaseOwnerNodeId = nodeIdName,
            MiddleOwnerNodeId = nodeIdName,
            LeafOwnerNodeId = nodeIdName
          };

          // puts one query per each node to the query cache
          _ = ExecuteSimpleQueryCaching(session);

          tx.Complete();
        }
      }
    }

    [Test]
    public void ReAddNodeTest()
    {
      var node = Domain.StorageNodeManager.GetNode(TestNodeId2);
      var queryCacheSize = Domain.QueryCache.Count;

      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = session.Query.Execute(SimpleQueryKey, q => q.All<BaseTestEntity>().Where(e => e.BaseName.Contains("B"))).ToList();
      }
      Assert.That(Domain.QueryCache.Count, Is.EqualTo(queryCacheSize));

      _ = Domain.StorageNodeManager.RemoveNode(TestNodeId2);
      Assert.That(Domain.QueryCache.Count, Is.EqualTo(queryCacheSize));

      CustomUpgradeHandler.CurrentNodeId = TestNodeId2;
      var nodeConfiguration = new NodeConfiguration(TestNodeId2);
      nodeConfiguration.SchemaMapping.Add(DefaultSchema, Schema1);
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Validate;
      _ = Domain.StorageNodeManager.AddNode(nodeConfiguration);

      node = Domain.StorageNodeManager.GetNode(TestNodeId2);

      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = session.Query.Execute(SimpleQueryKey, q => q.All<BaseTestEntity>().Where(e => e.BaseName.Contains("B"))).ToList();
      }
      Assert.That(Domain.QueryCache.Count, Is.EqualTo(queryCacheSize));

      CustomUpgradeHandler.CurrentNodeId = TestNodeId3;
      nodeConfiguration = new NodeConfiguration(TestNodeId3);
      nodeConfiguration.SchemaMapping.Add(DefaultSchema, Schema2);
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      _ = Domain.StorageNodeManager.AddNode(nodeConfiguration);
    }

    [Test]
    public void ReAddNodeWithAnotherSchemaMappingNoCacheCleanTest()
    {
      var node = Domain.StorageNodeManager.GetNode(TestNodeId2);
      var queryCacheSize = Domain.QueryCache.Count;

      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = session.Query.Execute(SimpleQueryKey, q => q.All<BaseTestEntity>().Where(e => e.BaseName.Contains("B"))).ToList();
      }
      Assert.That(Domain.QueryCache.Count, Is.EqualTo(queryCacheSize));

      _ = Domain.StorageNodeManager.RemoveNode(TestNodeId2);
      Assert.That(Domain.QueryCache.Count, Is.EqualTo(queryCacheSize));

      CustomUpgradeHandler.CurrentNodeId = TestNodeId2;
      var nodeConfiguration = new NodeConfiguration(TestNodeId2);
      nodeConfiguration.SchemaMapping.Add(DefaultSchema, Schema2);// uses schema of TestNodeId3
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Validate;
      _ = Domain.StorageNodeManager.AddNode(nodeConfiguration);

      node = Domain.StorageNodeManager.GetNode(TestNodeId2);

      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var results = session.Query.Execute(SimpleQueryKey, q => q.All<BaseTestEntity>().Where(e => e.BaseName.Contains("B"))).ToList();
        foreach(var item in results) {
          Assert.That(item.BaseOwnerNodeId, Is.EqualTo(node.Id)); // gets result from old schema
        }
      }
      Assert.That(Domain.QueryCache.Count, Is.EqualTo(queryCacheSize));
    }

    private List<BaseTestEntity> ExecuteSimpleQueryCaching(Session session) =>
     session.Query.Execute(SimpleQueryKey, q => q.All<BaseTestEntity>().Where(e => e.BaseName.Contains("B"))).ToList();
  }
}
