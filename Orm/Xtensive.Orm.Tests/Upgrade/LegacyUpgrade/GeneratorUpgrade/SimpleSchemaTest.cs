// Copyright (C) 2018-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2018.10.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using refModel = Xtensive.Orm.Tests.Upgrade.LegacyUpgrade.GeneratorUpgrade.ReferenceModel;
using lessGeneratorsModel = Xtensive.Orm.Tests.Upgrade.LegacyUpgrade.GeneratorUpgrade.LessGenerators;

namespace Xtensive.Orm.Tests.Upgrade.LegacyUpgrade.GeneratorUpgrade
{
  [TestFixture]
  public class SimpleSchemaTest
  {
    protected const int BaseInt16Seed = 16;
    protected const int BaseInt32Seed = 32;
    protected const int BaseInt64Seed = 64;

    protected const int BaseInt16CacheSize = BaseInt16Seed;
    protected const int BaseInt32CacheSize = BaseInt32Seed;
    protected const int BaseInt64CacheSize = BaseInt64Seed;

    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.AllFeaturesSupported(ProviderFeatures.Sequences);
    }

    [SetUp]
    public void TestSetup() => DropExistingSequences();

    protected virtual void ApplyCustomConfigurationSettings(DomainConfiguration configuration)
    {
    }

    protected virtual string[] GetUsedCatalogs() => new[] { WellKnownDatabases.MultiDatabase.MainDb };

