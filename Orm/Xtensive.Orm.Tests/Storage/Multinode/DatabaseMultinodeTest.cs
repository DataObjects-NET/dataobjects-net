// Copyright (C) 2014 Xtensive LLC.
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
  public class DatabaseMultinodeTest : StandardMultinodeTest
  {
    private const string DefaultSchema = Multimapping.MultidatabaseTest.DefaultSchemaName;
    private const string DefaultDatabase = Multimapping.MultidatabaseTest.Database1Name;
    private const string SecondDatabase = Multimapping.MultidatabaseTest.Database2Name;
    private const string ThirdDatabase = Multimapping.MultidatabaseTest.Database3Name;

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = DefaultSchema;
      configuration.DefaultDatabase = DefaultDatabase;
      return configuration;
    }

    protected override void PopulateNodes()
    {
      var configuration = new NodeConfiguration(TestNodeId2) {UpgradeMode = DomainUpgradeMode.Recreate};
      configuration.DatabaseMapping.Add(DefaultDatabase, SecondDatabase);
      Domain.StorageNodeManager.AddNode(configuration);

      configuration = new NodeConfiguration(TestNodeId3) {UpgradeMode = DomainUpgradeMode.Recreate};
      configuration.SchemaMapping.Add(DefaultDatabase, ThirdDatabase);
      Domain.StorageNodeManager.AddNode(configuration);
    }
  }
}