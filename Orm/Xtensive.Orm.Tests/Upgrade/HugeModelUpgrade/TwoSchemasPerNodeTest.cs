// Copyright (C) 2016-2020 Xtensive LLC.
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
      configuration.DefaultSchema = "dbo";

      var partOneType = typeof(TwoPartsModel.PartOne.TestEntityOne0);
      var partTwoType = typeof(TwoPartsModel.PartTwo.TestEntityTwo0);
      configuration.Types.Register(partOneType.Assembly, partOneType.Namespace);
      configuration.Types.Register(partTwoType.Assembly, partTwoType.Namespace);

      configuration.MappingRules
        .Map(partOneType.Assembly, partOneType.Namespace)
        .ToSchema("dbo");
      configuration.MappingRules
        .Map(partTwoType.Assembly, partTwoType.Namespace)
        .ToSchema("Model1");
      return configuration;
    }

    protected override void PopulateData(Domain domain)
    {
      var nodes = new[] {
        WellKnown.DefaultNodeId,
        "Node1", "Node2", "Node3", "Node4", "Node5",
      };

      foreach (var node in nodes) {
        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(node);
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
        "Node1", "Node2", "Node3", "Node4", "Node5",
      };

      foreach (var node in nodes) {
        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(node);
          using (var transaction = session.OpenTransaction()) {
            var checker = new ModelChecker();
            checker.Run(session);
          }
        }
      }
    }

    protected override IEnumerable<NodeConfiguration> GetAdditionalNodeConfigurations(DomainUpgradeMode upgradeMode)
    {
      var schemas = new[] {
        "Model2", "Model3",
        "Model4", "Model5",
        "Model6", "Model7",
        "Model8", "Model9",
        "Model10", "Model11",
      };

      for (int index = 0, nodeIndex = 1; index < 10; index += 2, nodeIndex++) {
        var node = new NodeConfiguration("Node" + nodeIndex);
        node.UpgradeMode = upgradeMode;
        node.SchemaMapping.Add("dbo", schemas[index]);
        node.SchemaMapping.Add("Model1", schemas[index + 1]);
        yield return node;
      }
    }
  }
}
