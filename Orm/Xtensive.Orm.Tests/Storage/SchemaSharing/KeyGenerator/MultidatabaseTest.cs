// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.06

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator
{
  public class MultidatabaseTest : SimpleTest
  {
    private const string AdditionalNodeId = "Additional";

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();

      configuration.MappingRules.Map(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace).To("DO-Tests-1", "Model1");
      configuration.MappingRules.Map(typeof (model.Part2.TestEntity3).Assembly, typeof (model.Part2.TestEntity3).Namespace).To("DO-Tests-1", "Model2");
      configuration.MappingRules.Map(typeof (model.Part3.TestEntity5).Assembly, typeof (model.Part3.TestEntity5).Namespace).To("DO-Tests-2", "Model1");
      configuration.MappingRules.Map(typeof (model.Part4.TestEntity7).Assembly, typeof (model.Part4.TestEntity7).Namespace).To("DO-Tests-2", "Model2");

      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.DefaultSchema = "Model1";
      return configuration;
    }

    protected override Dictionary<NodeConfiguration, int> GetSkipParameters(DomainUpgradeMode upgradeMode)
    {
      var dictionary = base.GetSkipParameters(upgradeMode);
      var additionalNode = new NodeConfiguration(AdditionalNodeId);
      additionalNode.UpgradeMode = upgradeMode;

      additionalNode.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-3");
      additionalNode.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-4");

      additionalNode.SchemaMapping.Add("Model1", "Model3");
      additionalNode.SchemaMapping.Add("Model2", "Model4");

      dictionary.Add(additionalNode, 7);
      return dictionary;
    }
  }
}