// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.02

using System;

namespace Xtensive.Integrity.Transactions
{
  /// <summary>
  /// Indicates operational state of the <see cref="TransactionBase{TScope,TTransaction}"/> instance.
  /// </summary>
  [Serializable]
  public enum TransactionState
  {
    /// <summary>
    /// Default transaction state (<see cref="Invalid"/>).
    /// </summary>
    Default = Invalid,

    /// <summary>
    /// The transaction state is invalid \ unknown.
    /// </summary>
    Invalid = 0,

    /// <summary>
    /// The transaction is completed.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// The transaction is active (is running).
    /// </summary>
    Active = 2,

    /// <summary>
    /// The transaction is completing (<see cref="TransactionBase{TScope, TTransaction}.Commit"/> or
    /// <see cref="TransactionBase{TScope,TTransaction}.Rollback"/> method is called, but not finished yet).
    /// </summary>
    Completing = 6,

    /// <summary>
    /// The transaction has been committed successfully.
    /// </summary>
    Committed = 8+1,
    /// <summary>
    /// The transaction has started <see cref="TransactionBase{TScope, TTransaction}.Commit"/> method but still running.
    /// </summary>
    Committing = 8+6,
    
    /// <summary>
    /// The transaction has been rolled back.
    /// </summary>
    RolledBack = 16+1,

    /// <summary>
    /// The transaction has started <see cref="TransactionBase{TScope, TTransaction}.Rollback"/> method but still running.
    /// </summary>
    RollingBack = 16+6,
  }
}