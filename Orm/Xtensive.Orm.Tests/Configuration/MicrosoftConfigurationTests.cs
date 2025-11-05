// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Transactions;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Configuration.TypesToUseInTests
{
  namespace NestedNamespace
  {
    [HierarchyRoot]
    public class DummyNestedEntity1 : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    public class DummyNestedEntity2 : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  [HierarchyRoot]
  public class DummyEntity1 : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class DummyEntity2 : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class DummyEntity3 : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Configuration
{
  [TestFixture]
  public sealed class AppConfigStyleConfigurationTest : MicrosoftConfigurationTestBase
  {
    protected override string Postfix => "AppConfig";

    protected override bool NameAttributeUnique => false;

    protected override void RegisterConfigurationFile(ConfigurationBuilder builder)
    {
      _ = builder.AddXmlFile("domainSettings.config");
    }
  }

  [TestFixture]
  public sealed class XmlConfigurationTest : MicrosoftConfigurationTestBase
  {
    protected override string Postfix => "Xml";

    protected override void RegisterConfigurationFile(ConfigurationBuilder builder)
    {
      _ = builder.AddXmlFile("domainSettings.config");
    }
  }

  [TestFixture]
  public sealed class JsonConfigurationTest : MicrosoftConfigurationTestBase
  {
    protected override string Postfix => "Json";

    protected override void RegisterConfigurationFile(ConfigurationBuilder builder)
    {
      _ = builder.AddJsonFile("domainSettings.json");
    }
  }

  public abstract class MicrosoftConfigurationTestBase
  {
    private IConfigurationRoot configuration;
    private IConfigurationSection configurationSection;

    protected abstract string Postfix { get; }
    protected virtual bool NameAttributeUnique => true;

    protected virtual string DefaultSectionName => $"Xtensive.Orm.{Postfix}";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      var configurationBuilder = new ConfigurationBuilder();
      _ = configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());

      RegisterConfigurationFile(configurationBuilder);
      configuration = configurationBuilder.Build();

      configurationSection = configuration.GetSection(DefaultSectionName);
      if (!configurationSection.GetChildren().Any()) {
        throw new InconclusiveException($"{DefaultSectionName} section seems to be empty. Check registered file with domain settings");
      }
    }

    protected abstract void RegisterConfigurationFile(ConfigurationBuilder builder);

    private DomainConfiguration LoadDomainConfiguration(string domainName, bool useRoot)
    {
      return useRoot
        ? DomainConfiguration.Load(configuration, DefaultSectionName, domainName)
        : DomainConfiguration.Load(configurationSection, domainName);
    }
    private LoggingConfiguration LoadLoggingConfiguration(IConfigurationSection customConfigurationSection = null)
    {
      var loggingConfiguration = LoggingConfiguration.Load(customConfigurationSection ?? configurationSection);
      return loggingConfiguration;
    }


    #region Simple Domain settings that used to be attributes

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ProviderAndConnectionStringTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithProviderAndConnectionString", useRoot);
      Assert.That(domainConfig.ConnectionInfo.Provider, Is.EqualTo(WellKnown.Provider.Sqlite));
      Assert.That(domainConfig.ConnectionInfo.ConnectionString, Is.EqualTo("Data Source=DO-Testsaaa.db3"));
      Assert.That(domainConfig.ConnectionInfo.ConnectionUrl, Is.Null);

      ValidateAllDefault(domainConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ConnectionUrlTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithConnectionUrl", useRoot);
      Assert.That(domainConfig.ConnectionInfo.Provider, Is.EqualTo(WellKnown.Provider.Sqlite));
      Assert.That(domainConfig.ConnectionInfo.ConnectionString, Is.Null);
      Assert.That(domainConfig.ConnectionInfo.ConnectionUrl.Url, Is.EqualTo("sqlite:///DO-Tests.db3"));

      ValidateAllDefault(domainConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomValidKeyCacheSizeTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidKeyCacheSize", useRoot);

      ValidateAllDefaultExcept(domainConfig, ((d) => d.KeyCacheSize, 192));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomInvalidKeyCacheSizeTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidKeyCacheSize", useRoot));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomValidKeyGeneratorCacheSizeTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidKeyGeneratorCacheSize", useRoot);

      ValidateAllDefaultExcept(domainConfig, ((d) => d.KeyGeneratorCacheSize, 192));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomInvalidKeyGeneratorCacheSizeTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidKeyGeneratorCacheSize", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomValidQueryCacheSizeTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidQueryCacheSize", useRoot);

      ValidateAllDefaultExcept(domainConfig, ((d) => d.QueryCacheSize, 192));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomInvalidQueryCacheSizeTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidQueryCacheSize", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomValidRecordSetMappingCacheSizeTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidRecordSetMappingCacheSize", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.RecordSetMappingCacheSize, 192));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomInvalidRecordSetMappingCacheSizeTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidRecordSetMappingCacheSize", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomDefaultDatabaseTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomDatabase", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "MyFancyDatabase"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true)
        );
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CustomDefaultSchemaTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomSchema", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultSchema, "MyFancySchema"),
        ((d) => d.IsMultidatabase, false),
        ((d) => d.IsMultischema, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void UpgradeModesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode1", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Default));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode2", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Recreate));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode3", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Perform));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode4", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.PerformSafely));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode5", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Validate));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode6", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.LegacyValidate));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode7", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Skip));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode8", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.LegacySkip));
      domainConfig.Lock();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void WrongUpgradeModeTest(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithWrongUpgradeMode", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ForeighKeyModesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode1", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.None));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode2", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Hierarchy));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode3", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Reference));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode4", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.All));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode5", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Default));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode6", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Hierarchy | ForeignKeyMode.Reference));
      domainConfig.Lock();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidForeighKeyModeTest(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidForeignKeyMode", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ChangeTrackingModesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode1", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Off));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode2", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Auto));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode3", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Manual));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode4", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.OffWithNoPopulation));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode5", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Default));
      domainConfig.Lock();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidChangeTrackingModeTest(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidChangeTrackingMode", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DomainOptionsTest1(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDomainOptionsValid1", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.Options, DomainOptions.Default));
      domainConfig.Lock();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DomainOptionsTest2(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDomainOptionsValid2", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.Options, DomainOptions.None));
      domainConfig.Lock();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidDomainOptionsTest(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithDomainOptionsInvalid", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void CollationTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithColation", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.Collation, "generalci"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void BriefSchemaSyncExceptionsTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithBriefSchemaSyncExceptions", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.SchemaSyncExceptionFormat, SchemaSyncExceptionFormat.Brief));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DetailedSchemaSyncExceptionsTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDetailedSchemaSyncExceptions", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.SchemaSyncExceptionFormat, SchemaSyncExceptionFormat.Detailed));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DefaultSchemaSyncExceptionsTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDefaultSchemaSyncExceptions", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.SchemaSyncExceptionFormat, SchemaSyncExceptionFormat.Default));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidSchemaSyncExceptionsTest(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidSchemaSyncExceptions", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TagsLocationNowhereTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationNowhere", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.Nowhere));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TagsLocationBeforeTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationBefore", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.BeforeStatement));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TagsLocationWithinTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationWithin", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.WithinStatement));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TagsLocationAfterTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationAfter", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.AfterStatement));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TagsLocationDefaultTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationDefault", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.Default));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTagsLocationTest(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithTagsLocationInvalid", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ForcedServerVersionTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithForcedServerVersion", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ForcedServerVersion, "10.0.0.0"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InitializationSqlTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithInitSql", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ConnectionInitializationSql, "use [OtherDb]"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void IncludeSqlInExceptionsTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("IncludeSqlInExceptionsTrue", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.IncludeSqlInExceptions, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DontIncludeSqlInExceptionsTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("IncludeSqlInExceptionsFalse", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.IncludeSqlInExceptions, false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AllowCyclicDatabaseDependanciesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("AllowCyclicDatabaseDependenciesTrue", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.AllowCyclicDatabaseDependencies, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DisallowCyclicDatabaseDependanciesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("AllowCyclicDatabaseDependenciesFalse", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.AllowCyclicDatabaseDependencies, false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void BuildInParallelTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("BuildInParallelTrue", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.BuildInParallel, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DontBuildInParallelTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("BuildInParallelFalse", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.BuildInParallel, false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AllowMultidatabaseKeysTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("MultidatabaseKeysTrue", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.MultidatabaseKeys, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DisallowMultidatabaseKeysTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("MultidatabaseKeysFalse", useRoot);
      ValidateAllDefaultExcept(domainConfig, ((d) => d.MultidatabaseKeys, false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ShareStorageSchemaOverNodesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("SharedStorageSchemaOn", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ShareStorageSchemaOverNodes, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DontShareStorageSchemaOverNodesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("SharedStorageSchemaOff", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ShareStorageSchemaOverNodes, false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EnsureConnectionIsAliveTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("EnableConnectionIsAliveTrue", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.EnsureConnectionIsAlive, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DontCheckConnectionIsAliveTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("EnableConnectionIsAliveFalse", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.EnsureConnectionIsAlive, false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void PreferTypeIdAsQueryParameterTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("PreferTypeIdsAsQueryParametersTrue", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.PreferTypeIdsAsQueryParameters, true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DontPreferTypeIdAsQueryParameterTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("PreferTypeIdsAsQueryParametersFalse", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.PreferTypeIdsAsQueryParameters, false));
    }
    #endregion

    #region NamingConvention

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest01(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention1", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Synonymize));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));

      var synonyms = namingConvention.NamespaceSynonyms;
      Assert.That(synonyms.Count, Is.EqualTo(2));
      Assert.That(synonyms["Xtensive.Orm"], Is.EqualTo("system"));
      Assert.That(synonyms["Xtensive.Orm.Tests"], Is.EqualTo("theRest"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest02(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention2", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Lowercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Synonymize));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));

      var synonyms = namingConvention.NamespaceSynonyms;
      Assert.That(synonyms.Count, Is.EqualTo(2));
      Assert.That(synonyms["Xtensive.Orm"], Is.EqualTo("system"));
      Assert.That(synonyms["Xtensive.Orm.Tests"], Is.EqualTo("theRest"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest03(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention3", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.AsIs));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Synonymize));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));

      var synonyms = namingConvention.NamespaceSynonyms;
      Assert.That(synonyms.Count, Is.EqualTo(2));
      Assert.That(synonyms["Xtensive.Orm"], Is.EqualTo("system"));
      Assert.That(synonyms["Xtensive.Orm.Tests"], Is.EqualTo("theRest"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest04(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention4", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Default));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Synonymize));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));

      var synonyms = namingConvention.NamespaceSynonyms;
      Assert.That(synonyms.Count, Is.EqualTo(2));
      Assert.That(synonyms["Xtensive.Orm"], Is.EqualTo("system"));
      Assert.That(synonyms["Xtensive.Orm.Tests"], Is.EqualTo("theRest"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest05(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention5", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.AsIs));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest06(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention6", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest07(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention7", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Omit));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest08(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention8", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Default));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest09(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention9", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreDots | NamingRules.RemoveHyphens));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest10(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention10", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.None));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionSettingsTest11(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention11", useRoot);
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.Default));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionInvalidSettingsTest1(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException> (() => LoadDomainConfiguration("DomainWithInvalidNamingConvention1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionInvalidSettingsTest2(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidNamingConvention2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingConventionInvalidSettingsTest3(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidNamingConvention3", useRoot));
    }

    #endregion

    #region VersioningConvention

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void VersioningConventionPessimisticTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention1", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Pessimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void VersioningConventionOptimisticTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention2", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Optimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void VersioningConventionDefaultTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention3", useRoot);
      ValidateAllDefault(domainConfig);

      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Default));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void VersioningConventionDenyEntitySetChangeVersionTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention4", useRoot);
      ValidateAllDefault(domainConfig);

      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Optimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(true));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void VersioningConventionAllowEntitySetChangeVersionTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention5", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Optimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void VersioningConventionInvalidTest(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidVersioningConvention1", useRoot));
    }

    #endregion

    #region Types registration

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TypesRegistrationAsTypesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTypes", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity2)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity3)), Is.False);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TypesRegistrationAsAssembliesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithAssemblies", useRoot);
      ValidateAllDefault(domainConfig);
      var ormAssembly = typeof(DomainConfiguration).Assembly;
      Assert.That(domainConfig.Types.Count, Is.GreaterThan(0));
      Assert.That(domainConfig.Types.All((t) => t.Assembly == ormAssembly), Is.True);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TypesRegistrationAsAssembliesWithNamespace(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithAssembliesAndNamespaces", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity2)), Is.True);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MixedTypeRegistration(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMixedRegistrations", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity2)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity3)), Is.False);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity2)), Is.True);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTypeRegistration1(bool useRoot)
    {
      // same type twice

      var domainConfig = LoadDomainConfiguration("DomainWithInvalidRegistrations1", useRoot);

      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity2)), Is.False);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity3)), Is.False);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTypeRegistration2(bool useRoot)
    {
      // same Assembly
      var domainConfig = LoadDomainConfiguration("DomainWithInvalidRegistrations2", useRoot);

      ValidateAllDefault(domainConfig);
      var ormAssembly = typeof(DomainConfiguration).Assembly;
      Assert.That(domainConfig.Types.Count, Is.GreaterThan(0));
      Assert.That(domainConfig.Types.All((t) => t.Assembly == ormAssembly), Is.True);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTypeRegistration3(bool useRoot)
    {
      // same assembly and namespace
      var domainConfig = LoadDomainConfiguration("DomainWithInvalidRegistrations3", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity2)), Is.True);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTypeRegistration4(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations4", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTypeRegistration5(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations5", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTypeRegistration6(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations6", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void InvalidTypeRegistration7(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations7", useRoot));
    }

    #endregion

    #region Sessions

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MultipleSessionsTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMultipleSessionConfigurations", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(2));
      var session = sessions[0];
      Assert.That(session.Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(session,
        ((s) => s.CacheType, SessionCacheType.Infinite),
        ((s) => s.BatchSize, 20),
        ((s) => s.EntityChangeRegistrySize, 255),
        ((s) => s.Options, SessionOptions.AllowSwitching | SessionOptions.AutoActivation | SessionOptions.ReadRemovedObjects | SessionOptions.ValidateEntityVersions));

      session = sessions[1];
      Assert.That(session.Name, Is.EqualTo(WellKnown.Sessions.System));
      ValidateAllDefaultExcept(session,
        ((s) => s.CacheType, SessionCacheType.Infinite),
        ((s) => s.BatchSize, 30),
        ((s) => s.Options, SessionOptions.ServerProfile));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithEmptyNameTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionEmptyName", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.ClientProfile));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithCustomNameTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomName", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.Not.EqualTo(WellKnown.Sessions.Default));
      Assert.That(sessions[0].Name, Is.EqualTo("UserCreated"));

      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.ClientProfile));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithCustomUserNameTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomUser", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.UserName, "User"),
        ((s) => s.Password, "126654"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithOptionsTest1(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionDefaultOptions", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.Default));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithOptionsTest2(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionServerProfile", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
         ((s) => s.Options, SessionOptions.ServerProfile));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithOptionsTest3(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionClientProfile", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.ClientProfile));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithOptionsTest4(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomOptions", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.AllowSwitching | SessionOptions.AutoActivation | SessionOptions.ReadRemovedObjects | SessionOptions.ValidateEntityVersions));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithCollectionSizesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionWithCollectionSizes", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.CacheSize, 399),
        ((s) => s.BatchSize, 20),
        ((s) => s.EntityChangeRegistrySize, 255));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionCustomCacheTypeTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomCacheType", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.CacheType, SessionCacheType.Infinite));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionCustomIsolationLevelTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomIsolationLevel", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.DefaultIsolationLevel, IsolationLevel.ReadCommitted));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionCustomCommandTimeoutTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomCommandTimeout", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.DefaultCommandTimeout, (int?)300));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionCustomPreloadingPolicyTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomPreloadingPolicy", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.ReaderPreloading, ReaderPreloadingPolicy.Always));

    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionCustomConnectionStringTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomConnectionString", useRoot);
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.ConnectionInfo, new ConnectionInfo("_dummy_", "Data Source=localhost;Initial Catalog=DO-Tests;Integrated Security=True;MultipleActiveResultSets=True")));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionCustomConnectionUrlTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomConnectionUrl", useRoot);
      ValidateAllDefault(domainConfig);
      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.ConnectionInfo, new ConnectionInfo("sqlserver://localhost/DO-Tests")));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidOptions(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithSessionInvalidOptions", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidCacheSizeTest1(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheSize1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidCacheSizeTest2(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheSize2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidCacheSizeTest3(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheSize3", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidBatchSizeTest1(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidBatchSize1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidBatchSizeTest2(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidBatchSize2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidEntityChangeRegistryTest1(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidEntityChangeRegistry1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidEntityChangeRegistryTest2(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidEntityChangeRegistry2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SessionWithInvalidCacheType1(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheType", useRoot));
    }

    #endregion

    #region Databases configuration

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DatabaseConfigurationOnlyAliasTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDatabases1", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Databases.Count, Is.EqualTo(2));
      var db1 = domainConfig.Databases[0];
      Assert.That(db1.Name, Is.EqualTo("main"));
      Assert.That(db1.RealName, Is.EqualTo("DO-Tests-1"));
      Assert.That(db1.MinTypeId, Is.EqualTo(Orm.Model.TypeInfo.MinTypeId));
      Assert.That(db1.MaxTypeId, Is.EqualTo(int.MaxValue));
      var db2 = domainConfig.Databases[1];
      Assert.That(db2.Name, Is.EqualTo("other"));
      Assert.That(db2.RealName, Is.EqualTo("DO-Tests-2"));
      Assert.That(db2.MinTypeId, Is.EqualTo(Orm.Model.TypeInfo.MinTypeId));
      Assert.That(db2.MaxTypeId, Is.EqualTo(int.MaxValue));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DatabaseConfigurationWithTypeIdsTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDatabases2", useRoot);
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Databases.Count, Is.EqualTo(2));
      var db1 = domainConfig.Databases[0];
      Assert.That(db1.Name, Is.EqualTo("main"));
      Assert.That(db1.RealName, Is.EqualTo("DO-Tests-1"));
      Assert.That(db1.MinTypeId, Is.EqualTo(100));
      Assert.That(db1.MaxTypeId, Is.EqualTo(1000));
      var db2 = domainConfig.Databases[1];
      Assert.That(db2.Name, Is.EqualTo("other"));
      Assert.That(db2.RealName, Is.EqualTo("DO-Tests-2"));
      Assert.That(db2.MinTypeId, Is.EqualTo(2000));
      Assert.That(db2.MaxTypeId, Is.EqualTo(3000));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DatabaseConfigurationNegativeMinTypeIdTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DatabaseConfigurationInvalidMinTypeIdTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DatabaseConfigurationNegativeMaxTypeIdTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases3", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DatabaseConfigurationInvalidMaxTypeIdTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases4", useRoot));
    }

    #endregion

    #region KeyGenerators

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SimpleKeyGeneratorTest(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGenerator", useRoot);
      ValidateAllDefault(domainConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeyGeneratorWithDatabaseNamesTest(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGeneratorsWithDatabaseNames", useRoot);
      ValidateAllDefault(domainConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeyGeneratorWithDatabaseNamesAllParamsTest(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGeneratorsWithDatabaseNamesAndKeyParams", useRoot);
      ValidateAllDefault(domainConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeyGeneratorsWithDatabaseAliasesTest(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGeneratorsWithDatabasesAliases", useRoot);
      ValidateAllDefault(domainConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeyGeneratorsWithConflictByDatabaseTest1(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorsConflict1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeyGeneratorsWithConflictByDatabaseTest2(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorsConflict2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeyGeneratorWithNegativeSeedTest(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorNegativeSeed", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeyGeneratorWithNegativeCacheSizeTest(bool useRoot)
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorNegativeCache", useRoot));
    }

    #endregion

    #region IgnoreRules

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void IgnoreRulesTest(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithIgnoreRules", useRoot);
      ValidateAllDefault(domainConfig);
      ValidateIgnoreRules(domainConfig.IgnoreRules);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void IgnoreColumnAndIndexAtTheSameTimeTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void IgnoreTableAndColumnAndIndexAtTheSameTimeTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void IgnoreDatabaseOnlyTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules3", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void IgnoreDatabaseAndSchemaOnlyTest(bool useRoot)
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules4", useRoot));
    }

    private void ValidateIgnoreRules(IgnoreRuleCollection rules)
    {
      Assert.That(rules.Count, Is.EqualTo(18));

      var rule = rules[0];
      Assert.That(rule.Database, Is.EqualTo("Other-DO40-Tests"));
      Assert.That(rule.Schema, Is.EqualTo("some-schema1"));
      Assert.That(rule.Table, Is.EqualTo("table1"));
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[1];
      Assert.That(rule.Database, Is.EqualTo("some-database"));
      Assert.That(rule.Schema, Is.EqualTo("some-schema2"));
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.EqualTo("column2"));
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[2];
      Assert.That(rule.Database, Is.EqualTo("some-database"));
      Assert.That(rule.Schema, Is.EqualTo("some-schema2"));
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.EqualTo("index2"));

      rule = rules[3];
      Assert.That(rule.Database, Is.EqualTo("some-database"));
      Assert.That(rule.Schema, Is.EqualTo("some-schema3"));
      Assert.That(rule.Table, Is.EqualTo("table2"));
      Assert.That(rule.Column, Is.EqualTo("col3"));
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[4];
      Assert.That(rule.Database, Is.EqualTo("some-database"));
      Assert.That(rule.Schema, Is.EqualTo("some-schema3"));
      Assert.That(rule.Table, Is.EqualTo("table2"));
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.EqualTo("index3"));

      rule = rules[5];
      Assert.That(rule.Database, Is.EqualTo("another-some-database"));
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.EqualTo("some-table"));
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[6];
      Assert.That(rule.Database, Is.EqualTo("database1"));
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.EqualTo("some-column"));
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[7];
      Assert.That(rule.Database, Is.EqualTo("database1"));
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.EqualTo("some-index"));

      rule = rules[8];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.EqualTo("schema1"));
      Assert.That(rule.Table, Is.EqualTo("table1"));
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[9];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.EqualTo("schema1"));
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.EqualTo("column2"));
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[10];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.EqualTo("schema1"));
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.EqualTo("index2"));

      rule = rules[11];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.EqualTo("schema1"));
      Assert.That(rule.Table, Is.EqualTo("table2"));
      Assert.That(rule.Column, Is.EqualTo("column3"));
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[12];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.EqualTo("schema1"));
      Assert.That(rule.Table, Is.EqualTo("table2"));
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.EqualTo("index3"));

      rule = rules[13];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.EqualTo("table4"));
      Assert.That(rule.Column, Is.EqualTo("column3"));
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[14];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.EqualTo("table4"));
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.EqualTo("index2"));

      rule = rules[15];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.EqualTo("single-table"));
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[16];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.EqualTo("single-column"));
      Assert.That(rule.Index, Is.Null.Or.Empty);

      rule = rules[17];
      Assert.That(rule.Database, Is.Null.Or.Empty);
      Assert.That(rule.Schema, Is.Null.Or.Empty);
      Assert.That(rule.Table, Is.Null.Or.Empty);
      Assert.That(rule.Column, Is.Null.Or.Empty);
      Assert.That(rule.Index, Is.EqualTo("single-index"));
    }

    #endregion

    #region MappingRules

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesTest1(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules1", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "main"),
        ((d) => d.DefaultSchema, "dbo"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true));
      var rules = domainConfig.MappingRules;

      Assert.That(rules.Count, Is.EqualTo(1));
      var rule = rules[0];
      Assert.That(rule.Assembly, Is.EqualTo(typeof(JsonConfigurationTest).Assembly));
      Assert.That(rule.Namespace, Is.Null);
      Assert.That(rule.Database, Is.EqualTo("DO-Tests-1"));
      Assert.That(rule.Schema, Is.Null);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesTest2(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules2", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "main"),
        ((d) => d.DefaultSchema, "dbo"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true));
      var rules = domainConfig.MappingRules;

      Assert.That(rules.Count, Is.EqualTo(1));
      var rule = rules[0];
      Assert.That(rule.Assembly, Is.EqualTo(typeof(JsonConfigurationTest).Assembly));
      Assert.That(rule.Namespace, Is.Null);
      Assert.That(rule.Database, Is.Null);
      Assert.That(rule.Schema, Is.EqualTo("Model1"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesTest3(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules3", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "main"),
        ((d) => d.DefaultSchema, "dbo"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true));
      var rules = domainConfig.MappingRules;

      Assert.That(rules.Count, Is.EqualTo(1));
      var rule = rules[0];

      Assert.That(rule.Assembly, Is.Null);
      Assert.That(rule.Namespace, Is.EqualTo("Xtensive.Orm.Configuration.Options"));
      Assert.That(rule.Database, Is.EqualTo("DO-Tests-2"));
      Assert.That(rule.Schema, Is.Null);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesTest4(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules4", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "main"),
        ((d) => d.DefaultSchema, "dbo"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true));
      var rules = domainConfig.MappingRules;

      Assert.That(rules.Count, Is.EqualTo(1));
      var rule = rules[0];

      Assert.That(rule.Assembly, Is.Null);
      Assert.That(rule.Namespace, Is.EqualTo("Xtensive.Orm.Tests.Configuration"));
      Assert.That(rule.Database, Is.Null);
      Assert.That(rule.Schema, Is.EqualTo("Model2"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesTest5(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules5", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "main"),
        ((d) => d.DefaultSchema, "dbo"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true));
      var rules = domainConfig.MappingRules;

      Assert.That(rules.Count, Is.EqualTo(1));
      var rule = rules[0];

      Assert.That(rule.Assembly, Is.EqualTo(typeof (JsonConfigurationTest).Assembly));
      Assert.That(rule.Namespace, Is.EqualTo("Xtensive.Orm.Tests.Configuration.TypesToUseInTests"));
      Assert.That(rule.Database, Is.EqualTo("DO-Tests-3"));
      Assert.That(rule.Schema, Is.Null);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesTest6(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules6", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "main"),
        ((d) => d.DefaultSchema, "dbo"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true));
      var rules = domainConfig.MappingRules;

      Assert.That(rules.Count, Is.EqualTo(1));
      var rule = rules[0];
      Assert.That(rule.Assembly, Is.EqualTo(typeof(JsonConfigurationTest).Assembly));
      Assert.That(rule.Namespace, Is.EqualTo("Xtensive.Orm.Tests.Configuration.TypesToUseInTests.NestedNamespace"));
      Assert.That(rule.Database, Is.Null);
      Assert.That(rule.Schema, Is.EqualTo("Model3"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesTest7(bool useRoot)
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules7", useRoot);
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "main"),
        ((d) => d.DefaultSchema, "dbo"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true));
      var rules = domainConfig.MappingRules;

      Assert.That(rules.Count, Is.EqualTo(1));
      var rule = rules[0];
      Assert.That(rule.Assembly, Is.EqualTo(typeof(JsonConfigurationTest).Assembly));
      Assert.That(rule.Namespace, Is.EqualTo("Xtensive.Orm.Tests.Indexing"));
      Assert.That(rule.Database, Is.EqualTo("main"));
      Assert.That(rule.Schema, Is.EqualTo("Model4"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRuleWithConflictByAssemblyTest(bool useRoot)
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithConflictMappingRules1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRuleWithConflictByNamespaceTest(bool useRoot)
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithConflictMappingRules2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesInvalidTest1(bool useRoot)
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules1", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesInvalidTest2(bool useRoot)
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules2", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesInvalidTest3(bool useRoot)
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules3", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesInvalidTest4(bool useRoot)
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules4", useRoot));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MappingRulesInvalidTest5(bool useRoot)
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules5", useRoot));
    }

    #endregion

    #region Logging

    [Test]
    public void LoggingConfigurationTest()
    {
      var configuration = LoadLoggingConfiguration();
      ValidateLoggingConfiguration(configuration);
    }

    [Test]
    public void LoggingEmptyLoggingSectionTest()
    {
      var section = configuration.GetSection($"Xtensive.Orm.EmptyLogging.{Postfix}");
      _ = Assert.Throws<InvalidOperationException>(() => LoadLoggingConfiguration(section));
    }

    [Test]
    public void LoggingEmptyLogsTest()
    {
      if (Postfix == "AppConfig")
        throw new IgnoreException("");

      var section = configuration.GetSection($"Xtensive.Orm.EmptyLogs.{Postfix}");
      _ = Assert.Throws<InvalidOperationException>(() => LoadLoggingConfiguration(section));
    }

    [Test]
    public void LoggingOnlyProviderDeclaredTest()
    {
      var section = configuration.GetSection($"Xtensive.Orm.OnlyLogProvider.{Postfix}");
      var loggingConfig = LoadLoggingConfiguration(section);
      Assert.That(loggingConfig.Logs.Count, Is.EqualTo(0));
      Assert.That(loggingConfig.Provider, Is.EqualTo("Xtensive.Orm.Logging.log4net.LogProvider"));
    }

    [Test]
    public void LoggingProviderAndEmptyLogsTest()
    {
      if (Postfix == "AppConfig")
        throw new IgnoreException("");

      var section = configuration.GetSection($"Xtensive.Orm.LogProviderAndEmptyLogs.{Postfix}");
      var loggingConfig = LoadLoggingConfiguration(section);
      Assert.That(loggingConfig.Logs.Count, Is.EqualTo(0));
      Assert.That(loggingConfig.Provider, Is.EqualTo("Xtensive.Orm.Logging.log4net.LogProvider"));
    }

    #endregion

    private void ValidateAllDefault(DomainConfiguration domainConfiguration)
    {
      Assert.That(domainConfiguration.AllowCyclicDatabaseDependencies, Is.EqualTo(DomainConfiguration.DefaultAllowCyclicDatabaseDependencies));
      Assert.That(domainConfiguration.BuildInParallel, Is.EqualTo(DomainConfiguration.DefaultBuildInParallel));
      Assert.That(domainConfiguration.Collation, Is.EqualTo(string.Empty));
      Assert.That(domainConfiguration.ConnectionInitializationSql, Is.EqualTo(string.Empty));
      Assert.That(domainConfiguration.DefaultDatabase, Is.EqualTo(string.Empty));
      Assert.That(domainConfiguration.DefaultSchema, Is.EqualTo(string.Empty));
      Assert.That(domainConfiguration.EnsureConnectionIsAlive, Is.EqualTo(DomainConfiguration.DefaultEnsureConnectionIsAlive));
      Assert.That(domainConfiguration.ForcedServerVersion, Is.EqualTo(string.Empty));
      Assert.That(domainConfiguration.ForeignKeyMode, Is.EqualTo(DomainConfiguration.DefaultForeignKeyMode));
      Assert.That(domainConfiguration.FullTextChangeTrackingMode, Is.EqualTo(DomainConfiguration.DefaultFullTextChangeTrackingMode));
      Assert.That(domainConfiguration.IncludeSqlInExceptions, Is.EqualTo(DomainConfiguration.DefaultIncludeSqlInExceptions));
      Assert.That(domainConfiguration.IsMultidatabase, Is.False);
      Assert.That(domainConfiguration.IsMultischema, Is.False);
      Assert.That(domainConfiguration.KeyCacheSize, Is.EqualTo(DomainConfiguration.DefaultKeyCacheSize));
      Assert.That(domainConfiguration.KeyGeneratorCacheSize, Is.EqualTo(DomainConfiguration.DefaultKeyGeneratorCacheSize));
      Assert.That(domainConfiguration.MultidatabaseKeys, Is.EqualTo(DomainConfiguration.DefaultMultidatabaseKeys));
      Assert.That(domainConfiguration.Options, Is.EqualTo(DomainConfiguration.DefaultDomainOptions));
      Assert.That(domainConfiguration.PreferTypeIdsAsQueryParameters, Is.EqualTo(DomainConfiguration.DefaultPreferTypeIdsAsQueryParameters));
      Assert.That(domainConfiguration.QueryCacheSize, Is.EqualTo(DomainConfiguration.DefaultQueryCacheSize));
      Assert.That(domainConfiguration.RecordSetMappingCacheSize, Is.EqualTo(DomainConfiguration.DefaultRecordSetMappingCacheSize));
      Assert.That(domainConfiguration.SchemaSyncExceptionFormat, Is.EqualTo(DomainConfiguration.DefaultSchemaSyncExceptionFormat));
      Assert.That(domainConfiguration.ShareStorageSchemaOverNodes, Is.EqualTo(DomainConfiguration.DefaultShareStorageSchemaOverNodes));
      Assert.That(domainConfiguration.TagsLocation, Is.EqualTo(DomainConfiguration.DefaultTagLocation));
      Assert.That(domainConfiguration.UpgradeMode, Is.EqualTo(DomainConfiguration.DefaultUpgradeMode));
    }

    private void ValidateAllDefaultExcept<TConfiguration, T1>(TConfiguration configuration,
      (Expression<Func<TConfiguration, T1>> expression, T1 expectedValue) property)
    {
      Assert.That(configuration, Is.Not.Null);
      var excludedProperties = new List<string>();


      if (!TryExtractPropertyFormLambda(property.expression, out var prop))
        throw new InconclusiveException("");

      Assert.That(property.expression.Compile()(configuration),
        Is.EqualTo(property.expectedValue));

      excludedProperties.Add(prop.Name);

      if (configuration is DomainConfiguration domainConfiguration)
        ValidateAllPropertiesExcept(domainConfiguration, excludedProperties);
      else if (configuration is SessionConfiguration sessionConfiguration)
        ValidateAllPropertiesExcept(sessionConfiguration, excludedProperties);
      else
        throw new ArgumentOutOfRangeException(nameof(configuration));

    }


    private void ValidateAllDefaultExcept<TConfiguration, T1, T2>(TConfiguration configuration,
      (Expression<Func<TConfiguration, T1>> expression, T1 expectedValue) property1,
      (Expression<Func<TConfiguration, T2>> expression, T2 expectedValue) property2)
    {
      Assert.That(configuration, Is.Not.Null);
      var excludedProperties = new List<string>();

      if (!TryExtractPropertyFormLambda(property1.expression, out var prop))
        throw new InconclusiveException("");

      Assert.That(property1.expression.Compile()(configuration),
        Is.EqualTo(property1.expectedValue));

      excludedProperties.Add(prop.Name);

      if (!TryExtractPropertyFormLambda(property2.expression, out prop))
        throw new InconclusiveException("");

      Assert.That(property2.expression.Compile()(configuration),
        Is.EqualTo(property2.expectedValue));

      excludedProperties.Add(prop.Name);

      if(configuration is DomainConfiguration domainConfiguration)
        ValidateAllPropertiesExcept(domainConfiguration, excludedProperties);
      else if (configuration is SessionConfiguration sessionConfiguration)
        ValidateAllPropertiesExcept(sessionConfiguration, excludedProperties);
      else
        throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    private void ValidateAllDefaultExcept<TConfiguration, T1, T2, T3>(TConfiguration configuration,
      (Expression<Func<TConfiguration, T1>> expression, T1 expectedValue) property1,
      (Expression<Func<TConfiguration, T2>> expression, T2 expectedValue) property2,
      (Expression<Func<TConfiguration, T3>> expression, T3 expectedValue) property3)
    {
      Assert.That(configuration, Is.Not.Null);
      var excludedProperties = new List<string>();

      if (!TryExtractPropertyFormLambda(property1.expression, out var prop))
        throw new InconclusiveException("");

      Assert.That(property1.expression.Compile()(configuration),
        Is.EqualTo(property1.expectedValue));

      excludedProperties.Add(prop.Name);

      if (!TryExtractPropertyFormLambda(property2.expression, out prop))
        throw new InconclusiveException("");

      Assert.That(property2.expression.Compile()(configuration),
        Is.EqualTo(property2.expectedValue));

      excludedProperties.Add(prop.Name);

      if (!TryExtractPropertyFormLambda(property3.expression, out prop))
        throw new InconclusiveException("");

      Assert.That(property3.expression.Compile()(configuration),
        Is.EqualTo(property3.expectedValue));

      excludedProperties.Add(prop.Name);

      if (configuration is DomainConfiguration domainConfiguration)
        ValidateAllPropertiesExcept(domainConfiguration, excludedProperties);
      else if (configuration is SessionConfiguration sessionConfiguration)
        ValidateAllPropertiesExcept(sessionConfiguration, excludedProperties);
      else
        throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    private void ValidateAllDefaultExcept<TConfiguration, T1, T2, T3, T4>(TConfiguration configuration,
      (Expression<Func<TConfiguration, T1>> expression, T1 expectedValue) property1,
      (Expression<Func<TConfiguration, T2>> expression, T2 expectedValue) property2,
      (Expression<Func<TConfiguration, T3>> expression, T3 expectedValue) property3,
      (Expression<Func<TConfiguration, T4>> expression, T4 expectedValue) property4)
    {
      Assert.That(configuration, Is.Not.Null);
      var excludedProperties = new List<string>();

      if (!TryExtractPropertyFormLambda(property1.expression, out var prop))
        throw new InconclusiveException("");

      Assert.That(property1.expression.Compile()(configuration),
        Is.EqualTo(property1.expectedValue));

      excludedProperties.Add(prop.Name);

      if (!TryExtractPropertyFormLambda(property2.expression, out prop))
        throw new InconclusiveException("");

      Assert.That(property2.expression.Compile()(configuration),
        Is.EqualTo(property2.expectedValue));
      
      excludedProperties.Add(prop.Name);

      if (!TryExtractPropertyFormLambda(property3.expression, out prop))
        throw new InconclusiveException("");

      Assert.That(property3.expression.Compile()(configuration),
        Is.EqualTo(property3.expectedValue));

      excludedProperties.Add(prop.Name);

      if (!TryExtractPropertyFormLambda(property4.expression, out prop))
        throw new InconclusiveException("");

      Assert.That(property4.expression.Compile()(configuration),
        Is.EqualTo(property4.expectedValue));

      excludedProperties.Add(prop.Name);

      if (configuration is DomainConfiguration domainConfiguration)
        ValidateAllPropertiesExcept(domainConfiguration, excludedProperties);
      else if (configuration is SessionConfiguration sessionConfiguration)
        ValidateAllPropertiesExcept(sessionConfiguration, excludedProperties);
      else
        throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    private bool TryExtractPropertyFormLambda<TConfiguration, T>(Expression<Func<TConfiguration, T>> lambda,
      out System.Reflection.PropertyInfo property)
    {
      if (lambda.Body.StripCasts() is MemberExpression mExpression && mExpression.Member is System.Reflection.PropertyInfo prop) {
        property = prop;
        return true;
      }
      property = null;
      return false;
    }

    private void ValidateAllPropertiesExcept(DomainConfiguration domainConfiguration, List<string> excludedProperties)
    {
      if (!nameof(domainConfiguration.AllowCyclicDatabaseDependencies).In(excludedProperties))
        Assert.That(domainConfiguration.AllowCyclicDatabaseDependencies, Is.EqualTo(DomainConfiguration.DefaultAllowCyclicDatabaseDependencies));

      if (!nameof(domainConfiguration.BuildInParallel).In(excludedProperties))
        Assert.That(domainConfiguration.BuildInParallel, Is.EqualTo(DomainConfiguration.DefaultBuildInParallel));

      if (!nameof(domainConfiguration.Collation).In(excludedProperties))
        Assert.That(domainConfiguration.Collation, Is.EqualTo(string.Empty));

      if (!nameof(domainConfiguration.ConnectionInitializationSql).In(excludedProperties))
        Assert.That(domainConfiguration.ConnectionInitializationSql, Is.EqualTo(string.Empty));

      if (!nameof(domainConfiguration.DefaultDatabase).In(excludedProperties))
        Assert.That(domainConfiguration.DefaultDatabase, Is.EqualTo(string.Empty));

      if (!nameof(domainConfiguration.DefaultSchema).In(excludedProperties))
        Assert.That(domainConfiguration.DefaultSchema, Is.EqualTo(string.Empty));

      if (!nameof(domainConfiguration.EnsureConnectionIsAlive).In(excludedProperties))
        Assert.That(domainConfiguration.EnsureConnectionIsAlive, Is.EqualTo(DomainConfiguration.DefaultEnsureConnectionIsAlive));

      if (!nameof(domainConfiguration.ForcedServerVersion).In(excludedProperties))
        Assert.That(domainConfiguration.ForcedServerVersion, Is.EqualTo(string.Empty));

      if (!nameof(domainConfiguration.ForeignKeyMode).In(excludedProperties))
        Assert.That(domainConfiguration.ForeignKeyMode, Is.EqualTo(DomainConfiguration.DefaultForeignKeyMode));

      if (!nameof(domainConfiguration.FullTextChangeTrackingMode).In(excludedProperties))
        Assert.That(domainConfiguration.FullTextChangeTrackingMode, Is.EqualTo(DomainConfiguration.DefaultFullTextChangeTrackingMode));

      if (!nameof(domainConfiguration.IncludeSqlInExceptions).In(excludedProperties))
        Assert.That(domainConfiguration.IncludeSqlInExceptions, Is.EqualTo(DomainConfiguration.DefaultIncludeSqlInExceptions));

      if (!nameof(domainConfiguration.IsMultidatabase).In(excludedProperties))
        Assert.That(domainConfiguration.IsMultidatabase, Is.False);

      if (!nameof(domainConfiguration.IsMultischema).In(excludedProperties))
        Assert.That(domainConfiguration.IsMultischema, Is.False);

      if (!nameof(domainConfiguration.KeyCacheSize).In(excludedProperties))
        Assert.That(domainConfiguration.KeyCacheSize, Is.EqualTo(DomainConfiguration.DefaultKeyCacheSize));

      if (!nameof(domainConfiguration.KeyGeneratorCacheSize).In(excludedProperties))
        Assert.That(domainConfiguration.KeyGeneratorCacheSize, Is.EqualTo(DomainConfiguration.DefaultKeyGeneratorCacheSize));

      if (!nameof(domainConfiguration.MultidatabaseKeys).In(excludedProperties))
        Assert.That(domainConfiguration.MultidatabaseKeys, Is.EqualTo(DomainConfiguration.DefaultMultidatabaseKeys));

      if (!nameof(domainConfiguration.Options).In(excludedProperties))
        Assert.That(domainConfiguration.Options, Is.EqualTo(DomainConfiguration.DefaultDomainOptions));

      if (!nameof(domainConfiguration.PreferTypeIdsAsQueryParameters).In(excludedProperties))
        Assert.That(domainConfiguration.PreferTypeIdsAsQueryParameters, Is.EqualTo(DomainConfiguration.DefaultPreferTypeIdsAsQueryParameters));

      if (!nameof(domainConfiguration.QueryCacheSize).In(excludedProperties))
        Assert.That(domainConfiguration.QueryCacheSize, Is.EqualTo(DomainConfiguration.DefaultQueryCacheSize));

      if (!nameof(domainConfiguration.RecordSetMappingCacheSize).In(excludedProperties))
        Assert.That(domainConfiguration.RecordSetMappingCacheSize, Is.EqualTo(DomainConfiguration.DefaultRecordSetMappingCacheSize));

      if (!nameof(domainConfiguration.SchemaSyncExceptionFormat).In(excludedProperties))
        Assert.That(domainConfiguration.SchemaSyncExceptionFormat, Is.EqualTo(DomainConfiguration.DefaultSchemaSyncExceptionFormat));

      if (!nameof(domainConfiguration.ShareStorageSchemaOverNodes).In(excludedProperties))
        Assert.That(domainConfiguration.ShareStorageSchemaOverNodes, Is.EqualTo(DomainConfiguration.DefaultShareStorageSchemaOverNodes));

      if (!nameof(domainConfiguration.TagsLocation).In(excludedProperties))
        Assert.That(domainConfiguration.TagsLocation, Is.EqualTo(DomainConfiguration.DefaultTagLocation));

      if (!nameof(domainConfiguration.UpgradeMode).In(excludedProperties))
        Assert.That(domainConfiguration.UpgradeMode, Is.EqualTo(DomainConfiguration.DefaultUpgradeMode));
    }

    private void ValidateAllPropertiesExcept(SessionConfiguration sessionConfiguration, List<string> excludedProperties)
    {
      if (!nameof(sessionConfiguration.UserName).In(excludedProperties))
        Assert.That(sessionConfiguration.UserName, Is.Empty);

      if (!nameof(sessionConfiguration.Password).In(excludedProperties))
        Assert.That(sessionConfiguration.Password, Is.Empty);

      if (!nameof(sessionConfiguration.Options).In(excludedProperties))
        Assert.That(sessionConfiguration.Options, Is.EqualTo(SessionConfiguration.DefaultSessionOptions));

      if (!nameof(sessionConfiguration.CacheSize).In(excludedProperties))
        Assert.That(sessionConfiguration.CacheSize, Is.EqualTo(SessionConfiguration.DefaultCacheSize));

      if (!nameof(sessionConfiguration.CacheType).In(excludedProperties))
        Assert.That(sessionConfiguration.CacheType, Is.EqualTo(SessionConfiguration.DefaultCacheType));

      if (!nameof(sessionConfiguration.DefaultIsolationLevel).In(excludedProperties))
        Assert.That(sessionConfiguration.DefaultIsolationLevel, Is.EqualTo(SessionConfiguration.DefaultDefaultIsolationLevel));

      if (!nameof(sessionConfiguration.DefaultCommandTimeout).In(excludedProperties))
        Assert.That(sessionConfiguration.DefaultCommandTimeout, Is.Null);

      if (!nameof(sessionConfiguration.BatchSize).In(excludedProperties))
        Assert.That(sessionConfiguration.BatchSize, Is.EqualTo(SessionConfiguration.DefaultBatchSize));

      if (!nameof(sessionConfiguration.EntityChangeRegistrySize).In(excludedProperties))
        Assert.That(sessionConfiguration.EntityChangeRegistrySize, Is.EqualTo(SessionConfiguration.DefaultEntityChangeRegistrySize));

      if (!nameof(sessionConfiguration.ReaderPreloading).In(excludedProperties))
        Assert.That(sessionConfiguration.ReaderPreloading, Is.EqualTo(SessionConfiguration.DefaultReaderPreloadingPolicy));

      if (!nameof(sessionConfiguration.ServiceContainerType).In(excludedProperties))
        Assert.That(sessionConfiguration.ServiceContainerType, Is.Null);

      if (!nameof(sessionConfiguration.ConnectionInfo).In(excludedProperties))
        Assert.That(sessionConfiguration.ConnectionInfo, Is.Null);
    }

    private void ValidateLoggingConfiguration(LoggingConfiguration configuration)
    {
      Assert.That(configuration.Provider, Is.Not.Null.Or.Empty);
      Assert.That(configuration.Provider, Is.EqualTo("Xtensive.Orm.Logging.log4net.LogProvider"));

      Assert.That(configuration.Logs[0].Source, Is.EqualTo("*"));
      Assert.That(configuration.Logs[0].Target, Is.EqualTo("Console"));

      Assert.That(configuration.Logs[1].Source, Is.EqualTo("SomeLogName"));
      Assert.That(configuration.Logs[1].Target, Is.EqualTo("DebugOnlyConsole"));

      Assert.That(configuration.Logs[2].Source, Is.EqualTo("FirstLogName,SecondLogName"));
      Assert.That(configuration.Logs[2].Target, Is.EqualTo(@"d:\log.txt"));

      Assert.That(configuration.Logs[3].Source, Is.EqualTo("LogName, AnotherLogName"));
      Assert.That(configuration.Logs[3].Target, Is.EqualTo("Console"));

      Assert.That(configuration.Logs[4].Source, Is.EqualTo("FileLog"));
      Assert.That(configuration.Logs[4].Target, Is.EqualTo("log.txt"));

      Assert.That(configuration.Logs[5].Source, Is.EqualTo("NullLog"));
      Assert.That(configuration.Logs[5].Target, Is.EqualTo("None"));

      Assert.That(configuration.Logs[6].Source, Is.EqualTo("Trash"));
      Assert.That(configuration.Logs[6].Target, Is.EqualTo("skjdhfjsdf sdfsdfksjdghj fgdfg"));
    }
  }
}
