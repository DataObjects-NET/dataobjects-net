// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.14

using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  public abstract class MultischemaTest : MultimappingTest
  {
    protected const string Schema1Name = "test1";
    protected const string Schema2Name = "test2";
    protected const string Schema3Name = "test3";
    protected const string Schema4Name = "test4";

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = Schema1Name;
      return configuration;
    }

    protected virtual void PrepareSchema(ConnectionInfo connectionInfo)
    {
      StorageTestHelper.DemandSchemas(
        connectionInfo, Schema1Name, Schema2Name, Schema3Name, Schema4Name);
    }
  }
}