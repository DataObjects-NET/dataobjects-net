// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Transactions;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Indexing
{
  /// <summary>
  /// Transactional storage view.
  /// </summary>
  public interface IStorageView : 
    IModelManager,
    IDataManager
  {
    /// <summary>
    /// Gets the transaction this view belongs to.
    /// </summary>
    ITransaction Transaction { get; }

    /// <summary>
    /// Gets the index.
    /// </summary>
    /// <param name="indexInfo">The index.</param>
    /// <returns>The unique ordered index.</returns>
    IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo);
  }
}