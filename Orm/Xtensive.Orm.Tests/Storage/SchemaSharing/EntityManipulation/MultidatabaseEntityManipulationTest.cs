// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.04.05

using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation
{
  public class MultidatabaseEntityManipulationTest : SimpleEntityManipulationTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override void ApplyCustomSettingsToInitialConfiguration(DomainConfiguration configuration)
    {
      base.ApplyCustomSettingsToInitialConfiguration(configuration);
      configuration.DefaultSchema = "Model1";
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.MappingRules.Map(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace).To("DO-Tests-1", "Model1");
      configuration.MappingRules.Map(typeof (model.Part2.TestEntity2).Assembly, typeof (model.Part2.TestEntity2).Namespace).To("DO-Tests-1", "Model1");
      configuration.MappingRules.Map(typeof (model.Part3.TestEntity3).Assembly, typeof (model.Part3.TestEntity3).Namespace).To("DO-Tests-2", "Model3");
      configuration.MappingRules.Map(typeof (model.Part4.TestEntity4).Assembly, typeof (model.Part4.TestEntity4).Namespace).To("DO-Tests-2", "Model3");
    }

    protected override void ApplyCustomSettingsToTestConfiguration(DomainConfiguration configuration)
    {
      base.ApplyCustomSettingsToTestConfiguration(configuration);
      configuration.DefaultSchema = "Model1";
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.MappingRules.Map(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace).To("DO-Tests-1", "Model1");
      configuration.MappingRules.Map(typeof (model.Part2.TestEntity2).Assembly, typeof (model.Part2.TestEntity2).Namespace).To("DO-Tests-1", "Model1");
      configuration.MappingRules.Map(typeof (model.Part3.TestEntity3).Assembly, typeof (model.Part3.TestEntity3).Namespace).To("DO-Tests-2", "Model3");
      configuration.MappingRules.Map(typeof (model.Part4.TestEntity4).Assembly, typeof (model.Part4.TestEntity4).Namespace).To("DO-Tests-2", "Model3");
    }

    protected override IEnumerable<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      var @default = new NodeConfiguration(WellKnown.DefaultNodeId) {UpgradeMode = upgradeMode};
      var additional = new NodeConfiguration("Additional") {UpgradeMode = upgradeMode};
      additional.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-2");
      additional.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-4");
      additional.SchemaMapping.Add("Model1", "Model2");
      additional.SchemaMapping.Add("Model3", "Model4");
      return new[] {@default, additional};
    }
  }
}