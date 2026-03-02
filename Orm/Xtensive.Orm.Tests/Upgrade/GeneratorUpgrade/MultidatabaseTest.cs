// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.10

using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade
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
        .Where(t => t.Namespace.Contains("Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade"))
        .GroupBy(t => t.Namespace)
        .Select(g => g.Key)
        .ToArray();
      if (namespaces.Length == 0) {
        configuration.MappingRules.Map("Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part1").ToDatabase(DefaultDatabase);
        configuration.MappingRules.Map("Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade.ReferenceModel.Part2").ToDatabase(AlternativeDatabase);
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
  }
}