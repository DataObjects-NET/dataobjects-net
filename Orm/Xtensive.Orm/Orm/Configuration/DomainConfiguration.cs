// Copyright (C) 2007-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Configuration;
using System.Linq;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Configuration.Elements;
using Xtensive.Orm.Internals;
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

    /// <summary>
    /// Default <see cref="AllowCyclicDatabaseDependencies" /> value: <see langword="false" />
    /// </summary>
    public const bool DefaultAllowCyclicDatabaseDependencies = false;

    /// <summary>
    /// Default <see cref="EnsureConnectionIsAlive"/> value: <see langword="true" />.
    /// </summary>
    public const bool DefaultEnsureConnectionIsAlive = true;

    /// <summary>
    /// Default <see cref="EntityVersioningPolicy"/> value;
    /// </summary>
    [Obsolete("Use VersioningConvention.DefaultVersioningPolicy")]
    public const EntityVersioningPolicy DefaultVersioningPolicy = EntityVersioningPolicy.Default;

    /// <summary>
    /// Default <see cref="UpgradeMode"/> value.
    /// </summary>
    public const DomainUpgradeMode DefaultUpgradeMode = DomainUpgradeMode.Default;

    /// <summary>
    /// Default <see cref="ForeignKeyMode"/> value.
    /// </summary>
    public const ForeignKeyMode DefauktForeignKeyMode = ForeignKeyMode.Default;

    /// <summary>
    /// Default <see cref="FullTextChangeTrackingMode"/> value.
    /// </summary>
    public const FullTextChangeTrackingMode DefaultFullTextChangeTrackingMode = FullTextChangeTrackingMode.Default;

    /// <summary>
    /// Default <see cref="Options"/> value.
    /// </summary>
    public const DomainOptions DefaultDomainOptions = DomainOptions.Default;

    /// <summary>
    /// Default <see cref="SchemaSyncExceptionFormat"/> value.
    /// </summary>
    public const SchemaSyncExceptionFormat DefaultSchemaSyncExceptionFormat = SchemaSyncExceptionFormat.Default;

    /// <summary>
    /// Default <see cref="TagsLocation"/> value.
    /// </summary>
    public const TagsLocation DefaultTagLocation = TagsLocation.Default;
    
    /// <summary>
    /// Default <see cref="PreferTypeIdsAsQueryParameters"/> value: <see langword="true" />.
    /// </summary>
    public const bool DefaultPreferTypeIdsAsQueryParameters = true;

    #endregion

    private static bool sectionNameIsDefined;
    private static string sectionName = WellKnown.DefaultConfigurationSection;

    private string name = string.Empty;
    private ConnectionInfo connectionInfo;
    private string defaultSchema = string.Empty;
    private string defaultDatabase = string.Empty;
    private DomainTypeRegistry types = new(new DomainTypeRegistrationHandler());
    private LinqExtensionRegistry linqExtensions = new();
    private SessionConfigurationCollection sessions = new();
    private MappingRuleCollection mappingRules = new();
    private DatabaseConfigurationCollection databases = new();
    private KeyGeneratorConfigurationCollection keyGenerators = new();
    private IgnoreRuleCollection ignoreRules = new();
    private NamingConvention namingConvention = new();
    private VersioningConvention versioningConvention = new();
    private int keyCacheSize = DefaultKeyCacheSize;
    private int keyGeneratorCacheSize = DefaultKeyGeneratorCacheSize;
    private int queryCacheSize = DefaultQueryCacheSize;
    private int recordSetMappingCacheSize = DefaultRecordSetMappingCacheSize;
    private int maxNumberOfConditons = WellKnown.DefaultMaxNumberOfConditions;
    private Type serviceContainerType;
    private string forcedServerVersion = string.Empty;
    private bool includeSqlInExceptions = DefaultIncludeSqlInExceptions;
    private bool buildInParallel = DefaultBuildInParallel;
    private bool allowCyclicDatabaseDependencies;
    private bool multidatabaseKeys = DefaultMultidatabaseKeys;
    private bool shareStorageSchemaOverNodes = DefaultShareStorageSchemaOverNodes;
    private bool ensureConnectionIsAlive = DefaultEnsureConnectionIsAlive;
    private bool preferTypeIdsAsQueryParameters = DefaultPreferTypeIdsAsQueryParameters;
    private DomainUpgradeMode upgradeMode = DefaultUpgradeMode;
    private ForeignKeyMode foreignKeyMode = DefauktForeignKeyMode;
    private FullTextChangeTrackingMode fullTextChangeTrackingMode = DefaultFullTextChangeTrackingMode;
    private DomainOptions options = DefaultDomainOptions;
    private SchemaSyncExceptionFormat schemaSyncExceptionFormat = DefaultSchemaSyncExceptionFormat;
    private TagsLocation tagsLocation = DefaultTagLocation;

    private bool? isMultidatabase;
    private bool? isMultischema;

    private string collation = string.Empty;
    private string connectionInitializationSql = string.Empty;

    /// <summary>
    /// Gets or sets the name of the section where storage configuration is configuration.
    /// </summary>
    /// <exception cref="NotSupportedException">The property is already defined once.</exception>
    public static string SectionName
    {
      get => sectionName;
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        if (sectionNameIsDefined) {
          throw Exceptions.AlreadyInitialized(nameof(SectionName));
        }
        sectionName = value;
        sectionNameIsDefined = true;
      }
    }

    /// <summary>
    /// Gets or sets the domain configuration name.
    /// </summary>
    public string Name
    {
      get => name;
      set {
        EnsureNotLocked();
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
      get => connectionInfo;
      set {
        EnsureNotLocked();
        connectionInfo = value;
      }
    }

    /// <summary>
    /// Gets or sets the default schema.
    /// </summary>
    public string DefaultSchema
    {
      get => defaultSchema;
      set {
        EnsureNotLocked();
        defaultSchema = value;
      }
    }

    /// <summary>
    /// Gets or sets the default database.
    /// If database aliases are configured, this should be an alias name.
    /// </summary>
    public string DefaultDatabase
    {
      get => defaultDatabase;
      set {
        EnsureNotLocked();
        defaultDatabase = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating domain upgrade behavior. 
    /// </summary>
    public DomainUpgradeMode UpgradeMode
    {
      get => upgradeMode;
      set {
        EnsureNotLocked();
        upgradeMode = value;
      }
    }

    /// <summary>
    /// Gets the collection of persistent <see cref="Type"/>s that are about to be 
    /// registered in the <see cref="Domain"/>.
    /// </summary>
    public DomainTypeRegistry Types => types;

    /// <summary>
    /// Gets the collection of LINQ extensions.
    /// </summary>
    public LinqExtensionRegistry LinqExtensions => linqExtensions;

    /// <summary>
    /// Gets or sets the naming convention.
    /// </summary>
    public NamingConvention NamingConvention
    {
      get => namingConvention;
      set {
        EnsureNotLocked();
        namingConvention = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the key cache.
    /// Default value is <see cref="DefaultKeyCacheSize"/>.
    /// </summary>
    public int KeyCacheSize
    {
      get => keyCacheSize;
      set {
        EnsureNotLocked();
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
      get => keyGeneratorCacheSize;
      set {
        EnsureNotLocked();
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
      get => queryCacheSize;
      set {
        EnsureNotLocked();
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
      get => recordSetMappingCacheSize;
      set {
        EnsureNotLocked();
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
      get => foreignKeyMode;
      set {
        EnsureNotLocked();
        foreignKeyMode = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating change tracking mode of full-text indexes.
    /// The property may have no effect for certain storages where there is no support for such option.
    /// </summary>
    public FullTextChangeTrackingMode FullTextChangeTrackingMode
    {
      get => fullTextChangeTrackingMode;
      set {
        EnsureNotLocked();
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
      get => schemaSyncExceptionFormat;
      set {
        EnsureNotLocked();
        schemaSyncExceptionFormat = value;
      }
    }

    /// <summary>
    /// Gets available session configurations.
    /// </summary>
    public SessionConfigurationCollection Sessions
    {
      get => sessions;
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        EnsureNotLocked();
        sessions = value;
      }
    }

    /// <summary>
    /// Gets or sets registered mapping rules.
    /// </summary>
    public MappingRuleCollection MappingRules
    {
      get => mappingRules;
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        EnsureNotLocked();
        mappingRules = value;
      }
    }

    /// <summary>
    /// Gets or sets registered database aliases.
    /// </summary>
    public DatabaseConfigurationCollection Databases
    {
      get => databases;
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        EnsureNotLocked();
        databases = value;
      }
    }

    /// <summary>
    /// Gets or sets key generators.
    /// </summary>
    public KeyGeneratorConfigurationCollection KeyGenerators
    {
      get => keyGenerators;
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        EnsureNotLocked();
        keyGenerators = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of the service container.
    /// </summary>
    public Type ServiceContainerType
    {
      get => serviceContainerType;
      set {
        EnsureNotLocked();
        serviceContainerType = value;
      }
    }

    /// <summary>
    /// Gets or sets value indicating whether SQL text of a query
    /// that caused error should be included in exception message.
    /// </summary>
    public bool IncludeSqlInExceptions
    {
      get => includeSqlInExceptions;
      set {
        EnsureNotLocked();
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
      get => allowCyclicDatabaseDependencies;
      set {
        EnsureNotLocked();
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
      get => forcedServerVersion;
      set {
        EnsureNotLocked();
        forcedServerVersion = value;
      }
    }

    /// <summary>
    /// Gets or sets value indicating whether <see cref="Domain.Build"/>
    /// process should be parallelized whenever possible.
    /// </summary>
    public bool BuildInParallel
    {
      get => buildInParallel;
      set {
        EnsureNotLocked();
        buildInParallel = value;
      }
    }

    /// <summary>
    /// Get or set registered ignore rules
    /// </summary>
    public IgnoreRuleCollection IgnoreRules
    {
      get => ignoreRules;
      set {
        EnsureNotLocked();
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
      get => collation;
      set {
        EnsureNotLocked();
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
      get => connectionInitializationSql;
      set {
        EnsureNotLocked();
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
      get => multidatabaseKeys;
      set {
        EnsureNotLocked();
        multidatabaseKeys = value;
      }
    }

    /// <summary>
    /// Gets or sets domain options.
    /// </summary>
    public DomainOptions Options
    {
      get => options;
      set {
        EnsureNotLocked();
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
      get => shareStorageSchemaOverNodes;
      set {
        EnsureNotLocked();
        shareStorageSchemaOverNodes = value;
      }
    }

    /// <summary>
    /// Gets or sets versioning convention.
    /// </summary>
    public VersioningConvention VersioningConvention
    {
      get => versioningConvention;
      set {
        EnsureNotLocked();
        versioningConvention = value;
      }
    }

    /// <summary>
    /// Enables extra check if connection is not broken on its opening.
    /// </summary>
    public bool EnsureConnectionIsAlive
    {
      get => ensureConnectionIsAlive;
      set {
        EnsureNotLocked();
        ensureConnectionIsAlive = value;
      }
    }

    /// <summary>
    /// Makes queries use parameters instead of constant values for persistent type identifiers.
    /// </summary>
    public bool PreferTypeIdsAsQueryParameters
    {
      get { return preferTypeIdsAsQueryParameters; }
      set {
        EnsureNotLocked();
        preferTypeIdsAsQueryParameters = value;
      }
    }

    /// <summary>
    /// Defines where tags will be placed when used within queries.
    /// </summary>
    public TagsLocation TagsLocation
    {
      get => tagsLocation;
      set {
        EnsureNotLocked();
        tagsLocation = value;
      }
    }

    /// <summary>
    /// Maximam number of filtering values in IN clause which are
    /// to be placed inside a resulted SQL command (as boolean predicate).
    /// Affects only <see cref="QueryableExtensions.In{T}(T, T[])"/> group of methods with 
    /// <see cref="IncludeAlgorithm.Auto"/>. If collection of parameters has more items
    /// than this parameter allows then temporary table will be used to store values.
    /// Default value is <see cref="WellKnown.DefaultMaxNumberOfConditions"/>
    /// </summary>
    /// <remarks>
    /// Some RDBMSs may have limitations for number of values in IN clause or for
    /// overall number of parameters of SQL command. Increasing of this paramter may
    /// cause you less IN clauses within one SQL command for the RDBMSs that limits overall
    /// parameters count and decreasing it may allow you to have more of them, but it also changes
    /// limit when temproary table will be chosen to store items.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Current value is not in allowed range of values.</exception>
    public int MaxNumberOfConditions
    {
      get => maxNumberOfConditons;
      set {
        EnsureNotLocked();
        maxNumberOfConditons = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this configuration is multi-database.
    /// </summary>
    public bool IsMultidatabase => isMultidatabase ?? GetIsMultidatabase();

    /// <summary>
    /// Gets a value indicating whether this configuration is multi-schema.
    /// </summary>
    public bool IsMultischema => isMultischema ?? GetIsMultischema();

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
      versioningConvention.Lock(true);

      base.Lock(recursive);

      // Everything locked fine, commit the flags
      isMultischema = multischema;
      isMultidatabase = multidatabase;
      multidatabaseKeys = multidatabaseKeys && multidatabase;
    }

    private void ValidateMappingConfiguration(bool multischema, bool multidatabase)
    {
      if (multischema && string.IsNullOrEmpty(DefaultSchema)) {
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaShouldBeSpecifiedWhenMultischemaOrMultidatabaseModeIsActive);
      }

      if (multidatabase && (string.IsNullOrEmpty(DefaultDatabase) || string.IsNullOrEmpty(DefaultSchema))) {
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaAndDefaultDatabaseShouldBeSpecifiedWhenMultidatabaseModeIsActive);
      }
    }

    private void ValidateIgnoreConfiguration()
    {
      foreach (var ignoreRule in IgnoreRules) {
        if (string.IsNullOrEmpty(ignoreRule.Table) && string.IsNullOrEmpty(ignoreRule.Column) && string.IsNullOrEmpty(ignoreRule.Index))
          throw new InvalidOperationException(string.Format(Strings.ExIgnoreRuleXMustBeAppliedToColumnIndexOrTable, ignoreRule));
      }
    }

    private void ValidateMaxNumberOfConditions()
    {
      if (MaxNumberOfConditions < 2 || MaxNumberOfConditions > 999) {
        throw new InvalidOperationException(string.Format(Strings.ExMaxNumberOfConditionsShouldBeBetweenXAndY, 2, 999));
      }
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone() => new DomainConfiguration();

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
      ensureConnectionIsAlive = configuration.EnsureConnectionIsAlive;
      options = configuration.Options;
      databases = (DatabaseConfigurationCollection) configuration.Databases.Clone();
      mappingRules = (MappingRuleCollection) configuration.MappingRules.Clone();
      keyGenerators = (KeyGeneratorConfigurationCollection) configuration.KeyGenerators.Clone();
      ignoreRules = (IgnoreRuleCollection) configuration.IgnoreRules.Clone();
      shareStorageSchemaOverNodes = configuration.ShareStorageSchemaOverNodes;
      versioningConvention = (VersioningConvention) configuration.VersioningConvention.Clone();
      preferTypeIdsAsQueryParameters = configuration.PreferTypeIdsAsQueryParameters;
      maxNumberOfConditons = configuration.MaxNumberOfConditions;
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>The clone of this configuration.</returns>
    public new DomainConfiguration Clone() => (DomainConfiguration) base.Clone();

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
    public static DomainConfiguration Load(string name) => Load(SectionName, name);

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
      if (section == null) {
        throw new InvalidOperationException(string.Format(
          Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      }
      return LoadConfigurationFromSection(section, name);
    }

    /// <summary>
    /// Loads the <see cref="DomainConfiguration"/> for <see cref="Domain"/>
    /// with the specified <paramref name="name"/>
    /// from application configuration file (section with <see cref="SectionName"/>).
    /// </summary>
    /// <param name="configuration">A <see cref="System.Configuration.Configuration"/>
    /// instance to load from.</param>
    /// <param name="name">Name of the <see cref="Domain"/>.</param>
    /// <returns>
    /// The <see cref="DomainConfiguration"/> for the specified domain.
    /// </returns>
    /// <exception cref="InvalidOperationException">Section <see cref="SectionName"/>
    /// is not found in application configuration file, or there is no configuration for
    /// the <see cref="Domain"/> with specified <paramref name="name"/>.</exception>
    public static DomainConfiguration Load(System.Configuration.Configuration configuration, string name) =>
      Load(configuration, SectionName, name);

    /// <summary>
    /// Loads the <see cref="DomainConfiguration"/> for <see cref="Domain"/>
    /// with the specified <paramref name="name"/>
    /// from application configuration file (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="configuration">A <see cref="System.Configuration.Configuration"/>
    /// instance to load from.</param>
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
      if (section == null) {
        throw new InvalidOperationException(string.Format(
          Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      }
      return LoadConfigurationFromSection(section, name);
    }

    internal bool Supports(DomainOptions optionsToCheck) => (options & optionsToCheck) == optionsToCheck;

    internal bool Supports(ForeignKeyMode modeToCheck) => (foreignKeyMode & modeToCheck) == modeToCheck;


    private static DomainConfiguration LoadConfigurationFromSection(ConfigurationSection section, string name)
    {
      var domainElement = section.Domains[name];
      if (domainElement == null) {
        throw new InvalidOperationException(string.Format(
          Strings.ExConfigurationForDomainIsNotFoundInApplicationConfigurationFile, name, sectionName));
      }
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
      types.Register(WellKnownOrmTypes.Persistent.Assembly, WellKnownOrmTypes.Persistent.Namespace);
    }
  }
}
