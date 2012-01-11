// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using System.Transactions;
using Xtensive.Core.Disposing;
using Xtensive.Core;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using SD=System.Data;

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

    internal TransactionScope OpenTransaction(TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      var transaction = Transaction;
      switch (mode) {
      case TransactionOpenMode.Auto:
        if (transaction!=null) {
          if (isolationLevel!=IsolationLevel.Unspecified && isolationLevel!=transaction.IsolationLevel)
            EnsureIsolationLevelCompatibility(transaction.IsolationLevel, isolationLevel);
          return TransactionScope.VoidScopeInstance;
        }
        if (isolationLevel==IsolationLevel.Unspecified)
          isolationLevel = Configuration.DefaultIsolationLevel;
        return Configuration.UsesAmbientTransactions
          ? CreateAmbientTransaction(isolationLevel)
          : CreateOutermostTransaction(isolationLevel);
      case TransactionOpenMode.New:
        if (isolationLevel==IsolationLevel.Unspecified)
          isolationLevel = Configuration.DefaultIsolationLevel;
        if (transaction!=null)
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
      if (!Configuration.UsesAutoShortenedTransactions || IsDisconnected)
        StartTransaction(transaction);
    }

    internal void CommitTransaction(Transaction transaction)
    {
      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXCommittingTransaction, this);
      
      SystemEvents.NotifyTransactionPrecommitting(transaction);
      Events.NotifyTransactionPrecommitting(transaction);
      
      SystemEvents.NotifyTransactionCommitting(transaction);
      Events.NotifyTransactionCommitting(transaction);

      Persist(PersistReason.Commit);

      if (!transaction.IsActuallyStarted)
        return;
      if (transaction.IsNested)
        Handler.ReleaseSavepoint(transaction.SavepointName);
      else
        Handler.CommitTransaction();
    }

    internal void RollbackTransaction(Transaction transaction)
    {
      try {
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXRollingBackTransaction, this);
        SystemEvents.NotifyTransactionRollbacking(transaction);
        Events.NotifyTransactionRollbacking(transaction);
      }
      finally {
        if (transaction.IsActuallyStarted)
          if (transaction.IsNested)
            Handler.RollbackToSavepoint(transaction.SavepointName);
          else
            Handler.RollbackTransaction();
        ClearChangeRegistry();
      }
    }

    internal void CompleteTransaction(Transaction transaction)
    {
      queryTasks.Clear();
      Pinner.ClearRoots();

      Transaction = transaction.Outer;

      switch (transaction.State) {
      case TransactionState.Committed:
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXCommittedTransaction, this);
        SystemEvents.NotifyTransactionCommitted(transaction);
        Events.NotifyTransactionCommitted(transaction);
        break;
      case TransactionState.RolledBack:
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXRolledBackTransaction, this);
        SystemEvents.NotifyTransactionRollbacked(transaction);
        Events.NotifyTransactionRollbacked(transaction);
        break;
      default:
        throw new ArgumentOutOfRangeException("transaction.State");
      }
    }

    internal void EnsureTransactionIsStarted()
    {
      var transaction = GetTransactionFromSessionOrDisconnectedState();
      if (transaction==null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      if (!transaction.IsActuallyStarted)
        StartTransaction(transaction);
    }

    internal Transaction GetTransactionFromSessionOrDisconnectedState()
    {
      return Transaction ?? (IsDisconnected ? DisconnectedState.AlreadyOpenedTransaction : null);
    }

    /// <exception cref="InvalidOperationException">Can't create a transaction
    /// with requested isolation level.</exception>
    private static void EnsureIsolationLevelCompatibility(IsolationLevel current, IsolationLevel requested)
    {
      var sdCurrent = IsolationLevelConverter.Convert(current);
      var sdRequested = IsolationLevelConverter.Convert(requested);
      if (sdRequested<=sdCurrent)
        return;
      // sdRequested > sdCurrent
      if (sdRequested==SD.IsolationLevel.Snapshot && sdCurrent==SD.IsolationLevel.Serializable)
        return; // The only good case here
      throw new InvalidOperationException(Strings.ExCanNotReuseOpenedTransactionRequestedIsolationLevelIsDifferent);
    }

    private void StartTransaction(Transaction transaction)
    {
      transaction.IsActuallyStarted = true;
      if (transaction.IsNested) {
        if (!transaction.Outer.IsActuallyStarted)
          StartTransaction(transaction.Outer);
        Persist(PersistReason.NestedTransaction);
        Handler.CreateSavepoint(transaction.SavepointName);
      }
      else
        Handler.BeginTransaction(transaction.IsolationLevel);
    }

    private string GetNextSavepointName()
    {
      return string.Format(SavepointNameFormat, nextSavepoint++);
    }

    private void ClearChangeRegistry()
    {
      foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.New))
        item.PersistenceState = PersistenceState.Synchronized;
      foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Modified))
        item.PersistenceState = PersistenceState.Synchronized;
      foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Removed))
        item.PersistenceState = PersistenceState.Synchronized;
      EntityChangeRegistry.Clear();
    }

    private TransactionScope CreateAmbientTransaction(IsolationLevel isolationLevel)
    {
      var newTransaction = new Transaction(this, isolationLevel);
      ambientTransactionScope = OpenTransactionScope(newTransaction);
      return TransactionScope.VoidScopeInstance;
    }

    private TransactionScope CreateOutermostTransaction(IsolationLevel isolationLevel)
    {
      var transaction = new Transaction(this, isolationLevel);
      return OpenTransactionScope(transaction);
    }

    private TransactionScope CreateNestedTransaction(IsolationLevel isolationLevel)
    {
      var newTransaction = new Transaction(this, isolationLevel, Transaction, GetNextSavepointName());
      return OpenTransactionScope(newTransaction);
    }

    private TransactionScope OpenTransactionScope(Transaction transaction)
    {
      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXOpeningTransaction, this);
      
      SystemEvents.NotifyTransactionOpening(transaction);
      Events.NotifyTransactionOpening(transaction);

      transaction.Begin();
      Transaction = transaction;

      IDisposable logIndentScope = null;
      if (IsDebugEventLoggingEnabled)
        logIndentScope = Log.DebugRegion(Strings.LogSessionXTransaction, this);
      
      SystemEvents.NotifyTransactionOpened(transaction);
      Events.NotifyTransactionOpened(transaction);

      return new TransactionScope(transaction, logIndentScope);
    }

    internal void SetTransaction(Transaction transaction)
    {
      Transaction = transaction;
    }
  }
}
