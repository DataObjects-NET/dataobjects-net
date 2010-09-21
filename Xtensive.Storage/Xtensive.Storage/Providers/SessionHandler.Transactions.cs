// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System;
using System.Transactions;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Providers
{
  partial class SessionHandler
  {
    /// <summary>
    /// Gets a value indicating whether transaction is actually started.
    /// This indicates presence of outermost transaction only.
    /// </summary>
    public abstract bool TransactionIsStarted { get; }

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction(Transaction transaction);

    /// <summary>
    /// Makes the savepoint.
    /// </summary>
    public abstract void CreateSavepoint(Transaction transaction);

    /// <summary>
    /// Rollbacks to savepoint.
    /// </summary>
    public virtual void RollbackToSavepoint(Transaction transaction)
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Releases the savepoint.
    /// </summary>
    public virtual void ReleaseSavepoint(Transaction transaction)
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Commits the transaction.
    /// </summary>    
    public virtual void CommitTransaction(Transaction transaction)
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>    
    public virtual void RollbackTransaction(Transaction transaction)
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Ensures the transaction is opened.
    /// </summary>
    /// <exception cref="InvalidOperationException">Transaction is not opened.</exception>
    protected void EnsureTransactionIsOpened()
    {
      var transaction = Session.Transaction ?? (Session.IsDisconnected ? Session.DisconnectedState.AlreadyOpenedTransaction : null);
      if (transaction == null)
        throw new InvalidOperationException(Strings.ExActiveTransactionIsRequiredForThisOperationUseTransactionOpenToOpenIt);
    }
  }
}