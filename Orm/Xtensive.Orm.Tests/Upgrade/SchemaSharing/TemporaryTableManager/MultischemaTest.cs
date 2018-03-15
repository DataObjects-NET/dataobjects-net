// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override Domain BuildReferenceDomain()
    {
      var configuration = GetDomainConfiguration();
      var domain = Domain.Build(configuration);
      domain.StorageNodeManager.AddNode(GetNodeConfiguration());

      return domain;
    }

    protected override Domain BuildTestDomain()
    {
      var configuration = GetDomainConfiguration();
      configuration.ShareStorageSchemaOverNodes = true;
      var domain = Domain.Build(configuration);
      domain.StorageNodeManager.AddNode(GetNodeConfiguration());

      return domain;
    }

    protected override DomainConfiguration GetDomainConfiguration()
    {
      var configuration = base.GetDomainConfiguration();
      configuration.DefaultSchema = "Model1";

      configuration.MappingRules.Map(typeof(model.Part1.TestEntity1).Assembly, typeof(model.Part1.TestEntity1).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof(model.Part2.TestEntity2).Assembly, typeof(model.Part2.TestEntity2).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof(model.Part3.TestEntity3).Assembly, typeof(model.Part3.TestEntity3).Namespace).ToSchema("Model2");
      configuration.MappingRules.Map(typeof(model.Part4.TestEntity4).Assembly, typeof(model.Part4.TestEntity4).Namespace).ToSchema("Model2");

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
      nodeConfiguration.SchemaMapping.Add("Model1", "Model3");
      nodeConfiguration.SchemaMapping.Add("Model2", "Model4");
      nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return nodeConfiguration;
    }
  }
}