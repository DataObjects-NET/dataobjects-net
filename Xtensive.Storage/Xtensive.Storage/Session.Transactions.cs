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
      switch (mode) {
      case TransactionOpenMode.Auto:
        if (Transaction!=null) {
          if (isolationLevel!=Transaction.IsolationLevel)
            throw new InvalidOperationException(Strings.ExCanNotReuseOpenedTransactionRequestedIsolationLevelIsDifferent);
          return TransactionScope.VoidScopeInstance;
        }
        return Configuration.UsesAmbientTransactions
          ? CreateAmbientTransaction(isolationLevel)
          : CreateOutermostTransaction(isolationLevel);
      case TransactionOpenMode.New:
        if (Transaction!=null)
          return CreateNestedTransaction(isolationLevel);
        if (Configuration.UsesAmbientTransactions) {
          CreateAmbientTransaction(isolationLevel);
          return CreateNestedTransaction(isolationLevel);
        }
        return CreateOutermostTransaction(isolationLevel);
      default:
        throw new ArgumentOutOfRangeException("mode");
      }
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
      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXBeginTransaction, this);
      NotifyTransactionOpen(transaction);
    }

    internal void CommitTransaction(Transaction transaction)
    {
      try {
        Persist();
        queryTasks.Clear();
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXCommittingTransaction, this);
        NotifyTransactionCommitting(transaction);
        if (transaction.IsActuallyStarted)
          if (transaction.IsNested)
            Handler.ReleaseSavepoint(transaction.SavepointName);
          else
            Handler.CommitTransaction();
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXCommittedTransaction, this);
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
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXRollingBackTransaction, this);
        NotifyTransactionRollbacking(transaction);
        if (transaction.IsActuallyStarted)
          if (transaction.IsNested)
            Handler.RollbackToSavepoint(transaction.SavepointName);
          else
            Handler.RollbackTransaction();
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXRolledBackTransaction, this);
        NotifyTransactionRollbacked(transaction);
      }
      finally {
        ClearTransactionalRegistries();
        CompleteTransaction(transaction);
      }
    }

    private void EnsureTransactionIsStarted()
    {
      if (Transaction==null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      if (!Transaction.IsActuallyStarted)
        StartTransaction(Transaction);
    }

    private void StartTransaction(Transaction transaction)
    {
      transaction.IsActuallyStarted = true;
      if (transaction.IsNested) {
        if (!transaction.Outer.IsActuallyStarted)
          StartTransaction(transaction.Outer);
        Persist();
        Handler.MakeSavepoint(transaction.SavepointName);
      }
      else
        Handler.BeginTransaction(transaction.IsolationLevel);

    }

    private void CompleteTransaction(Transaction transaction)
    {
      Transaction = transaction.Outer;
    }

    private void ClearTransactionalRegistries()
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

    private TransactionScope CreateAmbientTransaction(IsolationLevel isolationLevel)
    {
      var newTransaction = new Transaction(this, isolationLevel);
      ambientTransactionScope = OpenTransactionScope(newTransaction);
      return TransactionScope.VoidScopeInstance;
    }

    private TransactionScope CreateOutermostTransaction(IsolationLevel isolationLevel)
    {
      var newTransaction = new Transaction(this, isolationLevel);
      return OpenTransactionScope(newTransaction);
    }

    private TransactionScope CreateNestedTransaction(IsolationLevel isolationLevel)
    {
      var newTransaction = new Transaction(this, isolationLevel, Transaction, GetNextSavepointName());
      return OpenTransactionScope(newTransaction);
    }

    private TransactionScope OpenTransactionScope(Transaction transaction)
    {
      transaction.Begin();
      Transaction = transaction;
      return new TransactionScope(transaction);
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
