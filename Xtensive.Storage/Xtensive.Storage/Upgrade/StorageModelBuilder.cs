// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.10

using System;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Factory that allows storage providers to override some logic
  /// when constructing <see cref="StorageInfo"/> model via <see cref="DomainModelConverter"/>.
  /// </summary>
  public class StorageModelBuilder
  {
    /// <summary>
    /// Creates <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="length">Length.</param>
    /// <param name="precision">Precision.</param>
    /// <param name="scale">Scale.</param>
    /// <returns>Created <see cref="TypeInfo"/>.</returns>
    public virtual TypeInfo CreateType(Type type, int? length, int? precision, int? scale)
    {
      return new TypeInfo(type, length, scale, precision, null);
    }

    /// <summary>
    /// Creates <see cref="SecondaryIndexInfo"/>.
    /// </summary>
    /// <param name="owningTable">Owning table.</param>
    /// <param name="indexName">Name of index.</param>
    /// <param name="originalModelIndex">Original index from mapping model.</param>
    /// <returns>Created <see cref="SecondaryIndexInfo"/>.</returns>
    public virtual SecondaryIndexInfo CreateSecondaryIndex(TableInfo owningTable, string indexName, Model.IndexInfo originalModelIndex)
    {
      return new SecondaryIndexInfo(owningTable, indexName);
    }
  }
}