// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Metadata;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse.Providers;
using Type=System.Type;
using Xtensive.Reflection;

namespace Xtensive.Orm
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static partial class WellKnown
  {
    /// <summary>
    /// Default node identifier (empty string).
    /// </summary>
    public const string DefaultNodeId = "";

    /// <summary>
    /// Name of the default configuration section (Xtensive.Orm).
    /// </summary>
    public const string DefaultConfigurationSection = "Xtensive.Orm";

    /// <summary>
    /// Default name of domain.
    /// </summary>
    public const string DefaultDomainConfigurationName = "Default";

    /// <summary>
    /// Name of the <see cref="Entity.Key"/> field.
    /// </summary>
    public const string KeyFieldName = "Key";

    /// <summary>
    /// Name of the <see cref="Entity.TypeId"/> field.
    /// </summary>
    public const string TypeIdFieldName = "TypeId";

    /// <summary>
    /// Name of the <see cref="Entity.PersistenceState"/> property.
    /// </summary>
    public const string PersistenceStatePropertyName = "PersistenceState";

    /// <summary>
    /// Name of the field that describes master in <see cref="Multiplicity.ManyToMany"/>
    /// or <see cref="Multiplicity.ZeroToMany"/> association.
    /// </summary>
    public const string MasterFieldName = "Master";

    /// <summary>
    /// Name of the field that describes slave in <see cref="Multiplicity.ManyToMany"/>
    /// or <see cref="Multiplicity.ZeroToMany"/> association.
    /// </summary>
    public const string SlaveFieldName = "Slave";

    /// <summary>
    /// Name of the <see cref="Extension"/> that describes domain model.
    /// </summary>
    public const string DomainModelExtensionName = "Xtensive.Orm.Model";

    /// <summary>
    /// Name of the <see cref="Extension"/> that describes partial indexes.
    /// </summary>
    public const string PartialIndexDefinitionsExtensionName = "Xtensive.Orm.PartialIndexDefinitions";

    /// <summary>
    /// Name of column in key generator table.
    /// </summary>
    public const string GeneratorColumnName = "ID";

    /// <summary>
    /// Name of fake column in key generator table.
    /// </summary>
    public const string GeneratorFakeColumnName = "Fake";

    /// <summary>
    /// Max number of key fields.
    /// </summary>
    public const int MaxKeyFieldNumber = 8;

    /// <summary>
    /// Maximal supported length (count of values) of purely generic keys.
    /// </summary>
    public const int MaxGenericKeyLength = 4;

    /// <summary>
    /// Maximal number of filtering values in an <see cref="IncludeProvider"/>
    /// which are to be placed inside a resulted SQL command (as boolean predicate).
    /// </summary>
    [Obsolete("Use DefaultNumberOfConditions")]
    public const int MaxNumberOfConditions = 256;

    /// <summary>
    /// Default value of maximal number of filtering values in an <see cref="IncludeProvider"/>
    /// which are to be placed inside a resulted SQL command (as boolean predicate).
    /// </summary>
    public const int DefaultMaxNumberOfConditions = 256;

    /// <summary>
    /// Number of rows in small multi-row INSERT.
    /// </summary>
    public const int MultiRowInsertSmallBatchSize = 16;

    /// <summary>
    /// Number of rows in big multi-row INSERT.
    /// </summary>
    public const int MultiRowInsertBigBatchSize = DefaultMaxNumberOfConditions;

    /// <summary>
    /// Maximum number of cached keys in <see cref="EntitySetState"/>.
    /// </summary>
    public const int EntitySetCacheSize = 1024000;

    /// <summary>
    /// Number of items that are preloaded on first <see cref="EntitySet{TItem}"/> access.
    /// </summary>
    public const int EntitySetPreloadCount = 32;

    /// <summary>
    /// Well-known storage protocol names.
    /// </summary>
    public static class Provider
    {
      /// <summary>
      /// Microsoft SQL Server.
      /// </summary>
      public const string SqlServer = "sqlserver";
      /// <summary>
      /// Microsoft SQL Server.
      /// </summary>
      public const string SqlServerCe = "sqlserverce";
      /// <summary>
      /// PostgreSQL.
      /// </summary>
      public const string PostgreSql = "postgresql";
      /// <summary>
      /// Oracle.
      /// </summary>
      public const string Oracle = "oracle";
      /// <summary>
      /// MySQL.
      /// </summary>
      public const string MySql = "mysql";
      /// <summary>
      /// Firebird.
      /// </summary>
      public const string Firebird = "firebird";
      /// <summary>
      /// SQLite.
      /// </summary>
      public const string Sqlite = "sqlite";
      /// <summary>
      /// All supported protocols (for exception messages, etc).
      /// </summary>
      public const string All = "sqlserver, sqlserverce, postgresql, oracle, mysql, firebird, sqlite";
    }

    /// <summary>
    /// Well-known session configuration names.
    /// </summary>
    public static class Sessions
    {
      /// <summary>
      /// Name of default session configuration.
      /// </summary>
      public const string Default = "Default";

      /// <summary>
      /// System session name.
      /// </summary>
      public const string System = "System";

      /// <summary>
      /// Service session name.
      /// </summary>
      public const string Service = "Service";

      /// <summary>
      /// Generator session name.
      /// </summary>
      public const string KeyGenerator = "KeyGenerator";
    }
  }
}
