// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.02

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Indicates operational state of the <see cref="Transaction"/> instance.
  /// </summary>
  [Serializable]
  public enum TransactionState
  {
    /// <summary>
    /// Default transaction state (<see cref="NotActivated"/>).
    /// </summary>
    Default = NotActivated,

    /// <summary>
    /// The transaction state is not activated.
    /// </summary>
    NotActivated = 0,

    /// <summary>
    /// The transaction is completed.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// The transaction is active (is running).
    /// </summary>
    Active = 2,

    /// <summary>
    /// The transaction is completing.
    /// </summary>
    Completing = 6,

    /// <summary>
    /// The transaction has been committed successfully.
    /// </summary>
    Committed = 8+1,
    /// <summary>
    /// The transaction has started <see cref="Transaction.Commit"/> method but still running.
    /// </summary>
    Committing = 8+6,
    
    /// <summary>
    /// The transaction has been rolled back.
    /// </summary>
    RolledBack = 16+1,

    /// <summary>
    /// The transaction has started <see cref="Transaction.Rollback"/> method but still running.
    /// </summary>
    RollingBack = 16+6,
  }
}