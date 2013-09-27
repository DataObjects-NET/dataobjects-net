// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Configuration;
using Xtensive.Configuration;
using Xtensive.Core;

using Xtensive.Orm.Configuration.Elements;
using Xtensive.Orm.Configuration.Internals;

using ConfigurationSection=Xtensive.Orm.Configuration.Elements.ConfigurationSection;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// The configuration of the <see cref="Domain"/>.
  /// </summary> 
  [Serializable]
  public class DomainConfiguration : ConfigurationBase
  {
    #region Defaults

    /// <summary>
    /// Default <see cref="SectionName"/> value:
    /// "<see langword="Xtensive.Orm" />".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Orm";

    /// <summary>
    /// Default <see cref="DomainConfiguration.KeyCacheSize"/> value: 
    /// <see langword="16*1024" />.
    /// </summary>
    public const int DefaultKeyCacheSize = 16 * 1024;

    /// <summary>
    /// Default <see cref="DomainConfiguration.KeyGeneratorCacheSize"/> value: 
    /// <see langword="128" />.
    /// </summary>
    public const int DefaultKeyGeneratorCacheSize = 128;

    /// <summary>
    /// Default <see cref="DomainConfiguration.QueryCacheSize"/> value: 
    /// <see langword="256" />.
    /// </summary>
    public const int DefaultQueryCacheSize = 256;

    /// <summary>
    /// Default <see cref="DomainConfiguration.RecordSetMappingCacheSize"/> value: 
    /// <see langword="1024" />.
    /// </summary>
    public const int DefaultRecordSetMappingCacheSize = 1024;

    /// <summary>
    /// Default <see cref="DomainConfiguration.AutoValidation"/> value: 
    /// <see langword="true" />.
    /// </summary>
    public const bool DefaultAutoValidation = true;

    /// <summary>
    /// Default <see cref="DomainConfiguration.IncludeSqlInExceptions"/> value: 
    /// <see langword="true" />.
    /// </summary>
    public const bool DefaultIncludeSqlInExceptions = true;

    #endregion

    private static bool sectionNameIsDefined;
    private static string sectionName = DefaultSectionName;

    private string name = string.Empty;
    private ConnectionInfo connectionInfo;
    private string defaultSchema = string.Empty;
    private DomainTypeRegistry types = new DomainTypeRegistry(new DomainTypeRegistrationHandler());
    private LinqExtensionRegistry linqExtensions = new LinqExtensionRegistry();
    private NamingConvention namingConvention = new NamingConvention();
    private int keyCacheSize = DefaultKeyCacheSize;
    private int keyGeneratorCacheSize = DefaultKeyGeneratorCacheSize;
    private int queryCacheSize = DefaultQueryCacheSize;
    private int recordSetMappingCacheSize = DefaultRecordSetMappingCacheSize;
    private bool autoValidation = DefaultAutoValidation;
    private SessionConfigurationCollection sessions = new SessionConfigurationCollection();
    private DomainUpgradeMode upgradeMode = DomainUpgradeMode.Default;
    private ForeignKeyMode foreignKeyMode = ForeignKeyMode.Default;
    private ValidationMode validationMode = ValidationMode.Default;
    private Type serviceContainerType;
    private bool includeSqlInExceptions = DefaultIncludeSqlInExceptions;
    private string forcedServerVersion = string.Empty;
    private SchemaSyncExceptionFormat schemaSyncExceptionFormat = SchemaSyncExceptionFormat.Default;

    /// <summary>
    /// Gets or sets the name of the section where storage configuration is configuration.
    /// </summary>
    /// <exception cref="NotSupportedException">The property is already defined once.</exception>
    public static string SectionName
    {
      get { return sectionName; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        if (sectionNameIsDefined)
          throw Exceptions.AlreadyInitialized("SectionName");
        sectionName = value;
        sectionNameIsDefined = true;
      }
    }

    /// <summary>
    /// Gets or sets the domain configuration name.
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "Name");
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets the connection info.
    /// </summary>
    /// <example>
    /// <code lang="cs" source="..\Xtensive.Orm\Xtensive.Orm.Manual\DomainAndSession\DomainAndSessionSample.cs" region="Connection URL examples" />
    /// <code lang="cs">
    /// var configuration = new DomainConfiguration();
    /// configuration.ConnectionInfo = new ConnectionInfo(connectionUrl);
    /// </code>
    /// </example>
    public ConnectionInfo ConnectionInfo
    {
      get { return connectionInfo; }
      set
      {
        this.EnsureNotLocked();
        connectionInfo = value;
      }
    }

    /// <summary>
    /// Gets or sets the default schema.
    /// </summary>
    public string DefaultSchema
    {
      get { return defaultSchema; }
      set {
        this.EnsureNotLocked();
        defaultSchema = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating domain upgrade behavior. 
    /// </summary>
    public DomainUpgradeMode UpgradeMode
    {
      get { return upgradeMode; }
      set
      {
        this.EnsureNotLocked();
        upgradeMode = value;
      }
    }

    /// <summary>
    /// Gets the collection of persistent <see cref="Type"/>s that are about to be 
    /// registered in the <see cref="Domain"/>.
    /// </summary>
    public DomainTypeRegistry Types
    {
      get { return types; }
    }

    /// <summary>
    /// Gets the collection of LINQ extensions.
    /// </summary>
    public LinqExtensionRegistry LinqExtensions
    {
      get { return linqExtensions; }
    }

    /// <summary>
    /// Gets or sets the naming convention.
    /// </summary>
    public NamingConvention NamingConvention
    {
      get { return namingConvention; }
      set
      {
        this.EnsureNotLocked();
        namingConvention = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the key cache.
    /// Default value is <see cref="DefaultKeyCacheSize"/>.
    /// </summary>
    public int KeyCacheSize
    {
      get { return keyCacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsGreaterThan(value, 0, "value");
        keyCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the key generator cache.
    /// Default value is <see cref="DefaultKeyGeneratorCacheSize"/>.
    /// </summary>
    public int KeyGeneratorCacheSize
    {
      get { return keyGeneratorCacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsGreaterThan(value, 0, "value");
        keyGeneratorCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the query cache (see <see cref="Query.Execute{TElement}(System.Func{System.Linq.IQueryable{TElement}})"/>).
    /// Default value is <see cref="DefaultQueryCacheSize"/>.
    /// </summary>
    public int QueryCacheSize
    {
      get { return queryCacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsGreaterThan(value, 0, "value");
        queryCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the record set mapping cache.
    /// Default value is <see cref="DefaultRecordSetMappingCacheSize"/>.
    /// </summary>
    public int RecordSetMappingCacheSize
    {
      get { return recordSetMappingCacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsGreaterThan(value, 0, "value");
        recordSetMappingCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the value indicating whether changed entities should be validated or registered for validation automatically.
    /// Default value is <see cref="DomainConfigurationElement.AutoValidation"/>.
    /// </summary>
    public bool AutoValidation
    {
      get { return autoValidation; }
      set
      {
        this.EnsureNotLocked();
        autoValidation = value;
      }
    }

    /// <summary>
    /// Gets or sets the validation mode, that is used for validating entities within transactions.
    /// </summary>
    public ValidationMode ValidationMode
    {
      get { return validationMode; }
      set
      {
        this.EnsureNotLocked();
        validationMode = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating foreign key mode. 
    /// Default value is <see cref="Orm.ForeignKeyMode.Default"/>.
    /// </summary>
    public ForeignKeyMode ForeignKeyMode
    {
      get { return foreignKeyMode; }
      set
      {
        this.EnsureNotLocked();
        foreignKeyMode = value;
      }
    }

    /// <summary>
    /// Gets or sets <see cref="SchemaSynchronizationException"/> format.
    /// Default value is <see cref="Orm.Configuration.SchemaSyncExceptionFormat.Detailed"/>.
    /// To get old format that was used in DataObjects.Net prior to version 4.5
    /// set this to <see cref="Orm.Configuration.SchemaSyncExceptionFormat.Brief"/>.
    /// </summary>
    public SchemaSyncExceptionFormat SchemaSyncExceptionFormat
    {
      get { return schemaSyncExceptionFormat; }
      set
      {
        this.EnsureNotLocked();
        schemaSyncExceptionFormat = value;
      }
    }

    /// <summary>
    /// Gets available session configurations.
    /// </summary>
    public SessionConfigurationCollection Sessions
    {
      get { return sessions; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        sessions = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of the service container.
    /// </summary>
    public Type ServiceContainerType 
    {
      get { return serviceContainerType; }
      set {
        this.EnsureNotLocked();
        serviceContainerType = value;
      }
    }

    /// <summary>
    /// Gets or sets indicating whether SQL text of a query
    /// that caused error should be included in exception message.
    /// </summary>
    public bool IncludeSqlInExceptions
    {
      get { return includeSqlInExceptions; }
      set
      {
        this.EnsureNotLocked();
        includeSqlInExceptions = value;
      }
    }

    /// <summary>
    /// Gets or sets forced server version,
    /// if this property set to non-empty value,
    /// DataObjects.Net acts as it connected to server having
    /// specified version, ignoring actual version of the server.
    /// </summary>
    public string ForcedServerVersion
    {
      get { return forcedServerVersion; }
      set
      {
        this.EnsureNotLocked();
        forcedServerVersion = value;
      }
    }

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public override void Lock(bool recursive)
    {
      types.Lock(true);
      sessions.Lock(true);
      linqExtensions.Lock(true);
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public override void Validate()
    {
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new DomainConfiguration();
    }

    /// <summary>
    /// Copies the properties from the <paramref name="source"/>
    /// configuration to this one.
    /// Used by <see cref="ConfigurationBase.Clone"/> method implementation.
    /// </summary>
    /// <param name="source">The configuration to copy properties from.</param>
    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      var configuration = (DomainConfiguration) source;
      name = configuration.Name;
      connectionInfo = configuration.ConnectionInfo;
      defaultSchema = configuration.defaultSchema;
      types = (DomainTypeRegistry) configuration.Types.Clone();
      linqExtensions = (LinqExtensionRegistry) configuration.LinqExtensions.Clone();
      namingConvention = (NamingConvention) configuration.NamingConvention.Clone();
      keyCacheSize = configuration.KeyCacheSize;
      keyGeneratorCacheSize = configuration.KeyGeneratorCacheSize;
      queryCacheSize = configuration.QueryCacheSize;
      recordSetMappingCacheSize = configuration.RecordSetMappingCacheSize;
      sessions = (SessionConfigurationCollection) configuration.Sessions.Clone();
      upgradeMode = configuration.upgradeMode;
      autoValidation = configuration.autoValidation;
      validationMode = configuration.validationMode;
      foreignKeyMode = configuration.foreignKeyMode;
      serviceContainerType = configuration.serviceContainerType;
      includeSqlInExceptions = configuration.includeSqlInExceptions;
      forcedServerVersion = configuration.forcedServerVersion;
      schemaSyncExceptionFormat = configuration.schemaSyncExceptionFormat;
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>The clone of this configuration.</returns>
    public new DomainConfiguration Clone()
    {
      return (DomainConfiguration) base.Clone();
    }

    /// <summary>
    /// Loads the <see cref="DomainConfiguration"/> for <see cref="Domain"/>
    /// with the specified <paramref name="name"/>
    /// from application configuration file (section with <see cref="SectionName"/>).
    /// </summary>
    /// <param name="name">Name of the <see cref="Domain"/>.</param>
    /// <returns>
    /// The <see cref="DomainConfiguration"/> for the specified domain.
    /// </returns>
    /// <exception cref="InvalidOperationException">Section <see cref="SectionName"/>
    /// is not found in application configuration file, or there is no configuration for
    /// the <see cref="Domain"/> with specified <paramref name="name"/>.</exception>
    public static DomainConfiguration Load(string name)
    {
      return Load(SectionName, name);
    }

    /// <summary>
    /// Loads the <see cref="DomainConfiguration"/> for <see cref="Domain"/>
    /// with the specified <paramref name="name"/>
    /// from application configuration file (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="sectionName">Name of the section.</param>
    /// <param name="name">Name of the <see cref="Domain"/>.</param>
    /// <returns>
    /// The <see cref="DomainConfiguration"/> for the specified domain.
    /// </returns>
    /// <exception cref="InvalidOperationException">Section <paramref name="sectionName"/>
    /// is not found in application configuration file, or there is no configuration for
    /// the <see cref="Domain"/> with specified <paramref name="name"/>.</exception>
    public static DomainConfiguration Load(string sectionName, string name)
    {
      var section = (ConfigurationSection) ConfigurationManager.GetSection(sectionName);
      if (section==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      var domainElement = section.Domains[name];
      if (domainElement==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExConfigurationForDomainIsNotFoundInApplicationConfigurationFile, name, sectionName));
      return domainElement.ToNative();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="connectionUrl">The string containing connection URL for <see cref="Domain"/>.</param>
    public DomainConfiguration(string connectionUrl)
      : this()
    {
      connectionInfo = new ConnectionInfo(connectionUrl);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="connectionUrl">The connection URL.</param>
    public DomainConfiguration(UrlInfo connectionUrl)
      : this()
    {
      connectionInfo = new ConnectionInfo(connectionUrl);
    }

    /// <summary>
    ///	Initializes a new instance of this class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="connectionString">The connection string.</param>
    public DomainConfiguration(string provider, string connectionString)
      : this()
    {
      connectionInfo = new ConnectionInfo(provider, connectionString);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="connectionInfo">The connection info.</param>
    public DomainConfiguration(ConnectionInfo connectionInfo)
      : this()
    {
      this.connectionInfo = connectionInfo;
    }
    
    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public DomainConfiguration()
    {
      types.Register(typeof (Persistent).Assembly, typeof (Persistent).Namespace);
    }
  }
}