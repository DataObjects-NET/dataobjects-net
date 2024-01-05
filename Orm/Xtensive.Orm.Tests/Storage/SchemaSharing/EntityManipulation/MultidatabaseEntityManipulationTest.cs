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
  public class MultidatabaseEntityManipulationTest : SimpleEntityManipulationTest
  {
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string DOTests2Db = WellKnownDatabases.MultiDatabase.AdditionalDb2;
    private const string DOTests3Db = WellKnownDatabases.MultiDatabase.AdditionalDb3;
    private const string DOTests4Db = WellKnownDatabases.MultiDatabase.AdditionalDb4;

    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;
    private const string Schema3 = WellKnownSchemas.Schema3;
    private const string Schema4 = WellKnownSchemas.Schema4;

    protected override NodeConfigurationType NodeConfiguration => NodeConfigurationType.MultidatabaseNodes;

    protected override void CheckRequirements() => Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);

    protected override void ApplyCustomSettingsToInitialConfiguration(DomainConfiguration configuration)
    {
      base.ApplyCustomSettingsToInitialConfiguration(configuration);
      ApplyDomainCustomSettings(configuration);
    }

    protected override void ApplyCustomSettingsToTestConfiguration(DomainConfiguration configuration)
    {
      base.ApplyCustomSettingsToTestConfiguration(configuration);
      ApplyDomainCustomSettings(configuration);
    }

    private static void ApplyDomainCustomSettings(DomainConfiguration configuration)
    {
      configuration.DefaultSchema = Schema1;
      configuration.DefaultDatabase = DOTests1Db;
      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).To(DOTests1Db, Schema1);
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).To(DOTests1Db, Schema1);
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).To(DOTests2Db, Schema3);
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).To(DOTests2Db, Schema3);
    }

    protected override IEnumerable<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      var @default = new NodeConfiguration(WellKnown.DefaultNodeId) { UpgradeMode = upgradeMode };
      var additional = new NodeConfiguration("Additional") { UpgradeMode = upgradeMode };
      additional.DatabaseMapping.Add(DOTests1Db, DOTests3Db);
      additional.DatabaseMapping.Add(DOTests1Db, DOTests4Db);
      additional.SchemaMapping.Add(Schema1, Schema2);
      additional.SchemaMapping.Add(Schema3, Schema4);
      return new[] { @default, additional };
    }
  }
}