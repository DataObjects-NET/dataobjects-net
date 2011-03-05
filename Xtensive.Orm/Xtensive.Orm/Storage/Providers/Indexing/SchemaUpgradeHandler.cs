// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Indexing
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public abstract class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    /// <inheritdoc/>
    protected override TypeInfo CreateTypeInfo(Type type, int? length, int? precision, int? scale)
    {
      return new TypeInfo(type, length, scale, precision, null);
    }
  }
}