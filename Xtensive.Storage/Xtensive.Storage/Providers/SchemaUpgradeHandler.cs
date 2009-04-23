// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Comparison;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public abstract class SchemaUpgradeHandler : InitializableHandlerBase
  {

    /// <summary>
    /// Clears the storage schema.
    /// </summary>
    public abstract void ClearStorageSchema();

    /// <summary>
    /// Updates the storage schema according to current domain model.
    /// </summary>
    public abstract void UpdateStorageSchema();


    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}