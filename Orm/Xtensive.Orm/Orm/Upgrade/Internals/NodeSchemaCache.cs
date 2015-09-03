// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.09.03

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class NodeSchemaCache : IDisposable
  {
    #region CatalogMappingContainer
    private class CatalogMappingContainer
    {
      private NameMappingCollection DatabaseMapping { get; set; }
      private NameMappingCollection SchemaMapping { get; set; }

      public NodeConfiguration ToNodeConfiguration()
      {
        var nodeConfiguration = new NodeConfiguration(null);
        foreach (var pair in DatabaseMapping)
          nodeConfiguration.DatabaseMapping.Add(pair.Key, pair.Value);

        foreach (var pair in SchemaMapping)
          nodeConfiguration.SchemaMapping.Add(pair.Key, pair.Value);
        return nodeConfiguration;
      }

      public CatalogMappingContainer(NameMappingCollection databaseMapping, NameMappingCollection schemaMapping)
      {
        ArgumentValidator.EnsureArgumentNotNull(databaseMapping, "databaseMapping");
        ArgumentValidator.EnsureArgumentNotNull(schemaMapping, "schemaMapping");
        DatabaseMapping = databaseMapping;
        SchemaMapping = schemaMapping;
      }
    }
    #endregion

    private readonly object lockableObject = new object();
    private readonly ConcurrentDictionary<ConnectionInfo, ConcurrentDictionary<string, Catalog>> catalogsPerConnection = new ConcurrentDictionary<ConnectionInfo, ConcurrentDictionary<string, Catalog>>(); 
    //private readonly ConcurrentDictionary<string, Catalog> catalogs = new ConcurrentDictionary<string, Catalog>();
    private readonly ConcurrentDictionary<Catalog, CatalogMappingContainer> actualMapping = new ConcurrentDictionary<Catalog, CatalogMappingContainer>();

    private readonly DomainConfiguration domainConfiguration;
    private readonly DefaultSchemaInfo defaultSchemaInfo;

    private NodeSchemasMapper mapper;
    private bool isDisposed = false;

    public Catalog[] GetNodeSchema(NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, "nodeConfiguration");
      
      var databases = GetNodeDatabases(nodeConfiguration);
      var nodeCatalogs = new List<Catalog>(databases.Count);
      var connectionInfo = nodeConfiguration.ConnectionInfo ?? domainConfiguration.ConnectionInfo;
      ConcurrentDictionary<string, Catalog> catalogs;
      if (!catalogsPerConnection.TryGetValue(connectionInfo, out catalogs))
        return null;
      foreach (var database in databases) {
        Catalog catalog;
        if (!catalogs.TryGetValue(database, out catalog))
          return null;
        var mappingContainer = actualMapping[catalog];
        var mapping = mapper.Map(database, mappingContainer.ToNodeConfiguration(), nodeConfiguration);
        nodeCatalogs.Add(CreateNodeSpecificCatalog(catalog, mapping));
      }
      return nodeCatalogs.ToArray();
    }

    public bool Add(Catalog catalog, NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(catalog, "catalog");
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, "nodeConfiguration");
      var connectionInfo = nodeConfiguration.ConnectionInfo ?? domainConfiguration.ConnectionInfo;
      ConcurrentDictionary<string, Catalog> catalogs;
      if (!catalogsPerConnection.TryGetValue(connectionInfo, out catalogs)) {
        catalogs = new ConcurrentDictionary<string, Catalog>();
        catalogsPerConnection.AddOrUpdate(connectionInfo, catalogs, (info, dictionary) => {
          catalogs = dictionary;
          return dictionary;
        });
      }
      if (!catalogs.ContainsKey(catalog.Name)) {
        catalogs.AddOrUpdate(catalog.Name, catalog, (s, catalog1) => catalog1);
        var databaseMapping = (NameMappingCollection)nodeConfiguration.DatabaseMapping.Clone();
        var schemaMapping = (NameMappingCollection)nodeConfiguration.SchemaMapping.Clone();
        var mappingContainer = new CatalogMappingContainer(databaseMapping, schemaMapping);
        actualMapping.AddOrUpdate(catalog, mappingContainer, (catalog1, catalogMappingContainer) => catalogMappingContainer);
        return true;
      }
      return false;
    }

    public void Clear()
    {
      catalogsPerConnection.Clear();
      actualMapping.Clear();
    }

    public void Dispose()
    {
      lock (locableObject) {
        if(isDisposed)
          return;
        Clear();
        mapper = null;
        isDisposed = true;
      }
    }

    private IList<string> GetNodeDatabases(NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, "nodeConfiguration");

      var defaultDatabase = string.IsNullOrEmpty(domainConfiguration.DefaultDatabase)
        ? defaultSchemaInfo.Database
        : domainConfiguration.DefaultDatabase;

      return ((nodeConfiguration.DatabaseMapping.Count > 0)
        ? nodeConfiguration.DatabaseMapping.Select(el => el.Value)
        : Enumerable.Repeat(defaultDatabase, 1)).ToList();
    }

    private Catalog CreateNodeSpecificCatalog(Catalog sourceCatalog, NameMappingCollection mapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceCatalog, "sourceCatalog");
      ArgumentValidator.EnsureArgumentNotNull(mapping, "mapping");
      var targetCatalog = Cloner.Clone(sourceCatalog);
      foreach (var pair in mapping)
        CatalogHelper.MoveSchemaNodes(targetCatalog, pair.Key, pair.Value);
      return targetCatalog;
    }

    public NodeSchemaCache(UpgradeContext context)
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      domainConfiguration = context.Configuration;
      defaultSchemaInfo = context.Services.StorageDriver.GetDefaultSchema(context.Services.Connection);
      mapper = new NodeSchemasMapper(context.Configuration);
    }
  }
}
