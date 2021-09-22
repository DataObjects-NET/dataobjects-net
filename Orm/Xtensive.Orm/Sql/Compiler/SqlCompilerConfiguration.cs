// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// A various options for <see cref="SqlCompiler"/>.
  /// </summary>
  public sealed class SqlCompilerConfiguration
  {
    /// <summary>
    /// Gets or sets the parameter prefix.
    /// </summary>
    public string ParameterNamePrefix { get; set; }

    /// <summary>
    /// Always use database-qualified objects in generated SQL.
    /// This option could be enabled if and only if
    /// server supports <see cref="QueryFeatures.MultidatabaseQueries"/>.
    /// </summary>
    public bool DatabaseQualifiedObjects { get; set; }

    /// <summary>
    /// Gets database mapping.
    /// </summary>
    public IReadOnlyDictionary<string, string> SchemaMapping { get; private set; }

    /// <summary>
    /// Gets database mapping.
    /// </summary>
    public IReadOnlyDictionary<string, string> DatabaseMapping { get; private set; }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Clone of this instance.</returns>
    public SqlCompilerConfiguration Clone()
    {
      return new SqlCompilerConfiguration {
        ParameterNamePrefix = ParameterNamePrefix,
        DatabaseQualifiedObjects = DatabaseQualifiedObjects,
      };
    }

    public SqlCompilerConfiguration()
    {
      SchemaMapping = null;
      DatabaseMapping = null;
    }

    public SqlCompilerConfiguration([NotNull]IDictionary<string, string> databaseMapping, [NotNull]IDictionary<string, string> schemaMapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(databaseMapping, "databaseMapping");
      ArgumentValidator.EnsureArgumentNotNull(schemaMapping, "schemaMapping");
      DatabaseMapping = new ReadOnlyDictionary<string, string>(databaseMapping);
      SchemaMapping = new ReadOnlyDictionary<string, string>(schemaMapping);
    }
  }
}