// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using TypeInfo=Xtensive.Storage.Indexing.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public abstract class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    /// <inheritdoc/>
    protected override TypeInfo CreateTypeInfo(Type type, int? length)
    {
      return new TypeInfo(type, length);
    }
  }
}