// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.04.03

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.MetadataUpdate.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.MetadataUpdate
{
  public class MultidatabaseTest : SimpleTest
  {
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string DOTests2Db = WellKnownDatabases.MultiDatabase.AdditionalDb2;
    private const string DOTests3Db = WellKnownDatabases.MultiDatabase.AdditionalDb3;
    private const string DOTests4Db = WellKnownDatabases.MultiDatabase.AdditionalDb4;

    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;
    private const string Schema3 = WellKnownSchemas.Schema3;
    private const string Schema4 = WellKnownSchemas.Schema4;

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override void ApplyCustomSettingsToInitialConfiguration(DomainConfiguration domainConfiguration)
    {
      base.ApplyCustomSettingsToInitialConfiguration(domainConfiguration);
      ApplyCustomDomainSettings(domainConfiguration);
    }

    protected override void ApplyCustomSettingsToTestConfiguration(DomainConfiguration domainConfiguration)
    {
      base.ApplyCustomSettingsToTestConfiguration(domainConfiguration);
      ApplyCustomDomainSettings(domainConfiguration);
    }

    private static void ApplyCustomDomainSettings(DomainConfiguration domainConfiguration)
    {
      domainConfiguration.DefaultSchema = Schema1;
      domainConfiguration.DefaultDatabase = DOTests1Db;
      domainConfiguration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).To(DOTests1Db, Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).To(DOTests1Db, Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).To(DOTests2Db, Schema3);
      domainConfiguration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).To(DOTests2Db, Schema3);
    }

    protected override IEnumerable<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      var @default = new NodeConfiguration(WellKnown.DefaultNodeId) { UpgradeMode = upgradeMode };
      var additional = new NodeConfiguration("Additional") { UpgradeMode = upgradeMode };
      additional.DatabaseMapping.Add(DOTests1Db, DOTests3Db);
      additional.DatabaseMapping.Add(DOTests2Db, DOTests4Db);
      additional.SchemaMapping.Add(Schema1, Schema2);
      additional.SchemaMapping.Add(Schema3, Schema4);
      return new List<NodeConfiguration> {@default, additional};
    }
  }
}