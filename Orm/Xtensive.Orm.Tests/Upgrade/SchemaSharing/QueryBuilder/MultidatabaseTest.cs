// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.30

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.QueryBuilder
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

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = Schema1;
      configuration.DefaultDatabase = DOTests1Db;
      configuration.MappingRules.Map(typeof(Model.Part1.TestEntity1).Assembly, typeof(Model.Part1.TestEntity1).Namespace).To(DOTests1Db, Schema1);
      configuration.MappingRules.Map(typeof(Model.Part2.TestEntity2).Assembly, typeof(Model.Part2.TestEntity2).Namespace).To(DOTests1Db, Schema1);
      configuration.MappingRules.Map(typeof(Model.Part3.TestEntity3).Assembly, typeof(Model.Part3.TestEntity3).Namespace).To(DOTests2Db, Schema3);
      configuration.MappingRules.Map(typeof(Model.Part4.TestEntity4).Assembly, typeof(Model.Part4.TestEntity4).Namespace).To(DOTests2Db, Schema3);
      return configuration;
    }

    protected override List<NodeConfiguration> GetNodes(DomainUpgradeMode upgradeMode)
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