// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.03

using System.Collections.Generic;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.TemporaryTableManager
{
  public class MultischemaTest : SimpleTest
  {
    protected const string Schema1 = WellKnownSchemas.Schema1;
    protected const string Schema2 = WellKnownSchemas.Schema2;
    protected const string Schema3 = WellKnownSchemas.Schema3;
    protected const string Schema4 = WellKnownSchemas.Schema4;

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override Domain BuildReferenceDomain()
    {
      var configuration = GetDomainConfiguration();
      var domain = Domain.Build(configuration);
      _ = domain.StorageNodeManager.AddNode(GetNodeConfiguration());
      return domain;
    }

    protected override Domain BuildTestDomain()
    {
      var configuration = GetDomainConfiguration();
      configuration.ShareStorageSchemaOverNodes = true;
      var domain = Domain.Build(configuration);
      _ = domain.StorageNodeManager.AddNode(GetNodeConfiguration());
      return domain;
    }

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = Schema1;

      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).ToSchema(Schema2);
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).ToSchema(Schema2);

      return configuration;
    }

    protected override IEnumerable<string> GetNodeIdentifiers()
    {
      yield return WellKnown.DefaultNodeId;
      yield return "Additional";
    }

    protected virtual NodeConfiguration GetNodeConfiguration()
    {
      var nodeConfiguration = new NodeConfiguration("Additional");
      nodeConfiguration.SchemaMapping.Add(Schema1, Schema3);
      nodeConfiguration.SchemaMapping.Add(Schema2, Schema4);
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return nodeConfiguration;
    }
  }
}