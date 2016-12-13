// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.23

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Internals.Interfaces;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class NodeExtractedModelBuilder : ISchemaExtractionResultBuilder
  {
    private readonly MappingResolver mappingResolver;
    private readonly StorageNode defaultStorageNode;
    private readonly NodeConfiguration buildingNodeConfiguration;

    public SchemaExtractionResult Run()
    {
      var result = new SchemaExtractionResult();
      CopyCatalogs(result.Catalogs);
      return result;
    }

    private void CopyCatalogs(NodeCollection<Catalog> catalogs)
    {
      var cloner = new CatalogCloner();
      var defaultNodeCatalogs = GetDefaultNodeCatalogs();
      var currentCatalogNodes = GetCurrentExtratableCatalogs();
      if (currentCatalogNodes.Count==1) {
        catalogs.Add(cloner.Clone(defaultNodeCatalogs[0], mappingResolver, currentCatalogNodes[0]));
      }
      else {
        defaultNodeCatalogs
          .Select(
              catalog => cloner.Clone(catalog, mappingResolver, buildingNodeConfiguration.DatabaseMapping.Apply(catalog.Name)))
          .ForEach(catalogs.Add);
      }
    }

    private IList<string> GetCurrentExtratableCatalogs()
    {
      return mappingResolver.GetSchemaTasks().GroupBy(t => t.Catalog).Select(t=>t.Key).ToList();
    }

    private IList<Catalog> GetDefaultNodeCatalogs()
    {
      return defaultStorageNode.Mapping.GetAllSNodes().Select(node => node.Schema.Catalog).Distinct().ToList();
    }

    internal NodeExtractedModelBuilder(UpgradeServiceAccessor services, StorageNode defaultNode, NodeConfiguration buildingNodeConfiguration)
    {
      mappingResolver = services.MappingResolver;
      this.buildingNodeConfiguration = buildingNodeConfiguration;
      defaultStorageNode = defaultNode;
    }
  }
}