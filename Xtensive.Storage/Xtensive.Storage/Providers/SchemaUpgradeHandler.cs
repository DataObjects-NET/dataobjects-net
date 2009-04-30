// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using System.Linq;
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
    private StorageInfo domainModel;
    private StorageInfo storageModel;
    private ActionSequence upgradeActions;

    /// <summary>
    /// Gets the domain model.
    /// </summary>
    public StorageInfo DomainModel
    {
      get
      {
        if (domainModel == null)
          domainModel = GetDomainModel();
        return domainModel;
      }
    }

    /// <summary>
    /// Gets the storage model.
    /// </summary>
    public StorageInfo StorageModel
    {
      get
      {
        if (storageModel == null)
          storageModel = GetStorageModel();
        return storageModel;
      }
    }

    /// <summary>
    /// Gets the upgrade actions.
    /// </summary>
    public ActionSequence UpgradeActions
    {
      get
      {
        if (upgradeActions == null)
          upgradeActions = GetUpgradeActions();
        return upgradeActions;
      }
    }

    /// <summary>
    /// Recreates the storage schema.
    /// </summary>
    public void RecreateStorageSchema()
    {
      ClearStorageSchema();
      UpgradeStorageSchema();
    }

    /// <summary>
    /// Clears the storage schema.
    /// </summary>
    public abstract void ClearStorageSchema();

    /// <summary>
    /// Updates the storage schema according to current domain model.
    /// </summary>
    public abstract void UpgradeStorageSchema();
    
    /// <summary>
    /// Gets a value indicating whether elements 
    /// will be created during upgrade storage schema.
    /// </summary>
    public bool HasCreateActions
    {
      get
      {
        return
          UpgradeActions.Flatten()
            .OfType<CreateNodeAction>()
            .Any();
      }
    }

    /// <summary>
    /// Gets a value indicating whether elements 
    /// will be removed during upgrade storage schema.
    /// </summary>
    public bool HasRemoveActions
    {
      get
      {
        return
          UpgradeActions.Flatten()
            .OfType<RemoveNodeAction>()
            .Any();
      }
    }

    /// <summary>
    /// Determines whether specific generator is persistent.
    /// </summary>
    /// <param name="generatorInfo">The generator info.</param>
    /// <returns>
    /// <see langword="true"/> if generator is persistent; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected abstract bool IsGeneratorPersistent(GeneratorInfo generatorInfo);
    
    /// <summary>
    /// Gets the storage model.
    /// </summary>
    /// <returns>The storage model.</returns>
    protected abstract StorageInfo GetStorageModel();

    /// <summary>
    /// Gets the domain model.
    /// </summary>
    /// <param name="name">The model name.</param>
    /// <returns>The domain model.</returns>
    protected StorageInfo GetDomainModel()
    {
      // TODO: Do not compare model name
      var name = StorageModel.Name;

      var buildingContext = BuildingContext.Current;
      var buildForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Reference) > 0;
      var buildHierarchyForeignKeys =
        (buildingContext.Configuration.ForeignKeyMode
          & ForeignKeyMode.Hierarchy) > 0;
      var modelName = buildingContext.Configuration.ConnectionInfo.Resource;
      
      var domainModelConverter = new DomainModelConverter(
        buildForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName,
        buildHierarchyForeignKeys, buildingContext.NameBuilder.BuildForeignKeyName,
        IsGeneratorPersistent);

      return domainModelConverter.Convert(buildingContext.Model, name);
    }

    /// <summary>
    /// Compares the specified storage model with specific domain model.
    /// </summary>
    /// <param name="storageModel">The storage model.</param>
    /// <param name="domainModel">The domain model.</param>
    /// <param name="hints">The hints.</param>
    /// <returns>Comparison result.</returns>
    protected ActionSequence GetUpgradeActions()
    {
      // TODO: Gets hints from context
      var hints = new HintSet(StorageModel, DomainModel);
      var comparer = new Comparer();
      var diff = comparer.Compare(StorageModel, DomainModel, hints);
      var actions = new ActionSequence() {
        new Upgrader().GetUpgradeSequence(diff, hints, 
        new Comparer())
      };
      return actions;
    }
    

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}