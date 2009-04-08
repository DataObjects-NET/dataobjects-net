// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Model.Convert;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Comparison;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public abstract class SchemaUpgradeHandler: InitializableHandlerBase
  {
    private const string DomainSchemaName = "dbo";

    /// <summary>
    /// Gets the domain schema.
    /// </summary>
    protected StorageInfo DomainSchema { get; private set;}

    /// <summary>
    /// Gets schema difference between domain schema and storage schema.
    /// </summary>
    protected Difference SchemaDifference { get; private set; }

    /// <summary>
    /// Gets storage view.
    /// </summary>
    protected IStorageView StorageView { get; set; }

    /// <summary>
    /// Fully recreates the storage schema.
    /// </summary>
    /// <param name="storageView">The storage view.</param>
    public void RecreateSchema()
    {
      var storageSchema = StorageView.Model;
      if (storageSchema != null)
        StorageView.Update(BuildClearSchemaActions());
      StorageView.Update(BuildCreateSchemaActions());
    }

    /// <summary>
    /// Begins upgrade the storage schema. 
    /// Creates missing tables and columns in storage.
    /// </summary>
    /// <param name="storageView">The storage view.</param>
    public void BeginUpgrade()
    {
      var upgrageActions = BuildUpgradeActions(BuildDifference());
      StorageView.Update(upgrageActions);
    }

    /// <summary>
    /// Removes excess columns and tables from storage schema.
    /// </summary>
    /// <param name="storageView">The storage view.</param>
    public void ClearRecyclingData()
    {
      var clearRecyclingDataActions = BuildClearRecyclingDataActions(BuildDifference());
      StorageView.Update(clearRecyclingDataActions);
    }

    public StorageConformity CheckStorageConformity()
    {
      // StorageView = storageView;
      // var difference = BuildDifference();
      // ToDo: Use Difference for calculating StorageConformity.

      return StorageConformity.Match;
    }

    /// <summary>
    /// Build actions for create new storage schema according to domain schema.
    /// </summary>
    /// <returns>The set of actions.</returns>
    protected virtual ActionSequence BuildCreateSchemaActions()
    {
      var emptySchema = new StorageInfo(DomainSchema.Name);
      var hints = new HintSet(emptySchema, DomainSchema);
      var actions = new ActionSequence();
      using (hints.Activate()) {
        actions.Add(DomainSchema.GetDifferenceWith(emptySchema, null, false).ToActions());
      }
      return actions;
    }

    /// <summary>
    /// Build actions for clear storage schema.
    /// </summary>
    /// <param name="storageSchema">The storage schema.</param>
    /// <returns>The set of actions.</returns>
    protected virtual ActionSequence BuildClearSchemaActions()
    {
      var storageSchema = StorageView.Model;
      var emptySchema = new StorageInfo(storageSchema.Name);
      var hints = new HintSet(emptySchema, storageSchema);
      var actions = new ActionSequence();
      using (hints.Activate()) {
        actions.Add(storageSchema.GetDifferenceWith(emptySchema, null, false).ToActions());
      }
      return actions;
    }

    /// <summary>
    /// Calculate difference between domain schema and storage schema.
    /// </summary>
    /// <returns>The difference.</returns>
    protected virtual Difference BuildDifference()
    {
      var storageSchema = StorageView.Model;
      return DomainSchema.GetDifferenceWith(storageSchema);
    }

    /// <summary>
    /// Builds actions for add missing tables and columns to storage schema.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>The set of actions.</returns>
    protected virtual ActionSequence BuildUpgradeActions(Difference difference)
    {
      return new ActionSequence
        {
          difference.ToActions()
        };
    }

    /// <summary>
    /// Builds actions for removing excess columns and tables from storage schema.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>The set of actions.</returns>
    protected virtual ActionSequence BuildClearRecyclingDataActions(Difference difference)
    {
      return new ActionSequence
        {
          difference.ToActions()
        };
    }

    /// <inheritdoc/>`
    public override void Initialize()
    {
      var converter = new ModelConverter(Handlers.NameBuilder.BuildForeignKeyName,
        Handlers.NameBuilder.BuildForeignKeyName);
      DomainSchema = converter.Convert(Handlers.Domain.Model, DomainSchemaName);
    }

  }
}