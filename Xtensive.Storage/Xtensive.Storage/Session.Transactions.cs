// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using System.Transactions;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  public partial class Session
  {
    private TransactionScope ambientTransactionScope;
 
    /// <summary>
    /// Gets the active transaction.
    /// </summary>    
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Occurs on <see cref="Transaction"/> opening.
    /// </summary>
    public EventHandler<TransactionEventArgs> OnOpenTransaction;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to commit.
    /// </summary>
    public EventHandler<TransactionEventArgs> OnCommittingTransaction;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> committed.
    /// </summary>
    public EventHandler<TransactionEventArgs> OnCommitTransaction;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to rollback.
    /// </summary>
    public EventHandler<TransactionEventArgs> OnRollbackingTransaction;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> rolled back.
    /// </summary>
    public EventHandler<TransactionEventArgs> OnRollbackTransaction;

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="autoTransaction">if set to <see langword="true"/> auto transaction is demanded.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public TransactionScope OpenTransaction(IsolationLevel isolationLevel, bool autoTransaction)
    {
      if (Transaction != null)
        return null;
      if (autoTransaction && !Configuration.AllowsAutoTransactions)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      var transaction = new Transaction(this, isolationLevel);
      Transaction = transaction;
      var ts = (TransactionScope) transaction.Begin();
      if (ts!=null && Configuration.UsesAmbientTransactions) {
        ambientTransactionScope = ts;
        return null;
      }
      return ts;
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public TransactionScope OpenTransaction(IsolationLevel isolationLevel)
    {
      return OpenTransaction(isolationLevel, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public TransactionScope OpenTransaction(bool autoTransaction)
    {
      return OpenTransaction(Handler.DefaultIsolationLevel, autoTransaction);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public TransactionScope OpenTransaction()
    {
      return OpenTransaction(Handler.DefaultIsolationLevel, false);
    }

    /// <summary>
    /// Commits the ambient transaction.
    /// </summary>
    public void CommitAmbientTransaction()
    {
      var ts = ambientTransactionScope;
      try {
        ts.Complete();
      }
      finally {
        ambientTransactionScope = null;
        ts.DisposeSafely();
      }
    }

    /// <summary>
    /// Rolls back the ambient transaction.
    /// </summary>
    public void RollbackAmbientTransaction()
    {
      var ts = ambientTransactionScope;
      ambientTransactionScope = null;
      ts.DisposeSafely();
    }

    #region OnXxx event-like methods

    internal void BeginTransaction()
    {
      Handler.BeginTransaction();
      NotifyOpenTransaction(Transaction);

    }

    private void NotifyOpenTransaction(Transaction transaction)
    {
      if (OnOpenTransaction!=null)
        OnOpenTransaction(this, new TransactionEventArgs(transaction));
    }

    private void NotifyCommittingTransaction(Transaction transaction)
    {
      if (OnCommittingTransaction!=null)
        OnCommittingTransaction(this, new TransactionEventArgs(transaction));
    }

    private void NotifyCommitTransaction(Transaction transaction)
    {
      if (OnCommitTransaction!=null)
        OnCommitTransaction(this, new TransactionEventArgs(transaction));
    }

    private void NotifyRollbackingTransaction(Transaction transaction)
    {
      if (OnRollbackingTransaction!=null)
        OnRollbackingTransaction(this, new TransactionEventArgs(transaction));
    }

    private void NotifyRollbackTransaction(Transaction transaction)
    {
      if (OnRollbackTransaction!=null)
        OnRollbackTransaction(this, new TransactionEventArgs(transaction));
    }


    internal void CommitTransaction()
    {
      try {
        Persist();
        NotifyCommittingTransaction(Transaction);
        Handler.CommitTransaction();
        NotifyCommitTransaction(Transaction);
        CompleteTransaction();
      }
      catch {        
        RollbackTransaction();
        throw;
      }
    }

    internal void RollbackTransaction()
    {
      try {
        NotifyRollbackingTransaction(Transaction);
        Handler.RollbackTransaction();
        NotifyRollbackTransaction(Transaction);
      }
      finally {
        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.New))
          item.PersistenceState = PersistenceState.Synchronized;
        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.Modified))
          item.PersistenceState = PersistenceState.Synchronized;
        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.Removed))
          item.PersistenceState = PersistenceState.Synchronized;
        EntityStateRegistry.Clear();
        CompleteTransaction();
      }
    }

    private void CompleteTransaction()
    {
      Transaction = null;
    }

    #endregion
  }
}
