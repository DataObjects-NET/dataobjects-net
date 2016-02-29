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
    private readonly SchemaExtractionResult targetResult;

    public SchemaExtractionResult Run()
    {
      CopyCatalogs();
      RenameCatalogsAndSchemas();
      ValidateAndFixResult();
      return targetResult;
    }

    private void CopyCatalogs()
    {
      foreach (var catalog in GetCatalogs()) {
        var clonedCatalog = Cloner.Clone(catalog);
        targetResult.Catalogs.Add(clonedCatalog);
      }
    }

    private void RenameCatalogsAndSchemas()
    {
      foreach (var catalog in targetResult.Catalogs) {
        string catalogName = catalog.Name;
        foreach (var schema in catalog.Schemas) {
          var name = mappingResolver.GetNodeName(catalog.Name, schema.Name, "Dummy");
          var names = name.Split(NameElementSeparator);
          var schemaName = schema.Name;
          if (names.Length==3) {
            catalogName = names[0];
            schemaName = names[1];
          }
          else if (names.Length==2) {
            schemaName = names[0];
          }
          schema.Name = schemaName;
        }
        catalog.Name = catalogName;
      }
    }

    private void ValidateAndFixResult()
    {
      //sometimes there might be empty schemas or catalogs
      //here we handle it
      foreach (var group in mappingResolver.GetSchemaTasks().GroupBy(t => t.Catalog)) {
        var catalog = targetResult.Catalogs[group.Key];
        if (catalog==null) {
          catalog = new Catalog(group.Key);
          targetResult.Catalogs.Add(catalog);
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
      targetResult = new SchemaExtractionResult();
    }
  }
}