// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.14

using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  public abstract class MultidatabaseTest : AutoBuildTest
  {
    protected const string DefaultSchemaName = "dbo";

    protected const string Database1Name = "DO-Tests-1";
    protected const string Database2Name = "DO-Tests-2";
    protected const string Database3Name = "DO-Tests-3";
    protected const string Database4Name = "DO-Tests-4";

    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultDatabase = Database1Name;
      configuration.DefaultSchema = DefaultSchemaName;
      return configuration;
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }
  }
}