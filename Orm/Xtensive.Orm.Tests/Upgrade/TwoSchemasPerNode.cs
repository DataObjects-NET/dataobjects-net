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
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.TwoPartsModel;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  [TestFixture]
  [Explicit]
  public class TwoSchemasPerNode
  {
    [OneTimeSetUp]
    public void TestFixtureSetup() => Require.ProviderIs(StorageProvider.SqlServer);

    [Test]
    [Explicit]
    public void SequentialBuildingTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(), false)) {
        PopulateData(domain);
      }

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();
      using (var domain = BuildDomain(configuration, false)) {
        var counters = domain.Extensions.Get<PerformanceResultContainer>();
        Console.WriteLine(counters.ToString());
        CheckIfQueriesWork(domain);
      }
    }

    [Test]
    [Explicit]
    public async Task SequentialBuildingAsyncTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(), false)) {
        PopulateData(domain);
      }

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();
      using (var domain = await BuildDomainAsync(configuration, false)) {
        var counters = domain.Extensions.Get<PerformanceResultContainer>();
        Console.WriteLine(counters.ToString());
        CheckIfQueriesWork(domain);
      }
    }

    private Domain BuildDomain(DomainConfiguration configuration, bool isParallel)
    {
      var domain = Domain.Build(configuration);
      var nodes = GetConfigurations(configuration.UpgradeMode);
      if (isParallel) {
        Action<object> action = nodeConfg => domain.StorageNodeManager.AddNode((NodeConfiguration) nodeConfg);
        var tasks = new List<Task>();
        foreach (var nodeConfiguration in nodes) {
          tasks.Add(Task.Factory.StartNew(action, nodeConfiguration));
        }
        Task.WaitAll(tasks.ToArray());
      }
      else {
        foreach (var nodeConfiguration in nodes) {
          _ = domain.StorageNodeManager.AddNode(nodeConfiguration);
        }
      }

      return domain;
    }

    private async Task<Domain> BuildDomainAsync(DomainConfiguration configuration, bool isParallel)
    {
      var domain = await Domain.BuildAsync(configuration);
      var nodes = GetConfigurations(configuration.UpgradeMode);
      if (isParallel) {
        var tasks = new List<Task>();
        foreach (var nodeConfiguration in nodes) {
          tasks.Add(domain.StorageNodeManager.AddNodeAsync((NodeConfiguration) nodeConfiguration));
        }
        await Task.WhenAll(tasks.ToArray());
      }
      else {
        foreach (var nodeConfiguration in nodes) {
          _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration);
        }
      }

      return domain;
    }

    private DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      //configuration.DefaultDatabase = "DO-Tests";
      configuration.DefaultSchema = "dbo";
      configuration.Types.Register(typeof(TwoPartsModel.PartOne.TestEntityOne0).Assembly, typeof(TwoPartsModel.PartOne.TestEntityOne0).Namespace);
      configuration.Types.Register(typeof(TwoPartsModel.PartTwo.TestEntityTwo0).Assembly, typeof(TwoPartsModel.PartTwo.TestEntityTwo0).Namespace);

      configuration.MappingRules
        .Map(typeof(TwoPartsModel.PartOne.TestEntityOne0).Assembly, typeof(TwoPartsModel.PartOne.TestEntityOne0).Namespace)
        .ToSchema("dbo");
      configuration.MappingRules
        .Map(typeof(TwoPartsModel.PartTwo.TestEntityTwo0).Assembly, typeof(TwoPartsModel.PartTwo.TestEntityTwo0).Namespace)
        .ToSchema("Model1");
      configuration.Types.Register(typeof(UpgradePerformanceCounter));
      return configuration;
    }

    private void PopulateData(Domain domain)
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

    private void CheckIfQueriesWork(Domain domain)
    {
      var nodes = new[] {
        WellKnown.DefaultNodeId,
        "Node1", "Node2", "Node3", "Node4", "Node5",
      };

      foreach (var node in nodes) {
        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(node);
          using (var transaction = session.OpenTransaction()) {
            var populator = new ModelChecker();
            populator.Run(session);
          }
        }
      }
    }

    private IEnumerable<NodeConfiguration> GetConfigurations(DomainUpgradeMode upgradeMode)
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
