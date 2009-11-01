// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.18

using System;
using System.Transactions;
using Xtensive.Core;

namespace Xtensive.Integrity.Transactions
{
  public interface ITransaction : 
    IIdentified<Guid>
  {
    ///<summary>
    /// Gets the <see cref="TransactionState"/> of this instance.
    ///</summary>
    TransactionState State { get; }

    /// <summary>
    /// Gets the isolation level of the transaction.
    /// </summary>
    /// <value>The isolation level.</value>
    IsolationLevel IsolationLevel { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    void Commit();

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    void Rollback();
  }
}