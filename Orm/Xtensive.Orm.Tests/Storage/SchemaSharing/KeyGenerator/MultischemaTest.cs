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
  public class MultischemaTest : SimpleTest
  {
    private const string AdditionalNodeId = "Additional";

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();

      configuration.MappingRules.Map(typeof (model.Part1.TestEntity1).Assembly, typeof (model.Part1.TestEntity1).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (model.Part2.TestEntity3).Assembly, typeof (model.Part2.TestEntity3).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (model.Part3.TestEntity5).Assembly, typeof (model.Part3.TestEntity5).Namespace).ToSchema("Model2");
      configuration.MappingRules.Map(typeof (model.Part4.TestEntity7).Assembly, typeof (model.Part4.TestEntity7).Namespace).ToSchema("Model2");

      configuration.DefaultSchema = "Model1";
      return configuration;
    }

    protected override void FillInSkipParameters(Dictionary<NodeConfiguration, int> dictionary)
    {
      base.FillInSkipParameters(dictionary);

      var additionalNode = new NodeConfiguration(AdditionalNodeId);
      additionalNode.UpgradeMode = DomainUpgradeMode.Recreate;
      additionalNode.SchemaMapping.Add("Model1", "Model3");
      additionalNode.SchemaMapping.Add("Model2", "Model4");
      dictionary.Add(additionalNode, 7);
    }
  }
}
