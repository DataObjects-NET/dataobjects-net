// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.04.05

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation
{
  public class MultischemaEntityManipulationTest : SimpleEntityManipulationTest
  {
    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;
    private const string Schema3 = WellKnownSchemas.Schema3;
    private const string Schema4 = WellKnownSchemas.Schema4;

    protected override NodeConfigurationType NodeConfiguration => NodeConfigurationType.MultischemaNodes;

    protected override void CheckRequirements() => Require.AllFeaturesSupported(ProviderFeatures.Multischema);

    protected override void ApplyCustomSettingsToInitialConfiguration(DomainConfiguration configuration)
    {
      base.ApplyCustomSettingsToInitialConfiguration(configuration);
      configuration.DefaultSchema = Schema1;
      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).ToSchema(Schema3);
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).ToSchema(Schema3);
    }

    protected override void ApplyCustomSettingsToTestConfiguration(DomainConfiguration configuration)
    {
      base.ApplyCustomSettingsToTestConfiguration(configuration);
      configuration.DefaultSchema = Schema1;
      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).ToSchema(Schema3);
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).ToSchema(Schema3);
    }

    protected override IEnumerable<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      var @default = new NodeConfiguration(WellKnown.DefaultNodeId) { UpgradeMode = upgradeMode };
      var additional = new NodeConfiguration("Additional") { UpgradeMode = upgradeMode };
      additional.SchemaMapping.Add(Schema1, Schema2);
      additional.SchemaMapping.Add(Schema3, Schema4);
      return new[] { @default, additional };
    }
  }
}