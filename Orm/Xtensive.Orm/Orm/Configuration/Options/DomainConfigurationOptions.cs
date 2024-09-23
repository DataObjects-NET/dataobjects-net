// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class DomainConfigurationOptions
  {
    /// <summary>
    /// Domain configuration name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Url that will be used in <see cref="DomainConfiguration.ConnectionInfo"/>.
    /// </summary>
    public string ConnectionUrl { get; set; } = null;

    /// <summary>
    /// Connection string that will be used in <see cref="DomainConfiguration.ConnectionInfo"/>.
    /// </summary>
    public string ConnectionString { get; set; } = null;

    /// <summary>
    /// Provider that will be used in <see cref="DomainConfiguration.ConnectionInfo"/>.
    /// </summary>
    public string Provider { get; set; } = WellKnown.Provider.SqlServer;

    /// <summary>
    /// Types tha are about to be registered in <see cref="Domain"/>.
    /// </summary>
    public TypeRegistrationOptions[] Types { get; set; } = Array.Empty<TypeRegistrationOptions>();

    /// <summary>
    /// Size of the key cache. Default value is <see cref="DomainConfiguration.DefaultKeyCacheSize"/>
    /// </summary>
    public int KeyCacheSize { get; set; } = DomainConfiguration.DefaultKeyCacheSize;

    /// <summary>
    /// Size of the key generator cache size.
    /// Default value is <see cref="DomainConfiguration.DefaultKeyGeneratorCacheSize"/>
    /// </summary>
    public int KeyGeneratorCacheSize { get; set; } = DomainConfiguration.DefaultKeyGeneratorCacheSize;

    /// <summary>
    /// Size of the query cache (see <see cref="Query.Execute{TElement}(System.Func{System.Linq.IQueryable{TElement}})"/>).
    /// Default value is <see cref="DomainConfiguration.DefaultQueryCacheSize"/>.
    /// </summary>
    public int QueryCacheSize { get; set; } = DomainConfiguration.DefaultQueryCacheSize;

    /// <summary>
    /// Size of the record set mapping cache.
    /// Default value is <see cref="DomainConfiguration.DefaultRecordSetMappingCacheSize"/>.
    /// </summary>
    public int RecordSetMappingCacheSize { get; set; } = DomainConfiguration.DefaultRecordSetMappingCacheSize;

    /// <summary>
    /// Domain upgrade behavior.
    /// </summary>
    public DomainUpgradeMode UpgradeMode { get; set; } = DomainConfiguration.DefaultUpgradeMode;

    /// <summary>
    /// <see cref="SchemaSynchronizationException"/> format.
    /// </summary>
    public SchemaSyncExceptionFormat SchemaSyncExceptionFormat { get; set; } = DomainConfiguration.DefaultSchemaSyncExceptionFormat;

    /// <summary>
    /// Foreign key mode. 
    /// Default value is <see cref="Orm.ForeignKeyMode.Default"/>.
    /// </summary>
    public ForeignKeyMode ForeignKeyMode { get; set; } = DomainConfiguration.DefaultForeignKeyMode;

    /// <summary>
    /// Change tracking mode of full-text indexes.
    /// Default value is <see cref="DomainConfiguration.DefaultFullTextChangeTrackingMode"/>.
    /// </summary>
    public FullTextChangeTrackingMode FullTextChangeTrackingMode { get; set; } = DomainConfiguration.DefaultFullTextChangeTrackingMode;

    /// <summary>
    /// Collation for all columns. See <see cref="DomainConfiguration.Collation" /> for details.
    /// </summary>
    public string Collation { get; set; } = string.Empty;

    /// <summary>
    /// Session configurations.
    /// </summary>
    public SessionOptionsCollection Sessions { get; set; } = new SessionOptionsCollection();

    /// <summary>
    /// Registered mapping rules.
    /// </summary>
    public MappingRuleOptionsCollection MappingRules { get; set; } = new MappingRuleOptionsCollection();

    /// <summary>
    /// Registered ignore rules.
    /// </summary>
    public IgnoreRuleOptionsCollection IgnoreRules { get; set; } = new IgnoreRuleOptionsCollection();

    /// <summary>
    /// Registered database aliases.
    /// </summary>
    public DatabaseOptionsCollection Databases { get; set; } = new DatabaseOptionsCollection();

    /// <summary>
    /// Key generators.
    /// </summary>
    public KeyGeneratorOptionsCollection KeyGenerators {get; set;} = new KeyGeneratorOptionsCollection();

    /// <summary>
    /// Type of service container
    /// </summary>
    public string ServiceContainerType { get; set; } = null;

    /// <summary>
    /// Default schema.
    /// </summary>
    public string DefaultSchema { get; set; } = string.Empty;

    /// <summary>
    /// Default database.
    /// </summary>
    public string DefaultDatabase { get; set; } = string.Empty;


    /// <summary>
    /// Value indicating whether SQL should be included in exception messages.
    /// Default value is <see cref="DomainConfiguration.DefaultIncludeSqlInExceptions"/>
    /// </summary>
    public bool IncludeSqlInExceptions { get; set; } = DomainConfiguration.DefaultIncludeSqlInExceptions;

    /// <summary>
    /// Value indicating whether cyclic database dependencied are allowed.
    /// Default value is <see cref="DomainConfiguration.DefaultAllowCyclicDatabaseDependencies"/>
    /// </summary>
    public bool AllowCyclicDatabaseDependencies { get; set; } = DomainConfiguration.DefaultAllowCyclicDatabaseDependencies;

    /// <summary>
    /// Value indicating whether parallel build should be used where and if it is possible.
    /// Default value is <see cref="DomainConfiguration.DefaultBuildInParallel"/>
    /// </summary>
    public bool BuildInParallel { get; set; } = DomainConfiguration.DefaultBuildInParallel;

    /// <summary>
    /// Value indicating whether multidatabase keys should be used.
    /// Default value is <see cref="DomainConfiguration.DefaultMultidatabaseKeys"/>
    /// </summary>
    public bool MultidatabaseKeys { get; set; } = DomainConfiguration.DefaultMultidatabaseKeys;

    /// <summary>
    /// Value indicating whether same storage schema is shared across <see cref="StorageNode"/>s.
    /// Default value is <see cref="DomainConfiguration.DefaultShareStorageSchemaOverNodes"/>
    /// </summary>
    public bool ShareStorageSchemaOverNodes { get; set; } = DomainConfiguration.DefaultShareStorageSchemaOverNodes;

    /// <summary>
    /// Enables extra check if connection is not broken on its opening.
    /// </summary>
    public bool EnsureConnectionIsAlive { get; set; } = DomainConfiguration.DefaultEnsureConnectionIsAlive;

    /// <summary>
    /// Makes queries use parameters instead of constant values for persistent type identifiers.
    /// </summary>
    public bool PreferTypeIdsAsQueryParameters { get; set; } = DomainConfiguration.DefaultPreferTypeIdsAsQueryParameters;

    /// <summary>
    /// Forced server version.
    /// </summary>
    public string ForcedServerVersion { get; set; } = string.Empty;

    /// <summary>
    /// Connection initialization SQL script.
    /// </summary>
    public string ConnectionInitializationSql { get; set; } = string.Empty;

    /// <summary>
    /// Domain options
    /// </summary>
    public DomainOptions Options { get; set; } = DomainConfiguration.DefaultDomainOptions;

    /// <summary>
    /// Naming convention.
    /// </summary>
    public NamingConventionOptions NamingConventionRaw { get; internal set; } = null;

    /// <summary>
    /// Versioning convention.
    /// </summary>
    public VersioningConventionOptions VersioningConvention { get; set; } = null;

    /// <summary>
    /// Defines tags location within query or turn them off if <see cref="TagsLocation.Nowhere"/> is set.
    /// </summary>
    public TagsLocation TagsLocation { get; set; } = DomainConfiguration.DefaultTagLocation;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <returns></returns>
    /// <exception cref="System.AggregateException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public DomainConfiguration ToNative(IDictionary<string, string> connectionStrings)
    {
      var config = new DomainConfiguration {
        Name = Name,
        ConnectionInfo = ConnectionInfoParser.GetConnectionInfo(connectionStrings,
          ConnectionUrl, Provider, ConnectionString),
        KeyCacheSize = KeyCacheSize,
        KeyGeneratorCacheSize = KeyGeneratorCacheSize,
        QueryCacheSize = QueryCacheSize,
        RecordSetMappingCacheSize = RecordSetMappingCacheSize,
        DefaultSchema = DefaultSchema,
        DefaultDatabase = DefaultDatabase,
        UpgradeMode = UpgradeMode,
        ForeignKeyMode = ForeignKeyMode,
        SchemaSyncExceptionFormat = SchemaSyncExceptionFormat,
        Options = Options,
        ServiceContainerType = !ServiceContainerType.IsNullOrEmpty() ? Type.GetType(ServiceContainerType) : null,
        IncludeSqlInExceptions = IncludeSqlInExceptions,
        BuildInParallel = BuildInParallel,
        AllowCyclicDatabaseDependencies = AllowCyclicDatabaseDependencies,
        ForcedServerVersion = ForcedServerVersion,
        Collation = Collation,
        ConnectionInitializationSql = ConnectionInitializationSql,
        MultidatabaseKeys = MultidatabaseKeys,
        ShareStorageSchemaOverNodes = ShareStorageSchemaOverNodes,
        EnsureConnectionIsAlive = EnsureConnectionIsAlive,
        PreferTypeIdsAsQueryParameters = PreferTypeIdsAsQueryParameters,
        FullTextChangeTrackingMode = FullTextChangeTrackingMode,
        TagsLocation = TagsLocation,
      };

      if (NamingConventionRaw != null)
        config.NamingConvention = NamingConventionRaw.ToNative();
      if (VersioningConvention != null)
        config.VersioningConvention = VersioningConvention.ToNative();
      
      foreach (var element in Types) {
        _ = config.Types.Register(element.ToNative());
      }
      HashSet<object> uniqueElements;
      if (Databases.AnyExceptionOccur) {
        var exceptions = Databases.Exceptions;
        if (exceptions.Count == 1)
          throw exceptions[0];
        throw new System.AggregateException(Strings.ExASetOfExceptionsIsCaught + " during reading 'Databases' section of configuration", exceptions.ToArray());
      }
      else if (Databases.Count > 0) {
        foreach (var element in Databases) {
          config.Databases.Add(element.Value.ToNative());
        }
      }

      if (KeyGenerators.AnyExceptionOccur) {
        var exceptions = KeyGenerators.Exceptions;
        if (exceptions.Count == 1)
          throw exceptions[0];
        throw new System.AggregateException(Strings.ExASetOfExceptionsIsCaught + " during reading 'KeyGenerators' section of configuration", exceptions.ToArray());
      }
      else if (KeyGenerators.Count > 0) {
        uniqueElements = new HashSet<object>();
        foreach (var element in KeyGenerators) {
          //resolves cases when alias and database was used which provides different indetifiers but database conflict exists
          var identifier = element.GetMappedIdentifier(Databases);
          if (!uniqueElements.Add(identifier))
            throw new ArgumentException($"Key generator with name '{element.Name}' for database '{element.Database}' has already been declared.");

          config.KeyGenerators.Add(element.ToNative());
        }
      }
      if (IgnoreRules.AnyExceptionOccur) {
        var exceptions = IgnoreRules.Exceptions;
        if (exceptions.Count==1)
          throw exceptions[0];
        throw new System.AggregateException("Set of exceptions occured during reading 'IgnoreRules' section of configuration", exceptions.ToArray());
      }
      else if (IgnoreRules.Count > 0) {
        uniqueElements = new HashSet<object>();
        foreach (var element in IgnoreRules) {
          var identifier = element.GetMappedIdentifier(Databases);
          if (!uniqueElements.Add(identifier))
            throw new Exception("Ignore rule with same set of properties has already been declared.");
          config.IgnoreRules.Add(element.ToNative());
        }
      }

      if (MappingRules.AnyExceptionOccur) {
        var exceptions = MappingRules.Exceptions;
        if (exceptions.Count == 1)
          throw exceptions[0];
        throw new System.AggregateException("Set of exceptions occured during reading 'MappingRules' section of configuration", exceptions.ToArray());
      }
      else if (MappingRules.Count > 0) {
        foreach (var element in MappingRules) {
          config.MappingRules.Add(element.ToNative());
        }
      }

      foreach (var element in Sessions) {
        config.Sessions.Add(element.Value.ToNative(connectionStrings));
      }

      return config;
    }
  }
}
