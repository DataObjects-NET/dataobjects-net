// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.20

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Sql;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.LegacyUpgradeTest.Model;

namespace Xtensive.Orm.Tests.Upgrade.LegacyUpgradeTest.Model
{
  [Serializable]
  [HierarchyRoot]
  public class A : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [Serializable]
  public class B : A
  {
    [Field]
    public Guid? Number { get; set; }

    [Field]
    public C C { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class C : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }
  }

}


namespace Xtensive.Orm.Tests.Upgrade.LegacyUpgrade
{
  [TestFixture, Category("Upgrade")]
  public class LegacyUpgradeTest
  {
    #region Sql scripts

    private const string CreateValidDbScript = @"
      IF object_id('[dbo].[B]') is not null drop table [dbo].[B]
      IF object_id('[dbo].[A]') is not null drop table [dbo].[A]
      IF object_id('[dbo].[C]') is not null drop table [dbo].[C]
      IF object_id('[dbo].[Int32-Generator]') is not null drop table [dbo].[Int32-Generator]
      IF object_id('[dbo].[Metadata.Assembly]') is not null drop table [dbo].[Metadata.Assembly]
      IF object_id('[dbo].[Metadata.Extension]') is not null drop table [dbo].[Metadata.Extension]
      IF object_id('[dbo].[Metadata.Type]') is not null drop table [dbo].[Metadata.Type]

      CREATE TABLE [dbo].[A](
	      [Id] [int] NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Name] [nvarchar](4000) NOT NULL DEFAULT (NULL),
        CONSTRAINT [PK_A_TEST] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE TABLE [dbo].[B](
	      [Id] [int] NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Number] [uniqueidentifier] NOT NULL,
        [C.Id] [uniqueidentifier] NOT NULL,
        CONSTRAINT [PK_B.A] PRIMARY KEY CLUSTERED ([Id] ASC))
      ALTER TABLE [dbo].[B]  WITH CHECK ADD CONSTRAINT [FK_B_A] FOREIGN KEY([Id]) REFERENCES [dbo].[A] ([Id])
      ALTER TABLE [dbo].[B] CHECK CONSTRAINT [FK_B_A]
      CREATE TABLE [dbo].[C](
	      [Id] [uniqueidentifier] NOT NULL,
	      [TypeId] [int] NOT NULL,
        CONSTRAINT [PK_C] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE TABLE [dbo].[Int32-Generator](
	      [ID] [int] IDENTITY(128,128) NOT NULL,
        CONSTRAINT [PK_Int32-Generator] PRIMARY KEY CLUSTERED ([ID] ASC))";

    private const string CreateInvalidDbScript1 = @"
      IF object_id('[dbo].[B]') is not null drop table [dbo].[B]
      IF object_id('[dbo].[A]') is not null drop table [dbo].[A]
      IF object_id('[dbo].[C]') is not null drop table [dbo].[C]
      IF object_id('[dbo].[Int32-Generator]') is not null drop table [dbo].[Int32-Generator]
      IF object_id('[dbo].[Metadata.Assembly]') is not null drop table [dbo].[Metadata.Assembly]
      IF object_id('[dbo].[Metadata.Extension]') is not null drop table [dbo].[Metadata.Extension]
      IF object_id('[dbo].[Metadata.Type]') is not null drop table [dbo].[Metadata.Type]

      CREATE TABLE [dbo].[A](
	      [Id] [int] NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Name] [nvarchar](4000) NULL DEFAULT (NULL),
        CONSTRAINT [PK_A_TEST] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE TABLE [dbo].[C](
	      [Id] [uniqueidentifier] NOT NULL,
	      [TypeId] [int] NOT NULL,
        CONSTRAINT [PK_C] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE TABLE [dbo].[Int32-Generator](
	      [ID] [int] IDENTITY(128,128) NOT NULL,
        CONSTRAINT [PK_Int32-Generator] PRIMARY KEY CLUSTERED ([ID] ASC))
      CREATE TABLE [dbo].[Metadata.Assembly](
	      [Name] [nvarchar](1024) NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Version] [nvarchar](64) NULL DEFAULT (NULL),
      CONSTRAINT [PK_Assembly] PRIMARY KEY CLUSTERED ([Name] ASC))
      CREATE TABLE [dbo].[Metadata.Extension](
	      [Name] [nvarchar](1024) NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Text] [nvarchar](max) NULL DEFAULT (NULL),
	      [Data] [varbinary](max) NULL DEFAULT (NULL),
        CONSTRAINT [PK_Extension] PRIMARY KEY CLUSTERED ([Name] ASC))
      CREATE TABLE [dbo].[Metadata.Type](
	      [Id] [int] NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Name] [nvarchar](1000) NULL DEFAULT (NULL),
        CONSTRAINT [PK_Type_TEST] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE UNIQUE NONCLUSTERED INDEX [Type.IX_Name] ON [dbo].[Metadata.Type] ([Name] ASC)";

