// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.14

using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  public abstract class MultidatabaseTest : MultimappingTest
  {
    protected const string DefaultDatabaseName = "default";

    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultDatabase = DefaultDatabaseName;
      return configuration;
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }
  }
}