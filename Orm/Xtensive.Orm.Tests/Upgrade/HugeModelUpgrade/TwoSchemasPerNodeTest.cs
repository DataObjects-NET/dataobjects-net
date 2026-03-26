// Copyright (C) 2016-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.10.24

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.TwoPartsModel;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  /// <summary>
  /// The test takes unnormal count of databases and time.
  /// Run it on local machine only!
  /// </summary>
  [Explicit]
  public sealed class TwoSchemasPerNodeTest : HugeModelUpgradeTestBase
  {
    protected override bool SupportsParallel => false;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultSchema = WellKnownSchemas.SqlServerDefaultSchema;

      var partOneType = typeof(TwoPartsModel.PartOne.TestEntityOne0);
      var partTwoType = typeof(TwoPartsModel.PartTwo.TestEntityTwo0);
      configuration.Types.RegisterCaching(partOneType.Assembly, partOneType.Namespace);
      configuration.Types.RegisterCaching(partTwoType.Assembly, partTwoType.Namespace);

      configuration.MappingRules
        .Map(partOneType.Assembly, partOneType.Namespace)
        .ToSchema(WellKnownSchemas.SqlServerDefaultSchema);
      configuration.MappingRules
        .Map(partTwoType.Assembly, partTwoType.Namespace)
        .ToSchema(WellKnownSchemas.Schema1);
      return configuration;
    }

    protected override void PopulateData(Domain domain)
    {
      var nodes = new[] {
        WellKnown.DefaultNodeId,
        "Node1", "Node2", "Node3", "Node4", "Node5",
      };

      foreach (var node in nodes) {
        var selectedNode = domain.StorageNodeManager.GetNode(node);
        using (var session = selectedNode.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var populator = new ModelPopulator();
          populator.Run();
          transaction.Complete();
        }
      }
    }

    protected override void CheckIfQueriesWork(Domain domain)
    {
      var nodes = new[] {
        WellKnown.DefaultNodeId,
        "Node1", "Node2", "Node3", "Node4", "Node5",
      };

      foreach (var node in nodes) {
        var selectedNode = domain.StorageNodeManager.GetNode(node);
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var checker = new ModelChecker();
          checker.Run(session);
        }
      }
    }

    protected override IEnumerable<NodeConfiguration> GetAdditionalNodeConfigurations(DomainUpgradeMode upgradeMode)
    {
      var schemas = new[] {
        WellKnownSchemas.Schema2, WellKnownSchemas.Schema3,
        WellKnownSchemas.Schema4, WellKnownSchemas.Schema5,
        WellKnownSchemas.Schema6, WellKnownSchemas.Schema7,
        WellKnownSchemas.Schema8, WellKnownSchemas.Schema9,
        WellKnownSchemas.Schema10, WellKnownSchemas.Schema11,
      };

      for (int index = 0, nodeIndex = 1; index < 10; index += 2, nodeIndex++) {
        var node = new NodeConfiguration("Node" + nodeIndex);
        node.UpgradeMode = upgradeMode;
        node.SchemaMapping.Add(WellKnownSchemas.SqlServerDefaultSchema, schemas[index]);
        node.SchemaMapping.Add(WellKnownSchemas.Schema1, schemas[index + 1]);
        yield return node;
      }
    }
  }
}
