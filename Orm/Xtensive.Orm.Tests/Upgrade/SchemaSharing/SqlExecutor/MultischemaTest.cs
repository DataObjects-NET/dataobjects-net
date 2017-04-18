// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.28

using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.SqlExecutor.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.SqlExecutor
{
  public class MultischemaTest : SimpleTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = "Model1";
      configuration.MappingRules.Map(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (model.Part2.TestEntity2).Assembly, typeof (model.Part2.TestEntity2).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (model.Part3.TestEntity3).Assembly, typeof (model.Part3.TestEntity3).Namespace).ToSchema("Model3");
      configuration.MappingRules.Map(typeof (model.Part4.TestEntity4).Assembly, typeof (model.Part4.TestEntity4).Namespace).ToSchema("Model3");
      return configuration;
    }

    protected override List<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      var @default = new NodeConfiguration(WellKnown.DefaultNodeId) {UpgradeMode = upgradeMode};
      var additional = new NodeConfiguration("Additional") {UpgradeMode = upgradeMode};
      additional.SchemaMapping.Add("Model1", "Model2");
      additional.SchemaMapping.Add("Model3", "Model4");
      return new List<NodeConfiguration> {@default, additional};
    }
  }
}