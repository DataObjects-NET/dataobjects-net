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
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using SD=System.Data;

namespace Xtensive.Storage
{
  public partial class Session
  {
    private const string SavepointNameFormat = "s{0}";

    private int nextSavepoint;
    
    /// <summary>
    /// Gets the active transaction.
    /// </summary>    
    public Transaction Transaction { get; private set; }

    internal TransactionScope OpenTransaction(TransactionOpenMode mode, IsolationLevel isolationLevel, bool isAutomatic)
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
        return CreateOutermostTransaction(isolationLevel, isAutomatic);
      case TransactionOpenMode.New:
        if (isolationLevel==IsolationLevel.Unspecified)
          isolationLevel = Configuration.DefaultIsolationLevel;
        return transaction!=null 
          ? CreateNestedTransaction(isolationLevel, isAutomatic) 
          : CreateOutermostTransaction(isolationLevel, isAutomatic);
      default:
        throw new ArgumentOutOfRangeException("mode");
      }
    }

    internal void BeginTransaction(Transaction transaction)
    {
      if (!Configuration.UseAutoShortenedTransactions || transaction.IsNested || IsDisconnected)
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

      Handler.CompletingTransaction(transaction);
      if (!transaction.IsActuallyStarted)
        return;
      if (transaction.IsNested)
        Handler.ReleaseSavepoint(transaction);
      else
        Handler.CommitTransaction(transaction);
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
        try {
          Handler.CompletingTransaction(transaction);
        }
        finally {
          try {
            if (transaction.IsActuallyStarted)
              if (transaction.IsNested)
                Handler.RollbackToSavepoint(transaction);
              else
                Handler.RollbackTransaction(transaction);
          }
          finally {
            ClearChangeRegistry();
          }
        }
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
      var transaction = Transaction;
      if (transaction==null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      if (!transaction.IsActuallyStarted)
        StartTransaction(transaction);
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
        Handler.CreateSavepoint(transaction);
      }
      else
        Handler.BeginTransaction(transaction);
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

    private TransactionScope CreateOutermostTransaction(IsolationLevel isolationLevel, bool isAutomatic)
    {
      var transaction = new Transaction(this, isolationLevel, isAutomatic);
      return OpenTransactionScope(transaction);
    }

    private TransactionScope CreateNestedTransaction(IsolationLevel isolationLevel, bool isAutomatic)
    {
      var newTransaction = new Transaction(this, isolationLevel, isAutomatic, Transaction, GetNextSavepointName());
      return OpenTransactionScope(newTransaction);
    }

    private TransactionScope OpenTransactionScope(Transaction transaction)
    {
      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXOpeningTransaction, this);
      
      SystemEvents.NotifyTransactionOpening(transaction);
      Events.NotifyTransactionOpening(transaction);

      Transaction = transaction;
      transaction.Begin();

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
