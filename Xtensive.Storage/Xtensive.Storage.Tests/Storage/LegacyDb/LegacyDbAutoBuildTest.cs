// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.28

using System;
using Xtensive.Sql;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.LegacyDb
{
  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(null)]
  public sealed class Fake : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
  
  public abstract class LegacyDbAutoBuildTest : AutoBuildTest
  {
    public override void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      ClearDb();
      base.TestFixtureSetUp();
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
      config.UpgradeMode = DomainUpgradeMode.Legacy;
      return config;
    }

    private void ClearDb()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Fake));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      Domain.Build(config).Dispose();
    }

    private void PrepareDb(DomainConfiguration config)
    {
      var driver = SqlDriver.Create(config.ConnectionInfo);
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
      IF object_id('[dbo].[Fake]') is not null drop table [dbo].[Fake]
      IF object_id('[dbo].[Metadata.Assembly]') is not null drop table [dbo].[Metadata.Assembly]
      IF object_id('[dbo].[Metadata.Extension]') is not null drop table [dbo].[Metadata.Extension]
      IF object_id('[dbo].[Metadata.Type]') is not null drop table [dbo].[Metadata.Type]";
    }
  }
}