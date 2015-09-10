// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.09.03

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Internals;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Internals
{
  internal class SchemaCacheManager
  {
    private readonly NodeSchemaCache cache;

    /// <summary>
    /// Gets <see cref="SchemaExtractionResult"/> instance from cache for node if specified catalogs are cached.
    /// </summary>
    /// <param name="nodeConfiguration">Configuration of node.</param>
    /// <returns>Instance of <see cref="SchemaExtractionResult"/> if all the specified catalogs are in cache, otherwise, null.</returns>
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

    /// <summary>
    /// Puts catalogs of concrete instance of <see cref="SchemaExtractionResult"/> into the cache.
    /// </summary>
    /// <param name="extractionResult">Schema extraction result.</param>
    /// <param name="node">Node which uses the <paramref name="extractionResult"/>.</param>
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
