// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

using System;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Factory that allows storage providers to override some logic
  /// when constructing <see cref="StorageModel"/> model via <see cref="DomainModelConverter"/>.
  /// </summary>
  public class StorageModelBuilder
  {
    /// <summary>
    /// Creates <see cref="StorageTypeInfo"/>.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="length">Length.</param>
    /// <param name="precision">Precision.</param>
    /// <param name="scale">Scale.</param>
    /// <returns>Created <see cref="StorageTypeInfo"/>.</returns>
    public virtual StorageTypeInfo CreateType(Type type, int? length, int? precision, int? scale)
    {
      return new StorageTypeInfo(type, length, scale, precision, null);
    }

    /// <summary>
    /// Creates <see cref="SecondaryIndexInfo"/>.
    /// </summary>
    /// <param name="owningTable">Owning table.</param>
    /// <param name="indexName">Name of index.</param>
    /// <param name="originalModelIndex">Original index from mapping model.</param>
    /// <returns>Created <see cref="SecondaryIndexInfo"/>.</returns>
    public virtual SecondaryIndexInfo CreateSecondaryIndex(TableInfo owningTable, string indexName, Orm.Model.IndexInfo originalModelIndex)
    {
      return new SecondaryIndexInfo(owningTable, indexName);
    }
  }
}