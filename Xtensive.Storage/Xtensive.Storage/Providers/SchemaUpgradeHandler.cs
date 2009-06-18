// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Upgrade;

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
    public StorageInfo GetTargetSchema()
    {
      var buildingContext = BuildingContext.Demand();
      var generatorFactory = Handlers.HandlerFactory.CreateHandler<KeyGeneratorFactory>();
      var domainHandler = Handlers.DomainHandler;

      var buildForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Reference) > 0;
      var buildHierarchyForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Hierarchy) > 0;
      var providerInfo = domainHandler.ProviderInfo;

      var domainModelConverter = new DomainModelConverter(
        buildForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName,
        buildHierarchyForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName,
         generatorFactory.IsSchemaBoundGenerator, providerInfo, CreateTypeInfo);

      return domainModelConverter.Convert(buildingContext.Model);
    }

    /// <summary>
    /// Gets the extracted schema.
    /// </summary>
    /// <returns>The extracted schema.</returns>
    public abstract StorageInfo GetExtractedSchema();

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
    protected abstract TypeInfo CreateTypeInfo(Type type, int? length);

    /// <summary>
    /// Saves the schema in context.
    /// </summary>
    /// <param name="schema">The schema.</param>
    protected void SaveSchemaInContext(object schema)
    {
      var context = UpgradeContext.Demand();
      context.NativeExtractedSchema = schema;
    }
    

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}