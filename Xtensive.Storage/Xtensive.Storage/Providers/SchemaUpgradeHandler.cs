// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Comparison;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Conversion;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public abstract class SchemaUpgradeHandler : InitializableHandlerBase
  {
    /// <summary>
    /// Gets the domain model.
    /// </summary>
    /// <returns>The domain model.</returns>
    public virtual StorageInfo GetDomainModel()
    {
      // TODO: Do not compare model name

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
        IsGeneratorPersistent);

      return domainModelConverter.Convert(buildingContext.Model, "Model");
    }

    /// <summary>
    /// Gets the storage model.
    /// </summary>
    /// <returns>The storage model.</returns>
    public virtual StorageInfo GetStorageModel()
    {
      return new StorageInfo();
    }
    
    /// <summary>
    /// Upgrades the storage.
    /// </summary>
    /// <param name="actions">The upgrade actions.</param>
    /// <param name="newModel">The new model.</param>
    public abstract void UpgradeStorage(ActionSequence actions, StorageInfo newModel);
    
    /// <summary>
    /// Determines whether specific generator is persistent.
    /// </summary>
    /// <param name="generatorInfo">The generator info.</param>
    /// <returns>
    /// <see langword="true"/> if generator is persistent; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected abstract bool IsGeneratorPersistent(GeneratorInfo generatorInfo);
    

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}