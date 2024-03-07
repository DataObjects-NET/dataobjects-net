// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.06

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet;
using model = Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator.Model;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator
{
  public class MultischemaTest : SimpleTest
  {
    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;
    private const string Schema3 = WellKnownSchemas.Schema3;
    private const string Schema4 = WellKnownSchemas.Schema4;

    private const string AdditionalNodeId = "Additional";

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();

      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity3).Assembly, typeof(model.Part2.TestEntity3).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity5).Assembly, typeof(model.Part3.TestEntity5).Namespace).ToSchema(Schema2);
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity7).Assembly, typeof(model.Part4.TestEntity7).Namespace).ToSchema(Schema2);

      configuration.DefaultSchema = Schema1;
      return configuration;
    }

    protected override Dictionary<NodeConfiguration, int> GetSkipParameters(DomainUpgradeMode upgradeMode)
    {
      var dictionary = base.GetSkipParameters(upgradeMode);

      var additionalNode = new NodeConfiguration(AdditionalNodeId);
      additionalNode.UpgradeMode = upgradeMode;
      additionalNode.SchemaMapping.Add(Schema1, Schema3);
      additionalNode.SchemaMapping.Add(Schema2, Schema4);
      dictionary.Add(additionalNode, 7);
      return dictionary;
    }
  }
}
