// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.07

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// <see cref="PostCompiler"/> configuration.
  /// </summary>
  public sealed class SqlPostCompilerConfiguration
  {
    public HashSet<object> AlternativeBranches { get; } = new HashSet<object>();

    public Dictionary<object, string> PlaceholderValues { get; } = new Dictionary<object, string>();
    public TypeIdRegistry TypeIdRegistry { get; }
    public IReadOnlyDictionary<string, string> SchemaMapping { get; }

    public Dictionary<object, List<string[]>> DynamicFilterValues { get; } = new Dictionary<object, List<string[]>>();

    internal void AppendPlaceholderValue(StringBuilder sb, PlaceholderNode node)
    {
      switch (node.Id) {
        case TypeInfo typeInfo when TypeIdRegistry != null:
          if (TypeIdRegistry.GetTypeId(typeInfo) is var typeId && typeId != TypeInfo.NoTypeId) {
            sb.Append(typeId);      // We assume typeId > 0 here to avoid using .AppendFormat(CultureInfo.InvariantCulture) with boxing
            return;
          }
          if (typeInfo.IsInterface) {
            sb.Append(0);
            return;
          }
          break;
        case Schema schema:
          sb.Append(schema.GetActualDbName(SchemaMapping));
          return;
      }
      if (!PlaceholderValues.TryGetValue(node.Id, out var value)) {
        throw new InvalidOperationException(string.Format(Strings.ExValueForPlaceholderXIsNotSet, node.Id));
      }
      sb.Append(value);
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
      TypeIdRegistry = typeIdRegistry;
      SchemaMapping = schemaMapping;
    }
  }
}
