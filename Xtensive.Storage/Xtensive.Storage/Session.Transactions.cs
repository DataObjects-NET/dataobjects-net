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

    internal TransactionScope OpenTransaction()
    {
      return OpenTransaction(Handler.DefaultIsolationLevel);
    }

    internal TransactionScope OpenTransaction(IsolationLevel isolationLevel)
    {
      if (Transaction != null)
        return TransactionScope.VoidScopeInstance;
      var transaction = new Transaction(this, isolationLevel);
      Transaction = transaction;
      var transactionScope = (TransactionScope) transaction.Begin();
      if (transactionScope!=null && Configuration.UsesAmbientTransactions) {
        ambientTransactionScope = transactionScope;
        return TransactionScope.VoidScopeInstance;
      }
      return transactionScope;
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

    internal void BeginTransaction()
    {
      Handler.BeginTransaction();
      NotifyTransactionOpen(Transaction);
    }

    #region NotifyXxx methods

    private void NotifyTransactionOpen(Transaction transaction)
    {
      if (TransactionOpen!=null)
        TransactionOpen(this, new TransactionEventArgs(transaction));
    }

    private void NotifyTransactionCommitting(Transaction transaction)
    {
      if (TransactionCommitting!=null)
        TransactionCommitting(this, new TransactionEventArgs(transaction));
    }

    private void NotifyTransactionCommitted(Transaction transaction)
    {
      if (TransactionCommitted!=null)
        TransactionCommitted(this, new TransactionEventArgs(transaction));
    }

    private void NotifyTransactionRollbacking(Transaction transaction)
    {
      if (TransactionRollbacking!=null)
        TransactionRollbacking(this, new TransactionEventArgs(transaction));
    }

    private void NotifyTransactionRollbacked(Transaction transaction)
    {
      if (TransactionRollbacked!=null)
        TransactionRollbacked(this, new TransactionEventArgs(transaction));
    }
    
    internal void CommitTransaction()
    {
      try {
        Persist();
        queryTasks.Clear();
        NotifyTransactionCommitting(Transaction);
        Handler.CommitTransaction();
        NotifyTransactionCommitted(Transaction);
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
        NotifyTransactionRollbacking(Transaction);
        Handler.RollbackTransaction();
        NotifyTransactionRollbacked(Transaction);
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
