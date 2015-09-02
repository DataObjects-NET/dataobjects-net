// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.08.31

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Upgrade.Internals
{
  /// <summary>
  /// Allows to map schemas of one node to schemas of another node
  /// for specified catalog which is shared by both nodes.
  /// </summary>
  internal class NodeSchemasMapper
  {
    private readonly DomainConfiguration originConfiguration;
    
    /// <summary>
    /// Maps schemas of target node to schemas source node for specified catalog.
    /// </summary>
    /// <param name="catalogName">Catalog which is used by both nodes.</param>
    /// <param name="sourceConfiguration">Node which catalogs have already cached.</param>
    /// <param name="targetConfiguration">Node which catalogs are going to create by copy.</param>
    /// <returns>Collection of mappings.</returns>
    public NameMappingCollection Map(string catalogName, NodeConfiguration sourceConfiguration, NodeConfiguration targetConfiguration)
    {
      var originalCatalog = targetConfiguration.DatabaseMapping.Where(el => el.Value==catalogName).Select(el=>el.Key).First();
      if (string.IsNullOrEmpty(originalCatalog))
        return null;
      var mappings = GetSchemasForDatabase(originConfiguration, originalCatalog)
        .Select(
          el =>
            new KeyValuePair<string, string>(
              targetConfiguration.SchemaMapping.Apply(el),
              sourceConfiguration.SchemaMapping.Apply(el)
            ));
      return new NameMappingCollection(mappings);
    }

    private static IEnumerable<string> GetSchemasForDatabase(DomainConfiguration configuration, string database)
    {
      var userSchemas =
        from rule in configuration.MappingRules
        let db = string.IsNullOrEmpty(rule.Database) ? configuration.DefaultDatabase : rule.Database
        where db==database
        select string.IsNullOrEmpty(rule.Schema) ? configuration.DefaultSchema : rule.Schema;

      return userSchemas
        .Concat(Enumerable.Repeat(configuration.DefaultSchema, 1))
        .Distinct();
    }

    public NodeSchemasMapper(DomainConfiguration domainConfiguration)
    {
      originConfiguration = domainConfiguration;
    }
  }
}
