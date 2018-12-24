// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Configuration;
using System.Linq;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Configuration.Elements;
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
    [Obsolete("Use WellKnown.DefaultConfigurationSection instead."), UsedImplicitly]
    public const string DefaultSectionName = WellKnown.DefaultConfigurationSection;

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
    /// Default <see cref="DomainConfiguration.IncludeSqlInExceptions"/> value: 
    /// <see langword="true" />.
    /// </summary>
    public const bool DefaultIncludeSqlInExceptions = true;

    /// <summary>
    /// Default <see cref="DomainConfiguration.BuildInParallel"/> value: 
    /// <see langword="true" />.
    /// </summary>
    public const bool DefaultBuildInParallel = true;

    /// <summary>
    /// Default <see cref="MultidatabaseKeys"/> value:
    /// <see langword="false" />.
    /// </summary>
    public const bool DefaultMultidatabaseKeys = false;

    /// <summary>
    /// Default <see cref="ShareStorageSchemaOverNodes"/> value:
    /// <see langword="false"/>
    /// </summary>
    public const bool DefaultShareStorageSchemaOverNodes = false;

    #endregion

    private static bool sectionNameIsDefined;
    private static string sectionName = WellKnown.DefaultConfigurationSection;

    private string name = string.Empty;
    private ConnectionInfo connectionInfo;
    private string defaultSchema = string.Empty;
    private string defaultDatabase = string.Empty;
    private DomainTypeRegistry types = new DomainTypeRegistry(new DomainTypeRegistrationHandler());
    private LinqExtensionRegistry linqExtensions = new LinqExtensionRegistry();
    private NamingConvention namingConvention = new NamingConvention();
    private int keyCacheSize = DefaultKeyCacheSize;
    private int keyGeneratorCacheSize = DefaultKeyGeneratorCacheSize;
    private int queryCacheSize = DefaultQueryCacheSize;
    private int recordSetMappingCacheSize = DefaultRecordSetMappingCacheSize;
    private SessionConfigurationCollection sessions = new SessionConfigurationCollection();
    private DomainUpgradeMode upgradeMode = DomainUpgradeMode.Default;
    private ForeignKeyMode foreignKeyMode = ForeignKeyMode.Default;
    private FullTextChangeTrackingMode fullTextChangeTrackingMode = FullTextChangeTrackingMode.Default;
    private Type serviceContainerType;
    private bool includeSqlInExceptions = DefaultIncludeSqlInExceptions;
    private string forcedServerVersion = string.Empty;
    private bool buildInParallel = DefaultBuildInParallel;
    private bool allowCyclicDatabaseDependencies;
    private bool multidatabaseKeys = DefaultMultidatabaseKeys;
    private bool shareStorageSchemaOverNodes = DefaultShareStorageSchemaOverNodes;
    private DomainOptions options = DomainOptions.Default;
    private SchemaSyncExceptionFormat schemaSyncExceptionFormat = SchemaSyncExceptionFormat.Default;
    private MappingRuleCollection mappingRules = new MappingRuleCollection();
    private DatabaseConfigurationCollection databases = new DatabaseConfigurationCollection();
    private KeyGeneratorConfigurationCollection keyGenerators = new KeyGeneratorConfigurationCollection();
    private IgnoreRuleCollection ignoreRules = new IgnoreRuleCollection();

    

    private bool? isMultidatabase;
    private bool? isMultischema;

    private string collation = string.Empty;
    private string nativeLibraryCacheFolder = string.Empty;
    private string connectionInitializationSql = string.Empty;

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
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
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
    /// Gets or sets the default database.
    /// If database aliases are configured, this should be an alias name.
    /// </summary>
    public string DefaultDatabase
    {
      get { return defaultDatabase; }
      set {
        this.EnsureNotLocked();
        defaultDatabase = value;
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
    /// Gets or sets a value indicating change tracking mode of full-text indexes.
    /// The property may have no effect for certain storages where there is no support for such option.
    /// </summary>
    public FullTextChangeTrackingMode FullTextChangeTrackingMode
    {
      get { return fullTextChangeTrackingMode; }
      set
      {
        this.EnsureNotLocked();
        fullTextChangeTrackingMode = value;
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
    /// Gets or sets registered mapping rules.
    /// </summary>
    public MappingRuleCollection MappingRules
    {
      get { return mappingRules; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        mappingRules = value;
      }
    }

    /// <summary>
    /// Gets or sets registered database aliases.
    /// </summary>
    public DatabaseConfigurationCollection Databases
    {
      get { return databases; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        databases = value;
      }
    }

    /// <summary>
    /// Gets or sets key generators.
    /// </summary>
    public KeyGeneratorConfigurationCollection KeyGenerators
    {
      get { return keyGenerators; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        keyGenerators = value;
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
    /// Gets or sets value indicating whether SQL text of a query
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
    /// Gets or sets value indicating whether
    /// cyclic database dependencies are allowed.
    /// This option has no effect unless <see cref="IsMultidatabase"/> is true.
    /// </summary>
    public bool AllowCyclicDatabaseDependencies
    {
      get { return allowCyclicDatabaseDependencies; }
      set
      {
        this.EnsureNotLocked();
        allowCyclicDatabaseDependencies = value;
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
    /// Gets or sets value indicating whether <see cref="Domain.Build"/>
    /// process should be parallelized whenever possible.
    /// </summary>
    public bool BuildInParallel
    {
      get { return buildInParallel; }
      set
      {
        this.EnsureNotLocked();
        buildInParallel = value;
      }
    }

    /// <summary>
    /// Get or set registered ignore rules
    /// </summary>
    public IgnoreRuleCollection IgnoreRules
    {
      get { return ignoreRules; }
      set
      {
        this.EnsureNotLocked();
        ignoreRules = value;
      }
    }

    /// <summary>
    /// Gets or sets collation for all columns.
    /// If provider does not utilize collations this setting is ignored.
    /// <remarks>
    /// Only 'sqlserver', 'sqlserverce' and 'sqlite' providers support this setting.
    /// For 'sqlite' provider the following non-standard collations are supported
    /// in addition to any user-provided collations:
    /// StringComparer_Ordinal, StringComparer_Ordinal_IgnoreCase,
    /// StringComparer_CurrentCulture, StringComparer_CurrentCulture_IgnoreCase,
    /// StringComparer_InvariantCulture, StringComparer_InvariantCulture_IgnoreCase.
    /// </remarks>
    /// </summary>
    public string Collation
    {
      get { return collation; }
      set
      {
        this.EnsureNotLocked();
        collation = value;
      }
    }

    /// <summary>
    /// Gets or sets connection initialization SQL script.
    /// This script is executed for each created connection
    /// (including system connections) just after connection has been opened.
    /// </summary>
    public string ConnectionInitializationSql
    {
      get { return connectionInitializationSql; }
      set
      {
        this.EnsureNotLocked();
        connectionInitializationSql = value;
      }
    }

    /// <summary>
    /// Gets or sets multidatabase key mode.
    /// In this mode keys generated for entities in different databases
    /// are treated as compatible. Enable this option if you want to
    /// implement persistent interfaces by entities mapped to different databases.
    /// </summary>
    public bool MultidatabaseKeys
    {
      get { return multidatabaseKeys; }
      set
      {
        this.EnsureNotLocked();
        multidatabaseKeys = value;
      }
    }

    /// <summary>
    /// Gets or sets domain options.
    /// </summary>
    public DomainOptions Options
    {
      get { return options; }
      set
      {
        this.EnsureNotLocked();
        options = value;
      }
    }

    /// <summary>
    /// Enables sharing of catalog (or catalogs) of default node over additional nodes.
    /// Such sharing leads to overall decrease in nodes memory consumption.
    /// <para>NOTICE! When this option is set to <see langword="true"/> 
    /// real names of catalogs or schemas of certain <see cref="StorageNode"/> will be calculated on query translation
    /// according to <see cref="NodeConfiguration.DatabaseMapping"/> and <see cref="NodeConfiguration.SchemaMapping"/> of the Storage Node.
    /// </para>
    /// </summary>
    public bool ShareStorageSchemaOverNodes
    {
      get { return shareStorageSchemaOverNodes; }
      set {
        this.EnsureNotLocked();
        shareStorageSchemaOverNodes = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this configuration is multi-database.
    /// </summary>
    public bool IsMultidatabase { get { return isMultidatabase ?? GetIsMultidatabase(); } }

    /// <summary>
    /// Gets a value indicating whether this configuration is multi-schema.
    /// </summary>
    public bool IsMultischema { get { return isMultischema ?? GetIsMultischema(); } }

    

    private bool GetIsMultidatabase()
    {
      return !string.IsNullOrEmpty(DefaultDatabase)
        || MappingRules.Any(rule => !string.IsNullOrEmpty(rule.Database));
    }

    private bool GetIsMultischema()
    {
      return !string.IsNullOrEmpty(DefaultSchema)
        || MappingRules.Any(rule => !string.IsNullOrEmpty(rule.Schema))
        || GetIsMultidatabase();
    }

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public override void Lock(bool recursive)
    {
      var multischema = GetIsMultischema();
      var multidatabase = GetIsMultidatabase();

      // This couldn't be done in Validate() method
      // because override sequence of Lock() is so broken.
      ValidateMappingConfiguration(multischema, multidatabase);
      ValidateIgnoreConfiguration();

      types.Lock(true);
      sessions.Lock(true);
      linqExtensions.Lock(true);
      databases.Lock(true);
      mappingRules.Lock(true);
      keyGenerators.Lock(true);
      ignoreRules.Lock(true);

      base.Lock(recursive);

      // Everything locked fine, commit the flags
      isMultischema = multischema;
      isMultidatabase = multidatabase;
      multidatabaseKeys = multidatabaseKeys && multidatabase;
    }

    private void ValidateMappingConfiguration(bool multischema, bool multidatabase)
    {
      if (multischema && string.IsNullOrEmpty(DefaultSchema))
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaShouldBeSpecifiedWhenMultischemaOrMultidatabaseModeIsActive);
      
      if (multidatabase && (string.IsNullOrEmpty(DefaultDatabase) || string.IsNullOrEmpty(DefaultSchema)))
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaAndDefaultDatabaseShouldBeSpecifiedWhenMultidatabaseModeIsActive);
    }

    private void ValidateIgnoreConfiguration()
    {
      foreach (var ignoreRule in IgnoreRules) {
        if (string.IsNullOrEmpty(ignoreRule.Table) && string.IsNullOrEmpty(ignoreRule.Column))
          throw new InvalidOperationException(string.Format(Strings.ExIgnoreRuleXMustBeAppliedToColumnOrTable, ignoreRule));
      }
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
      defaultSchema = configuration.DefaultSchema;
      defaultDatabase = configuration.DefaultDatabase;
      types = (DomainTypeRegistry) configuration.Types.Clone();
      linqExtensions = (LinqExtensionRegistry) configuration.LinqExtensions.Clone();
      namingConvention = (NamingConvention) configuration.NamingConvention.Clone();
      keyCacheSize = configuration.KeyCacheSize;
      keyGeneratorCacheSize = configuration.KeyGeneratorCacheSize;
      queryCacheSize = configuration.QueryCacheSize;
      recordSetMappingCacheSize = configuration.RecordSetMappingCacheSize;
      sessions = (SessionConfigurationCollection) configuration.Sessions.Clone();
      upgradeMode = configuration.UpgradeMode;
      foreignKeyMode = configuration.ForeignKeyMode;
      serviceContainerType = configuration.ServiceContainerType;
      includeSqlInExceptions = configuration.IncludeSqlInExceptions;
      forcedServerVersion = configuration.ForcedServerVersion;
      buildInParallel = configuration.BuildInParallel;
      allowCyclicDatabaseDependencies = configuration.AllowCyclicDatabaseDependencies;
      collation = configuration.Collation;
      connectionInitializationSql = configuration.ConnectionInitializationSql;
      schemaSyncExceptionFormat = configuration.SchemaSyncExceptionFormat;
      multidatabaseKeys = configuration.MultidatabaseKeys;
      options = configuration.Options;
      databases = (DatabaseConfigurationCollection) configuration.Databases.Clone();
      mappingRules = (MappingRuleCollection) configuration.MappingRules.Clone();
      keyGenerators = (KeyGeneratorConfigurationCollection) configuration.KeyGenerators.Clone();
      ignoreRules = (IgnoreRuleCollection) configuration.IgnoreRules.Clone();
      shareStorageSchemaOverNodes = configuration.ShareStorageSchemaOverNodes;
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
      var section = (ConfigurationSection)ConfigurationManager.GetSection(sectionName);
      if (section == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      return LoadConfigurationFromSection(section, name);
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
    public static DomainConfiguration Load(System.Configuration.Configuration configuration, string name)
    {
      return Load(configuration, SectionName, name);
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
    public static DomainConfiguration Load(System.Configuration.Configuration configuration, string sectionName, string name)
    {
      var section = (ConfigurationSection) configuration.GetSection(sectionName);
      if (section==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      return LoadConfigurationFromSection(section, name);
    }

    internal bool Supports(DomainOptions optionsToCheck)
    {
      return (options & optionsToCheck)==optionsToCheck;
    }

    internal bool Supports(ForeignKeyMode modeToCheck)
    {
      return (foreignKeyMode & modeToCheck)==modeToCheck;
    }


    private static DomainConfiguration LoadConfigurationFromSection(ConfigurationSection section, string name)
    {
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