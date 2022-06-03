// Copyright (C) 2012-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Configuration
{
  internal static class NodeConfigurationExtensions
  {
    public static string GetActualNameFor(this NodeConfiguration nodeConfiguration, Catalog catalog) =>
      nodeConfiguration.DatabaseMapping.Apply(catalog.GetNameInternal());

    public static string GetActualNameFor(this NodeConfiguration nodeConfiguration, Schema schema) =>
      nodeConfiguration.SchemaMapping.Apply(schema.GetNameInternal());

    public static IReadOnlyDictionary<string, string> GetDatabaseMapping(this NodeConfiguration nodeConfiguration) =>
      nodeConfiguration.DatabaseMapping.ToDictionary(key => key.Key, value => value.Value);

    public static IReadOnlyDictionary<string, string> GetSchemaMapping(this NodeConfiguration nodeConfiguration) =>
      nodeConfiguration.SchemaMapping.ToDictionary(key => key.Key, value => value.Value);
  }
}
