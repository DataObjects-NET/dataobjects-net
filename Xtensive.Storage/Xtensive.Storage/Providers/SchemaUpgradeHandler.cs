// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Conversion;

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
    public virtual StorageInfo GetTargetSchema()
    {
      var buildingContext = BuildingContext.Current;
      var buildForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Reference) > 0;
      var buildHierarchyForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Hierarchy) > 0;
      
      var domainModelConverter = new DomainModelConverter(
        buildForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName,
        buildHierarchyForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName,
        IsSchemaBoundGenerator);

      return domainModelConverter.Convert(buildingContext.Model, "Model");
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
    /// <param name="targetSchema">The target schema.</param>
    public abstract void UpgradeSchema(ActionSequence upgradeActions, StorageInfo targetSchema);
    
    /// <summary>
    /// Determines whether specific generator requires corresponding object in schema.
    /// </summary>
    /// <param name="generatorInfo">The generator info.</param>
    /// <returns>
    /// <see langword="true"/> if generator requires corresponding object in schema.
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected abstract bool IsSchemaBoundGenerator(GeneratorInfo generatorInfo);
    

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}