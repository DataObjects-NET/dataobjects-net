// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
using System.Configuration;
using Xtensive.Collections.Configuration;
using Xtensive.Configuration;
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
    private const string AutoValidationElementName = "autoValidation";
    private const string DefaultSchemaElementName = "defaultSchema";
    private const string DefaultDatabaseElementName = "defaultDatabase";
    private const string SessionsElementName = "sessions";
    private const string MappingRulesElementName = "mappingRules";
    private const string DatabaseAliasesElementName = "databaseAliases";
    private const string TypeAliasesElementName = "typeAliases";
    private const string ServicesElementName = "services";
    private const string ValidationModeElementName = "validationMode";
    private const string ServiceContainerTypeElementName = "serviceContainerType";
    private const string IncludeSqlInExceptionsElementName = "includeSqlInExceptions";

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
    /// <see cref="DomainConfiguration.AutoValidation" copy="true"/>
    /// </summary>
    [ConfigurationProperty(AutoValidationElementName, DefaultValue = DomainConfiguration.DefaultAutoValidation)]
    public bool AutoValidation
    {
      get { return (bool) this[AutoValidationElementName]; }
      set { this[AutoValidationElementName] = value; }
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
    /// <see cref="DomainConfiguration.ForeignKeyMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ForeignKeyModeElementName, DefaultValue = "Default")]
    public string ForeignKeyMode
    {
      get { return (string) this[ForeignKeyModeElementName]; }
      set { this[ForeignKeyModeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ValidationMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ValidationModeElementName, DefaultValue = "Default")]
    public string ValidationMode
    {
      get { return (string) this[ValidationModeElementName]; }
      set { this[ValidationModeElementName] = value; }
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
    /// <see cref="DomainConfiguration.DatabaseAliases" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DatabaseAliasesElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<DatabaseAliasElement>), AddItemName = "alias")]
    public ConfigurationCollection<DatabaseAliasElement> DatabaseAliases
    {
      get { return (ConfigurationCollection<DatabaseAliasElement>) this[DatabaseAliasesElementName]; }
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
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="DomainConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public DomainConfiguration ToNative()
    {
      var connectionInfo = GetConnectionInfo();

      var config = new DomainConfiguration {
        Name = Name,
        ConnectionInfo = connectionInfo,
        NamingConvention = NamingConvention.ToNative(),
        KeyCacheSize = KeyCacheSize,
        KeyGeneratorCacheSize = KeyGeneratorCacheSize,
        QueryCacheSize = QueryCacheSize,
        RecordSetMappingCacheSize = RecordSetMappingCacheSize,
        AutoValidation = AutoValidation,
        DefaultSchema = DefaultSchema,
        DefaultDatabase = DefaultDatabase,
        ValidationMode = (ValidationMode) Enum.Parse(typeof (ValidationMode), ValidationMode, true),
        UpgradeMode = (DomainUpgradeMode) Enum.Parse(typeof (DomainUpgradeMode), UpgradeMode, true),
        ForeignKeyMode = (ForeignKeyMode) Enum.Parse(typeof (ForeignKeyMode), ForeignKeyMode, true),
        ServiceContainerType = ServiceContainerType.IsNullOrEmpty() ? null : Type.GetType(ServiceContainerType),
        IncludeSqlInExceptions = IncludeSqlInExceptions,
      };

      foreach (var element in Types)
        config.Types.Register(element.ToNative());
      foreach (var element in Sessions)
        config.Sessions.Add(element.ToNative());
      foreach (var element in MappingRules)
        config.MappingRules.Add(element.ToNative());
      foreach (var element in DatabaseAliases)
        config.DatabaseAliases.Add(element.ToNative());

      return config;
    }

    private ConnectionInfo GetConnectionInfo()
    {
      bool connectionUrlSpecified = !string.IsNullOrEmpty(ConnectionUrl);
      bool connectionStringSpecified = !string.IsNullOrEmpty(ConnectionString) && !string.IsNullOrEmpty(Provider);

      if (connectionUrlSpecified && connectionStringSpecified)
        throw new InvalidOperationException(
          Strings.ExConnectionInfoIsWrongYouShouldSetEitherConnectionUrlElementOrProviderAndConnectionStringElements);

      if (connectionUrlSpecified)
        return new ConnectionInfo(ConnectionUrl);

      string actualConnectionString = ConnectionString;
      if (connectionStringSpecified) {
        if (ConnectionString.StartsWith("#")) {
          string connectionStringName = ConnectionString.Substring(1);
          actualConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
          if (string.IsNullOrEmpty(actualConnectionString))
            throw new InvalidOperationException();
        }
        return new ConnectionInfo(Provider, actualConnectionString);
      }

      // Neither connection string, nor connection url specified.
      // Leave connection information undefined.
      return null;
    }
  }
}