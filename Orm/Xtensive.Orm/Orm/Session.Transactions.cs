// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using System.Transactions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Validation;
using SD=System.Data;

namespace Xtensive.Orm
{
  public partial class Session
  {
    private const string SavepointNameFormat = "s{0}";

    private readonly StateLifetimeToken sessionLifetimeToken = new StateLifetimeToken();
    private int nextSavepoint;

    /// <summary>
    /// Gets the active transaction.
    /// </summary>    
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public TransactionScope OpenTransaction()
    {
      return OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public TransactionScope OpenTransaction(IsolationLevel isolationLevel)
    {
      return OpenTransaction(TransactionOpenMode.Default, isolationLevel, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public TransactionScope OpenTransaction(TransactionOpenMode mode)
    {
      return OpenTransaction(mode, IsolationLevel.Unspecified, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public TransactionScope OpenTransaction(TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      return OpenTransaction(mode, isolationLevel, false);
    }

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


    // OpenAuto method group

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal TransactionScope OpenAutoTransaction()
    {
      return OpenAutoTransaction(TransactionalBehavior.Auto, IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="behavior">The automatic transaction behavior.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal TransactionScope OpenAutoTransaction(TransactionalBehavior behavior)
    {
      return OpenAutoTransaction(behavior, IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal TransactionScope OpenAutoTransaction(IsolationLevel isolationLevel)
    {
      return OpenAutoTransaction(TransactionalBehavior.Auto, isolationLevel);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="behavior">The automatic transaction behavior.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    internal TransactionScope OpenAutoTransaction(TransactionalBehavior behavior, IsolationLevel isolationLevel)
    {
      switch (behavior) {
      case TransactionalBehavior.Auto:
       if (Configuration.Supports(SessionOptions.AutoTransactionOpenMode))
         goto case TransactionalBehavior.Open;
       if (Configuration.Supports(SessionOptions.AutoTransactionSuppressMode))
         goto case TransactionalBehavior.Suppress;
       goto case TransactionalBehavior.Require;
      case TransactionalBehavior.Require:
        return TransactionScope.VoidScopeInstance;
      case TransactionalBehavior.Open:
        if (IsDisconnected && Transaction!=null && !Transaction.IsDisconnected)
          goto case TransactionalBehavior.New;
        return OpenTransaction(TransactionOpenMode.Auto, isolationLevel, true);
      case TransactionalBehavior.New:
        return OpenTransaction(TransactionOpenMode.New, isolationLevel, true);
      case TransactionalBehavior.Suppress:
        return TransactionScope.VoidScopeInstance;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }
    
    internal void BeginTransaction(Transaction transaction)
    {
      if (transaction.IsNested) {
        Persist(PersistReason.NestedTransaction);
        Handler.CreateSavepoint(transaction);
      }
      else
        Handler.BeginTransaction(transaction);
    }

    internal void CommitTransaction(Transaction transaction)
    {
      OrmLog.Debug(Strings.LogSessionXCommittingTransaction, this);

      SystemEvents.NotifyTransactionPrecommitting(transaction);
      Events.NotifyTransactionPrecommitting(transaction);

      Persist(PersistReason.Commit);
      ValidationContext.Validate(ValidationReason.Commit);

      SystemEvents.NotifyTransactionCommitting(transaction);
      Events.NotifyTransactionCommitting(transaction);

      Handler.CompletingTransaction(transaction);
      if (transaction.IsNested)
        Handler.ReleaseSavepoint(transaction);
      else
        Handler.CommitTransaction(transaction);
    }

    internal void RollbackTransaction(Transaction transaction)
    {
      try {
        OrmLog.Debug(Strings.LogSessionXRollingBackTransaction, this);
        SystemEvents.NotifyTransactionRollbacking(transaction);
        Events.NotifyTransactionRollbacking(transaction);
      }
      finally {
        try {
          Handler.CompletingTransaction(transaction);
        }
        finally {
          try {
            
            if (Configuration.Supports(SessionOptions.SuppressRollbackExceptions))
              RollbackWithSuppression(transaction);
            else
              Rollback(transaction);
          }
          finally {
            if(!persistingIsFailed) {
              CancelEntitySetsChanges();
              ClearChangeRegistry();
            }
            persistingIsFailed = false;
          }
        }
      }
    }

    private void RollbackWithSuppression(Transaction transaction)
    {
      try {
        Rollback(transaction);
      }
      catch(Exception e) {
        OrmLog.Warning(e);
      }
    }

    private void Rollback(Transaction transaction)
    {
      if (transaction.IsNested)
        Handler.RollbackToSavepoint(transaction);
      else
        Handler.RollbackTransaction(transaction);
    }

    internal void CompleteTransaction(Transaction transaction)
    {
      queryTasks.Clear();
      pinner.ClearRoots();
      ValidationContext.Reset();

      Transaction = transaction.Outer;

      switch (transaction.State) {
      case TransactionState.Committed:
        OrmLog.Debug(Strings.LogSessionXCommittedTransaction, this);
        SystemEvents.NotifyTransactionCommitted(transaction);
        Events.NotifyTransactionCommitted(transaction);
        break;
      case TransactionState.RolledBack:
        OrmLog.Debug(Strings.LogSessionXRolledBackTransaction, this);
        SystemEvents.NotifyTransactionRollbacked(transaction);
        Events.NotifyTransactionRollbacked(transaction);
        break;
      default:
        throw new ArgumentOutOfRangeException("transaction.State");
      }
    }

    internal Transaction DemandTransaction()
    {
      var result = Transaction;
      if (result==null)
        throw new InvalidOperationException(Strings.ExActiveTransactionIsRequiredForThisOperationUseSessionOpenTransactionToOpenIt);
      return result;
    }

    /// <exception cref="InvalidOperationException">Can't create a transaction
    /// with requested isolation level.</exception>
    private void EnsureIsolationLevelCompatibility(IsolationLevel current, IsolationLevel requested)
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
      OrmLog.Debug(Strings.LogSessionXOpeningTransaction, this);
      
      SystemEvents.NotifyTransactionOpening(transaction);
      Events.NotifyTransactionOpening(transaction);

      Transaction = transaction;
      transaction.Begin();

      IDisposable logIndentScope = null;
      if (IsDebugEventLoggingEnabled)
        logIndentScope = OrmLog.DebugRegion(Strings.LogSessionXTransaction, this);
      
      SystemEvents.NotifyTransactionOpened(transaction);
      Events.NotifyTransactionOpened(transaction);

      return new TransactionScope(transaction, logIndentScope);
    }

    internal void SetTransaction(Transaction transaction)
    {
      Transaction = transaction;
    }

    internal StateLifetimeToken GetLifetimeToken()
    {
      var transaction = Transaction;
      if (transaction!=null)
        return transaction.LifetimeToken;
      if (Configuration.Supports(SessionOptions.NonTransactionalReads))
        return sessionLifetimeToken;
      throw new InvalidOperationException(Strings.ExActiveTransactionIsRequiredForThisOperationUseSessionOpenTransactionToOpenIt);
    }
  }
}
