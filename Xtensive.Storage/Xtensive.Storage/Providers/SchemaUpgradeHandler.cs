// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Building;
using Xtensive.Storage.StorageModel;
using Xtensive.Storage.Upgrade;
using Xtensive.Core;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public abstract class SchemaUpgradeHandler : InitializableHandlerBase
  {
    /// <summary>
    /// Gets the target schema.
    /// </summary>
    /// <returns>The target schema.</returns>
    public Func<StorageInfo> GetTargetSchemaProvider()
    {
      var buildingContext = BuildingContext.Demand();
      var domainHandler = Handlers.DomainHandler;

      var buildForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Reference) > 0;
      var buildHierarchyForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Hierarchy) > 0;
      var providerInfo = domainHandler.ProviderInfo;

      var domainModelConverter = new DomainModelConverter(
        providerInfo, 
        buildForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName, 
        buildHierarchyForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName, 
        CreateTypeInfo);

      var upgradeContext = UpgradeContext.Current;
      var session = Session.Current;
      return () => {
        using (upgradeContext==null ? null : upgradeContext.Activate())
        using (new BuildingScope(buildingContext))
        using (session==null ? null : session.Activate()) {
          return domainModelConverter.Convert(buildingContext.Model);
        }
      };
    }

    /// <summary>
    /// Gets the extracted schema.
    /// This method caches the schema inside <see cref="UpgradeContext"/>.
    /// </summary>
    /// <returns>The extracted schema.</returns>
    public Func<StorageInfo> GetExtractedSchemaProvider()
    {
      var upgradeContext = UpgradeContext.Current;
      if (upgradeContext!=null && upgradeContext.ExtractedSchemaCache!=null)
        return () => upgradeContext.ExtractedSchemaCache;

      var buildingContext = BuildingContext.Current;
      var session = Session.Current;
      return () => {
        StorageInfo schema;
        using (upgradeContext==null ? null : upgradeContext.Activate())
        using (buildingContext==null ? null : new BuildingScope(buildingContext))
        using (session==null ? null : session.Activate()) {
          schema = ExtractSchema();
        }
        if (upgradeContext!=null) {
          lock (upgradeContext) {
            upgradeContext.ExtractedSchemaCache = schema;
          }
        }
        return schema;
      };
    }

    /// <summary>
    /// Gets the native extracted schema.
    /// This method caches the schema inside <see cref="UpgradeContext"/>.
    /// </summary>
    /// <returns>The native extracted schema.</returns>
    public object GetNativeExtractedSchema()
    {
      var upgradeContext = UpgradeContext.Current;
      if (upgradeContext!=null && upgradeContext.NativeExtractedSchemaCache!=null)
        return upgradeContext.NativeExtractedSchemaCache;

      var schema = ExtractNativeSchema();
      if (upgradeContext!=null)
        upgradeContext.NativeExtractedSchemaCache = schema;
      return schema;
    }

    /// <summary>
    /// Clears the extracted schema cache.
    /// </summary>
    public void ClearExtractedSchemaCache()
    {
      var upgradeContext = UpgradeContext.Current;
      if (upgradeContext==null)
        return;
      upgradeContext.ExtractedSchemaCache = null;
      upgradeContext.NativeExtractedSchemaCache = null;
    }

    /// <summary>
    /// Extracts the schema.
    /// </summary>
    /// <returns>The extracted schema.</returns>
    protected abstract StorageInfo ExtractSchema();

    /// <summary>
    /// Extracts the native schema.
    /// </summary>
    /// <returns>The native extracted schema.</returns>
    protected abstract object ExtractNativeSchema();

    /// <summary>
    /// Upgrades the storage.
    /// </summary>
    /// <param name="upgradeActions">The upgrade actions.</param>
    /// <param name="sourceSchema">The source schema.</param>
    /// <param name="targetSchema">The target schema.</param>
    public abstract void UpgradeSchema(ActionSequence upgradeActions, StorageInfo sourceSchema, StorageInfo targetSchema);

    /// <summary>
    /// Creates the type info.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    /// <returns>Newly created <see cref="TypeInfo"/>.</returns>
    protected abstract TypeInfo CreateTypeInfo(Type type, int? length, int? precision, int? scale);


    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}