// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.20

using Xtensive.Sql;
using System;

namespace Xtensive.Storage.Tests.Upgrade.LegacyUpgradeTest.Model
{
  [HierarchyRoot]
  public class A : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  public class B : A
  {
    [Field]
    public Guid? Number { get; set; }

    [Field]
    public C C { get; set; }
  }

  [HierarchyRoot]
  public class C : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }
  }

}


namespace Xtensive.Storage.Tests.Upgrade.LegacyUpgrade
{
  using Upgrade.LegacyUpgradeTest.Model;
  using NUnit.Framework;
  using Core.Testing;

  public class LegacyUpgradeTest : AutoBuildTest
  {
    #region Sql scripts

    private string createValidDbScript = @"
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

    private string createInvalidDbScript1 = @"
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

    private string createInvalidDbScript2 = @"
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

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.SqlServer);
    }

    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (A).Assembly, typeof (A).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override Domain BuildDomain(Xtensive.Storage.Configuration.DomainConfiguration configuration)
    {
      return base.BuildDomain(configuration);
    }

    [Test]
    public void BuildWithValidDbTest()
    {
      CreateDb(createValidDbScript);
      var config = BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Legacy;
      Domain.Build(config);
    }

    [Test]
    public void BuildWithInalidDbTest()
    {
      var config = BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Legacy;
      
      CreateDb(createInvalidDbScript1);
      AssertEx.Throws<SchemaSynchronizationException>(() => Domain.Build(config));
      CreateDb(createInvalidDbScript2);
      AssertEx.Throws<SchemaSynchronizationException>(() => Domain.Build(config));
    }

    private void CreateDb(string script)
    {
      var config = BuildConfiguration();
      var driver = SqlDriver.Create(config.ConnectionInfo);
      var connection = driver.CreateConnection(config.ConnectionInfo);
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