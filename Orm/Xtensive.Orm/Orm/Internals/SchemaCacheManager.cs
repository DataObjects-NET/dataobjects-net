using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Internals;
using Xtensive.Sql;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Internals
{
  internal class SchemaCacheManager
  {
    private readonly NodeSchemaCache cache;

    public SchemaExtractionResult TryGetExtractionResult(NodeConfiguration nodeConfiguration)
    {
      var catalogs = cache.GetNodeSchema(nodeConfiguration);
      if (catalogs==null)
        return null;
      var extractionResult = new SchemaExtractionResult();
      foreach (var catalog in catalogs)
        extractionResult.Catalogs.Add(catalog);
      return extractionResult;
    }

    public void CacheExtractionResult(SchemaExtractionResult extractionResult, NodeConfiguration node)
    {
      foreach (var catalog in extractionResult.Catalogs)
        cache.Add(catalog, node);
    }

    /// <summary>
    /// Clears cache.
    /// </summary>
    public void ClearCache()
    {
      cache.Clear();
    }

    public SchemaCacheManager(DomainConfiguration configuration, DefaultSchemaInfo defaultSchemaInfo)
    {
      cache = new NodeSchemaCache(configuration, defaultSchemaInfo);
    }
  }
}
