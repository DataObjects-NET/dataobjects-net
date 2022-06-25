// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Model;
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
    /// Insert Placeholder nodes instead of real schema names
    /// Allows to share compiled query over multiple schemas.
    /// </summary>
    public bool ParametrizeSchemaNames { get; set; }

    /// <summary>
    /// Gets or sets comment location.
    /// </summary>
    public SqlCommentLocation CommentLocation { get; set; } = SqlCommentLocation.Nowhere;

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
        ParametrizeSchemaNames = ParametrizeSchemaNames,
      };
    }

    public SqlCompilerConfiguration()
    {
      SchemaMapping = null;
      DatabaseMapping = null;
    }

    public SqlCompilerConfiguration([NotNull]IReadOnlyDictionary<string, string> databaseMapping, [NotNull] IReadOnlyDictionary<string, string> schemaMapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(databaseMapping, "databaseMapping");
      ArgumentValidator.EnsureArgumentNotNull(schemaMapping, "schemaMapping");
      DatabaseMapping = databaseMapping;
      SchemaMapping = schemaMapping;
    }
  }
}
