using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Configuration
{
  internal static class NodeConfigurationExtensions
  {
    public static string GetActualNameFor(this NodeConfiguration nodeConfiguration, Catalog catalog)
    {
      return nodeConfiguration.DatabaseMapping.Apply(catalog.GetNameInternal());
    }

    public static string GetActualNameFor(this NodeConfiguration nodeConfiguration, Schema schema)
    {
      return nodeConfiguration.SchemaMapping.Apply(schema.GetNameInternal());
    }
  }
}
