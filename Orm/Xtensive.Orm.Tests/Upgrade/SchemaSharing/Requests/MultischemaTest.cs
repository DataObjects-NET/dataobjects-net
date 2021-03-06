// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.29

using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.Requests
{
  public class MultischemaTest : SimpleTest
  {
    private const string Model1 = WellKnownSchemas.Schema1;
    private const string Model2 = WellKnownSchemas.Schema2;
    private const string Model3 = WellKnownSchemas.Schema3;
    private const string Model4 = WellKnownSchemas.Schema4;

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = Model1;
      configuration.MappingRules.Map(typeof(Model.Part1.TestEntity1).Assembly, typeof(Model.Part1.TestEntity1).Namespace).ToSchema(Model1);
      configuration.MappingRules.Map(typeof(Model.Part2.TestEntity2).Assembly, typeof(Model.Part2.TestEntity2).Namespace).ToSchema(Model1);
      configuration.MappingRules.Map(typeof(Model.Part3.TestEntity3).Assembly, typeof(Model.Part3.TestEntity3).Namespace).ToSchema(Model3);
      configuration.MappingRules.Map(typeof(Model.Part4.TestEntity4).Assembly, typeof(Model.Part4.TestEntity4).Namespace).ToSchema(Model3);
      return configuration;
    }

    protected override List<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
    {
      var @default = new NodeConfiguration(WellKnown.DefaultNodeId) { UpgradeMode = upgradeMode };
      var additional = new NodeConfiguration("Additional") { UpgradeMode = upgradeMode };
      additional.SchemaMapping.Add(Model1, Model2);
      additional.SchemaMapping.Add(Model3, Model4);
      return new List<NodeConfiguration> {@default, additional};
    }
  }
}
