// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
using refModel = Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel;
using lessGeneratorsModel = Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.LessGenerators;

namespace Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade
{
  [TestFixture]
  public class SimpleSchemaTest
  {
    private const string TableGeneratorsVersion = "9.0.0.0";

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

    protected virtual string[] GetUsedCatalogs() => new[] { "DO-Tests" };

    [Test]
    public void NoChangesInGeneratorsTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      using (var domain = BuildInitialDomain(refModelTypes)) {
        TriggerGenerators(domain);
      }

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

      using (var domain = BuildInitialDomain(refModelTypes)) {
        TriggerGenerators(domain);
      }

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

      using (var domain = BuildInitialDomain(refModelTypes)) {
        TriggerGenerators(domain);
      }

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

      using (var domain = BuildInitialDomain(refModelTypes)) {
        TriggerGenerators(domain);
      }

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

      using (var domain = BuildInitialDomain(lessModelTypes)) {
        TriggerGenerators(domain);
      }

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

      using (var domain = BuildInitialDomain(lessModelTypes)) {
        TriggerGenerators(domain);
      }

      _ = Assert.Throws<SchemaSynchronizationException>(() => BuildUpgradedDomain(DomainUpgradeMode.PerformSafely, refModelTypes));
    }

    [Test]
    public void GeneratorSeedIncreasedTest1()
    {
      var refModelTypes = new[] {
        new TypeRegistration(typeof (refModel.Part1.IntKeyEntityPart1).Assembly, typeof (refModel.Part1.IntKeyEntityPart1).Namespace),
        new TypeRegistration(typeof (refModel.Part2.IntKeyEntityPart2).Assembly, typeof (refModel.Part2.IntKeyEntityPart2).Namespace)
      };

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration, 128, 0);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration, 128, 0);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration, 0, 128);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

      var initConfiguration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, refModelTypes);
      initConfiguration.ForcedServerVersion = TableGeneratorsVersion;
      InitializeGenerators(initConfiguration, 0, 128);

      using (var domain = Domain.Build(initConfiguration)) {
        TriggerGenerators(domain);
      }

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

    private Domain BuildInitialDomain(TypeRegistration[] typeRegistrations)
    {
      var configuration = BuildDomainConfiguration(DomainUpgradeMode.Recreate, typeRegistrations);
      configuration.ForcedServerVersion = TableGeneratorsVersion;
      return Domain.Build(configuration);
    }

    private Domain BuildUpgradedDomain(DomainUpgradeMode upgradeMode, TypeRegistration[] typeRegistrations)
    {
      var configuration = BuildDomainConfiguration(upgradeMode, typeRegistrations);
      return Domain.Build(configuration);
    }

    private DomainConfiguration BuildDomainConfiguration(DomainUpgradeMode upgradeMode, TypeRegistration[] registrations)
    {
      var configuration = BuildDomainConfiguration();
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

    private void InitializeGenerators(DomainConfiguration configuration, long seedIncrease = 0, long cacheSizeIncrease = 0)
    {
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int16") { Seed = 16 + seedIncrease, CacheSize = 16 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int32") { Seed = 32 + seedIncrease, CacheSize = 32 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("Int64") { Seed = 64 + seedIncrease, CacheSize = 64 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedShortKeyEntityPart1") { Seed = 16 + seedIncrease, CacheSize = 16 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedIntKeyEntityPart1") { Seed = 32 + seedIncrease, CacheSize = 32 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedLongKeyEntityPart1") { Seed = 64 + seedIncrease, CacheSize = 64 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedShortKeyEntityPart2") { Seed = 16 + seedIncrease, CacheSize = 16 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedIntKeyEntityPart2") { Seed = 32 + seedIncrease, CacheSize = 32 + cacheSizeIncrease });
      configuration.KeyGenerators.Add(new KeyGeneratorConfiguration("NamedLongKeyEntityPart2") { Seed = 64 + seedIncrease, CacheSize = 64 + cacheSizeIncrease });
    }
  }
}