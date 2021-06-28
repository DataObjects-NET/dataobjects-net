// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.03

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager
{
  public sealed class MultidatabaseTest : MultischemaTest
  {
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string DOTests2Db = WellKnownDatabases.MultiDatabase.AdditionalDb2;
    private const string DOTests3Db = WellKnownDatabases.MultiDatabase.AdditionalDb3;
    private const string DOTests4Db = WellKnownDatabases.MultiDatabase.AdditionalDb4;

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = Schema1;
      configuration.DefaultDatabase = DOTests1Db;

      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).To(DOTests1Db, Schema1);
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).To(DOTests1Db, Schema2);
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).To(DOTests2Db, Schema1);
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).To(DOTests2Db, Schema2);

      return configuration;
    }

    protected override NodeConfiguration GetNodeConfiguration()
    {
      var nodeConfiguration = new NodeConfiguration("Additional");
      nodeConfiguration.DatabaseMapping.Add(DOTests1Db, DOTests3Db);
      nodeConfiguration.DatabaseMapping.Add(DOTests2Db, DOTests4Db);
      nodeConfiguration.SchemaMapping.Add(Schema1, Schema3);
      nodeConfiguration.SchemaMapping.Add(Schema2, Schema4);
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return nodeConfiguration;
    }
  }
}