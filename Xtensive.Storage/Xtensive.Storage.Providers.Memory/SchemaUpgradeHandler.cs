// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Conversion;
using Xtensive.Storage.Providers.Index;

namespace Xtensive.Storage.Providers.Memory
{
  [Serializable]
  public class SchemaUpgradeHandler : Index.SchemaUpgradeHandler
  {
    /// <summary>
    /// Gets the storage view.
    /// </summary>
    protected IndexStorageView StorageView {
      get {
        var session = BuildingContext.Current.SystemSessionHandler;
        var view = ((SessionHandler) session).StorageView;
        return view as IndexStorageView;
      }
    }
    
    /// <inheritdoc/>
    public override StorageInfo GetStorageModel()
    {
      return (StorageInfo) StorageView.Model.Clone(null, StorageInfo.DefaultName);
    }

    /// <inheritdoc/>
    public override void UpgradeStorage(ActionSequence actions, StorageInfo newModel)
    {
      StorageView.ClearSchema();
      StorageView.CreateNewSchema(newModel);
    }

    /// <inheritdoc/>
    protected override bool IsGeneratorPersistent(GeneratorInfo generatorInfo)
    {
      return false;
    }
  }
}