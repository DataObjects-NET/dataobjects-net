// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using System.Threading;
using System.Threading.Tasks;
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
    public TransactionScope OpenTransaction() =>
      OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified, false);

    /// <summary>
    /// Asynchronously opens a new or joins to the already running transaction.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public ValueTask<TransactionScope> OpenTransactionAsync(CancellationToken token = default) =>
      OpenTransactionAsync(TransactionOpenMode.Default, IsolationLevel.Unspecified, false, token);

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public TransactionScope OpenTransaction(IsolationLevel isolationLevel) =>
      OpenTransaction(TransactionOpenMode.Default, isolationLevel, false);

    /// <summary>
    /// Asynchronously opens a new or joins to the already running transaction.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public ValueTask<TransactionScope> OpenTransactionAsync(
      IsolationLevel isolationLevel, CancellationToken token = default) =>
      OpenTransactionAsync(TransactionOpenMode.Default, isolationLevel, false, token);

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public TransactionScope OpenTransaction(TransactionOpenMode mode) =>
      OpenTransaction(mode, IsolationLevel.Unspecified, false);

    /// <summary>
    /// Asynchronously opens a new or joins to the already running transaction.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="mode">The mode.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public ValueTask<TransactionScope> OpenTransactionAsync(
      TransactionOpenMode mode, CancellationToken token = default) =>
      OpenTransactionAsync(mode, IsolationLevel.Unspecified, false, token);

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
    public TransactionScope OpenTransaction(TransactionOpenMode mode, IsolationLevel isolationLevel) =>
      OpenTransaction(mode, isolationLevel, false);

    /// <summary>
    /// Asynchronously opens a new or joins to the already running transaction.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="mode">The mode.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public ValueTask<TransactionScope> OpenTransactionAsync(
      TransactionOpenMode mode, IsolationLevel isolationLevel, CancellationToken token = default) =>
      OpenTransactionAsync(mode, isolationLevel, false, token);

    internal TransactionScope OpenTransaction(
      TransactionOpenMode mode, IsolationLevel isolationLevel, bool isAutomatic) =>
      InnerOpenTransaction(mode, isolationLevel, isAutomatic, false).GetAwaiter().GetResult();

    internal ValueTask<TransactionScope> OpenTransactionAsync(
      TransactionOpenMode mode, IsolationLevel isolationLevel, bool isAutomatic, CancellationToken token = default) =>
      InnerOpenTransaction(mode, isolationLevel, isAutomatic, true, token);

    private async ValueTask<TransactionScope> InnerOpenTransaction(
      TransactionOpenMode mode, IsolationLevel isolationLevel, bool isAutomatic, bool isAsync, CancellationToken token = default)
    {
      var transaction = Transaction;
      switch (mode) {
        case TransactionOpenMode.Auto:
          if (transaction != null) {
            if (isolationLevel != IsolationLevel.Unspecified && isolationLevel != transaction.IsolationLevel) {
              EnsureIsolationLevelCompatibility(transaction.IsolationLevel, isolationLevel);
            }

            return TransactionScope.VoidScopeInstance;
          }

          if (isolationLevel == IsolationLevel.Unspecified) {
            isolationLevel = Configuration.DefaultIsolationLevel;
          }

          return
            isAsync
              ? await CreateOutermostTransactionAsync(isolationLevel, isAutomatic, token).ConfigureAwait(false)
              : CreateOutermostTransaction(isolationLevel, isAutomatic);
        case TransactionOpenMode.New:
          if (isolationLevel == IsolationLevel.Unspecified) {
            isolationLevel = Configuration.DefaultIsolationLevel;
          }

          return
            isAsync
              ? transaction != null
                ? await CreateNestedTransactionAsync(isolationLevel, isAutomatic, token).ConfigureAwait(false)
                : await CreateOutermostTransactionAsync(isolationLevel, isAutomatic, token).ConfigureAwait(false)
              : transaction != null
                ? CreateNestedTransaction(isolationLevel, isAutomatic)
                : CreateOutermostTransaction(isolationLevel, isAutomatic);
        default:
          throw new ArgumentOutOfRangeException(nameof(mode));
      }
    }


    // OpenAuto method group

    /// <summary>
    /// Opens the automatic transaction, or does nothing.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal TransactionScope OpenAutoTransaction()
    {
      return TransactionScope.VoidScopeInstance;
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal TransactionScope OpenAutoTransaction(IsolationLevel isolationLevel)
    {
      return TransactionScope.VoidScopeInstance;
    }

    internal void BeginTransaction(Transaction transaction)
    {
      if (transaction.IsNested) {
        Persist(PersistReason.NestedTransaction);
        Handler.CreateSavepoint(transaction);
      }
      else {
        Handler.BeginTransaction(transaction);
      }
    }

    internal async Task BeginTransactionAsync(Transaction transaction, CancellationToken token)
    {
      if (transaction.IsNested) {
        await PersistAsync(PersistReason.NestedTransaction, token).ConfigureAwait(false);
        await Handler.CreateSavepointAsync(transaction, token).ConfigureAwait(false);
      }
      else {
        await Handler.BeginTransactionAsync(transaction, token).ConfigureAwait(false);
      }
    }

    internal async ValueTask CommitTransaction(Transaction transaction, bool isAsync)
    {
      if (IsDebugEventLoggingEnabled) {
        OrmLog.Debug(nameof(Strings.LogSessionXCommittingTransaction), this);
      }

      SystemEvents.NotifyTransactionPrecommitting(transaction);
      Events.NotifyTransactionPrecommitting(transaction);

      if (isAsync) {
        await PersistAsync(PersistReason.Commit).ConfigureAwait(false);
      }
      else {
        Persist(PersistReason.Commit);
      }

      ValidationContext.Validate(ValidationReason.Commit);

      SystemEvents.NotifyTransactionCommitting(transaction);
      Events.NotifyTransactionCommitting(transaction);

      Handler.CompletingTransaction(transaction);
      if (transaction.IsNested) {
        if (isAsync) {
          await Handler.ReleaseSavepointAsync(transaction).ConfigureAwait(false);
        }
        else {
          Handler.ReleaseSavepoint(transaction);
        }
      }
      else {
        if (isAsync) {
          await Handler.CommitTransactionAsync(transaction).ConfigureAwait(false);
        }
        else {
          Handler.CommitTransaction(transaction);
        }
      }
    }

    internal async ValueTask RollbackTransaction(Transaction transaction, bool isAsync)
    {
      try {
        if (IsDebugEventLoggingEnabled) {
          OrmLog.Debug(nameof(Strings.LogSessionXRollingBackTransaction), this);
        }

        SystemEvents.NotifyTransactionRollbacking(transaction);
        Events.NotifyTransactionRollbacking(transaction);
      }
      finally {
        try {
          Handler.CompletingTransaction(transaction);
        }
        finally {
          try {
            if (Configuration.Supports(SessionOptions.SuppressRollbackExceptions)) {
              await RollbackWithSuppression(transaction, isAsync).ConfigureAwait(false);
            }
            else {
              if (isAsync) {
                await RollbackAsync(transaction).ConfigureAwait(false);
              }
              else {
                Rollback(transaction);
              }
            }
          }
          finally {
            if(!persistingIsFailed || !Configuration.Supports(SessionOptions.NonTransactionalReads)) {
              CancelEntitySetsChanges();
              ClearChangeRegistry();
              NonPairedReferencesRegistry.Clear();
              EntitySetChangeRegistry.Clear();
            }
            persistingIsFailed = false;
          }
        }
      }
    }

    private async ValueTask RollbackWithSuppression(Transaction transaction, bool isAsync)
    {
      try {
        if (isAsync) {
          await RollbackAsync(transaction).ConfigureAwait(false);
        }
        else {
          Rollback(transaction);
        }
      }
      catch(Exception e) {
        OrmLog.Warning(e);
      }
    }

    private void Rollback(Transaction transaction)
    {
      if (transaction.IsNested) {
        Handler.RollbackToSavepoint(transaction);
      }
      else {
        Handler.RollbackTransaction(transaction);
      }
    }

    private async ValueTask RollbackAsync(Transaction transaction)
    {
      if (transaction.IsNested) {
        await Handler.RollbackToSavepointAsync(transaction).ConfigureAwait(false);
      }
      else {
        await Handler.RollbackTransactionAsync(transaction).ConfigureAwait(false);
      }
    }

    internal void CompleteTransaction(Transaction transaction)
    {
      userDefinedQueryTasks.Clear();
      pinner.ClearRoots();
      ValidationContext.Reset();

      Transaction = transaction.Outer;

      switch (transaction.State) {
      case TransactionState.Committed:
        if (IsDebugEventLoggingEnabled) {
          OrmLog.Debug(nameof(Strings.LogSessionXCommittedTransaction), this);
        }

        SystemEvents.NotifyTransactionCommitted(transaction);
        Events.NotifyTransactionCommitted(transaction);
        break;
      case TransactionState.RolledBack:
        if (IsDebugEventLoggingEnabled) {
          OrmLog.Debug(nameof(Strings.LogSessionXRolledBackTransaction), this);
        }

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
      return OpenTransactionScope(transaction, false).GetAwaiter().GetResult();
    }

    private ValueTask<TransactionScope> CreateOutermostTransactionAsync(
      IsolationLevel isolationLevel, bool isAutomatic, CancellationToken token)
    {
      var transaction = new Transaction(this, isolationLevel, isAutomatic);
      return OpenTransactionScope(transaction, true, token);
    }

    private TransactionScope CreateNestedTransaction(IsolationLevel isolationLevel, bool isAutomatic)
    {
      var newTransaction = new Transaction(this, isolationLevel, isAutomatic, Transaction, GetNextSavepointName());
      return OpenTransactionScope(newTransaction, false).GetAwaiter().GetResult();
    }

    private ValueTask<TransactionScope> CreateNestedTransactionAsync(
      IsolationLevel isolationLevel, bool isAutomatic, CancellationToken token)
    {
      var newTransaction = new Transaction(this, isolationLevel, isAutomatic, Transaction, GetNextSavepointName());
      return OpenTransactionScope(newTransaction, false, token);
    }

    private async ValueTask<TransactionScope> OpenTransactionScope(
      Transaction transaction, bool isAsync, CancellationToken token = default)
    {
      if (IsDebugEventLoggingEnabled) {
        OrmLog.Debug(nameof(Strings.LogSessionXOpeningTransaction), this);
      }

      SystemEvents.NotifyTransactionOpening(transaction);
      Events.NotifyTransactionOpening(transaction);

      Transaction = transaction;
      if (isAsync) {
        await transaction.BeginAsync(token).ConfigureAwait(false);
      }
      else {
        transaction.Begin();
      }

      IDisposable logIndentScope = null;
      if (IsDebugEventLoggingEnabled) {
        logIndentScope = OrmLog.DebugRegion(nameof(Strings.LogSessionXTransaction), this);
      }

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
