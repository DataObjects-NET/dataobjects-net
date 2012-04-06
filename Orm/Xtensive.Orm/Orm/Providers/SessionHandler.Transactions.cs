// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System;
using Xtensive.Orm;


namespace Xtensive.Orm.Providers
{
  partial class SessionHandler
  {
    // Transaction related methods

    /// <summary>
    /// Gets a value indicating whether transaction is actually started.
    /// This property indicates presence of outermost transaction only.
    /// </summary>
    public abstract bool TransactionIsStarted { get; }

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction(Transaction transaction);

    /// <summary>
    /// Clears transaction-related caches.
    /// This method is called for non-actual transactions as well.
    /// </summary>    
    public virtual void CompletingTransaction(Transaction transaction)
    {
      prefetchManager.CancelTasks();
    }

    /// <summary>
    /// Commits the transaction.
    /// This method is invoked for actual transactions only.
    /// </summary>    
    public abstract void CommitTransaction(Transaction transaction);

    /// <summary>
    /// Rollbacks the transaction.
    /// This method is invoked for actual transactions only.
    /// </summary>    
    public abstract void RollbackTransaction(Transaction transaction);

    // Savepoint related methods

    /// <summary>
    /// Creates the savepoint.
    /// </summary>
    public abstract void CreateSavepoint(Transaction transaction);

    /// <summary>
    /// Releases the savepoint.
    /// </summary>
    public abstract void ReleaseSavepoint(Transaction transaction);

    /// <summary>
    /// Rollbacks to savepoint.
    /// </summary>
    public abstract void RollbackToSavepoint(Transaction transaction);

    private void EnsureTransactionIsOpened()
    {
      var transaction = Session.Transaction;
      if (transaction==null)
        throw new InvalidOperationException(Strings.ExActiveTransactionIsRequiredForThisOperationUseTransactionOpenToOpenIt);
    }
  }
}