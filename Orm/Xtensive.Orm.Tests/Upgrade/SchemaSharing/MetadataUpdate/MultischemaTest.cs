// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.04.03

using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.MetadataUpdate.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.MetadataUpdate
{
  public class MultischemaTest : SimpleTest
  {
    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;
    private const string Schema3 = WellKnownSchemas.Schema3;
    private const string Schema4 = WellKnownSchemas.Schema4;

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
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
      domainConfiguration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema(Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).ToSchema(Schema1);
      domainConfiguration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).ToSchema(Schema3);
      domainConfiguration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).ToSchema(Schema3);
    }

    protected override IEnumerable<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      var @default = new NodeConfiguration(WellKnown.DefaultNodeId) { UpgradeMode = upgradeMode };
      var additional = new NodeConfiguration("Additional") { UpgradeMode = upgradeMode };
      additional.SchemaMapping.Add(Schema1, Schema2);
      additional.SchemaMapping.Add(Schema3, Schema4);
      return new List<NodeConfiguration> {@default, additional};
    }
  }
}