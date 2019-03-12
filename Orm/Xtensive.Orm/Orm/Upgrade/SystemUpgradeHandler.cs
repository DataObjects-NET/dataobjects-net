// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// <see cref="UpgradeHandler"/> implementation 
  /// for <see cref="Xtensive.Orm"/> assembly.
  /// </summary>
  public sealed class SystemUpgradeHandler : UpgradeHandler
  {
    /// <inheritdoc/>
    public override bool IsEnabled
    {
      get
      {
        // Enabled just for Xtensive.Orm
        return Assembly==GetType().Assembly;
      }
    }

    public override void OnBeforeStage()
    {
      base.OnBeforeStage();

      if (UpgradeContext.Stage==UpgradeStage.Upgrading) {
        CheckAssemblies();
        SaveExtractedTypeMap();
        ParseStoredDomainModel();
        return;
      }

      if (UpgradeContext.UpgradeMode==DomainUpgradeMode.Validate) {
        SaveExtractedTypeMap();
        ParseStoredDomainModel();
      }
    }

    /// <inheritdoc/>
    public override void OnStage()
    {
      var context = UpgradeContext;
      var upgradeMode = context.UpgradeMode;
      var session = Session.Demand();
      CheckUserDefinedTypeMap(session.Domain);

      switch (context.Stage) {
      case UpgradeStage.Upgrading:
        // Perform or PerformSafely
        BuildTypeIds(session.Domain);
        UpdateMetadata(session);
        SaveFullTypeMap(context.StorageNode.TypeIdRegistry);
        break;
      case UpgradeStage.Final:
        if (upgradeMode.IsUpgrading()) {
          // Recreate, Perform or PerformSafely
          BuildTypeIds(session.Domain);
          UpdateMetadata(session);
        }
        else if (upgradeMode.IsLegacy()) {
          // LegacySkip and LegacyValidate
          BuildTypeIds(session.Domain);
        }
        else {
          // Skip and Validate
          SaveExtractedTypeMap();
          BuildTypeIds(session.Domain);
        }
        break;
      default:
        throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    private void BuildTypeIds(Domain domain)
    {
      var builder = new TypeIdBuilder(domain, UpgradeContext.TypeIdProvider);
      var storageNode = UpgradeContext.StorageNode;
      var registry = storageNode.TypeIdRegistry;

      builder.BuildTypeIds(registry);
      registry.Lock();

      if (storageNode.Id==WellKnown.DefaultNodeId)
        builder.SetDefaultTypeIds(registry);
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    private void UpdateMetadata(Session session)
    {
      var groups = BuildMetadata(session.Domain, session.StorageNode.TypeIdRegistry);
      var driver = session.Handlers.StorageDriver;
      var mapping = new MetadataMapping(driver, session.Handlers.NameBuilder);
      var executor = session.Services.Demand<IProviderExecutor>();
      var resolver = UpgradeContext.Services.MappingResolver;
      var metadataSchema = UpgradeContext.Configuration.DefaultSchema;
      var sqlModel = UpgradeContext.ExtractedSqlModelCache;
      var namesShouldBeActualized = UpgradeContext.Configuration.ShareStorageSchemaOverNodes && UpgradeContext.Services.ProviderInfo.Supports(ProviderFeatures.Multischema);
      var nodeConfiguration = UpgradeContext.NodeConfiguration;

      foreach (var group in groups) {
        var metadataDatabase = group.Key;
        var metadata = group.Value;
        var schema = resolver.ResolveSchema(sqlModel, metadataDatabase, metadataSchema);
        var task = (!namesShouldBeActualized)
          ? new SqlExtractionTask(schema.Catalog.GetNameInternal(), schema.GetNameInternal())
          : new SqlExtractionTask(nodeConfiguration.GetActualNameFor(schema.Catalog), nodeConfiguration.GetActualNameFor(schema));
        var writer = new MetadataWriter(driver, mapping, task, executor);
        writer.Write(metadata);
      }

      var flatMetadata = new MetadataSet();
      foreach (var metadata in groups.Values)
        flatMetadata.UnionWith(metadata);
      UpgradeContext.Metadata = flatMetadata;
    }

    private void CheckAssemblies()
    {
      var oldMetadata = UpgradeContext.Metadata.Assemblies ?? Enumerable.Empty<AssemblyMetadata>();

      var oldAssemblies = oldMetadata
        .GroupBy(a => a.Name)
        .ToDictionary(g => g.Key, g => g.ToList());

      var handlers = UpgradeContext.OrderedUpgradeHandlers;

      foreach (var handler in handlers) {
        List<AssemblyMetadata> assemblies;
        if (oldAssemblies.TryGetValue(handler.AssemblyName, out assemblies)) {
          foreach (var assembly in assemblies)
            if (!handler.CanUpgradeFrom(assembly.Version))
              throw HandlerCanNotUpgrade(handler, assembly.Version);
        }
        else {
          if (!handler.CanUpgradeFrom(null))
            throw HandlerCanNotUpgrade(handler, Strings.ZeroAssemblyVersion);
        }
      }
    }

    private static DomainBuilderException HandlerCanNotUpgrade(IUpgradeHandler handler, string sourceVersion)
    {
      return new DomainBuilderException(string.Format(
        Strings.ExUpgradeOfAssemblyXFromVersionYToZIsNotSupported,
        handler.AssemblyName, sourceVersion, handler.AssemblyVersion));
    }

    private Dictionary<string, MetadataSet> BuildMetadata(Domain domain, TypeIdRegistry registry)
    {
      var model = domain.Model;
      var metadataGroups = model.Databases.ToDictionary(db => db.Name, db => new MetadataSet());
      if (metadataGroups.Count==0)
        metadataGroups.Add(string.Empty, new MetadataSet());

      foreach (var group in metadataGroups) {
        var database = group.Key;
        var metadata = group.Value;
        Func<TypeInfo, bool> filter = t => t.MappingDatabase==database;
        var types = model.Types.Where(filter).ToList();
        var typeMetadata = GetTypeMetadata(types, registry);
        var assemblies = types.Select(t => t.UnderlyingType.Assembly).ToHashSet();
        var assemblyMetadata = GetAssemblyMetadata(assemblies);
        var storedModel = model.ToStoredModel(registry, filter);
        // Since we support storage nodes, stored domain model and real model of a node
        // must be synchronized. So we must update types' mappings
        storedModel.UpdateMappings(UpgradeContext.NodeConfiguration);
        var serializedModel = storedModel.Serialize();
        var modelExtension = new ExtensionMetadata(WellKnown.DomainModelExtensionName, serializedModel);
        var indexesExtension = GetPartialIndexes(domain, types);
        metadata.Assemblies.AddRange(assemblyMetadata);
        metadata.Types.AddRange(typeMetadata);
        metadata.Extensions.Add(modelExtension);
        if (indexesExtension!=null)
          metadata.Extensions.Add(indexesExtension);
      }

      return metadataGroups;
    }

    private IEnumerable<AssemblyMetadata> GetAssemblyMetadata(HashSet<Assembly> assemblies)
    {
      var assemblyMetadata = UpgradeContext.OrderedUpgradeHandlers
        .Where(handler => assemblies.Contains(handler.Assembly))
        .Select(handler => new AssemblyMetadata(handler.AssemblyName, handler.AssemblyVersion));
      return assemblyMetadata;
    }

    private static IEnumerable<TypeMetadata> GetTypeMetadata(IEnumerable<TypeInfo> types, TypeIdRegistry registry)
    {
      return types
        .Where(t => t.IsEntity && registry.Contains(t))
        .Select(type => new TypeMetadata(registry[type], type.UnderlyingType.GetFullName()));
    }

    private ExtensionMetadata GetPartialIndexes(Domain domain, IEnumerable<TypeInfo> types)
    {
      if (!domain.StorageProviderInfo.Supports(ProviderFeatures.PartialIndexes))
        return null;
      var compiler = UpgradeContext.Services.IndexFilterCompiler;
      var handlers = domain.Handlers;
      var indexes = types
        .SelectMany(type => type.Indexes
          .Where(i => i.IsPartial && !i.IsVirtual && !i.IsAbstract)
          .Select(i => new {Type = type, Index = i}))
        .Select(item => new StoredPartialIndexFilterInfo {
          Database = item.Type.MappingDatabase,
          Schema = item.Type.MappingSchema,
          Table = item.Type.MappingName,
          Name = item.Index.MappingName,
          Filter = compiler.Compile(handlers, item.Index)
        })
        .ToArray();
      if (indexes.Length==0)
        return null;
      var items = new StoredPartialIndexFilterInfoCollection {
        Items = indexes
      };
      return new ExtensionMetadata(WellKnown.PartialIndexDefinitionsExtensionName, items.Serialize());
    }

    private void ParseStoredDomainModel()
    {
      var context = UpgradeContext;
      var extensions = context.Metadata.Extensions.Where(e => e.Name==WellKnown.DomainModelExtensionName);
      try {
        var found = false;
        var types = new List<StoredTypeInfo>();

        foreach (var extension in extensions) {
          found = true;
          var part = StoredDomainModel.Deserialize(extension.Value);
          types.AddRange(part.Types);
        }

        if (!found) {
          UpgradeLog.Info(Strings.LogDomainModelIsNotFoundInStorage);
          return;
        }

        var model = new StoredDomainModel {Types = types.ToArray()};
        model.UpdateReferences();

        context.ExtractedDomainModel = model;
      }
      catch (Exception e) {
        UpgradeLog.Warning(e, Strings.LogFailedToExtractDomainModelFromStorage);
      }
    }

    private void SaveExtractedTypeMap()
    {
      var map = UpgradeContext.Metadata.Types.ToDictionary(t => t.Name, t => t.Id);
      UpgradeContext.FullTypeMap = UpgradeContext.ExtractedTypeMap = map;
    }

    private void SaveFullTypeMap(TypeIdRegistry registry)
    {
      UpgradeContext.FullTypeMap = registry.Types.ToDictionary(t => t.UnderlyingType.GetFullName(), t => registry[t]);
    }

    private void CheckUserDefinedTypeMap(Domain domain)
    {
      if (UpgradeContext.UserDefinedTypeMap.Count == 0)
        return;
      var types = domain.Model.Types;
      var typesExtracted = UpgradeContext.ExtractedTypeMap != null;
      var extractedTypeMapping = UpgradeContext.ExtractedTypeMap;
      var reversedTypeMapping = (typesExtracted) ? extractedTypeMapping.ToDictionary(el => el.Value, el => el.Key) : null;
      var mapping = UpgradeContext.UpgradedTypesMapping ?? new Dictionary<string, string>();
      foreach (var userDefindeTypeMap in UpgradeContext.UserDefinedTypeMap) {
        var type = types.Find(userDefindeTypeMap.Key);
        if (type==null)
          throw new DomainBuilderException(
            string.Format(Strings.ExUnableToDefineTypeIdentifierXForTypeYTypeIsNotExists, userDefindeTypeMap.Value, userDefindeTypeMap.Key));

        if (typesExtracted) {
          string extractedTypeName;
          if (reversedTypeMapping.TryGetValue(userDefindeTypeMap.Value, out extractedTypeName)) {
            string oldTypeName;
            if (!TryGetOldTypeName(userDefindeTypeMap.Key, out oldTypeName)) {
              throw new DomainBuilderException(
                string.Format(
                  Strings.ExCustomTypeIdentifierXOfTypeYConflictsWithTypeZInExtractedMapOfTypes,
                  userDefindeTypeMap.Value,
                  userDefindeTypeMap.Key,
                  extractedTypeName));
            }
            else {
              if (extractedTypeName!=oldTypeName)
                throw new DomainBuilderException(
                  string.Format(
                    Strings.ExCustomTypeIdentifierXOfTypeYConflictsWithTypeZInExtractedMapOfTypes,
                    userDefindeTypeMap.Value,
                    userDefindeTypeMap.Key,
                    oldTypeName));
            }
          }
        }

        var minimalTypeId = TypeInfo.MinTypeId;
        var maximalTypeId = int.MaxValue;
        if (domain.Model.Databases.Count!=0) {
          var databaseConfiguration = domain.Model.Databases[type.MappingDatabase].Configuration;
          minimalTypeId = databaseConfiguration.MinTypeId;
          maximalTypeId = databaseConfiguration.MaxTypeId;
        }

        if (userDefindeTypeMap.Value < minimalTypeId || userDefindeTypeMap.Value > maximalTypeId)
          throw new DomainBuilderException(
            string.Format(Strings.ExCustomTypeIdentifierXOfTypeYBeyongsTheLimitsDefinedForDatabase,
              userDefindeTypeMap.Value,
              userDefindeTypeMap.Key));
      }
    }

    public bool TryGetOldTypeName(string newTypeName, out string oldTypeName)
    {
      string oldName;
      if(UpgradeContext.UpgradedTypesMapping==null) {
        oldTypeName = newTypeName;
        return true;
      }
      if (UpgradeContext.UpgradedTypesMapping.TryGetValue(newTypeName, out oldName)) {
        oldTypeName = oldName;
        return true;
      }
      oldTypeName = newTypeName;
      return false;
    }
  }
}