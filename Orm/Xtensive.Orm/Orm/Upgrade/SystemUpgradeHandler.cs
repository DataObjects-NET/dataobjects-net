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
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;

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

      if (UpgradeContext.Stage!=UpgradeStage.Upgrading)
        return;

      CheckAssemblies();
      BuildTypeIdMap();
      LoadStoredDomainModel();
    }

    /// <inheritdoc/>
    public override void OnStage()
    {
      var context = UpgradeContext;
      var upgradeMode = context.Configuration.UpgradeMode;
      var session = Session.Demand();
      var builder = new TypeIdBuilder(session.Domain, context.TypeIdProvider);

      switch (context.Stage) {
      case UpgradeStage.Upgrading:
        // Perform or PerformSafely
        builder.BuildTypeIds();
        UpdateMetadata(session);
        break;
      case UpgradeStage.Final:
        if (upgradeMode.IsUpgrading()) {
          // Recreate, Perform or PerformSafely
          builder.BuildTypeIds();
          UpdateMetadata(session);
        }
        else if (upgradeMode.IsLegacy()) {
          // LegacySkip and LegacyValidate
          builder.BuildTypeIds();
        }
        else {
          // Skip and Validate
          BuildTypeIdMap();
          builder.BuildTypeIds();
        }
        break;
      default:
        throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    private void UpdateMetadata(Session session)
    {
      var groups = BuildMetadata(session.Domain);
      var driver = session.Handlers.StorageDriver;
      var mapping = new MetadataMapping(driver, session.Handlers.NameBuilder);
      var executor = session.Services.Demand<IProviderExecutor>();
      var resolver = UpgradeContext.Services.Resolver;
      var metadataSchema = UpgradeContext.Configuration.DefaultSchema;
      var sqlModel = UpgradeContext.ExtractedSqlModelCache;

      foreach (var group in groups) {
        var metadataDatabase = group.Key;
        var metadata = group.Value;
        var schema = resolver.GetSchema(sqlModel, metadataDatabase, metadataSchema);
        var task = new SqlExtractionTask(schema.Catalog.Name, schema.Name);
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

    private Dictionary<string, MetadataSet> BuildMetadata(Domain domain)
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
        var typeMetadata = GetTypeMetadata(types);
        var assemblies = types.Select(t => t.UnderlyingType.Assembly).ToHashSet();
        var assemblyMetadata = GetAssemblyMetadata(assemblies);
        var serializedModel = model.ToStoredModel(filter).Serialize();
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

    private static IEnumerable<TypeMetadata> GetTypeMetadata(IEnumerable<TypeInfo> types)
    {
      return types
        .Where(t => t.IsEntity && t.TypeId!=TypeInfo.NoTypeId)
        .Select(type => new TypeMetadata(type.TypeId, type.UnderlyingType.GetFullName()));
    }

    private ExtensionMetadata GetPartialIndexes(Domain domain, IEnumerable<TypeInfo> types)
    {
      var compiler = UpgradeContext.Services.IndexFilterCompiler;
      var handlers = domain.Handlers;
      var indexes = types
        .SelectMany(type => type.Indexes
          .Where(i => i.IsPartial && !i.IsVirtual)
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

    private void LoadStoredDomainModel()
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

    private void BuildTypeIdMap()
    {
      var context = UpgradeContext;
      context.ExtractedTypeMap = context.Metadata.Types.ToDictionary(t => t.Name, t => t.Id);
    }
  }
}