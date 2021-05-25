// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string DOTests2Db = WellKnownDatabases.MultiDatabase.AdditionalDb2;
    private const string DOTests3Db = WellKnownDatabases.MultiDatabase.AdditionalDb3;
    private const string DOTests4Db = WellKnownDatabases.MultiDatabase.AdditionalDb4;

    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;
    private const string Schema3 = WellKnownSchemas.Schema3;
    private const string Schema4 = WellKnownSchemas.Schema4;

    private const string AdditionalNodeId = "Additional";

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();

      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).To(DOTests1Db, Schema1);
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity3).Assembly, typeof(model.Part2.TestEntity3).Namespace).To(DOTests1Db, Schema2);
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity5).Assembly, typeof(model.Part3.TestEntity5).Namespace).To(DOTests2Db, Schema1);
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity7).Assembly, typeof(model.Part4.TestEntity7).Namespace).To(DOTests2Db, Schema2);

      configuration.DefaultDatabase = DOTests1Db;
      configuration.DefaultSchema = Schema1;
      return configuration;
    }

    protected override Dictionary<NodeConfiguration, int> GetSkipParameters(DomainUpgradeMode upgradeMode)
    {
      var dictionary = base.GetSkipParameters(upgradeMode);
      var additionalNode = new NodeConfiguration(AdditionalNodeId);
      additionalNode.UpgradeMode = upgradeMode;

      additionalNode.DatabaseMapping.Add(DOTests1Db, DOTests3Db);
      additionalNode.DatabaseMapping.Add(DOTests2Db, DOTests4Db);

      additionalNode.SchemaMapping.Add(Schema1, Schema3);
      additionalNode.SchemaMapping.Add(Schema2, Schema4);

      dictionary.Add(additionalNode, 7);
      return dictionary;
    }
  }
}