// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.28

using Xtensive.Sql;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Sql;

namespace Xtensive.Orm.Tests.Storage.LegacyDb
{
  public abstract class LegacyDbAutoBuildTest : AutoBuildTest
  {
    public override void TestFixtureSetUp()
    {
      CheckRequirements();
      ClearDb();
      base.TestFixtureSetUp();
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      PrepareDb(configuration);
      return base.BuildDomain(configuration);
    }

    protected abstract string GetCreateDbScript(DomainConfiguration config);
    
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      return config;
    }

    private void ClearDb()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      Domain.Build(config).Dispose();
    }

    private void PrepareDb(DomainConfiguration config)
    {
      var driver = TestSqlDriver.Create(config.ConnectionInfo);
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        using (var cmd = connection.CreateCommand()) {
          cmd.CommandText = GetCleanDbScript();
          cmd.ExecuteNonQuery();
          cmd.CommandText = GetCreateDbScript(config);
          cmd.ExecuteNonQuery();
        }
      }
    }

    private static string GetCleanDbScript()
    {
      return @"
      IF object_id('[dbo].[Metadata.Assembly]') is not null drop table [dbo].[Metadata.Assembly]
      IF object_id('[dbo].[Metadata.Extension]') is not null drop table [dbo].[Metadata.Extension]
      IF object_id('[dbo].[Metadata.Type]') is not null drop table [dbo].[Metadata.Type]";
    }
  }
}