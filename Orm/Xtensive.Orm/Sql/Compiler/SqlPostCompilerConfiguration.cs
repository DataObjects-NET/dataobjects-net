// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.07

using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Xtensive.Core;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// <see cref="PostCompiler"/> configuration.
  /// </summary>
  public sealed class SqlPostCompilerConfiguration
  {
    public HashSet<object> AlternativeBranches { get; private set; } = new HashSet<object>();

    public Dictionary<object, string> PlaceholderValues { get; private set; } = new Dictionary<object, string>();

    public Dictionary<object, List<string[]>> DynamicFilterValues { get; private set; } = new Dictionary<object, List<string[]>>();

    /// <summary>
    /// Gets database mapping.
    /// </summary>
    public IReadOnlyDictionary<string, string> SchemaMapping { get; private set; }

    /// <summary>
    /// Gets database mapping.
    /// </summary>
    public IReadOnlyDictionary<string, string> DatabaseMapping { get; private set; }


    // Constructors

    public SqlPostCompilerConfiguration()
    {
      // this prevents wrong query to be executed.
      SchemaMapping = null;
      DatabaseMapping = null;
    }

    public SqlPostCompilerConfiguration([NotNull] IReadOnlyDictionary<string, string> databaseMapping, [NotNull] IReadOnlyDictionary<string, string> schemaMapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(databaseMapping, "databaseMapping");
      ArgumentValidator.EnsureArgumentNotNull(schemaMapping, "schemaMapping");

      DatabaseMapping = databaseMapping;
      SchemaMapping = schemaMapping;
    }
  }
}