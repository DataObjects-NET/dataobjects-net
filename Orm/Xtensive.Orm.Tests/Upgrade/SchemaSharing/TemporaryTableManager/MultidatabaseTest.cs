// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.03

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager
{
  public sealed class MultidatabaseTest : MultischemaTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = "Model1";
      configuration.DefaultDatabase = "DO-Tests-1";

      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).To("DO-Tests-1", "Model1");
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).To("DO-Tests-1", "Model2");
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).To("DO-Tests-2", "Model1");
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).To("DO-Tests-2", "Model2");

      return configuration;
    }

    protected override NodeConfiguration GetNodeConfiguration()
    {
      var nodeConfiguration = new NodeConfiguration("Additional");
      nodeConfiguration.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-3");
      nodeConfiguration.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-4");
      nodeConfiguration.SchemaMapping.Add("Model1", "Model3");
      nodeConfiguration.SchemaMapping.Add("Model2", "Model4");
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return nodeConfiguration;
    }
  }
}