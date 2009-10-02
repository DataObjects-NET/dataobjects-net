// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Storage.Configuration.Elements
{
  /// <summary>
  /// <see cref="Domain"/> configuration element within a configuration file.
  /// </summary>
  public class DomainConfigurationElement : ConfigurationCollectionElementBase
  {
    private const string UpgradeModeElementName = "upgradeMode";
    private const string ForeignKeyModeElementName = "foreignKeyMode";
    private const string NameElementName = "name";
    private const string ConnectionUrlElementName = "connectionUrl";
    private const string TypesElementName = "types";
    private const string NamingConventionElementName = "namingConvention";
    private const string KeyCacheSizeElementName = "keyCacheSize";
    private const string KeyGeneratorCacheSizeElementName = "generatorCacheSize";
    private const string QueryCacheSizeElementName = "queryCacheSize";
    private const string SessionPoolSizeElementName = "sessionPoolSize";
    private const string RecordSetMappingCacheSizeElementName = "recordSetMappingCacheSizeSize";
    private const string AutoValidationElementName = "autoValidation";
    private const string InconsistentTransactionsElementName = "inconsistentTransactions";
    private const string SessionsElementName = "sessions";
    private const string CompilerContainersElementName = "compilerContainers";
    private const string TypeAliasesElementName = "typeAliases";
    private const string ServicesElementName = "services";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return Name; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true, IsRequired = false, DefaultValue = "")]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ConnectionInfo" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ConnectionUrlElementName, DefaultValue = null, IsRequired = true)]
    public string ConnectionUrl
    {
      get { return (string) this[ConnectionUrlElementName]; }
      set { this[ConnectionUrlElementName] = value; }
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
    [ConfigurationProperty(NamingConventionElementName, IsRequired = false)]
    public NamingConventionElement NamingConvention
    {
      get { return (NamingConventionElement) this[NamingConventionElementName]; }
      set { this[NamingConventionElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.KeyCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(KeyCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultKeyCacheSize, IsRequired = false)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int KeyCacheSize
    {
      get { return (int) this[KeyCacheSizeElementName]; }
      set { this[KeyCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.KeyGeneratorCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(KeyGeneratorCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultKeyGeneratorCacheSize, IsRequired = false)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int KeyGeneratorCacheSize
    {
      get { return (int) this[KeyGeneratorCacheSizeElementName]; }
      set { this[KeyGeneratorCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.QueryCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(QueryCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultQueryCacheSize, IsRequired = false)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int QueryCacheSize
    {
      get { return (int) this[QueryCacheSizeElementName]; }
      set { this[QueryCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.SessionPoolSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SessionPoolSizeElementName, DefaultValue = DomainConfiguration.DefaultSessionPoolSize, IsRequired = false)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int SessionPoolSize
    {
      get { return (int) this[SessionPoolSizeElementName]; }
      set { this[SessionPoolSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.RecordSetMappingCacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(RecordSetMappingCacheSizeElementName, DefaultValue = DomainConfiguration.DefaultRecordSetMappingCacheSize, IsRequired = false)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int RecordSetMappingCacheSize
    {
      get { return (int) this[RecordSetMappingCacheSizeElementName]; }
      set { this[RecordSetMappingCacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.AutoValidation" copy="true"/>
    /// </summary>
    [ConfigurationProperty(AutoValidationElementName, IsRequired = false, DefaultValue = DomainConfiguration.DefaultAutoValidation)]
    public bool AutoValidation
    {
      get { return (bool) this[AutoValidationElementName]; }
      set { this[AutoValidationElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.InconsistentTransactions" copy="true"/>
    /// </summary>
    [ConfigurationProperty(InconsistentTransactionsElementName, IsRequired = false, DefaultValue = DomainConfiguration.DefaultInconsistentTransactions)]
    public bool InconsistentTransactions
    {
      get { return (bool) this[InconsistentTransactionsElementName]; }
      set { this[InconsistentTransactionsElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.UpgradeMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(UpgradeModeElementName, IsRequired = false, DefaultValue = "Default")]
    public string UpgradeMode
    {
      get { return (string)this[UpgradeModeElementName]; }
      set { this[UpgradeModeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ForeignKeyMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ForeignKeyModeElementName, IsRequired = false, DefaultValue = "Default")]
    public string ForeignKeyMode
    {
      get { return (string)this[ForeignKeyModeElementName]; }
      set { this[ForeignKeyModeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Sessions" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SessionsElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<SessionElement>), AddItemName = "session")]
    public ConfigurationCollection<SessionElement> Sessions
    {
      get { return (ConfigurationCollection<SessionElement>) this[SessionsElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.CompilerContainers" copy="true"/>
    /// </summary>
    [ConfigurationProperty(CompilerContainersElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<TypeRegistrationElement>), AddItemName = "add")]
    public ConfigurationCollection<TypeRegistrationElement> CompilerContainers
    {
      get { return (ConfigurationCollection<TypeRegistrationElement>) base[CompilerContainersElementName]; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="DomainConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public DomainConfiguration ToNative()
    {
      var dc = new DomainConfiguration {
        Name = Name,
        ConnectionInfo = new UrlInfo(ConnectionUrl),
        NamingConvention = NamingConvention.ToNative(),
        KeyCacheSize = KeyCacheSize,
        KeyGeneratorCacheSize = KeyGeneratorCacheSize,
        QueryCacheSize = QueryCacheSize,
        SessionPoolSize = SessionPoolSize,
        RecordSetMappingCacheSize = RecordSetMappingCacheSize,
        AutoValidation = AutoValidation,
        InconsistentTransactions = InconsistentTransactions,
        UpgradeMode = (DomainUpgradeMode) Enum.Parse(typeof (DomainUpgradeMode), UpgradeMode, true),
        ForeignKeyMode = (ForeignKeyMode) Enum.Parse(typeof (ForeignKeyMode), ForeignKeyMode, true)
      };
      foreach (var entry in Types)
        dc.Types.Register(entry.ToNative());
      foreach (var entry in CompilerContainers)
        dc.CompilerContainers.Register(entry.ToNative());
      foreach (var session in Sessions)
        dc.Sessions.Add(session.ToNative());
      return dc;
    }
  }
}