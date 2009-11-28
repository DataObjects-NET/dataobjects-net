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
  [HierarchyRoot]
  public sealed class Fake : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  public class LegacyDbTestBase : AutoBuildTest
  {
    protected sealed override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Fake));
      return config;
    }

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.SqlServer);
    }
  }

  public abstract class LegacyDbAutoBuildTest : LegacyDbTestBase
  {
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      var config = BuildConfiguration();
      PrepareDb(config);
      Domain = BuildDomain(config);
      if (Domain!=null)
        ProviderInfo = Domain.StorageProviderInfo;
    }

    private void PrepareDb(DomainConfiguration config)
    {
      var driver = SqlDriver.Create(config.ConnectionInfo);
      var connection = driver.CreateConnection(config.ConnectionInfo);
      connection.Open();
      try {
        using (var cmd = connection.CreateCommand()) {
          cmd.CommandText = GetCleanDbScript();
          cmd.ExecuteNonQuery();
          cmd.CommandText = GetCreateDbScript(config);
          cmd.ExecuteNonQuery();
        }
      }
      finally {
        connection.Close();
      }
    }

    private string GetCleanDbScript()
    {
      return @"
      IF object_id('[dbo].[Fake]') is not null drop table [dbo].[Fake]
      IF object_id('[dbo].[Metadata.Assembly]') is not null drop table [dbo].[Metadata.Assembly]
      IF object_id('[dbo].[Metadata.Extension]') is not null drop table [dbo].[Metadata.Extension]
      IF object_id('[dbo].[Metadata.Type]') is not null drop table [dbo].[Metadata.Type]";
    }

    protected abstract string GetCreateDbScript(DomainConfiguration config);


    protected virtual new DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = DomainUpgradeMode.Legacy;
      return config;
    }
  }
}