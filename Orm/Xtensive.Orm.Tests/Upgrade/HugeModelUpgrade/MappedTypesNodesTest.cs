// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
  [TestFixture]
  [Explicit]
  public class MappedTypesNodesTest
  {
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
      using (var domain = BuildDomain(configuration, false))
      {
        var counters = domain.Extensions.Get<PerformanceResultContainer>();
        Console.WriteLine(counters.ToString());
        CheckIfQueriesWork(domain);
      }
    }

    [Test]
    [Explicit]
    public void ParallelBuildingTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(), false)) {
        PopulateData(domain);
      }

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();
      using (var domain = BuildDomain(configuration, true)) {
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
      configuration.DefaultDatabase = "DO-Tests";
      configuration.DefaultSchema = "dbo";
      configuration.Types.Register(typeof (TestEntity0).Assembly, typeof (TestEntity0).Namespace);
      configuration.Types.Register(typeof(UpgradePerformanceCounter));
      return configuration;
    }

    protected Domain BuildDomain(DomainConfiguration configuration, bool isParallel)
    {
      var domain = Domain.Build(configuration);
      var nodes = GetConfigurations(configuration.UpgradeMode);
      if (isParallel) {
        Action<object> action = nodeConfg => domain.StorageNodeManager.AddNode((NodeConfiguration)nodeConfg);
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

    private void CheckIfQueriesWork(Domain domain)
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
            var populator = new ModelChecker();
            populator.Run(session);
          }
        }
      }
    }

    private IEnumerable<NodeConfiguration> GetConfigurations(DomainUpgradeMode upgradeMode)
    {
      var databases = new[] {
        "DO-Tests-1",
        "DO-Tests-2",
        "DO-Tests-3",
        "DO-Tests-4",
        "DO-Tests-5",
        "DO-Tests-6",
        "DO-Tests-7",
        "DO-Tests-8",
        "DO-Tests-9",
        "DO-Tests-10",
        "DO-Tests-11",
        "DO-Tests-12",
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
