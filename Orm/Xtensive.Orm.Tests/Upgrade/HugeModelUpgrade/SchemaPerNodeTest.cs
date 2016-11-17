// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.10.19

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.RegularModel;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  [TestFixture]
  [Explicit]
  public class SchemaPerNodeTest
  {
    [Test]
    [Explicit]
    public void SequentialBuildingTest()
    {
      using (var domain = BuildDomain(BuildConfiguration())) {
        PopulateData(domain);
      }

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();
      using (var domain = BuildDomain(configuration)) {
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
      configuration.DefaultSchema = "dbo";
      configuration.Types.Register(typeof (TestEntity0).Assembly, typeof (TestEntity0).Namespace);
      configuration.Types.Register(typeof (UpgradePerformanceCounter));
      return configuration;
    }

    protected Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = Domain.Build(configuration);
      var nodes = GetConfigurations(configuration.UpgradeMode);
      foreach (var nodeConfiguration in nodes)
        domain.StorageNodeManager.AddNode(nodeConfiguration);
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
      var schemas = new[] {
        "Model1", "Model2", "Model3", "Model4", "Model5", "Model6",
        "Model7", "Model8", "Model9", "Model10", "Model11", "Model12",
      };

      var index = 0;
      foreach (var schema in schemas) {
        index++;
        var node = new NodeConfiguration("Node" + index);
        node.UpgradeMode = upgradeMode;
        node.SchemaMapping.Add("dbo", schema);
        yield return node;
      }
    }
  }
}