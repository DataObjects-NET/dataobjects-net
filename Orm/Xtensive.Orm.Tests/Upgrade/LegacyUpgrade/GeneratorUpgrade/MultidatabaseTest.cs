// Copyright (C) 2019-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.10

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.LegacyUpgrade.GeneratorUpgrade
{
  public class MultidatabaseTest : SimpleSchemaTest
  {
    private const string DefaultDatabase = WellKnownDatabases.MultiDatabase.MainDb;
    private const string AlternativeDatabase = WellKnownDatabases.MultiDatabase.AdditionalDb1;

    protected override void ApplyCustomConfigurationSettings(DomainConfiguration configuration)
    {
      base.ApplyCustomConfigurationSettings(configuration);
      configuration.DefaultDatabase = DefaultDatabase;
      configuration.DefaultSchema = WellKnownSchemas.SqlServerDefaultSchema;
      var namespaces = configuration.Types
        .Where(t => t.Namespace.Contains("Xtensive.Orm.Tests.Upgrade.LegacyUpgrade.GeneratorUpgrade"))
        .GroupBy(t => t.Namespace)
        .Select(g => g.Key)
        .ToArray();
      if (namespaces.Length == 0) {
        configuration.MappingRules.Map("Xtensive.Orm.Tests.Upgrade.LegacyUpgrade.GeneratorUpgrade.ReferenceModel.Part1").ToDatabase(DefaultDatabase);
        configuration.MappingRules.Map("Xtensive.Orm.Tests.Upgrade.LegacyUpgrade.GeneratorUpgrade.ReferenceModel.Part2").ToDatabase(AlternativeDatabase);
      }
      else {
        configuration.MappingRules.Map(namespaces[0]).ToDatabase(DefaultDatabase);
        configuration.MappingRules.Map(namespaces[1]).ToDatabase(AlternativeDatabase);
      }
    }

    protected override string[] GetUsedCatalogs() => new[] { DefaultDatabase, AlternativeDatabase };

    protected override void InitializeGenerators(DomainConfiguration configuration, long seedIncrease = 0, long cacheSizeIncrease = 0)
    {
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int16") { Seed = 16 + seedIncrease, CacheSize = 16 + cacheSizeIncrease, Database = DefaultDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int32") { Seed = 32 + seedIncrease, CacheSize = 32 + cacheSizeIncrease, Database = DefaultDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int64") { Seed = 64 + seedIncrease, CacheSize = 64 + cacheSizeIncrease, Database = DefaultDatabase });

      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int16") { Seed = 16 + seedIncrease, CacheSize = 16 + cacheSizeIncrease, Database = AlternativeDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int32") { Seed = 32 + seedIncrease, CacheSize = 32 + cacheSizeIncrease, Database = AlternativeDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int64") { Seed = 64 + seedIncrease, CacheSize = 64 + cacheSizeIncrease, Database = AlternativeDatabase });

      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedShortKeyEntityPart1") { Seed = 16 + seedIncrease, CacheSize = 16 + cacheSizeIncrease, Database = DefaultDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedIntKeyEntityPart1") { Seed = 32 + seedIncrease, CacheSize = 32 + cacheSizeIncrease, Database = DefaultDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedLongKeyEntityPart1") { Seed = 64 + seedIncrease, CacheSize = 64 + cacheSizeIncrease, Database = DefaultDatabase });

      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedShortKeyEntityPart2") { Seed = 16 + seedIncrease, CacheSize = 16 + cacheSizeIncrease, Database = AlternativeDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedIntKeyEntityPart2") { Seed = 32 + seedIncrease, CacheSize = 32 + cacheSizeIncrease, Database = AlternativeDatabase });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedLongKeyEntityPart2") { Seed = 64 + seedIncrease, CacheSize = 64 + cacheSizeIncrease, Database = AlternativeDatabase });
    }

    protected override string GetSystemItemsCleanup()
    {
      return
        $"IF object_id('[{DefaultDatabase}].[dbo].[Metadata.Assembly]') is not null drop table [{DefaultDatabase}].[dbo].[Metadata.Assembly];" +
        $"IF object_id('[{DefaultDatabase}].[dbo].[Metadata.Extension]') is not null drop table [{DefaultDatabase}].[dbo].[Metadata.Extension];" +
        $"IF object_id('[{DefaultDatabase}].[dbo].[Metadata.Type]') is not null drop table [{DefaultDatabase}].[dbo].[Metadata.Type];" +
        $"IF object_id('[{AlternativeDatabase}].[dbo].[Metadata.Assembly]') is not null drop table [{AlternativeDatabase}].[dbo].[Metadata.Assembly];" +
        $"IF object_id('[{AlternativeDatabase}].[dbo].[Metadata.Extension]') is not null drop table [{AlternativeDatabase}].[dbo].[Metadata.Extension];" +
        $"IF object_id('[{AlternativeDatabase}].[dbo].[Metadata.Type]') is not null drop table [{AlternativeDatabase}].[dbo].[Metadata.Type];" +
        $"USE [{DefaultDatabase}];" +
        $"IF object_id('[dbo].[Int16-Generator]') is not null drop sequence [dbo].[Int16-Generator];" +
        $"IF object_id('[dbo].[Int32-Generator]') is not null drop sequence [dbo].[Int32-Generator];" +
        $"IF object_id('[dbo].[Int64-Generator]') is not null drop sequence [dbo].[Int64-Generator];" +
        $"USE [{AlternativeDatabase}];" +
        $"IF object_id('[dbo].[Int16-Generator]') is not null drop sequence [dbo].[Int16-Generator];" +
        $"IF object_id('[dbo].[Int32-Generator]') is not null drop sequence [dbo].[Int32-Generator];" +
        $"IF object_id('[dbo].[Int64-Generator]') is not null drop sequence [dbo].[Int64-Generator];" +
        $"USE [master];";
    }

    protected override string GetInitDomainStructureCreationScript(bool defaultGeneratorSettings, bool lessGenerators,
      long seedIncrease, long cacheSizeIncrease)
    {
      var sharedPart =
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[Metadata.Assembly] ([Name] nvarchar(1024) NOT NULL, [Version] nvarchar(64), CONSTRAINT [PK_Assembly] PRIMARY KEY CLUSTERED ([Name]));" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[Metadata.Extension] ([Name] nvarchar(1024) NOT NULL, [Text] nvarchar(max), [Data] varbinary(max), CONSTRAINT [PK_Extension] PRIMARY KEY CLUSTERED ([Name]));" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id] integer NOT NULL, [Name] nvarchar(1000), CONSTRAINT [PK_Type] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE UNIQUE INDEX [Type.IX_Name] ON [{DefaultDatabase}].[dbo].[Metadata.Type] ([Name] ASC);" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[ShortKeyEntityPart1] ([Id] smallint NOT NULL, CONSTRAINT [PK_ShortKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[IntKeyEntityPart1] ([Id] integer NOT NULL, CONSTRAINT [PK_IntKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id] ) );" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[LongKeyEntityPart1] ([Id] bigint NOT NULL, CONSTRAINT [PK_LongKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id] ) );" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedShortKeyEntityPart1] ([Id] smallint NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1] ([Id] integer NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1] ([Id] bigint NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Metadata.Assembly] ([Name] nvarchar(1024) NOT NULL, [Version] nvarchar(64), CONSTRAINT [PK_Assembly] PRIMARY KEY CLUSTERED ([Name]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Metadata.Extension] ([Name] nvarchar(1024) NOT NULL, [Text] nvarchar(max), [Data] varbinary(max), CONSTRAINT [PK_Extension] PRIMARY KEY CLUSTERED ([Name]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Id] integer NOT NULL, [Name] nvarchar(1000), CONSTRAINT [PK_Type] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE UNIQUE INDEX [Type.IX_Name] ON [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Name] ASC);" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[ShortKeyEntityPart2] ([Id] smallint NOT NULL, CONSTRAINT [PK_ShortKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[IntKeyEntityPart2] ([Id] integer NOT NULL, CONSTRAINT [PK_IntKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[LongKeyEntityPart2] ([Id] bigint NOT NULL, CONSTRAINT [PK_LongKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedShortKeyEntityPart2] ([Id] smallint NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2] ([Id] integer NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2] ([Id] bigint NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));";

      if (defaultGeneratorSettings) {
        if (lessGenerators) {
          return
            sharedPart +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
        }
        else {
          return
            sharedPart +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int16-Generator] ([ID] smallint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int16-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int16-Generator] ([ID] smallint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int16-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedShortKeyEntityPart1-Generator] ([ID] smallint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedShortKeyEntityPart2-Generator] ([ID] smallint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
        }
      }
      else {
        var increaseInt16 = BaseInt16CacheSize + cacheSizeIncrease;
        var increaseInt32 = BaseInt32CacheSize + cacheSizeIncrease;
        var increaseInt64 = BaseInt64CacheSize + cacheSizeIncrease;

        var seedInt16 = increaseInt16 + BaseInt16Seed + seedIncrease;
        var seedInt32 = increaseInt32 + BaseInt32Seed + seedIncrease;
        var seedInt64 = increaseInt64 + BaseInt64Seed + seedIncrease;

        if (lessGenerators) {
          return sharedPart +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
        }
        else {
          return sharedPart +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int16-Generator] ([ID] smallint IDENTITY ({seedInt16}, {increaseInt16}) NOT NULL, CONSTRAINT [PK_Int16-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int16-Generator] ([ID] smallint IDENTITY ({seedInt16}, {increaseInt16}) NOT NULL, CONSTRAINT [PK_Int16-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int32-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[Int64-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedShortKeyEntityPart1-Generator] ([ID] smallint IDENTITY ({seedInt16}, {increaseInt16}) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedShortKeyEntityPart2-Generator] ([ID] smallint IDENTITY ({seedInt16}, {increaseInt16}) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
        }
      }
    }

    protected override string[] GetTouchGeneratorsScripts(bool lessGenerators)
    {
      if (lessGenerators) {
        return new[] {
          $"INSERT INTO [{DefaultDatabase}].[dbo].[Int32-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[Int64-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[Int32-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[Int64-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();"
        };
      }
      else {
        return new[] {
          $"INSERT INTO [{DefaultDatabase}].[dbo].[Int16-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[Int32-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[Int64-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[Int16-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[Int32-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[Int64-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedShortKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedShortKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();"
        };
      }
    }

    protected override string GetDataInsertsScript(bool standardGenerators, bool lessGenerators,
      long seedIncrease, long cacheSizeIncrease)
    {
      int int16Seed = 0, int32Seed = 0, int64Seed = 0;

      if (!standardGenerators) {
        var sameSeed = seedIncrease == 0;
        var sameCacheSize = cacheSizeIncrease == 0;
        if (!sameSeed && !sameCacheSize) {
          throw new NotSupportedException();
        }
        if (!sameSeed && sameCacheSize) {
          int16Seed = 144;
          int32Seed = 160;
          int64Seed = 192;
        }
        else {
          int16Seed = 16;
          int32Seed = 32;
          int64Seed = 64;
        }
      }

      if (lessGenerators) {
        return
          $"INSERT INTO [{DefaultDatabase}].[dbo].[IntKeyEntityPart1] ([Id])        VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[LongKeyEntityPart1] ([Id])       VALUES ({int64Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1] ([Id])  VALUES ({int64Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[IntKeyEntityPart2] ([Id])        VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[LongKeyEntityPart2] ([Id])       VALUES ({int64Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2] ([Id])  VALUES ({int64Seed + 1});";
      }
      else {
        return
          $"INSERT INTO [{DefaultDatabase}].[dbo].[ShortKeyEntityPart1] ([Id])      VALUES ({int16Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[IntKeyEntityPart1] ([Id])        VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[LongKeyEntityPart1] ([Id])       VALUES ({int64Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedShortKeyEntityPart1] ([Id]) VALUES ({int16Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedIntKeyEntityPart1] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{DefaultDatabase}].[dbo].[NamedLongKeyEntityPart1] ([Id])  VALUES ({int64Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[ShortKeyEntityPart2] ([Id])      VALUES ({int16Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[IntKeyEntityPart2] ([Id])        VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[LongKeyEntityPart2] ([Id])       VALUES ({int64Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedShortKeyEntityPart2] ([Id]) VALUES ({int16Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedIntKeyEntityPart2] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [{AlternativeDatabase}].[dbo].[NamedLongKeyEntityPart2] ([Id])  VALUES ({int64Seed + 1});";
      }
    }

    protected override string PopulateSystemTablesScript()
    {
      return
        $"DELETE FROM [{DefaultDatabase}].[dbo].[Metadata.Type];" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (2, N'Xtensive.Orm.Metadata.Assembly' );" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (3, N'Xtensive.Orm.Metadata.Extension' );" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (1, N'Xtensive.Orm.Metadata.Type' );" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (110, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.ShortKeyEntityPart1' );" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (100, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.IntKeyEntityPart1' );" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (102, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.LongKeyEntityPart1');" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (108, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedShortKeyEntityPart1');" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (104, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedIntKeyEntityPart1');" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (106, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedLongKeyEntityPart1');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (111, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.ShortKeyEntityPart2');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (101, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.IntKeyEntityPart2');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (103, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.LongKeyEntityPart2');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (109, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedShortKeyEntityPart2');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (105, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedIntKeyEntityPart2');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Type] ([Id], [Name] ) VALUES (107, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedLongKeyEntityPart2');" +
        $"DELETE FROM [{DefaultDatabase}].[dbo].[Metadata.Assembly];" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Assembly] ([Name], [Version] ) VALUES (N'Xtensive.Orm', N'7.2.0.0');" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Assembly] ([Name], [Version] ) VALUES (N'Xtensive.Orm.Tests', N'7.2.0.0');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Assembly] ([Name], [Version] ) VALUES (N'Xtensive.Orm', N'7.2.0.0');" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Assembly] ([Name], [Version] ) VALUES (N'Xtensive.Orm.Tests', N'7.2.0.0');" +
        $"DELETE FROM [{DefaultDatabase}].[dbo].[Metadata.Extension] WHERE ([Metadata.Extension].[Name] IN (N'Xtensive.Orm.Model', N'Xtensive.Orm.PartialIndexDefinitions' ));" +
        $"INSERT INTO [{DefaultDatabase}].[dbo].[Metadata.Extension] ([Name], [Text])" +
        " VALUES (N'Xtensive.Orm.Model', N'<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<DomainModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <Types>\r\n    <Type>\r\n      <Name>Structure</Name>\r\n      <MappingName>Structure</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Structure</UnderlyingType>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields />\r\n      <Associations />\r\n      <IsStructure>true</IsStructure>\r\n    </Type>\r\n    <Type>\r\n      <Name>Assembly</Name>\r\n      <MappingName>Metadata.Assembly</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Metadata.Assembly</UnderlyingType>\r\n      <TypeId>2</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Name</Name>\r\n          <MappingName>Name</MappingName>\r\n          <PropertyName>Name</PropertyName>\r\n          <OriginalName>Name</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>1024</Length>\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>Version</Name>\r\n          <MappingName>Version</MappingName>\r\n          <PropertyName>Version</PropertyName>\r\n          <OriginalName>Version</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>64</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n      <IsSystem>true</IsSystem>\r\n    </Type>\r\n    <Type>\r\n      <Name>Extension</Name>\r\n      <MappingName>Metadata.Extension</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Metadata.Extension</UnderlyingType>\r\n      <TypeId>3</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Name</Name>\r\n          <MappingName>Name</MappingName>\r\n          <PropertyName>Name</PropertyName>\r\n          <OriginalName>Name</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>1024</Length>\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>Text</Name>\r\n          <MappingName>Text</MappingName>\r\n          <PropertyName>Text</PropertyName>\r\n          <OriginalName>Text</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>2147483647</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n        <Field>\r\n          <Name>Data</Name>\r\n          <MappingName>Data</MappingName>\r\n          <PropertyName>Data</PropertyName>\r\n          <OriginalName>Data</OriginalName>\r\n          <ValueType>System.Byte[]</ValueType>\r\n          <Fields />\r\n          <Length>2147483647</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n      <IsSystem>true</IsSystem>\r\n    </Type>\r\n    <Type>\r\n      <Name>Type</Name>\r\n      <MappingName>Metadata.Type</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Metadata.Type</UnderlyingType>\r\n      <TypeId>1</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>Name</Name>\r\n          <MappingName>Name</MappingName>\r\n          <PropertyName>Name</PropertyName>\r\n          <OriginalName>Name</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>1000</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n      <IsSystem>true</IsSystem>\r\n    </Type>\r\n    <Type>\r\n      <Name>ShortKeyEntityPart1</Name>\r\n      <MappingName>ShortKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.ShortKeyEntityPart1</UnderlyingType>\r\n      <TypeId>105</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>IntKeyEntityPart1</Name>\r\n      <MappingName>IntKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.IntKeyEntityPart1</UnderlyingType>\r\n      <TypeId>100</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>LongKeyEntityPart1</Name>\r\n      <MappingName>LongKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.LongKeyEntityPart1</UnderlyingType>\r\n      <TypeId>101</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedShortKeyEntityPart1</Name>\r\n      <MappingName>NamedShortKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedShortKeyEntityPart1</UnderlyingType>\r\n      <TypeId>104</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedIntKeyEntityPart1</Name>\r\n      <MappingName>NamedIntKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedIntKeyEntityPart1</UnderlyingType>\r\n      <TypeId>102</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedLongKeyEntityPart1</Name>\r\n      <MappingName>NamedLongKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedLongKeyEntityPart1</UnderlyingType>\r\n      <TypeId>103</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n  </Types>\r\n</DomainModel>' );" +
        $"INSERT INTO [{AlternativeDatabase}].[dbo].[Metadata.Extension] ([Name], [Text])" +
        " VALUES (N'Xtensive.Orm.Model', N'<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<DomainModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <Types>\r\n    <Type>\r\n      <Name>ShortKeyEntityPart2</Name>\r\n      <MappingName>ShortKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.ShortKeyEntityPart2</UnderlyingType>\r\n      <TypeId>111</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests-1</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>IntKeyEntityPart2</Name>\r\n      <MappingName>IntKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.IntKeyEntityPart2</UnderlyingType>\r\n      <TypeId>106</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests-1</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>LongKeyEntityPart2</Name>\r\n      <MappingName>LongKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.LongKeyEntityPart2</UnderlyingType>\r\n      <TypeId>107</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests-1</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedShortKeyEntityPart2</Name>\r\n      <MappingName>NamedShortKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedShortKeyEntityPart2</UnderlyingType>\r\n      <TypeId>110</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests-1</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedIntKeyEntityPart2</Name>\r\n      <MappingName>NamedIntKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedIntKeyEntityPart2</UnderlyingType>\r\n      <TypeId>108</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests-1</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedLongKeyEntityPart2</Name>\r\n      <MappingName>NamedLongKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedLongKeyEntityPart2</UnderlyingType>\r\n      <TypeId>109</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <MappingSchema>dbo</MappingSchema>\r\n      <MappingDatabase>DO-Tests-1</MappingDatabase>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n  </Types>\r\n</DomainModel>' );";
    }
  }
}