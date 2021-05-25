// Copyright (C) 2014-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.26

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  [TestFixture]
  public class SchemaMultinodeTest : StandardMultinodeTest
  {
    private const string DefaultSchema = WellKnownSchemas.Schema1;
    private const string SecondSchema = WellKnownSchemas.Schema2;
    private const string ThirdSchema = WellKnownSchemas.Schema3;

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = DefaultSchema;
      return configuration;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      StorageTestHelper.DemandSchemas(configuration.ConnectionInfo, DefaultSchema, SecondSchema, ThirdSchema);
      return base.BuildDomain(configuration);
    }

    protected override void PopulateNodes()
    {
      var configuration = new NodeConfiguration(TestNodeId2) {UpgradeMode = DomainUpgradeMode.Recreate};
      configuration.SchemaMapping.Add(DefaultSchema, SecondSchema);
      _ = Domain.StorageNodeManager.AddNode(configuration);

      configuration = new NodeConfiguration(TestNodeId3) {UpgradeMode = DomainUpgradeMode.Recreate};
      configuration.SchemaMapping.Add(DefaultSchema, ThirdSchema);
      _ = Domain.StorageNodeManager.AddNode(configuration);
    }
  }
}