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
    private const string SavepointNameFormat = "s{0}";

    private int nextSavepoint;
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
    
    internal TransactionScope OpenTransaction(TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      return Configuration.UsesAmbientTransactions
        ? OpenAmbientTransaction(isolationLevel)
        : OpenRegularTransaction(mode, isolationLevel);
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

    internal void BeginTransaction(Transaction transaction)
    {
      if (!Configuration.UsesAutoshortenedTransactions)
        StartTransaction(transaction);
      NotifyTransactionOpen(transaction);
    }

    internal void CommitTransaction(Transaction transaction)
    {
      try {
        EnsureCanCompleteTransaction(transaction);
        Persist();
        queryTasks.Clear();
        NotifyTransactionCommitting(transaction);
        if (transaction.IsActuallyStarted && !transaction.IsNested)
          Handler.CommitTransaction();
        NotifyTransactionCommitted(transaction);
        CompleteTransaction(transaction);
      }
      catch {
        RollbackTransaction(transaction);
        throw;
      }
    }

    internal void RollbackTransaction(Transaction transaction)
    {
      try {
        EnsureCanCompleteTransaction(transaction);
        NotifyTransactionRollbacking(transaction);
        if (transaction.IsActuallyStarted)
          if (transaction.IsNested)
            Handler.RollbackToSavepoint(transaction.SavepointName);
          else
            Handler.RollbackTransaction();
        NotifyTransactionRollbacked(transaction);
      }
      finally {
        ClearTransactionalRegistires();
        CompleteTransaction(transaction);
      }
    }

    private void EnsureTransactionIsStarted(Transaction transaction)
    {
      if (transaction==null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      if (!transaction.IsActuallyStarted)
        StartTransaction(transaction);
    }

    private void StartTransaction(Transaction transaction)
    {
      transaction.IsActuallyStarted = true;
      if (transaction.IsNested) {
        EnsureTransactionIsStarted(transaction);
        Persist();
        Handler.MakeSavepoint(transaction.SavepointName);
      }
      else
        Handler.BeginTransaction(transaction.IsolationLevel);
    }

    private void EnsureCanCompleteTransaction(Transaction transaction)
    {
      if (transaction.Inner==null)
        return;

      Transaction = null;
      Handler.RollbackTransaction();
      ClearTransactionalRegistires();

      throw new InvalidOperationException(Strings.ExCanNotCompleteOuterTransactionInnerTransactionIsActive);
    }

    private void CompleteTransaction(Transaction transaction)
    {
      Transaction = transaction.Outer;
      if (Transaction!=null)
        Transaction.Inner = null;
    }

    private void ClearTransactionalRegistires()
    {
      foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.New))
        item.PersistenceState = PersistenceState.Synchronized;
      foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Modified))
        item.PersistenceState = PersistenceState.Synchronized;
      foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Removed))
        item.PersistenceState = PersistenceState.Synchronized;
      EntityChangeRegistry.Clear();
      queryTasks.Clear();      
    }

    private string GetNextSavepointName()
    {
      return string.Format(SavepointNameFormat, nextSavepoint++);
    }

    private TransactionScope OpenAmbientTransaction(IsolationLevel isolationLevel)
    {
      if (Transaction==null) {
        var newTransaction = new Transaction(this, isolationLevel);
        ambientTransactionScope = (TransactionScope) newTransaction.Begin();
        Transaction = newTransaction;
      }
      return TransactionScope.VoidScopeInstance;
    }

    private TransactionScope OpenRegularTransaction(TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      Transaction newTransaction = null;

      if (Transaction==null)
        newTransaction = new Transaction(this, isolationLevel);
      else
        switch (mode) {
        case TransactionOpenMode.New:
          if (Transaction.Inner!=null)
            throw new InvalidOperationException(Strings.ExCanNotOpenMoreThanOneInnerTransaction);
          newTransaction = new Transaction(this, isolationLevel, Transaction, GetNextSavepointName());
          break;
        case TransactionOpenMode.Auto:
          if (isolationLevel!=Transaction.IsolationLevel)
            throw new InvalidOperationException(Strings.ExCanNotReuseOpenedTransactionRequestedIsolationLevelIsDifferent);
          break;
        default:
          throw new ArgumentOutOfRangeException("mode");
        }

      if (newTransaction==null)
        return TransactionScope.VoidScopeInstance;

      TransactionScope result;
      try {
        result = (TransactionScope) newTransaction.Begin();
      }
      catch {
        if (Transaction!=null)
          Transaction.Inner = null;
        throw;
      }

      Transaction = newTransaction;
      return result;
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
    
    #endregion
  }
}
