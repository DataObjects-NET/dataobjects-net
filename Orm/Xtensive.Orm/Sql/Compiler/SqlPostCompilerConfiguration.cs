// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.07

using System;
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

    internal void AppendPlaceholderValue(StringBuilder sb, PlaceholderNode node)
    {
      switch (node.Id) {
        case TypeInfo typeInfo when TypeIdRegistry != null:
          if (TypeIdRegistry.GetTypeId(typeInfo) is var typeId && typeId != TypeInfo.NoTypeId) {
            _ = sb.Append(typeId);      // We assume typeId > 0 here to avoid using .AppendFormat(CultureInfo.InvariantCulture) with boxing
            return;
          }
          break;
        case Schema schema:
          _ = sb.Append(schema.GetActualDbName(SchemaMapping));
          return;
      }
      if (!PlaceholderValues.TryGetValue(node.Id, out var value)) {
        throw new InvalidOperationException(string.Format(Strings.ExValueForPlaceholderXIsNotSet, node.Id));
      }
      _ = sb.Append(value);
    }

    // Constructors

    public SqlPostCompilerConfiguration(StorageNode storageNode = null)
    {
      if (storageNode != null) {
        TypeIdRegistry = storageNode.TypeIdRegistry;
        SchemaMapping = storageNode.Configuration.GetSchemaMapping();
      }
    }

    public SqlPostCompilerConfiguration(TypeIdRegistry typeIdRegistry, IReadOnlyDictionary<string, string> schemaMapping)
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
