// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.30

using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.QueryBuilder
{
  public class MultischemaTest : SimpleTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = "Model1";
      configuration.MappingRules.Map(typeof (Model.Part1.TestEntity1).Assembly, typeof (Model.Part1.TestEntity1).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (Model.Part2.TestEntity2).Assembly, typeof (Model.Part2.TestEntity2).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (Model.Part3.TestEntity3).Assembly, typeof (Model.Part3.TestEntity3).Namespace).ToSchema("Model3");
      configuration.MappingRules.Map(typeof (Model.Part4.TestEntity4).Assembly, typeof (Model.Part4.TestEntity4).Namespace).ToSchema("Model3");
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