    [Test]
    public void NoChangesInGeneratorsTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: true);

      using (var domain = BuildUpgradedDomain(DomainUpgradeMode.Perform, refModelTypes)) {
        TriggerGenerators(domain);
        ValidateGeneratorsState(domain);
      }

      using (BuildUpgradedDomain(DomainUpgradeMode.Validate, refModelTypes)) { }
    }

    [Test]
    public void NoChangesInGeneratorsTest2()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: true);

      _ = Assert.Throws<SchemaSynchronizationException>(() => BuildUpgradedDomain(DomainUpgradeMode.PerformSafely, refModelTypes));
    }

    [Test]
    public void LessGeneratorsTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      var lessModelTypes = new[] {
        new TypeRegistration(typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Assembly, typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Assembly, typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(true);

      using (var domain = BuildUpgradedDomain(DomainUpgradeMode.Perform, lessModelTypes)) {
        TriggerGenerators(domain);
        ValidateGeneratorsState(domain);
      }

      using (BuildUpgradedDomain(DomainUpgradeMode.Validate, lessModelTypes)) { }
    }

    [Test]
    public void LessGeneratorsTest2()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      var lessModelTypes = new[] {
        new TypeRegistration(typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Assembly, typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Assembly, typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: true);

      _ = Assert.Throws<SchemaSynchronizationException>(() => BuildUpgradedDomain(DomainUpgradeMode.PerformSafely, lessModelTypes));
    }

    [Test]
    public void MoreGeneratorsTest1()
    {
      var lessModelTypes = new[] {
        new TypeRegistration(typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Assembly, typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Assembly, typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Namespace)
      };

      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: true, lessGenerators:true);

      using (var domain = BuildUpgradedDomain(DomainUpgradeMode.Perform, refModelTypes)) {
        TriggerGenerators(domain);
        ValidateGeneratorsState(domain);
      }
      using (BuildUpgradedDomain(DomainUpgradeMode.Validate, refModelTypes)) { }
    }

    [Test]
    public void MoreGeneratorsTest2()
    {
      var lessModelTypes = new[] {
        new TypeRegistration(typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Assembly, typeof (lessGeneratorsModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Assembly, typeof (lessGeneratorsModel.Part2.IntKeyEntityPart2).Namespace)
      };

      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: true, lessGenerators: true);

      _ = Assert.Throws<SchemaSynchronizationException>(() => BuildUpgradedDomain(DomainUpgradeMode.PerformSafely, refModelTypes));
    }

    [Test]
    public void GeneratorSeedIncreasedTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators: false);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Perform, refModelTypes);
      InitializeGenerators(upgradeConfiguration, 128, 0);

      using (var domain = Domain.Build(upgradeConfiguration)) {
        TriggerGenerators(domain);
        ValidateGeneratorsState(domain);
      }

      upgradeConfiguration = upgradeConfiguration.Clone();
      upgradeConfiguration.UpgradeMode = DomainUpgradeMode.Validate;
      using (Domain.Build(upgradeConfiguration)) { }
    }

    [Test]
    public void GeneratorSeedIncreasedTest2()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators: false);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.PerformSafely, refModelTypes);
      InitializeGenerators(upgradeConfiguration, 128, 0);

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfiguration));
    }

    [Test]
    public void GeneratorCacheSizeIncreasedTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators: false);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Perform, refModelTypes);
      InitializeGenerators(upgradeConfiguration, 0, 128);

      using (var domain = Domain.Build(upgradeConfiguration)) {
        TriggerGenerators(domain);
        ValidateGeneratorsState(domain, 128);
      }

      upgradeConfiguration = upgradeConfiguration.Clone();
      upgradeConfiguration.UpgradeMode = DomainUpgradeMode.Validate;
      using (var domain = Domain.Build(upgradeConfiguration)) { }
    }

    [Test]
    public void GeneratorCacheSizeIncreasedTest2()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators: false);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.PerformSafely, refModelTypes);
      InitializeGenerators(upgradeConfiguration, 0, 128);

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfiguration));
    }

    [Test]
    public void GeneratorSeedDecreasedTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators:false, 128, 0);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Perform, refModelTypes);
      InitializeGenerators(upgradeConfiguration);

      using (var domain = Domain.Build(upgradeConfiguration)) {
        TriggerGenerators(domain);
        ValidateGeneratorsState(domain);
      }

      upgradeConfiguration = upgradeConfiguration.Clone();
      upgradeConfiguration.UpgradeMode = DomainUpgradeMode.Validate;
      using (var domain = Domain.Build(upgradeConfiguration)) { }
    }

    [Test]
    public void GeneratorSeedDecreasedTest2()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators: false, 128, 0);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.PerformSafely, refModelTypes);
      InitializeGenerators(upgradeConfiguration);

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfiguration));
    }

    [Test]
    public void GeneratorCacheSizeDecreasedTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators:false, 0, 128);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Perform, refModelTypes);
      InitializeGenerators(upgradeConfiguration);

      using (var domain = Domain.Build(upgradeConfiguration)) {
        TriggerGenerators(domain);
        ValidateGeneratorsState(domain, -128);
      }

      upgradeConfiguration = upgradeConfiguration.Clone();
      upgradeConfiguration.UpgradeMode = DomainUpgradeMode.Validate;
      using (var domain = Domain.Build(upgradeConfiguration)) {  }
    }

    [Test]
    public void GeneratorCacheSizeDecreasedTest2()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      CreateDb(defaultGeneratorSettings: false, lessGenerators: false, 0, 128);

      var upgradeConfiguration = BuildDomainConfiguration(DomainUpgradeMode.PerformSafely, refModelTypes);
      InitializeGenerators(upgradeConfiguration);

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfiguration));
    }

    private void DropExistingSequences()
    {
      var configuration = BuildDomainConfiguration();

      var sqlDriver = TestSqlDriver.Create(configuration.ConnectionInfo);
      using (var connection = sqlDriver.CreateConnection()) {
        connection.Open();

        var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId) { UpgradeMode = configuration.UpgradeMode };
        var mappingResolver = MappingResolver.Create(configuration, defaultNodeConfiguration, sqlDriver.GetDefaultSchema(connection));
        var extractionResult = sqlDriver.Extract(connection, mappingResolver.GetSchemaTasks());

        var queryBatch = SqlDml.Batch();
        extractionResult.Catalogs
          .Where(c => c.GetDbNameInternal().In(GetUsedCatalogs()))
          .SelectMany(x => x.Schemas)
          .SelectMany(x => x.Sequences)
          .Select(SqlDdl.Drop)
          .ForEach(statement => queryBatch.Add(statement));

        if (queryBatch.Count == 0) {
          return;
        }

        var compiledBatch = sqlDriver.Compile(queryBatch, new SqlCompilerConfiguration { DatabaseQualifiedObjects = true })
          .GetCommandText();

        if (string.IsNullOrWhiteSpace(compiledBatch)) {
          return;
        }

        using (var cmd = connection.CreateCommand(compiledBatch)) {
          _ = cmd.ExecuteNonQuery();
        }
      }
    }

    private void TriggerGenerators(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        domain.Model.Types.Entities
          .Where(e => !e.IsSystem)
          .ForEach(t => Activator.CreateInstance(t.UnderlyingType));
        transaction.Complete();
      }
    }

    private void ValidateGeneratorsState(Domain domain, long incrementDifference = 0)
    {
      using (var session = domain.OpenSession())
      using (session.OpenTransaction()) {
        var entityPairs = session.Domain.Model.Types.Entities
          .Where(ti => !ti.IsSystem)
          .Select(tf => session.Query.All(tf.UnderlyingType))
          .Cast<IQueryable<Entity>>()
          .Select(Enumerable.ToArray)
          .ToArray();

        Assert.That(entityPairs, Is.Not.Empty);

        foreach (var entityPair in entityPairs) {
          var currentIncrement = entityPair[0].TypeInfo.Key.Sequence.Increment;
          if (entityPair.Length == 1) {
            continue;
          }

          var id0 = Convert.ToInt64(entityPair[0]["Id"]);
          var id1 = Convert.ToInt64(entityPair[1]["Id"]);

          Assert.That(id0, Is.GreaterThan(0));
          Assert.That(id1, Is.GreaterThan(0));
          Assert.That(id1, Is.GreaterThan(id0));

          var oldIncrement = currentIncrement - incrementDifference;
          var expectedIncrement = (incrementDifference != 0) ? 2 * oldIncrement : currentIncrement;

          Assert.That(id1 - id0, Is.EqualTo(expectedIncrement));
        }
      }
    }

    private Domain BuildUpgradedDomain(DomainUpgradeMode upgradeMode, TypeRegistration[] typeRegistrations)
    {
      var configuration = BuildDomainConfiguration(upgradeMode, typeRegistrations);
      return Domain.Build(configuration);
    }

    private DomainConfiguration BuildDomainConfiguration(DomainUpgradeMode upgradeMode, TypeRegistration[] registrations)
    {
      var configuration = DomainConfigurationFactory.Create();
      registrations.ForEach(t => configuration.Types.Register(t));
      configuration.UpgradeMode = upgradeMode;
      ApplyCustomConfigurationSettings(configuration);
      return configuration;
    }

    private DomainConfiguration BuildDomainConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      ApplyCustomConfigurationSettings(configuration);
      return configuration;
    }

    protected virtual void InitializeGenerators(DomainConfiguration configuration, long seedIncrease = 0, long cacheSizeIncrease = 0)
    {
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int16") { Seed = BaseInt16Seed + seedIncrease, CacheSize = BaseInt16CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int32") { Seed = BaseInt32Seed + seedIncrease, CacheSize = BaseInt32CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int64") { Seed = BaseInt64Seed + seedIncrease, CacheSize = BaseInt64CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedShortKeyEntityPart1") { Seed = BaseInt16Seed + seedIncrease, CacheSize = BaseInt16CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedIntKeyEntityPart1") { Seed = BaseInt32Seed + seedIncrease, CacheSize = BaseInt32CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedLongKeyEntityPart1") { Seed = BaseInt64Seed + seedIncrease, CacheSize = BaseInt64CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedShortKeyEntityPart2") { Seed = BaseInt16Seed + seedIncrease, CacheSize = BaseInt16CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedIntKeyEntityPart2") { Seed = BaseInt32Seed + seedIncrease, CacheSize = BaseInt32CacheSize + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedLongKeyEntityPart2") { Seed = BaseInt64Seed + seedIncrease, CacheSize = BaseInt64CacheSize + cacheSizeIncrease });
    }

    private void CreateDb(bool defaultGeneratorSettings, bool lessGenerators = false, long seedIncrease = 0, long cacheSizeIncrease = 0)
    {
      var config = BuildDomainConfiguration().Clone();
      config.Types.Register(typeof(CustomUpgrdeHandler));

      // clean db - deletes all the tables except for metadata tables
      // tables from previous tests are unpredictable and this is reliable thought expensive way of cleaning :-)
      using (var domain = Domain.Build(config)) { }

      var driver = TestSqlDriver.Create(config.ConnectionInfo);

      //complete removal
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        try {
          using (var cmd = connection.CreateCommand()) {
            cmd.CommandText = GetSystemItemsCleanup();
            _ = cmd.ExecuteNonQuery();
          }
        }
        finally {
          connection.Close();
        }
      }

      //init db
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        try {
          long int16Seed;
          long int32Seed;
          long int64Seed;
          using (var cmd = connection.CreateCommand()) {
            cmd.CommandText = GetInitDomainStructureCreationScript(defaultGeneratorSettings, lessGenerators, seedIncrease, cacheSizeIncrease);
            _ = cmd.ExecuteNonQuery();
          }

          using (var cmd = connection.CreateCommand()) {
            cmd.CommandText = PopulateSystemTablesScript();
            _ = cmd.ExecuteNonQuery();
          }


          var touchGeneratorScripts = GetTouchGeneratorsScripts(lessGenerators);
          var generatorValues = new long[touchGeneratorScripts.Length];
          for(int i = 0; i < touchGeneratorScripts.Length; i++) {
            var cmdText = touchGeneratorScripts[i];
            using (var cmd = connection.CreateCommand()) {
              cmd.CommandText = cmdText;
              generatorValues[i] = (long)(decimal)cmd.ExecuteScalar();
            }
          }

          using (var cmd = connection.CreateCommand()) {
            cmd.CommandText = GetDataInsertsScript(defaultGeneratorSettings, lessGenerators, seedIncrease, cacheSizeIncrease);
            _ = cmd.ExecuteNonQuery();
          }
        }
        finally {
          connection.Close();
        }
      }
    }

    protected virtual string GetSystemItemsCleanup()
    {
      return "IF object_id('[dbo].[Int32-Generator]') is not null drop sequence [dbo].[Int32-Generator];" +
        "IF object_id('[dbo].[Metadata.Assembly]') is not null drop table [dbo].[Metadata.Assembly];" +
        "IF object_id('[dbo].[Metadata.Extension]') is not null drop table [dbo].[Metadata.Extension];" +
        "IF object_id('[dbo].[Metadata.Type]') is not null drop table [dbo].[Metadata.Type];";
    }

    protected virtual string GetInitDomainStructureCreationScript(bool defaultGeneratorSettings, bool lessGenerators,
      long seedIncrease, long cacheSizeIncrease)
    {
      var sharedPart =
        "CREATE TABLE [dbo].[Metadata.Assembly] ([Name] nvarchar(1024) NOT NULL, [Version] nvarchar(64), CONSTRAINT [PK_Assembly] PRIMARY KEY CLUSTERED ([Name]));" +
        "CREATE TABLE [dbo].[Metadata.Extension] ([Name] nvarchar(1024) NOT NULL, [Text] nvarchar(max), [Data] varbinary(max), CONSTRAINT [PK_Extension] PRIMARY KEY CLUSTERED ([Name]));" +
        "CREATE TABLE [dbo].[Metadata.Type] ([Id] integer NOT NULL, [Name] nvarchar(1000), CONSTRAINT [PK_Type] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE UNIQUE INDEX [Type.IX_Name] ON [dbo].[Metadata.Type] ([Name] ASC);" +
        "CREATE TABLE [dbo].[ShortKeyEntityPart1] ([Id] smallint NOT NULL, CONSTRAINT [PK_ShortKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[IntKeyEntityPart1] ([Id] integer NOT NULL, CONSTRAINT [PK_IntKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id] ) );" +
        "CREATE TABLE [dbo].[LongKeyEntityPart1] ([Id] bigint NOT NULL, CONSTRAINT [PK_LongKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id] ) );" +
        "CREATE TABLE [dbo].[NamedShortKeyEntityPart1] ([Id] smallint NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[NamedIntKeyEntityPart1] ([Id] integer NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[NamedLongKeyEntityPart1] ([Id] bigint NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[ShortKeyEntityPart2] ([Id] smallint NOT NULL, CONSTRAINT [PK_ShortKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[IntKeyEntityPart2] ([Id] integer NOT NULL, CONSTRAINT [PK_IntKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[LongKeyEntityPart2] ([Id] bigint NOT NULL, CONSTRAINT [PK_LongKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[NamedShortKeyEntityPart2] ([Id] smallint NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[NamedIntKeyEntityPart2] ([Id] integer NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));" +
        "CREATE TABLE [dbo].[NamedLongKeyEntityPart2] ([Id] bigint NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2] PRIMARY KEY CLUSTERED ([Id]));";

      if (defaultGeneratorSettings) {
        if (lessGenerators) {
          return
            sharedPart +
            "CREATE TABLE [dbo].[Int32-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            "CREATE TABLE [dbo].[Int64-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            "CREATE TABLE [dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
        }
        else {
          return
            sharedPart +
            "CREATE TABLE [dbo].[Int16-Generator] ([ID] smallint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int16-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            "CREATE TABLE [dbo].[Int32-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            "CREATE TABLE [dbo].[Int64-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID] ) );" +
            "CREATE TABLE [dbo].[NamedShortKeyEntityPart1-Generator] ([ID] smallint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedShortKeyEntityPart2-Generator] ([ID] smallint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            "CREATE TABLE [dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY (128, 128) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
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
            $"CREATE TABLE [dbo].[Int32-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[Int64-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
        }
        else {
          return sharedPart +
            $"CREATE TABLE [dbo].[Int16-Generator] ([ID] smallint IDENTITY ({seedInt16}, {increaseInt16}) NOT NULL, CONSTRAINT [PK_Int16-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[Int32-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_Int32-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[Int64-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_Int64-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedShortKeyEntityPart1-Generator] ([ID] smallint IDENTITY ({seedInt16}, {increaseInt16}) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedIntKeyEntityPart1-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedLongKeyEntityPart1-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart1-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedShortKeyEntityPart2-Generator] ([ID] smallint IDENTITY ({seedInt16}, {increaseInt16}) NOT NULL, CONSTRAINT [PK_NamedShortKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedIntKeyEntityPart2-Generator] ([ID] integer IDENTITY ({seedInt32}, {increaseInt32}) NOT NULL, CONSTRAINT [PK_NamedIntKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));" +
            $"CREATE TABLE [dbo].[NamedLongKeyEntityPart2-Generator] ([ID] bigint IDENTITY ({seedInt64}, {increaseInt64}) NOT NULL, CONSTRAINT [PK_NamedLongKeyEntityPart2-Generator] PRIMARY KEY NONCLUSTERED ([ID]));";
        }
      }
    }

    protected virtual string[] GetTouchGeneratorsScripts(bool lessGenerators)
    {
      if (lessGenerators) {
        return new[] {
          "INSERT INTO [dbo].[Int32-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[Int64-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedIntKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedLongKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedIntKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedLongKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();"
        };
      }
      else {
        return new[] {
          "INSERT INTO [dbo].[Int16-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[Int32-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[Int64-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedShortKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedIntKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedLongKeyEntityPart1-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedShortKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedIntKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();",
          "INSERT INTO [dbo].[NamedLongKeyEntityPart2-Generator] DEFAULT VALUES;SELECT SCOPE_IDENTITY();"
        };
      }
    }

    protected virtual string GetDataInsertsScript(bool standardGenerators, bool lessGenerators,
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
          $"INSERT INTO [dbo].[IntKeyEntityPart1] ([Id])        VALUES ({int32Seed + 1});" +
          $"INSERT INTO [dbo].[LongKeyEntityPart1] ([Id])       VALUES ({int64Seed + 1});" +
          $"INSERT INTO [dbo].[NamedIntKeyEntityPart1] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [dbo].[NamedLongKeyEntityPart1] ([Id])  VALUES ({int64Seed + 1});" +
          $"INSERT INTO [dbo].[IntKeyEntityPart2] ([Id])        VALUES ({int32Seed + 2});" +
          $"INSERT INTO [dbo].[LongKeyEntityPart2] ([Id])       VALUES ({int64Seed + 2});" +
          $"INSERT INTO [dbo].[NamedIntKeyEntityPart2] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [dbo].[NamedLongKeyEntityPart2] ([Id])  VALUES ({int64Seed + 1});";
      }
      else {
        return
          $"INSERT INTO [dbo].[ShortKeyEntityPart1] ([Id])      VALUES ({int16Seed + 1});" +
          $"INSERT INTO [dbo].[IntKeyEntityPart1] ([Id])        VALUES ({int32Seed + 1});" +
          $"INSERT INTO [dbo].[LongKeyEntityPart1] ([Id])       VALUES ({int64Seed + 1});" +
          $"INSERT INTO [dbo].[NamedShortKeyEntityPart1] ([Id]) VALUES ({int16Seed + 1});" +
          $"INSERT INTO [dbo].[NamedIntKeyEntityPart1] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [dbo].[NamedLongKeyEntityPart1] ([Id])  VALUES ({int64Seed + 1});" +
          $"INSERT INTO [dbo].[ShortKeyEntityPart2] ([Id])      VALUES ({int16Seed + 2});" +
          $"INSERT INTO [dbo].[IntKeyEntityPart2] ([Id])        VALUES ({int32Seed + 2});" +
          $"INSERT INTO [dbo].[LongKeyEntityPart2] ([Id])       VALUES ({int64Seed + 2});" +
          $"INSERT INTO [dbo].[NamedShortKeyEntityPart2] ([Id]) VALUES ({int16Seed + 1});" +
          $"INSERT INTO [dbo].[NamedIntKeyEntityPart2] ([Id])   VALUES ({int32Seed + 1});" +
          $"INSERT INTO [dbo].[NamedLongKeyEntityPart2] ([Id])  VALUES ({int64Seed + 1});";
      }
    }

    protected virtual string PopulateSystemTablesScript()
    {
      return "DELETE FROM [dbo].[Metadata.Type];" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (2, N'Xtensive.Orm.Metadata.Assembly' );" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (3, N'Xtensive.Orm.Metadata.Extension' );" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (1, N'Xtensive.Orm.Metadata.Type' );" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (110, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.ShortKeyEntityPart1' );" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (100, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.IntKeyEntityPart1' );" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (102, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.LongKeyEntityPart1');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (108, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedShortKeyEntityPart1');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (104, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedIntKeyEntityPart1');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (106, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedLongKeyEntityPart1');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (111, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.ShortKeyEntityPart2');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (101, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.IntKeyEntityPart2');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (103, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.LongKeyEntityPart2');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (109, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedShortKeyEntityPart2');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (105, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedIntKeyEntityPart2');" +
        "INSERT INTO [dbo].[Metadata.Type] ([Id], [Name] ) VALUES (107, N'Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedLongKeyEntityPart2');" +
        "DELETE FROM [dbo].[Metadata.Assembly];" +
        "INSERT INTO [dbo].[Metadata.Assembly] ([Name], [Version] ) VALUES (N'Xtensive.Orm', N'7.2.0.0');" +
        "INSERT INTO [dbo].[Metadata.Assembly] ([Name], [Version] ) VALUES (N'Xtensive.Orm.Tests', N'7.2.0.0');" +
        "DELETE FROM [dbo].[Metadata.Extension] WHERE ([Metadata.Extension].[Name] IN (N'Xtensive.Orm.Model', N'Xtensive.Orm.PartialIndexDefinitions' ));" +
        "INSERT INTO [dbo].[Metadata.Extension] ([Name], [Text])" +
        " VALUES (N'Xtensive.Orm.Model', N'<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<DomainModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <Types>\r\n    <Type>\r\n      <Name>Structure</Name>\r\n      <MappingName>Structure</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Structure</UnderlyingType>\r\n      <Fields />\r\n      <Associations />\r\n      <IsStructure>true</IsStructure>\r\n    </Type>\r\n    <Type>\r\n      <Name>Assembly</Name>\r\n      <MappingName>Metadata.Assembly</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Metadata.Assembly</UnderlyingType>\r\n      <TypeId>2</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Name</Name>\r\n          <MappingName>Name</MappingName>\r\n          <PropertyName>Name</PropertyName>\r\n          <OriginalName>Name</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>1024</Length>\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>Version</Name>\r\n          <MappingName>Version</MappingName>\r\n          <PropertyName>Version</PropertyName>\r\n          <OriginalName>Version</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>64</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n      <IsSystem>true</IsSystem>\r\n    </Type>\r\n    <Type>\r\n      <Name>Extension</Name>\r\n      <MappingName>Metadata.Extension</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Metadata.Extension</UnderlyingType>\r\n      <TypeId>3</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Name</Name>\r\n          <MappingName>Name</MappingName>\r\n          <PropertyName>Name</PropertyName>\r\n          <OriginalName>Name</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>1024</Length>\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>Text</Name>\r\n          <MappingName>Text</MappingName>\r\n          <PropertyName>Text</PropertyName>\r\n          <OriginalName>Text</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>2147483647</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n        <Field>\r\n          <Name>Data</Name>\r\n          <MappingName>Data</MappingName>\r\n          <PropertyName>Data</PropertyName>\r\n          <OriginalName>Data</OriginalName>\r\n          <ValueType>System.Byte[]</ValueType>\r\n          <Fields />\r\n          <Length>2147483647</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n      <IsSystem>true</IsSystem>\r\n    </Type>\r\n    <Type>\r\n      <Name>Type</Name>\r\n      <MappingName>Metadata.Type</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Metadata.Type</UnderlyingType>\r\n      <TypeId>1</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>Name</Name>\r\n          <MappingName>Name</MappingName>\r\n          <PropertyName>Name</PropertyName>\r\n          <OriginalName>Name</OriginalName>\r\n          <ValueType>System.String</ValueType>\r\n          <Fields />\r\n          <Length>1000</Length>\r\n          <IsPrimitive>true</IsPrimitive>\r\n          <IsNullable>true</IsNullable>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n      <IsSystem>true</IsSystem>\r\n    </Type>\r\n    <Type>\r\n      <Name>ShortKeyEntityPart1</Name>\r\n      <MappingName>ShortKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.ShortKeyEntityPart1</UnderlyingType>\r\n      <TypeId>110</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>IntKeyEntityPart1</Name>\r\n      <MappingName>IntKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.IntKeyEntityPart1</UnderlyingType>\r\n      <TypeId>100</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>LongKeyEntityPart1</Name>\r\n      <MappingName>LongKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.LongKeyEntityPart1</UnderlyingType>\r\n      <TypeId>102</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedShortKeyEntityPart1</Name>\r\n      <MappingName>NamedShortKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedShortKeyEntityPart1</UnderlyingType>\r\n      <TypeId>108</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedIntKeyEntityPart1</Name>\r\n      <MappingName>NamedIntKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedIntKeyEntityPart1</UnderlyingType>\r\n      <TypeId>104</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedLongKeyEntityPart1</Name>\r\n      <MappingName>NamedLongKeyEntityPart1</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1.NamedLongKeyEntityPart1</UnderlyingType>\r\n      <TypeId>106</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>ShortKeyEntityPart2</Name>\r\n      <MappingName>ShortKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.ShortKeyEntityPart2</UnderlyingType>\r\n      <TypeId>111</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>IntKeyEntityPart2</Name>\r\n      <MappingName>IntKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.IntKeyEntityPart2</UnderlyingType>\r\n      <TypeId>101</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>LongKeyEntityPart2</Name>\r\n      <MappingName>LongKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.LongKeyEntityPart2</UnderlyingType>\r\n      <TypeId>103</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedShortKeyEntityPart2</Name>\r\n      <MappingName>NamedShortKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedShortKeyEntityPart2</UnderlyingType>\r\n      <TypeId>109</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int16</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedIntKeyEntityPart2</Name>\r\n      <MappingName>NamedIntKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedIntKeyEntityPart2</UnderlyingType>\r\n      <TypeId>105</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n    <Type>\r\n      <Name>NamedLongKeyEntityPart2</Name>\r\n      <MappingName>NamedLongKeyEntityPart2</MappingName>\r\n      <UnderlyingType>Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2.NamedLongKeyEntityPart2</UnderlyingType>\r\n      <TypeId>107</TypeId>\r\n      <HierarchyRoot>ConcreteTable</HierarchyRoot>\r\n      <Fields>\r\n        <Field>\r\n          <Name>Id</Name>\r\n          <MappingName>Id</MappingName>\r\n          <PropertyName>Id</PropertyName>\r\n          <OriginalName>Id</OriginalName>\r\n          <ValueType>System.Int64</ValueType>\r\n          <Fields />\r\n          <IsPrimaryKey>true</IsPrimaryKey>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n        <Field>\r\n          <Name>TypeId</Name>\r\n          <MappingName>TypeId</MappingName>\r\n          <PropertyName>TypeId</PropertyName>\r\n          <OriginalName>TypeId</OriginalName>\r\n          <ValueType>System.Int32</ValueType>\r\n          <Fields />\r\n          <IsSystem>true</IsSystem>\r\n          <IsTypeId>true</IsTypeId>\r\n          <IsPrimitive>true</IsPrimitive>\r\n        </Field>\r\n      </Fields>\r\n      <Associations />\r\n      <IsEntity>true</IsEntity>\r\n    </Type>\r\n  </Types>\r\n</DomainModel>' );";
    }
  }
}