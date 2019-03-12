// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
using System.Configuration;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// <see cref="Domain"/> configuration element within a configuration file.
  /// </summary>
  public class DomainConfigurationElement : ConfigurationCollectionElementBase
  {
    private const string UpgradeModeElementName = "upgradeMode";
    private const string ForeignKeyModeElementName = "foreignKeyMode";
    private const string NameElementName = "name";
    private const string ProviderElementName = "provider";
    private const string ConnectionStringElementName = "connectionString";
    private const string ConnectionUrlElementName = "connectionUrl";
    private const string TypesElementName = "types";
    private const string NamingConventionElementName = "namingConvention";
    private const string KeyCacheSizeElementName = "keyCacheSize";
    private const string KeyGeneratorCacheSizeElementName = "generatorCacheSize";
    private const string QueryCacheSizeElementName = "queryCacheSize";
    private const string RecordSetMappingCacheSizeElementName = "recordSetMappingCacheSizeSize";
    private const string DefaultSchemaElementName = "defaultSchema";
    private const string DefaultDatabaseElementName = "defaultDatabase";
    private const string SessionsElementName = "sessions";
    private const string MappingRulesElementName = "mappingRules";
    private const string DatabasesElementName = "databases";
    private const string KeyGeneratorsElementName = "keyGenerators";
    private const string ServicesElementName = "services";
    private const string CollationElementName = "collation";
    private const string ServiceContainerTypeElementName = "serviceContainerType";
    private const string IncludeSqlInExceptionsElementName = "includeSqlInExceptions";
    private const string BuildInParallelElementName = "buildInParallel";
    private const string AllowCyclicDatabaseDependenciesElementName = "allowCyclicDatabaseDependencies";
    private const string ForcedServerVersionElementName = "forcedServerVersion";
    private const string SchemaSyncExceptionFormatElementName = "schemaSyncExceptionFormat";
    private const string ConnectionInitializationSqlElementName = "connectionInitializationSql";
    private const string IgnoreRulesElementName = "ignoreRules";
    private const string MultidatabaseKeysElementName = "multidatabaseKeys";
    private const string OptionsElementName = "options";
    private const string ShareStorageSchemaOverNodesElementName = "shareStorageSchemaOverNodes";
    private const string FullTextChangeTrackingModeElementName = "fullTextChangeTrackingMode";
    private const string VersioningConventionElementName = "versioningConvention";
    private const string EnsureConnectionIsAliveElementName = "ensureConnectionIsAlive";


    /// <inheritdoc/>
    public override object Identifier { get { return Name; } }

    /// <summary>
    /// <see cref="DomainConfiguration.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true, DefaultValue = "")]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ConnectionInfo" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ConnectionUrlElementName, DefaultValue = null)]
    public string ConnectionUrl
    {
      get { return (string) this[ConnectionUrlElementName]; }
      set { this[ConnectionUrlElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ConnectionInfo" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ConnectionStringElementName, DefaultValue = null)]
    public string ConnectionString
    {
      get { return (string) this[ConnectionStringElementName]; }
      set { this[ConnectionStringElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ConnectionInfo" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ProviderElementName, DefaultValue = WellKnown.Provider.SqlServer)]
    public string Provider
    {
      get { return (string) this[ProviderElementName]; }
      set { this[ProviderElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Types" copy="true"/>
    /// </summary>
    [ConfigurationProperty(TypesElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<TypeRegistrationElement>), AddItemName = "add")]
    public ConfigurationCollection<TypeRegistrationElement> Types
    {
      get { return (ConfigurationCollection<TypeRegistrationElement>) base[TypesElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.NamingConvention" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NamingConventionElementName)]
    public NamingConventionElement NamingConvention
    {
      get { return (NamingConventionElement) this[NamingConventionElementName]; }
      set { this[NamingConventionElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.KeyCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(KeyCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultKeyCacheSize)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int KeyCacheSize
    {
      get { return (int) this[KeyCacheSizeElementName]; }
      set { this[KeyCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.KeyGeneratorCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(KeyGeneratorCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultKeyGeneratorCacheSize)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int KeyGeneratorCacheSize
    {
      get { return (int) this[KeyGeneratorCacheSizeElementName]; }
      set { this[KeyGeneratorCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.QueryCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(QueryCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultQueryCacheSize)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int QueryCacheSize
    {
      get { return (int) this[QueryCacheSizeElementName]; }
      set { this[QueryCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.RecordSetMappingCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(RecordSetMappingCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultRecordSetMappingCacheSize)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int RecordSetMappingCacheSize
    {
      get { return (int) this[RecordSetMappingCacheSizeElementName]; }
      set { this[RecordSetMappingCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.UpgradeMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(UpgradeModeElementName, DefaultValue = "Default")]
    public string UpgradeMode
    {
      get { return (string) this[UpgradeModeElementName]; }
      set { this[UpgradeModeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.SchemaSyncExceptionFormat" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SchemaSyncExceptionFormatElementName, DefaultValue = "Default")]
    public string SchemaSyncExceptionFormat
    {
      get { return (string) this[SchemaSyncExceptionFormatElementName]; }
      set { this[SchemaSyncExceptionFormatElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ForeignKeyMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ForeignKeyModeElementName, DefaultValue = "Default")]
    public string ForeignKeyMode
    {
      get { return (string) this[ForeignKeyModeElementName]; }
      set { this[ForeignKeyModeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.FullTextChangeTrackingMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(FullTextChangeTrackingModeElementName, DefaultValue = "Default")]
    public string FullTextChangeTrackingMode
    {
      get { return (string) this[FullTextChangeTrackingModeElementName]; }
      set { this[FullTextChangeTrackingModeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Collation" copy="true"/>
    /// </summary>
    [ConfigurationProperty(CollationElementName)]
    public string Collation
    {
      get { return (string) this[CollationElementName]; }
      set { this[CollationElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Sessions" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SessionsElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<SessionConfigurationElement>), AddItemName = "session")]
    public ConfigurationCollection<SessionConfigurationElement> Sessions
    {
      get { return (ConfigurationCollection<SessionConfigurationElement>) this[SessionsElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.MappingRules" copy="true"/>
    /// </summary>
    [ConfigurationProperty(MappingRulesElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<MappingRuleElement>), AddItemName = "rule")]
    public ConfigurationCollection<MappingRuleElement> MappingRules
    {
      get { return (ConfigurationCollection<MappingRuleElement>) this[MappingRulesElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Databases" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DatabasesElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<DatabaseConfigurationElement>), AddItemName = "database")]
    public ConfigurationCollection<DatabaseConfigurationElement> Databases
    {
      get { return (ConfigurationCollection<DatabaseConfigurationElement>) this[DatabasesElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.KeyGenerators" copy="true"/>
    /// </summary>
    [ConfigurationProperty(KeyGeneratorsElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<DatabaseConfigurationElement>), AddItemName = "keyGenerator")]
    public ConfigurationCollection<KeyGeneratorConfigurationElement> KeyGenerators
    {
      get { return (ConfigurationCollection<KeyGeneratorConfigurationElement>) this[KeyGeneratorsElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ServiceContainerType" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ServiceContainerTypeElementName, DefaultValue = null)]
    public string ServiceContainerType
    {
      get { return (string)this[ServiceContainerTypeElementName]; }
      set { this[ServiceContainerTypeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.DefaultSchema" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DefaultSchemaElementName)]
    public string DefaultSchema
    {
      get { return (string) this[DefaultSchemaElementName]; }
      set { this[DefaultSchemaElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.DefaultDatabase" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DefaultDatabaseElementName)]
    public string DefaultDatabase
    {
      get { return (string) this[DefaultDatabaseElementName]; }
      set { this[DefaultDatabaseElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.IncludeSqlInExceptions" copy="true"/>
    /// </summary>
    [ConfigurationProperty(IncludeSqlInExceptionsElementName,
      DefaultValue = DomainConfiguration.DefaultIncludeSqlInExceptions)]
    public bool IncludeSqlInExceptions
    {
      get { return (bool) this[IncludeSqlInExceptionsElementName]; }
      set { this[IncludeSqlInExceptionsElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.AllowCyclicDatabaseDependencies" copy="true"/>
    /// </summary>
    [ConfigurationProperty(AllowCyclicDatabaseDependenciesElementName, DefaultValue = false)]
    public bool AllowCyclicDatabaseDependencies
    {
      get { return (bool) this[AllowCyclicDatabaseDependenciesElementName]; }
      set { this[AllowCyclicDatabaseDependenciesElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.BuildInParallel" copy="true"/>
    /// </summary>
    [ConfigurationProperty(BuildInParallelElementName,
      DefaultValue = DomainConfiguration.DefaultBuildInParallel)]
    public bool BuildInParallel
    {
      get { return (bool) this[BuildInParallelElementName]; }
      set { this[BuildInParallelElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ForcedServerVersion" copy="true" />
    /// </summary>
    [ConfigurationProperty(ForcedServerVersionElementName)]
    public string ForcedServerVersion
    {
      get { return (string) this[ForcedServerVersionElementName]; }
      set { this[ForcedServerVersionElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ConnectionInitializationSql" copy="true" />
    /// </summary>
    [ConfigurationProperty(ConnectionInitializationSqlElementName)]
    public string ConnectionInitializationSql
    {
      get { return (string) this[ConnectionInitializationSqlElementName]; }
      set { this[ConnectionInitializationSqlElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.IgnoreRules" copy="true"/>
    /// </summary>
    [ConfigurationProperty(IgnoreRulesElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<IgnoreRuleElement>), AddItemName = "rule")]
    public ConfigurationCollection<IgnoreRuleElement> IgnoreRules
    {
      get { return (ConfigurationCollection<IgnoreRuleElement>) this[IgnoreRulesElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.MultidatabaseKeys" copy="true"/>
    /// </summary>
    [ConfigurationProperty(MultidatabaseKeysElementName,
      DefaultValue = DomainConfiguration.DefaultMultidatabaseKeys)]
    public bool MultidatabaseKeys
    {
      get { return (bool) this[MultidatabaseKeysElementName]; }
      set { this[MultidatabaseKeysElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Options" copy="true"/>
    /// </summary>
    [ConfigurationProperty(OptionsElementName, DefaultValue = "Default")]
    public string Options
    {
      get { return (string) this[OptionsElementName]; }
      set { this[OptionsElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ShareStorageSchemaOverNodes" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ShareStorageSchemaOverNodesElementName, DefaultValue = false)]
    public bool ShareStorageSchemaOverNodes
    {
      get { return (bool) this[ShareStorageSchemaOverNodesElementName]; }
      set { this[ShareStorageSchemaOverNodesElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.NamingConvention" copy="true"/>
    /// </summary>
    [ConfigurationProperty(VersioningConventionElementName)]
    public VersioningConventionElement VersioningConvention
    {
      get { return (VersioningConventionElement)this[VersioningConventionElementName]; }
      set { this[VersioningConventionElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.EnsureConnectionisAlive" copy="true"/>
    /// </summary>
    [ConfigurationProperty(EnsureConnectionIsAliveElementName, DefaultValue = true)]
    public bool EnsureConnectionIsAlive
    {
      get { return (bool)this[EnsureConnectionIsAliveElementName]; }
      set { this[EnsureConnectionIsAliveElementName] = value; }
    }


    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="DomainConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public DomainConfiguration ToNative()
    {
      var config = new DomainConfiguration {
        Name = Name,
        ConnectionInfo = ConnectionInfoParser.GetConnectionInfo(CurrentConfiguration, ConnectionUrl, Provider, ConnectionString),
        NamingConvention = NamingConvention.ToNative(),
        KeyCacheSize = KeyCacheSize,
        KeyGeneratorCacheSize = KeyGeneratorCacheSize,
        QueryCacheSize = QueryCacheSize,
        RecordSetMappingCacheSize = RecordSetMappingCacheSize,
        DefaultSchema = DefaultSchema,
        DefaultDatabase = DefaultDatabase,
        UpgradeMode = ParseEnum<DomainUpgradeMode>(UpgradeMode),
        ForeignKeyMode = ParseEnum<ForeignKeyMode>(ForeignKeyMode),
        SchemaSyncExceptionFormat = ParseEnum<SchemaSyncExceptionFormat>(SchemaSyncExceptionFormat),
        Options = ParseEnum<DomainOptions>(Options),
        ServiceContainerType = ServiceContainerType.IsNullOrEmpty() ? null : Type.GetType(ServiceContainerType),
        IncludeSqlInExceptions = IncludeSqlInExceptions,
        BuildInParallel = BuildInParallel,
        AllowCyclicDatabaseDependencies = AllowCyclicDatabaseDependencies,
        ForcedServerVersion = ForcedServerVersion,
        Collation = Collation,
        ConnectionInitializationSql = ConnectionInitializationSql,
        MultidatabaseKeys = MultidatabaseKeys,
        ShareStorageSchemaOverNodes = ShareStorageSchemaOverNodes,
        EnsureConnectionIsAlive = EnsureConnectionIsAlive,
        FullTextChangeTrackingMode = ParseEnum<FullTextChangeTrackingMode>(FullTextChangeTrackingMode),
        VersioningConvention = VersioningConvention.ToNative()
      };

      foreach (var element in Types)
        config.Types.Register(element.ToNative());
      foreach (var element in Sessions)
        config.Sessions.Add(element.ToNative());
      foreach (var element in MappingRules)
        config.MappingRules.Add(element.ToNative());
      foreach (var element in Databases)
        config.Databases.Add(element.ToNative());
      foreach (var element in KeyGenerators)
        config.KeyGenerators.Add(element.ToNative());
      foreach (var element in IgnoreRules)
        config.IgnoreRules.Add(element.ToNative());

      return config;
    }

    private static T ParseEnum<T>(string value)
    {
      return (T) Enum.Parse(typeof (T), value, true);
    }
  }
}