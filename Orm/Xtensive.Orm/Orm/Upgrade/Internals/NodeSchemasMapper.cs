// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.08.31

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Upgrade.Internals
{
  /// <summary>
  /// Allows to map schemas of one node to schemas of another node
  /// for specified catalog which is shared by both nodes.
  /// </summary>
  internal class NodeSchemasMapper
  {
    private readonly DomainConfiguration originConfiguration;
    private readonly DefaultSchemaInfo defaultSchemaInfo;
    
    /// <summary>
    /// Maps schemas of target node to schemas source node for specified catalog.
    /// </summary>
    /// <param name="catalogName">Catalog which is used by both nodes.</param>
    /// <param name="sourceConfiguration">Node which catalogs have already cached.</param>
    /// <param name="targetConfiguration">Node which catalogs are going to create by copy.</param>
    /// <returns>Map from <paramref name="sourceConfiguration"/> schemas to <paramref name="targetConfiguration"/> for <paramref name="catalogName"> the catalog</paramref>.</returns>
    public NameMappingCollection Map(string catalogName, NodeConfiguration sourceConfiguration, NodeConfiguration targetConfiguration)
    {
      string originalCatalog;
      if (targetConfiguration.DatabaseMapping.Count==0)
        originalCatalog = defaultSchemaInfo.Database;
      else
        originalCatalog = targetConfiguration.DatabaseMapping.Where(el => el.Value==catalogName).Select(el=>el.Key).FirstOrDefault();
      if (string.IsNullOrEmpty(originalCatalog))
        throw new InvalidOperationException(string.Format("Catalog '{0}' is not found in configuration of node '{1}'.", catalogName, targetConfiguration.NodeId));
      var databases = GetSchemasForDatabase(originConfiguration, originalCatalog);
      var mappings = databases
        .Select(
          el =>
            new KeyValuePair<string, string>(
              sourceConfiguration.SchemaMapping.Apply(el),
              targetConfiguration.SchemaMapping.Apply(el)
            ));
      return new NameMappingCollection(mappings);
    }

    private IEnumerable<string> GetSchemasForDatabase(DomainConfiguration configuration, string database)
    {
      var userSchemas =
        from rule in configuration.MappingRules
        let db = string.IsNullOrEmpty(rule.Database) ? configuration.DefaultDatabase : rule.Database
        let db1 = string.IsNullOrEmpty(db) ? defaultSchemaInfo.Database : db
        where db1==database
        select string.IsNullOrEmpty(rule.Schema) ? configuration.DefaultSchema : rule.Schema;

      return userSchemas
        .Concat(Enumerable.Repeat(configuration.DefaultSchema, 1))
        .Distinct();
    }

    public NodeSchemasMapper(DomainConfiguration domainConfiguration, DefaultSchemaInfo defaultSchemaInfo)
    {
      originConfiguration = domainConfiguration;
      this.defaultSchemaInfo = defaultSchemaInfo;
    }
  }
}
