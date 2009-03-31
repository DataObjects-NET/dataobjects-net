// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Indexing.Storage
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
  }
}