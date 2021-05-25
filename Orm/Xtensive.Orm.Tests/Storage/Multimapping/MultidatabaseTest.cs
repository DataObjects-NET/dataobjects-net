// Copyright (C) 2012-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.14

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  public abstract class MultidatabaseTest : MultimappingTest
  {
    public const string DefaultSchemaName = WellKnownSchemas.SqlServerDefaultSchema;

    public const string Database1Name = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    public const string Database2Name = WellKnownDatabases.MultiDatabase.AdditionalDb2;
    public const string Database3Name = WellKnownDatabases.MultiDatabase.AdditionalDb3;
    public const string Database4Name = WellKnownDatabases.MultiDatabase.AdditionalDb4;

    protected override DomainConfiguration BuildConfiguration()
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