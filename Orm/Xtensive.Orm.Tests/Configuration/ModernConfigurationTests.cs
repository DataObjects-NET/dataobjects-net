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
  public sealed class AppConfigStyleConfigurationTest : ConfigurationFileTestBase
  {
    protected override string DefaultSectionName => "Xtensive.Orm.AppConfig";
    protected override string Postfix => "AppConfig";

    protected override bool NameAttributeUnique => false;

    protected override void RegisterConfigurationFile(ConfigurationBuilder builder)
    {
      _ = builder.AddXmlFile("domainSettings.config");
    }
  }

  [TestFixture]
  public sealed class XmlConfigurationTest : ConfigurationFileTestBase
  {
    protected override string DefaultSectionName => "Xtensive.Orm.Xml";
    protected override string Postfix => "Xml";

    protected override void RegisterConfigurationFile(ConfigurationBuilder builder)
    {
      _ = builder.AddXmlFile("domainSettings.config");
    }
  }

  [TestFixture]
  public sealed class JsonConfigurationTest : ConfigurationFileTestBase
  {
    protected override string DefaultSectionName => "Xtensive.Orm.Json";
    protected override string Postfix => "Json";

    protected override void RegisterConfigurationFile(ConfigurationBuilder builder)
    {
      _ = builder.AddJsonFile("domainSettings.json");
    }
  }

  public abstract class ConfigurationFileTestBase
  {

    private IConfigurationRoot configuration;
    private IConfigurationSection configurationSection;

    protected abstract string DefaultSectionName { get; }
    protected abstract string Postfix { get; }
    protected virtual bool NameAttributeUnique => true;

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

    private DomainConfiguration LoadDomainConfiguration(string domainName)
    {
      var domainConfiguration = DomainConfiguration.Load(configurationSection, domainName);
      return domainConfiguration;
    }
    private LoggingConfiguration LoadLoggingConfiguration(IConfigurationSection customConfigurationSection = null)
    {
      var loggingConfiguration = LoggingConfiguration.Load(customConfigurationSection ?? configurationSection);
      return loggingConfiguration;
    }


    #region Simple Domain settings that used to be attributes

    [Test]
    public void ProviderAndConnectionStringTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithProviderAndConnectionString");
      Assert.That(domainConfig.ConnectionInfo.Provider, Is.EqualTo(WellKnown.Provider.Sqlite));
      Assert.That(domainConfig.ConnectionInfo.ConnectionString, Is.EqualTo("Data Source=DO-Testsaaa.db3"));
      Assert.That(domainConfig.ConnectionInfo.ConnectionUrl, Is.Null);

      ValidateAllDefault(domainConfig);
    }

    [Test]
    public void ConnectionUrlTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithConnectionUrl");
      Assert.That(domainConfig.ConnectionInfo.Provider, Is.EqualTo(WellKnown.Provider.Sqlite));
      Assert.That(domainConfig.ConnectionInfo.ConnectionString, Is.Null);
      Assert.That(domainConfig.ConnectionInfo.ConnectionUrl.Url, Is.EqualTo("sqlite:///DO-Tests.db3"));

      ValidateAllDefault(domainConfig);
    }

    [Test]
    public void CustomValidKeyCacheSizeTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidKeyCacheSize");

      ValidateAllDefaultExcept(domainConfig, ((d) => d.KeyCacheSize, 192));
    }

    [Test]
    public void CustomInvalidKeyCacheSizeTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidKeyCacheSize"));

    }

    [Test]
    public void CustomValidKeyGeneratorCacheSizeTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidKeyGeneratorCacheSize");

      ValidateAllDefaultExcept(domainConfig, ((d) => d.KeyGeneratorCacheSize, 192));
    }

    [Test]
    public void CustomInvalidKeyGeneratorCacheSizeTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidKeyGeneratorCacheSize"));
    }

    [Test]
    public void CustomValidQueryCacheSizeTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidQueryCacheSize");

      ValidateAllDefaultExcept(domainConfig, ((d) => d.QueryCacheSize, 192));
    }

    [Test]
    public void CustomInvalidQueryCacheSizeTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidQueryCacheSize"));
    }

    [Test]
    public void CustomValidRecordSetMappingCacheSizeTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomValidRecordSetMappingCacheSize");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.RecordSetMappingCacheSize, 192));
    }

    [Test]
    public void CustomInvalidRecordSetMappingCacheSizeTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomInvalidRecordSetMappingCacheSize"));
    }

    [Test]
    public void CustomDefaultDatabaseTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomDatabase");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultDatabase, "MyFancyDatabase"),
        ((d) => d.IsMultidatabase, true),
        ((d) => d.IsMultischema, true)
        );
    }

    [Test]
    public void CustomDefaultSchemaTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithCustomSchema");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.DefaultSchema, "MyFancySchema"),
        ((d) => d.IsMultidatabase, false),
        ((d) => d.IsMultischema, true));
    }

    [Test]
    public void UpgradeModesTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode1");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Default));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode2");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Recreate));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode3");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Perform));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode4");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.PerformSafely));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode5");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Validate));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode6");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.LegacyValidate));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode7");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.Skip));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithUpgradeMode8");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.UpgradeMode, DomainUpgradeMode.LegacySkip));
      domainConfig.Lock();
    }

    [Test]
    public void WrongUpgradeModeTest()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithWrongUpgradeMode"));
    }

    [Test]
    public void ForeighKeyModesTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode1");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.None));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode2");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Hierarchy));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode3");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Reference));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode4");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.All));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode5");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Default));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithForeignKeyMode6");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.ForeignKeyMode, ForeignKeyMode.Hierarchy | ForeignKeyMode.Reference));
      domainConfig.Lock();
    }

    [Test]
    public void InvalidForeighKeyModeTest()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidForeignKeyMode"));
    }

    [Test]
    public void ChangeTrackingModesTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode1");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Off));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode2");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Auto));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode3");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Manual));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode4");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.OffWithNoPopulation));
      domainConfig.Lock();

      domainConfig = LoadDomainConfiguration("DomainWithChangeTrackingMode5");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.FullTextChangeTrackingMode, FullTextChangeTrackingMode.Default));
      domainConfig.Lock();
    }

    [Test]
    public void InvalidChangeTrackingModeTest()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidChangeTrackingMode"));
    }

    [Test]
    public void DomainOptionsTest1()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDomainOptionsValid1");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.Options, DomainOptions.Default));
      domainConfig.Lock();
    }

    [Test]
    public void DomainOptionsTest2()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDomainOptionsValid2");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.Options, DomainOptions.None));
      domainConfig.Lock();
    }
    [Test]
    public void InvalidDomainOptionsTest()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithDomainOptionsInvalid"));
    }

    [Test]
    public void CollationTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithColation");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.Collation, "generalci"));
    }

    [Test]
    public void BriefSchemaSyncExceptionsTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithBriefSchemaSyncExceptions");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.SchemaSyncExceptionFormat, SchemaSyncExceptionFormat.Brief));
    }

    [Test]
    public void DetailedSchemaSyncExceptionsTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDetailedSchemaSyncExceptions");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.SchemaSyncExceptionFormat, SchemaSyncExceptionFormat.Detailed));
    }

    [Test]
    public void DefaultSchemaSyncExceptionsTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDefaultSchemaSyncExceptions");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.SchemaSyncExceptionFormat, SchemaSyncExceptionFormat.Default));
    }

    [Test]
    public void InvalidSchemaSyncExceptionsTest()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidSchemaSyncExceptions"));
    }

    [Test]
    public void TagsLocationNowhereTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationNowhere");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.Nowhere));
    }

    [Test]
    public void TagsLocationBeforeTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationBefore");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.BeforeStatement));
    }

    [Test]
    public void TagsLocationWithinTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationWithin");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.WithinStatement));
    }

    [Test]
    public void TagsLocationAfterTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationAfter");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.AfterStatement));
    }

    [Test]
    public void TagsLocationDefaultTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTagsLocationDefault");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.TagsLocation, TagsLocation.Default));
    }

    [Test]
    public void InvalidTagsLocationTest()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithTagsLocationInvalid"));
    }

    [Test]
    public void ForcedServerVersionTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithForcedServerVersion");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ForcedServerVersion, "10.0.0.0"));
    }

    [Test]
    public void InitializationSqlTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithInitSql");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ConnectionInitializationSql, "use [OtherDb]"));
    }

    [Test]
    public void IncludeSqlInExceptionsTest()
    {
      var domainConfig = LoadDomainConfiguration("IncludeSqlInExceptionsTrue");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.IncludeSqlInExceptions, true));
    }

    [Test]
    public void DontIncludeSqlInExceptionsTest()
    {
      var domainConfig = LoadDomainConfiguration("IncludeSqlInExceptionsFalse");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.IncludeSqlInExceptions, false));
    }

    [Test]
    public void AllowCyclicDatabaseDependanciesTest()
    {
      var domainConfig = LoadDomainConfiguration("AllowCyclicDatabaseDependenciesTrue");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.AllowCyclicDatabaseDependencies, true));
    }

    [Test]
    public void DisallowCyclicDatabaseDependanciesTest()
    {
      var domainConfig = LoadDomainConfiguration("AllowCyclicDatabaseDependenciesFalse");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.AllowCyclicDatabaseDependencies, false));
    }

    [Test]
    public void BuildInParallelTest()
    {
      var domainConfig = LoadDomainConfiguration("BuildInParallelTrue");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.BuildInParallel, true));
    }

    [Test]
    public void DontBuildInParallelTest()
    {
      var domainConfig = LoadDomainConfiguration("BuildInParallelFalse");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.BuildInParallel, false));
    }

    [Test]
    public void AllowMultidatabaseKeysTest()
    {
      var domainConfig = LoadDomainConfiguration("MultidatabaseKeysTrue");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.MultidatabaseKeys, true));
    }

    [Test]
    public void DisallowMultidatabaseKeysTest()
    {
      var domainConfig = LoadDomainConfiguration("MultidatabaseKeysFalse");
      ValidateAllDefaultExcept(domainConfig, ((d) => d.MultidatabaseKeys, false));
    }

    [Test]
    public void ShareStorageSchemaOverNodesTest()
    {
      var domainConfig = LoadDomainConfiguration("SharedStorageSchemaOn");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ShareStorageSchemaOverNodes, true));
    }

    [Test]
    public void DontShareStorageSchemaOverNodesTest()
    {
      var domainConfig = LoadDomainConfiguration("SharedStorageSchemaOff");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.ShareStorageSchemaOverNodes, false));
    }

    [Test]
    public void EnsureConnectionIsAliveTest()
    {
      var domainConfig = LoadDomainConfiguration("EnableConnectionIsAliveTrue");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.EnsureConnectionIsAlive, true));
    }

    [Test]
    public void DontCheckConnectionIsAliveTest()
    {
      var domainConfig = LoadDomainConfiguration("EnableConnectionIsAliveFalse");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.EnsureConnectionIsAlive, false));
    }

    [Test]
    public void PreferTypeIdAsQueryParameterTest()
    {
      var domainConfig = LoadDomainConfiguration("PreferTypeIdsAsQueryParametersTrue");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.PreferTypeIdsAsQueryParameters, true));
    }

    [Test]
    public void DontPreferTypeIdAsQueryParameterTest()
    {
      var domainConfig = LoadDomainConfiguration("PreferTypeIdsAsQueryParametersFalse");
      ValidateAllDefaultExcept(domainConfig,
        ((d) => d.PreferTypeIdsAsQueryParameters, false));
    }
    #endregion

    #region NamingConvention

    [Test]
    public void NamingConventionSettingsTest01()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention1");
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
    public void NamingConventionSettingsTest02()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention2");
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
    public void NamingConventionSettingsTest03()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention3");
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
    public void NamingConventionSettingsTest04()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention4");
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
    public void NamingConventionSettingsTest05()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention5");
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.AsIs));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    public void NamingConventionSettingsTest06()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention6");
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    public void NamingConventionSettingsTest07()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention7");
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Omit));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    public void NamingConventionSettingsTest08()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention8");
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Default));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreHyphens | NamingRules.RemoveDots));
    }

    [Test]
    public void NamingConventionSettingsTest09()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention9");
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.UnderscoreDots | NamingRules.RemoveHyphens));
    }

    [Test]
    public void NamingConventionSettingsTest10()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention10");
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.None));

    }

    [Test]
    public void NamingConventionSettingsTest11()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithNamingConvention11");
      ValidateAllDefault(domainConfig);
      var namingConvention = domainConfig.NamingConvention;
      Assert.That(namingConvention.LetterCasePolicy, Is.EqualTo(LetterCasePolicy.Uppercase));
      Assert.That(namingConvention.NamespacePolicy, Is.EqualTo(NamespacePolicy.Hash));
      Assert.That(namingConvention.NamingRules, Is.EqualTo(NamingRules.Default));
    }

    [Test]
    public void NamingConventionInvalidSettingsTest1()
    {
      _ = Assert.Throws<InvalidOperationException> (()=> LoadDomainConfiguration("DomainWithInvalidNamingConvention1"));
    }

    [Test]
    public void NamingConventionInvalidSettingsTest2()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidNamingConvention2"));
    }

    [Test]
    public void NamingConventionInvalidSettingsTest3()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidNamingConvention3"));
    }

    #endregion

    #region VersioningConvention

    [Test]
    public void VersioningConventionPessimisticTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention1");
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Pessimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    public void VersioningConventionOptimisticTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention2");
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Optimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    public void VersioningConventionDefaultTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention3");
      ValidateAllDefault(domainConfig);

      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Default));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    public void VersioningConventionDenyEntitySetChangeVersionTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention4");
      ValidateAllDefault(domainConfig);

      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Optimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(true));
    }

    [Test]
    public void VersioningConventionAllowEntitySetChangeVersionTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithVersioningConvention5");
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.VersioningConvention.EntityVersioningPolicy, Is.EqualTo(EntityVersioningPolicy.Optimistic));
      Assert.That(domainConfig.VersioningConvention.DenyEntitySetOwnerVersionChange, Is.EqualTo(false));
    }

    [Test]
    public void VersioningConventionInvalidTest()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithInvalidVersioningConvention1"));
    }

    #endregion

    #region Types registration
    [Test]
    public void TypesRegistrationAsTypesTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithTypes");
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity2)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity3)), Is.False);
    }

    [Test]
    public void TypesRegistrationAsAssembliesTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithAssemblies");
      ValidateAllDefault(domainConfig);
      var ormAssembly = typeof(DomainConfiguration).Assembly;
      Assert.That(domainConfig.Types.Count, Is.GreaterThan(0));
      Assert.That(domainConfig.Types.All((t) => t.Assembly == ormAssembly), Is.True);
    }

    [Test]
    public void TypesRegistrationAsAssembliesWithNamespace()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithAssembliesAndNamespaces");
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity2)), Is.True);
    }

    [Test]
    public void MixedTypeRegistration()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMixedRegistrations");
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity2)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity3)), Is.False);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity2)), Is.True);
    }

    [Test]
    public void InvalidTypeRegistration1()
    {
      // same type twice

      var domainConfig = LoadDomainConfiguration("DomainWithInvalidRegistrations1");

      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity2)), Is.False);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.DummyEntity3)), Is.False);
    }

    [Test]
    public void InvalidTypeRegistration2()
    {
      // same Assembly
      var domainConfig = LoadDomainConfiguration("DomainWithInvalidRegistrations2");

      ValidateAllDefault(domainConfig);
      var ormAssembly = typeof(DomainConfiguration).Assembly;
      Assert.That(domainConfig.Types.Count, Is.GreaterThan(0));
      Assert.That(domainConfig.Types.All((t) => t.Assembly == ormAssembly), Is.True);
    }

    [Test]
    public void InvalidTypeRegistration3()
    {
      // same assembly and namespace
      var domainConfig = LoadDomainConfiguration("DomainWithInvalidRegistrations3");
      ValidateAllDefault(domainConfig);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity1)), Is.True);
      Assert.That(domainConfig.Types.Contains(typeof(TypesToUseInTests.NestedNamespace.DummyNestedEntity2)), Is.True);
    }

    [Test]
    public void InvalidTypeRegistration4()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations4"));
    }

    [Test]
    public void InvalidTypeRegistration5()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations5"));
    }

    [Test]
    public void InvalidTypeRegistration6()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations6"));
    }

    [Test]
    public void InvalidTypeRegistration7()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidRegistrations7"));
    }

    #endregion

    #region Sessions

    [Test]
    public void MultipleSessionsTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMultipleSessionConfigurations");
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
    public void SessionWithEmptyNameTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionEmptyName");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.ClientProfile));
    }

    [Test]
    public void SessionWithCustomNameTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomName");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.Not.EqualTo(WellKnown.Sessions.Default));
      Assert.That(sessions[0].Name, Is.EqualTo("UserCreated"));

      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.ClientProfile));
    }

    [Test]
    public void SessionWithCustomUserNameTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomUser");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.UserName, "User"),
        ((s) => s.Password, "126654"));
    }

    [Test]
    public void SessionWithOptionsTest1()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionDefaultOptions");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.Default));

    }

    [Test]
    public void SessionWithOptionsTest2()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionServerProfile");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
         ((s) => s.Options, SessionOptions.ServerProfile));

    }

    [Test]
    public void SessionWithOptionsTest3()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionClientProfile");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.ClientProfile));

    }

    [Test]
    public void SessionWithOptionsTest4()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomOptions");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.Options, SessionOptions.AllowSwitching | SessionOptions.AutoActivation | SessionOptions.ReadRemovedObjects | SessionOptions.ValidateEntityVersions));

    }

    [Test]
    public void SessionWithCollectionSizesTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionWithCollectionSizes");
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
    public void SessionCustomCacheTypeTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomCacheType");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.CacheType, SessionCacheType.Infinite));

    }

    [Test]
    public void SessionCustomIsolationLevelTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomIsolationLevel");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.DefaultIsolationLevel, IsolationLevel.ReadCommitted));

    }

    [Test]
    public void SessionCustomCommandTimeoutTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomCommandTimeout");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.DefaultCommandTimeout, (int?)300));

    }

    [Test]
    public void SessionCustomPreloadingPolicyTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomPreloadingPolicy");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.ReaderPreloading, ReaderPreloadingPolicy.Always));

    }

    [Test]
    public void SessionCustomConnectionStringTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomConnectionString");
      ValidateAllDefault(domainConfig);

      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.ConnectionInfo, new ConnectionInfo("_dummy_", "Data Source=localhost;Initial Catalog=DO-Tests;Integrated Security=True;MultipleActiveResultSets=True")));
    }

    [Test]
    public void SessionCustomConnectionUrlTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithSessionCustomConnectionUrl");
      ValidateAllDefault(domainConfig);
      var sessions = domainConfig.Sessions;
      Assert.That(sessions.Count, Is.EqualTo(1));
      Assert.That(sessions[0].Name, Is.EqualTo(WellKnown.Sessions.Default));
      ValidateAllDefaultExcept(sessions[0],
        ((s) => s.ConnectionInfo, new ConnectionInfo("sqlserver://localhost/DO-Tests")));
    }

    [Test]
    public void SessionWithInvalidOptions()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithSessionInvalidOptions"));
    }

    [Test]
    public void SessionWithInvalidCacheSizeTest1()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheSize1"));
    }

    [Test]
    public void SessionWithInvalidCacheSizeTest2()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheSize2"));
    }

    [Test]
    public void SessionWithInvalidCacheSizeTest3()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheSize3"));
    }

    [Test]
    public void SessionWithInvalidBatchSizeTest1()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidBatchSize1"));
    }

    [Test]
    public void SessionWithInvalidBatchSizeTest2()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidBatchSize2"));
    }

    [Test]
    public void SessionWithInvalidEntityChangeRegistryTest1()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidEntityChangeRegistry1"));
    }

    [Test]
    public void SessionWithInvalidEntityChangeRegistryTest2()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithSessionInvalidEntityChangeRegistry2"));
    }

    [Test]
    public void SessionWithInvalidCacheType1()
    {
      _ = Assert.Throws<InvalidOperationException>(() => LoadDomainConfiguration("DomainWithSessionInvalidCacheType"));
    }

    #endregion

    #region Databases configuration

    [Test]
    public void DatabaseConfigurationOnlyAliasTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDatabases1");
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
    public void DatabaseConfigurationWithTypeIdsTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithDatabases2");
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
    public void DatabaseConfigurationNegativeMinTypeIdTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases1"));
    }

    [Test]
    public void DatabaseConfigurationInvalidMinTypeIdTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases2"));
    }

    [Test]
    public void DatabaseConfigurationNegativeMaxTypeIdTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases3"));
    }

    [Test]
    public void DatabaseConfigurationInvalidMaxTypeIdTest()
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithInvalidDatabases4"));
    }

    #endregion

    #region KeyGenerators

    [Test]
    public void SimpleKeyGeneratorTest()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGenerator");
      ValidateAllDefault(domainConfig);
    }

    [Test]
    public void KeyGeneratorWithDatabaseNamesTest()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGeneratorsWithDatabaseNames");
      ValidateAllDefault(domainConfig);
    }

    [Test]
    public void KeyGeneratorWithDatabaseNamesAllParamsTest()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGeneratorsWithDatabaseNamesAndKeyParams");
      ValidateAllDefault(domainConfig);
    }

    [Test]
    public void KeyGeneratorsWithDatabaseAliasesTest()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      var domainConfig = LoadDomainConfiguration("DomainWithCustomGeneratorsWithDatabasesAliases");
      ValidateAllDefault(domainConfig);
    }

    [Test]
    public void KeyGeneratorsWithConflictByDatabaseTest1()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorsConflict1"));
    }

    [Test]
    public void KeyGeneratorsWithConflictByDatabaseTest2()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorsConflict2"));
    }

    [Test]
    public void KeyGeneratorWithNegativeSeedTest()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorNegativeSeed"));
    }

    [Test]
    public void KeyGeneratorWithNegativeCacheSizeTest()
    {
      if (!NameAttributeUnique)
        throw new IgnoreException("");
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => LoadDomainConfiguration("DomainWithCustomGeneratorNegativeCache"));
    }

    #endregion

    #region IgnoreRules

    [Test]
    public void IgnoreRulesTest()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithIgnoreRules");
      ValidateAllDefault(domainConfig);
      ValidateIgnoreRules(domainConfig.IgnoreRules);
    }

    [Test]
    public void IgnoreColumnAndIndexAtTheSameTimeTest()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules1"));
    }

    [Test]
    public void IgnoreTableAndColumnAndIndexAtTheSameTimeTest()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules2"));
    }

    [Test]
    public void IgnoreDatabaseOnlyTest()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules3"));
    }

    [Test]
    public void IgnoreDatabaseAndSchemaOnlyTest()
    {
      _ = Assert.Throws<ArgumentException>(() => LoadDomainConfiguration("DomainWithInvalidIgnoreRules4"));
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
    public void MappingRulesTest1()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules1");
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
    public void MappingRulesTest2()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules2");
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
    public void MappingRulesTest3()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules3");
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
    public void MappingRulesTest4()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules4");
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
    public void MappingRulesTest5()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules5");
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
    public void MappingRulesTest6()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules6");
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
    public void MappingRulesTest7()
    {
      var domainConfig = LoadDomainConfiguration("DomainWithMappingRules7");
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
    public void MappingRuleWithConflictByAssemblyTest()
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithConflictMappingRules1"));
    }

    [Test]
    public void MappingRuleWithConflictByNamespaceTest()
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithConflictMappingRules2"));
    }

    [Test]
    public void MappingRulesInvalidTest1()
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules1"));
    }

    [Test]
    public void MappingRulesInvalidTest2()
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules2"));
    }

    [Test]
    public void MappingRulesInvalidTest3()
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules3"));
    }

    [Test]
    public void MappingRulesInvalidTest4()
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules4"));
    }

    [Test]
    public void MappingRulesInvalidTest5()
    {
      var exception = Assert.Throws<System.ArgumentException>(
        () => LoadDomainConfiguration("DomainWithInvalidMappingRules5"));
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

    private void ValidateAllDefault(SessionConfiguration sessionConfiguration)
    {
      Assert.That(sessionConfiguration.UserName, Is.Empty);
      Assert.That(sessionConfiguration.Password, Is.Empty);
      Assert.That(sessionConfiguration.Options, Is.EqualTo(SessionConfiguration.DefaultSessionOptions));
      Assert.That(sessionConfiguration.CacheSize, Is.EqualTo(SessionConfiguration.DefaultCacheSize));
      Assert.That(sessionConfiguration.CacheType, Is.EqualTo(SessionConfiguration.DefaultCacheType));
      Assert.That(sessionConfiguration.DefaultIsolationLevel, Is.EqualTo(SessionConfiguration.DefaultDefaultIsolationLevel));
      Assert.That(sessionConfiguration.DefaultCommandTimeout, Is.Null);
      Assert.That(sessionConfiguration.BatchSize, Is.EqualTo(SessionConfiguration.DefaultBatchSize));
      Assert.That(sessionConfiguration.EntityChangeRegistrySize, Is.EqualTo(SessionConfiguration.DefaultEntityChangeRegistrySize));
      Assert.That(sessionConfiguration.ReaderPreloading, Is.EqualTo(SessionConfiguration.DefaultReaderPreloadingPolicy));
      Assert.That(sessionConfiguration.ServiceContainerType, Is.Null);
      Assert.That(sessionConfiguration.ConnectionInfo, Is.Null);
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