    private const string CreateInvalidDbScript2 = @"
      IF object_id('[dbo].[B]') is not null drop table [dbo].[B]
      IF object_id('[dbo].[A]') is not null drop table [dbo].[A]
      IF object_id('[dbo].[C]') is not null drop table [dbo].[C]    
      IF object_id('[dbo].[Int32-Generator]') is not null drop table [dbo].[Int32-Generator]
      IF object_id('[dbo].[Metadata.Assembly]') is not null drop table [dbo].[Metadata.Assembly]
      IF object_id('[dbo].[Metadata.Extension]') is not null drop table [dbo].[Metadata.Extension]
      IF object_id('[dbo].[Metadata.Type]') is not null drop table [dbo].[Metadata.Type]

      CREATE TABLE [dbo].[A](
	      [Id] [int] NOT NULL,
	      [TypeId] [nvarchar](20) NOT NULL,
	      [Name] [nvarchar](4000) NOT NULL DEFAULT (NULL),
        CONSTRAINT [PK_A_TEST] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE TABLE [dbo].[B](
	      [Id] [int] NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Number] [int] NOT NULL DEFAULT ((0)),
        CONSTRAINT [PK_B.A] PRIMARY KEY CLUSTERED ([Id] ASC))
      ALTER TABLE [dbo].[B]  WITH CHECK ADD CONSTRAINT [FK_B_A] FOREIGN KEY([Id]) REFERENCES [dbo].[A] ([Id])
      ALTER TABLE [dbo].[B] CHECK CONSTRAINT [FK_B_A]
      CREATE TABLE [dbo].[C](
	      [Id] [uniqueidentifier] NOT NULL,
	      [TypeId] [int] NOT NULL,
        CONSTRAINT [PK_C] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE TABLE [dbo].[Int32-Generator](
	      [ID] [int] IDENTITY(128,128) NOT NULL,
        CONSTRAINT [PK_Int32-Generator] PRIMARY KEY CLUSTERED ([ID] ASC))
      CREATE TABLE [dbo].[Metadata.Assembly](
	      [Name] [nvarchar](1024) NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Version] [nvarchar](64) NULL DEFAULT (NULL),
        CONSTRAINT [PK_Assembly] PRIMARY KEY CLUSTERED ([Name] ASC))
      CREATE TABLE [dbo].[Metadata.Extension](
	      [Name] [nvarchar](1024) NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Text] [nvarchar](max) NULL DEFAULT (NULL),
	      [Data] [varbinary](max) NULL DEFAULT (NULL),
        CONSTRAINT [PK_Extension] PRIMARY KEY CLUSTERED ([Name] ASC))
      CREATE TABLE [dbo].[Metadata.Type](
	      [Id] [int] NOT NULL,
	      [TypeId] [int] NOT NULL,
	      [Name] [nvarchar](1000) NULL DEFAULT (NULL),
        CONSTRAINT [PK_Type_TEST] PRIMARY KEY CLUSTERED ([Id] ASC))
      CREATE UNIQUE NONCLUSTERED INDEX [Type.IX_Name] ON [dbo].[Metadata.Type] ([Name] ASC)";

    #endregion

#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtMost(StorageProviderVersion.SqlServer2008R2);
    }

    private static DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (A).Assembly, typeof (A).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    [Test]
    public void BuildWithValidDbTest()
    {
      CreateDb(CreateValidDbScript);
      var config = BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      Domain.Build(config).Dispose();
    }

    [Test]
    public void BuildWithInvalidDbTest()
    {
      var config = BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.LegacyValidate;
      
      CreateDb(CreateInvalidDbScript1);
      AssertEx.Throws<SchemaSynchronizationException>(() => Domain.Build(config));
      CreateDb(CreateInvalidDbScript2);
      AssertEx.Throws<SchemaSynchronizationException>(() => Domain.Build(config));
    }

    [Test]
    public void BuildInSkipModeTest()
    {
      CreateDb(CreateValidDbScript);
      var config = BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.LegacySkip;
      Domain.Build(config).Dispose();
    }

    private static void CreateDb(string script)
    {
      var config = BuildConfiguration();
      var driver = TestSqlDriver.Create(config.ConnectionInfo);
      var connection = driver.CreateConnection();
      connection.Open();
      try {
        using (var cmd = connection.CreateCommand()) {
          cmd.CommandText = script;
          cmd.ExecuteNonQuery();
        }
      }
      finally {
        connection.Close();
      }
    }
  }
}