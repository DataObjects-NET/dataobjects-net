// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
using System.Configuration;
using System.Reflection;
using Microsoft.Practices.Unity.Configuration;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Building;

namespace Xtensive.Storage.Configuration.Elements
{
  /// <summary>
  /// <see cref="Domain"/> configuration element within a configuration file.
  /// </summary>
  public class DomainConfigurationElement : ConfigurationCollectionElementBase
  {
    private const string BuildModeElementName = "buildMode";
    private const string ForeignKeyModeElementName = "foreignKeyMode";
    private const string NameElementName = "name";
    private const string ConnectionUrlElementName = "connectionUrl";
    private const string TypesElementName = "types";
    private const string BuildersElementName = "builders";
    private const string NamingConventionElementName = "namingConvention";
    private const string KeyCacheSizeElementName = "keyCacheSize";
    private const string KeyGeneratorCacheSizeElementName = "generatorCacheSize";
    private const string SessionPoolSizeElementName = "sessionPoolSize";
    private const string RecordSetMappingCacheSizeElementName = "recordSetMappingCacheSizeSize";
    private const string AutoValidationElementName = "autoValidation";
    private const string InconsistentTransactionsElementName = "inconsistentTransactions";
    private const string SessionsElementName = "sessions";
    private const string MappingsElementName = "mappings";
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
    [ConfigurationCollection(typeof (ConfigurationCollection<TypeElement>), AddItemName = "add")]
    public ConfigurationCollection<TypeElement> Types
    {
      get { return (ConfigurationCollection<TypeElement>) base[TypesElementName]; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.Builders" copy="true"/>
    /// </summary>
    [ConfigurationProperty(BuildersElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<BuilderElement>), AddItemName = "builder")]
    public ConfigurationCollection<BuilderElement> Builders
    {
      get { return (ConfigurationCollection<BuilderElement>) base[BuildersElementName]; }
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
    /// <see cref="DomainConfiguration.BuildMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(BuildModeElementName, IsRequired = false, DefaultValue = DomainConfiguration.DefaultBuildMode)]
    public DomainBuildMode BuildMode
    {
      get { return (DomainBuildMode)this[BuildModeElementName]; }
      set { this[BuildModeElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ForeignKeyMode" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ForeignKeyModeElementName, IsRequired = false, DefaultValue = DomainConfiguration.DefaultForeignKeyMode)]
    public ForeignKeyMode ForeignKeyMode
    {
      get { return (ForeignKeyMode)this[ForeignKeyModeElementName]; }
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
    [ConfigurationProperty(MappingsElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<MappingElement>), AddItemName = "mapping")]
    public ConfigurationCollection<MappingElement> Mappings
    {
      get { return (ConfigurationCollection<MappingElement>)this[MappingsElementName]; }
    }

    /// <summary>
    /// Provides access to the type alias information in the section.
    /// </summary>
    [ConfigurationProperty(TypeAliasesElementName, IsRequired = false)]
    [ConfigurationCollection(typeof(UnityTypeAliasCollection), AddItemName = "typeAlias")]
    public UnityTypeAliasCollection TypeAliases
    {
      get { return (UnityTypeAliasCollection)base[TypeAliasesElementName]; }
    }


    /// <summary>
    /// A <see cref="ConfigurationElement" /> that stores the configuration information
    /// for a services provided by <see cref="Microsoft.Practices.Unity.IUnityContainer" />.
    /// </summary>
    [ConfigurationProperty(ServicesElementName)]
    [ConfigurationCollection(typeof(UnityTypeElementCollection), AddItemName = "service")]
    public UnityTypeElementCollection Services
    {
      get
      {
        var services = (UnityTypeElementCollection)base[ServicesElementName];
        services.TypeResolver = new UnityTypeResolver(TypeAliases);
        return services;
      }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="DomainConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public DomainConfiguration ToNative()
    {
      var c = new DomainConfiguration();
      c.Name = Name;
      c.ConnectionInfo = new UrlInfo(ConnectionUrl);
      c.NamingConvention = NamingConvention.ToNative();
      c.KeyCacheSize = KeyCacheSize;
      c.KeyGeneratorCacheSize = KeyGeneratorCacheSize;
      c.SessionPoolSize = SessionPoolSize;
      c.RecordSetMappingCacheSize = RecordSetMappingCacheSize;
      c.AutoValidation = AutoValidation;
      c.InconsistentTransactions = InconsistentTransactions;
      c.BuildMode = BuildMode;
      c.ForeignKeyMode = ForeignKeyMode;
      foreach (var builder in Builders) {
        var type = Type.GetType(builder.Type, true);
        c.Builders.Add(type);
      }
      foreach (var entry in Types) {
        var assembly = Assembly.Load(entry.Assembly);
        if (entry.Namespace.IsNullOrEmpty())
          c.Types.Register(assembly);
        else
          c.Types.Register(assembly, entry.Namespace);
      }
      foreach (var session in Sessions)
        c.Sessions.Add(session.ToNative());

      foreach (var mapping in Mappings) {
        var mappingConfiguration = mapping.ToNative();
        var assembly = Assembly.Load(mappingConfiguration.Assembly);
        c.CompilerContainers.Register(assembly);
      }

      c.ServicesConfiguration = Services;      

      return c;
    }
  }
}