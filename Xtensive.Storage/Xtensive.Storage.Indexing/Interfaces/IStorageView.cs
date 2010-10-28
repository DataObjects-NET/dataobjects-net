// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage.Indexing
{
  /// <summary>
  /// Transactional storage view.
  /// </summary>
  public interface IStorageView : 
    IModelManager,
    IDataManager,
    IIndexResolver
  {
    /// <summary>
    /// Gets the transaction this view belongs to.
    /// </summary>
    ITransaction Transaction { get; }
  }
}