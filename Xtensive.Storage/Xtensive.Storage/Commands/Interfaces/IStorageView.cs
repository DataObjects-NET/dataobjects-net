// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using Xtensive.Transactions;

namespace Xtensive.Storage.Commands
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

    /// <summary>
    /// Gets the session handler this vierw belongs to.
    /// </summary>
    Providers.SessionHandler SessionHandler { get; }
  }
}