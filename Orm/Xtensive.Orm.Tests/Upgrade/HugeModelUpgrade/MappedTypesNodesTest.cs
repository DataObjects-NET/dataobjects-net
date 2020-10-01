// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.10.19

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.ModelWithMappings;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  /// <summary>
  /// The test takes unnormal count of databases and time.
  /// Run it on local machine only!
  /// </summary>
  [Explicit]
  public sealed class MappedTypesNodesTest : HugeModelUpgradeTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultDatabase = "DO-Tests";
      configuration.DefaultSchema = "dbo";
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
        "Node1", "Node2", "Node3", "Node4", "Node5", "Node6",
        "Node7", "Node8", "Node9", "Node10", "Node11", "Node12",
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
      var databases = new[] {
        "DO-Tests-1", "DO-Tests-2", "DO-Tests-3", "DO-Tests-4", "DO-Tests-5", "DO-Tests-6",
        "DO-Tests-7", "DO-Tests-8", "DO-Tests-9", "DO-Tests-10", "DO-Tests-11", "DO-Tests-12",
      };

      var index = 0;
      foreach (var database in databases) {
        index++;
        var node = new NodeConfiguration("Node" + index);
        node.UpgradeMode = upgradeMode;
        node.DatabaseMapping.Add("DO-Tests", database);
        yield return node;
      }
    }
  }
}
