// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Building;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Indexing.Memory
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public class SchemaUpgradeHandler : Indexing.SchemaUpgradeHandler
  {
    /// <summary>
    /// Gets the storage view.
    /// </summary>
    protected IndexStorageView StorageView {
      get {
        var session = BuildingContext.Demand().SystemSessionHandler;
        var view = ((SessionHandler) session).StorageView;
        return view as IndexStorageView;
      }
    }
    
    /// <inheritdoc/>
    protected override StorageInfo ExtractSchema()
    {
      return (StorageInfo) GetNativeExtractedSchema();
    }

    /// <inheritdoc/>
    protected override object ExtractNativeSchema()
    {
      return StorageView.Model.Clone(null, StorageInfo.DefaultName);
    }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      StorageView.ClearSchema();
      StorageView.CreateNewSchema(targetSchema);
    }
  }
}