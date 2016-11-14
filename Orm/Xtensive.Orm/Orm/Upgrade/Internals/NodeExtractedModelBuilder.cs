// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.23

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Internals.Interfaces;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class NodeExtractedModelBuilder : ISchemaExtractionResultBuilder
  {
    private const char NameElementSeparator = ':';

    private readonly MappingResolver mappingResolver;
    private readonly StorageNode defaultStorageNode;

    public SchemaExtractionResult Run()
    {
      var result = new SchemaExtractionResult();
      CopyCatalogs(result.Catalogs);
      ValidateAndFixResult(result.Catalogs);
      return result;
    }

    private void CopyCatalogs(NodeCollection<Catalog> catalogs)
    {
      var cloner = new CatalogCloner();
      GetCatalogs().Select(catalog => cloner.Clone(catalog, mappingResolver)).ForEach(catalogs.Add);
    }

    private void ValidateAndFixResult(NodeCollection<Catalog> catalogs)
    {
      foreach (var group in mappingResolver.GetSchemaTasks().GroupBy(t => t.Catalog)) {
        var catalog = catalogs[group.Key];
        if (catalog==null) {
          catalog = new Catalog(group.Key);
          catalogs.Add(catalog);
        }
        foreach (var sqlExtractionTask in group) {
          if(catalog.Schemas[sqlExtractionTask.Schema]!=null)
            continue;
          catalog.CreateSchema(sqlExtractionTask.Schema);
        }
      }
    }

    private IEnumerable<Catalog> GetCatalogs()
    {
      return defaultStorageNode.Mapping.GetAllSNodes().Select(node => node.Schema.Catalog).Distinct();
    }

    internal NodeExtractedModelBuilder(UpgradeServiceAccessor services, StorageNode defaultNode)
    {
      mappingResolver = services.MappingResolver;
      defaultStorageNode = defaultNode;
    }
  }
}