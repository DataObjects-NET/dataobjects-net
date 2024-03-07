// Copyright (C) 2016-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.10.19

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.TwoPartsModel;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  /// <summary>
  /// The test takes unnormal count of databases and time.
  /// Run it on local machine only!
  /// </summary>
  [Explicit]
  public sealed class TwoDatabasesPerNodeTest : HugeModelUpgradeTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultDatabase = WellKnownDatabases.MultiDatabase.MainDb;
      configuration.DefaultSchema = WellKnownSchemas.SqlServerDefaultSchema;

      var partOneType = typeof(TwoPartsModel.PartOne.TestEntityOne0);
      var partTwoType = typeof(TwoPartsModel.PartTwo.TestEntityTwo0);
      configuration.Types.Register(partOneType.Assembly, partOneType.Namespace);
      configuration.Types.Register(partTwoType.Assembly, partTwoType.Namespace);

      configuration.MappingRules
        .Map(partOneType.Assembly, partOneType.Namespace)
        .ToDatabase(WellKnownDatabases.MultiDatabase.MainDb);
      configuration.MappingRules
        .Map(partTwoType.Assembly, partTwoType.Namespace)
        .ToDatabase(WellKnownDatabases.MultiDatabase.AdditionalDb1);
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
        using (var session = selectedNode.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var checker = new ModelChecker();
          checker.Run(session);
        }
      }
    }

    protected override IEnumerable<NodeConfiguration> GetAdditionalNodeConfigurations(DomainUpgradeMode upgradeMode)
    {
      var databases = new[] {
        WellKnownDatabases.MultiDatabase.AdditionalDb2, WellKnownDatabases.MultiDatabase.AdditionalDb3,
        WellKnownDatabases.MultiDatabase.AdditionalDb4, WellKnownDatabases.MultiDatabase.AdditionalDb5,
        WellKnownDatabases.MultiDatabase.AdditionalDb6, WellKnownDatabases.MultiDatabase.AdditionalDb7,
        WellKnownDatabases.MultiDatabase.AdditionalDb8, WellKnownDatabases.MultiDatabase.AdditionalDb9,
        WellKnownDatabases.MultiDatabase.AdditionalDb10, WellKnownDatabases.MultiDatabase.AdditionalDb11,
      };

      for (int index = 0, nodeIndex=1 ; index < 10; index += 2, nodeIndex++) {
        var node = new NodeConfiguration("Node" + nodeIndex) {
          UpgradeMode = upgradeMode
        };
        node.DatabaseMapping.Add(WellKnownDatabases.MultiDatabase.MainDb, databases[index]);
        node.DatabaseMapping.Add(WellKnownDatabases.MultiDatabase.AdditionalDb1, databases[index+1]);
        yield return node;
      }
    }
  }
}
