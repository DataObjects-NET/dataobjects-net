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
    public event EventHandler<TransactionEventArgs> TransactionOpen;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to be committed.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionCommitting;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is committed.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionCommitted;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to be rolled back.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionRollbacking;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is rolled back.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionRollbacked;

    internal TransactionScope OpenTransaction(IsolationLevel isolationLevel, bool autoTransaction)
    {
      if (Transaction != null)
        return null;
      if (autoTransaction && !Configuration.AllowsAutoTransactions)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      var transaction = new Transaction(this, isolationLevel);
      Transaction = transaction;
      var transactionScope = (TransactionScope) transaction.Begin();
      if (transactionScope!=null && Configuration.UsesAmbientTransactions) {
        ambientTransactionScope = transactionScope;
        return null;
      }
      return transactionScope;
    }

    internal TransactionScope OpenTransaction(IsolationLevel isolationLevel)
    {
      return OpenTransaction(isolationLevel, false);
    }

    internal TransactionScope OpenTransaction(bool autoTransaction)
    {
      return OpenTransaction(Handler.DefaultIsolationLevel, autoTransaction);
    }

    internal TransactionScope OpenTransaction()
    {
      return OpenTransaction(Handler.DefaultIsolationLevel, false);
    }

    /// <summary>
    /// Commits the ambient transaction.
    /// </summary>
    public void CommitAmbientTransaction()
    {
      var scope = ambientTransactionScope;
      try {
        scope.Complete();
      }
      finally {
        ambientTransactionScope = null;
        scope.DisposeSafely();
      }
    }

    /// <summary>
    /// Rolls back the ambient transaction.
    /// </summary>
    public void RollbackAmbientTransaction()
    {
      var scope = ambientTransactionScope;
      ambientTransactionScope = null;
      scope.DisposeSafely();
    }

    #region OnXxx event-like methods

    internal void BeginTransaction()
    {
      Handler.BeginTransaction();
      OnTransactionOpen(Transaction);
    }

    private void OnTransactionOpen(Transaction transaction)
    {
      if (TransactionOpen!=null)
        TransactionOpen(this, new TransactionEventArgs(transaction));
    }

    private void OnTransactionCommitting(Transaction transaction)
    {
      if (TransactionCommitting!=null)
        TransactionCommitting(this, new TransactionEventArgs(transaction));
    }

    private void OnTransactionCommitted(Transaction transaction)
    {
      if (TransactionCommitted!=null)
        TransactionCommitted(this, new TransactionEventArgs(transaction));
    }

    private void OnTransactionRollbacking(Transaction transaction)
    {
      if (TransactionRollbacking!=null)
        TransactionRollbacking(this, new TransactionEventArgs(transaction));
    }

    private void NotifyRollbackTransaction(Transaction transaction)
    {
      if (TransactionRollbacked!=null)
        TransactionRollbacked(this, new TransactionEventArgs(transaction));
    }


    internal void CommitTransaction()
    {
      try {
        Persist();
        queryTasks.Clear();
        OnTransactionCommitting(Transaction);
        Handler.CommitTransaction();
        OnTransactionCommitted(Transaction);
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
        OnTransactionRollbacking(Transaction);
        Handler.RollbackTransaction();
        NotifyRollbackTransaction(Transaction);
      }
      finally {
        foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.New))
          item.PersistenceState = PersistenceState.Synchronized;
        foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Modified))
          item.PersistenceState = PersistenceState.Synchronized;
        foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Removed))
          item.PersistenceState = PersistenceState.Synchronized;
        EntityChangeRegistry.Clear();
        queryTasks.Clear();
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
