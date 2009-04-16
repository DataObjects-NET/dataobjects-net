// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model.Convert;

namespace Xtensive.Storage.Providers.Memory
{
  [Serializable]
  public class SchemaUpgradeHandler : Index.SchemaUpgradeHandler
  {
    protected IndexStorageView StorageView
    {
      get
      {
        var ctx = BuildingContext.Current;
        var session = ctx.SystemSessionHandler;
        var view = ((SessionHandler) session).StorageView;

        return view as IndexStorageView;
      }
    }

    /// <inheritdoc/>
    public override void ClearStorageSchema()
    {
      StorageView.ClearSchema();
    }

    /// <inheritdoc/>
    public override void UpdateStorageSchema()
    {
      var converter = new ModelConverter(Handlers.NameBuilder.BuildForeignKeyName,
        Handlers.NameBuilder.BuildForeignKeyName);
      var newSchema = converter.Convert(BuildingContext.Current.Model, StorageView.Storage.Name);
      StorageView.CreateNewSchema(newSchema);
    }
  }
}