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
    [Test]
    [Explicit]
    public void SequentialBuildingTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(), false))
      {
        PopulateData(domain);
      }

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();
      using (var domain = BuildDomain(configuration, false))
      {
        var counters = domain.Extensions.Get<PerformanceResultContainer>();
        Console.WriteLine(counters.ToString());
        CheckIfQueriesWork(domain);
      }
    }

    protected void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      //configuration.DefaultDatabase = "DO-Tests";
      configuration.DefaultSchema = "dbo";
      configuration.Types.Register(typeof(HugeModelUpgrade.TwoPartsModel.PartOne.TestEntityOne0).Assembly, typeof(HugeModelUpgrade.TwoPartsModel.PartOne.TestEntityOne0).Namespace);
      configuration.Types.Register(typeof(HugeModelUpgrade.TwoPartsModel.PartTwo.TestEntityTwo0).Assembly, typeof(HugeModelUpgrade.TwoPartsModel.PartTwo.TestEntityTwo0).Namespace);

      configuration.MappingRules
        .Map(typeof(HugeModelUpgrade.TwoPartsModel.PartOne.TestEntityOne0).Assembly, typeof(HugeModelUpgrade.TwoPartsModel.PartOne.TestEntityOne0).Namespace)
        .ToSchema("dbo");
      configuration.MappingRules
        .Map(typeof(HugeModelUpgrade.TwoPartsModel.PartTwo.TestEntityTwo0).Assembly, typeof(HugeModelUpgrade.TwoPartsModel.PartTwo.TestEntityTwo0).Namespace)
        .ToSchema("Model1");
      configuration.Types.Register(typeof(UpgradePerformanceCounter));
      return configuration;
    }

    protected Domain BuildDomain(DomainConfiguration configuration, bool isParallel)
    {
      var domain = Domain.Build(configuration);
      var nodes = GetConfigurations(configuration.UpgradeMode);
      if (isParallel) {
        Action<object> action = nodeConfg => domain.StorageNodeManager.AddNode((NodeConfiguration) nodeConfg);
        var tasks = new List<Task>();
        foreach (var nodeConfiguration in nodes)
          tasks.Add(Task.Factory.StartNew(action, nodeConfiguration));
        Task.WaitAll(tasks.ToArray());
      }
      else {
        foreach (var nodeConfiguration in nodes)
          domain.StorageNodeManager.AddNode(nodeConfiguration);
      }

      return domain;
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

      for (int index = 0, nodeIndex = 1; index < 10; index += 2, nodeIndex++)
      {
        var node = new NodeConfiguration("Node" + nodeIndex);
        node.UpgradeMode = upgradeMode;
        node.SchemaMapping.Add("dbo", schemas[index]);
        node.SchemaMapping.Add("Model1", schemas[index + 1]);
        yield return node;
      }
    }
  }
}
