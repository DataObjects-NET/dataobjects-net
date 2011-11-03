// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Metadata;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse.Providers.Compilable;
using Type=System.Type;
using System.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static class WellKnown
  {
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
    /// Name of the <see cref="Extension"/> that describes domain model.
    /// </summary>
    public const string MergedAssemblyName = "Xtensive.DataObjects.Net";

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
    public const int MaxNumberOfConditions = 256;

    /// <summary>
    /// Maximum number of cached keys in <see cref="EntitySetState"/>.
    /// </summary>
    public const int EntitySetCacheSize = 1024000;

    /// <summary>
    /// Number of items that are preloaded on first <see cref="EntitySet{TItem}"/> access.
    /// </summary>
    public const int EntitySetPreloadCount = 32;

    /// <summary>
    /// Gets a read-only hash set containing all supported integer types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> SupportedIntegerTypes = 
      new ReadOnlyHashSet<Type>(
        new HashSet<Type>{
          typeof(sbyte),
          typeof(byte),
          typeof(short),
          typeof(ushort),
          typeof(int),
          typeof(uint),
          typeof(long),
          typeof(ulong),
          }
        );

    /// <summary>
    /// Gets a read-only hash set containing all supported numeric types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> SupportedNumericTypes = 
      new ReadOnlyHashSet<Type>(
        new [] {
          typeof(decimal),
          typeof(double),
          typeof(float),
          }.Concat(SupportedIntegerTypes).ToHashSet()
        );

    /// <summary>
    /// Gets a read-only hash set containing all supported primitive types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> SupportedPrimitiveTypes = 
      new ReadOnlyHashSet<Type>(
        new [] {
          typeof(string),
          typeof(Guid),
          typeof(DateTime),
          typeof(TimeSpan),
          typeof(byte[]),
          }.Concat(SupportedNumericTypes).ToHashSet()
        );

    /// <summary>
    /// Gets a read-only hash set containing all supported nullable types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> SupportedNullableTypes = 
      new ReadOnlyHashSet<Type>(
        SupportedPrimitiveTypes.Select(type => type.ToNullable())
        .ToHashSet()
      );

    /// <summary>
    /// Gets a read-only hash set containing all supported primitive and nullable types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> SupportedPrimitiveAndNullableTypes = 
      new ReadOnlyHashSet<Type>(
        SupportedPrimitiveTypes.Union(SupportedNullableTypes)
        .ToHashSet()
      );

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
      /// All supported protocols (for exception messages, etc).
      /// </summary>
      public const string All = "'sqlserver', 'sqlserverce', 'postgresql', 'oracle', 'mysql', 'firebird'";
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