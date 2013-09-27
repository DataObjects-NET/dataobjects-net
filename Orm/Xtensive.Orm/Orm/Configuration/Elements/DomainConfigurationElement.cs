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

    private const string SessionsElementName = "sessions";
    private const string TypeAliasesElementName = "typeAliases";
    private const string ServicesElementName = "services";
    private const string ValidationModeElementName = "validationMode";
    private const string ServiceContainerTypeElementName = "serviceContainerType";
    private const string IncludeSqlInExceptionsElementName = "includeSqlInExceptions";
    private const string ForcedServerVersionElementName = "forcedServerVersion";
    private const string SchemaSyncExceptionFormatElementName = "schemaSyncExceptionFormat";

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
    [ConfigurationCollection(typeof(ConfigurationCollection<SessionConfigurationElement>), AddItemName = "session")]
    public ConfigurationCollection<SessionConfigurationElement> Sessions
    {
      get { return (ConfigurationCollection<SessionConfigurationElement>) this[SessionsElementName]; }
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
    /// <see cref="DomainConfiguration.ForcedServerVersion" copy="true" />
    /// </summary>
    [ConfigurationProperty(ForcedServerVersionElementName)]
    public string ForcedServerVersion
    {
      get { return (string) this[ForcedServerVersionElementName]; }
      set { this[ForcedServerVersionElementName] = value; }
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
        ConnectionInfo = ConnectionInfoParser.GetConnectionInfo(ConnectionUrl, Provider, ConnectionString),
        NamingConvention = NamingConvention.ToNative(),
        KeyCacheSize = KeyCacheSize,
        KeyGeneratorCacheSize = KeyGeneratorCacheSize,
        QueryCacheSize = QueryCacheSize,
        RecordSetMappingCacheSize = RecordSetMappingCacheSize,
        AutoValidation = AutoValidation,
        DefaultSchema = DefaultSchema,
        ValidationMode = (ValidationMode) Enum.Parse(typeof (ValidationMode), ValidationMode, true),
        UpgradeMode = (DomainUpgradeMode) Enum.Parse(typeof (DomainUpgradeMode), UpgradeMode, true),
        ForeignKeyMode = (ForeignKeyMode) Enum.Parse(typeof (ForeignKeyMode), ForeignKeyMode, true),
        SchemaSyncExceptionFormat = (SchemaSyncExceptionFormat) Enum.Parse(typeof (SchemaSyncExceptionFormat), SchemaSyncExceptionFormat, true),
        ServiceContainerType = ServiceContainerType.IsNullOrEmpty() ? null : Type.GetType(ServiceContainerType),
        IncludeSqlInExceptions = IncludeSqlInExceptions,
        ForcedServerVersion = ForcedServerVersion,
      };

      foreach (var entry in Types)
        config.Types.Register(entry.ToNative());
      foreach (var session in Sessions)
        config.Sessions.Add(session.ToNative());
      return config;
    }
  }
}