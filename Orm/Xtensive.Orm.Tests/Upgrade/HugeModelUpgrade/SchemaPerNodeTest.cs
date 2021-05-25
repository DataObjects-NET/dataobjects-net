// Copyright (C) 2016-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.10.19

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.RegularModel;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  /// <summary>
  /// The test takes unnormal count of databases and time.
  /// Run it on local machine only!
  /// </summary>
  [Explicit]
  public sealed class SchemaPerNodeTest : HugeModelUpgradeTestBase
  {
    protected override bool SupportsParallel => false;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = WellKnownSchemas.SqlServerDefaultSchema;
      configuration.Types.Register(typeof(TestEntity0).Assembly, typeof(TestEntity0).Namespace);
      return configuration;
    }

    protected override void PopulateData(Domain domain)
    {
      var nodes = new[] {
        WellKnown.DefaultNodeId,
        "Node1", "Node2", "Node3", "Node4", "Node5", "Node6",
        "Node7", "Node8", "Node9", "Node10", "Node11", "Node12",
      };

      foreach (var node in nodes) {
        var selectedNode = domain.StorageNodeManager.GetNode(node);
        using (var session = selectedNode.OpenSession()) {
          using (var transaction = session.OpenTransaction()) {
            var populator = new ModelPopulator();
            populator.Run();
            transaction.Complete();
          }
        }
      }
    }

    protected override void CheckIfQueriesWork(Domain domain)
    {
      var nodes = new[] {
        WellKnown.DefaultNodeId,
        "Node1", "Node2", "Node3", "Node4", "Node5", "Node6",
        "Node7", "Node8", "Node9", "Node10", "Node11", "Node12",
      };

      foreach (var node in nodes) {
        var selectedNode = domain.StorageNodeManager.GetNode(node);
        using (var session = selectedNode.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var checker = new ModelChecker();
          checker.Run(session);
        }
      }
    }

    protected override IEnumerable<NodeConfiguration> GetAdditionalNodeConfigurations(DomainUpgradeMode upgradeMode)
    {
      var schemas = new[] {
        WellKnownSchemas.Schema1, WellKnownSchemas.Schema2, WellKnownSchemas.Schema3,
        WellKnownSchemas.Schema4, WellKnownSchemas.Schema5, WellKnownSchemas.Schema6,
        WellKnownSchemas.Schema7, WellKnownSchemas.Schema8, WellKnownSchemas.Schema9,
        WellKnownSchemas.Schema10, WellKnownSchemas.Schema11, WellKnownSchemas.Schema12,
      };

      var index = 0;
      foreach (var schema in schemas) {
        index++;
        var node = new NodeConfiguration("Node" + index) {
          UpgradeMode = upgradeMode
        };
        node.SchemaMapping.Add(WellKnownSchemas.SqlServerDefaultSchema, schema);
        yield return node;
      }
    }
  }
}
