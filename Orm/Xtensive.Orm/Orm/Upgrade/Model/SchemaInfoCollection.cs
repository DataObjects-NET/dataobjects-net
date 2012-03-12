// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Collection of <see cref="SchemaInfo"/>.
  /// </summary>
  [Serializable]
  public class SchemaInfoCollection : NodeCollectionBase<SchemaInfo, StorageModel>,
    IUnorderedNodeCollection
  {
    public SchemaInfoCollection(StorageModel parent)
      : base(parent, "Schemas")
    {
    }
  }
}