// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Commands;
using Xtensive.Storage.Model;
using Xtensive.Orm.Internals;
using Xtensive.Storage.Providers.Indexing.Resources;

namespace Xtensive.Storage.Providers.Indexing
{
  /// <summary>
  /// <see cref="Orm.Session"/>-level handler for index storage.
  /// </summary>
  public class SessionHandler : Providers.SessionHandler,
    IIndexResolver
  {
    protected IndexStorage storage;
    private bool isDebugLoggingEnabled;

    /// <summary>
    /// Gets the storage view.
    /// </summary>
    public IStorageView StorageView { get; private set; }

    /// <inheritdoc/>
    public override bool TransactionIsStarted { get { return StorageView!=null; } }

    public override void SetCommandTimeout(int? commandTimeout)
    {
      // TODO: implement this method
    }

    /// <inheritdoc/>
    public override void BeginTransaction(Orm.Transaction transaction)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXBeginningTransactionWithYIsolationLevel, 
            Session.ToStringSafely(), transaction.IsolationLevel);
        lock (ConnectionSyncRoot) {
          if (StorageView!=null)
            throw new InvalidOperationException(Strings.ExTransactionIsAlreadyOpened);
          StorageView = storage.CreateView(transaction.IsolationLevel);
          ((IndexStorageView) StorageView).Initialize(this);
          // TODO: Implement transactions
        }
      }
      catch (Exception e) {
        throw TranslateException(null, e);
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Transaction is not open.</exception>
    public override void CommitTransaction(Orm.Transaction transaction)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXCommitTransaction, Session.ToStringSafely());
        lock (ConnectionSyncRoot) {
          if (StorageView==null)
            throw new InvalidOperationException(Strings.ExTransactionIsNotOpened);
          StorageView.Transaction.Commit();
          StorageView = null;
          // TODO: Implement transactions
        }
      }
      catch (Exception e) {
        throw TranslateException(null, e);
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Transaction is not open.</exception>
    public override void RollbackTransaction(Orm.Transaction transaction)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXRollbackTransaction, Session.ToStringSafely());
        lock (ConnectionSyncRoot) {
          if (StorageView==null)
            throw new InvalidOperationException(Strings.ExTransactionIsNotOpened);
          StorageView.Transaction.Rollback();
          StorageView = null;
          // TODO: Implement transactions
        }
      }
      catch (Exception e) {
        throw TranslateException(null, e);
      }
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(Orm.Transaction transaction)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXMakeSavepointY, Session.ToStringSafely(), GetSavepointName(transaction));
        // TODO: Implement nested transactions
      }
      catch (Exception e) {
        throw TranslateException(null, e);
      }
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(Orm.Transaction transaction)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXRollbackToSavepointY, Session.ToStringSafely(), GetSavepointName(transaction));
        throw new NotSupportedException(Strings.ExCurrentStorageProviderDoesNotSupportSavepoints);
        // TODO: Implement nested transactions
      }
      catch (Exception e) {
        throw TranslateException(null, e);
      }
    }

    public override void ReleaseSavepoint(Orm.Transaction transaction)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXReleaseSavepointY, Session.ToStringSafely(), GetSavepointName(transaction));
        // TODO: Implement nested transactions
      }
      catch (Exception e) {
        throw TranslateException(null, e);
      }
    }

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool allowPartialExecution)
    {
      lock (ConnectionSyncRoot) {
        var batched = persistActions.SelectMany(statePair => CreateCommandBatch(statePair)).Batch(0, 256, 256);
        foreach (var batch in batched)
          foreach (var command in batch)
            StorageView.Execute(command);
      }
    }

    #region CreateXxx methods

    private IEnumerable<Command> CreateCommandBatch(PersistAction persistAction)
    {
      switch (persistAction.ActionKind) {
      case PersistActionKind.Insert:
        return CreateInsert(persistAction.EntityState);
      case PersistActionKind.Update:
        return CreateUpdate(persistAction.EntityState);
      case PersistActionKind.Remove:
        return CreateRemove(persistAction.EntityState);
      default:
        throw new ArgumentOutOfRangeException("statePair.Second");
      }
    }

    private IEnumerable<Command> CreateInsert(EntityState state)
    {
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var transform = ((DomainHandler) Handlers.DomainHandler).GetTransform(index, state.Type);
        var transformed = transform.Apply(TupleTransformType.Tuple, state.Tuple);
        yield return IndexUpdateCommand.Insert(index.ReflectedType.MappingName, state.Key.Value, transformed);
      }
    }

    private IEnumerable<Command> CreateUpdate(EntityState state)
    {
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var transform = ((DomainHandler) Handlers.DomainHandler).GetTransform(index, state.Type);
        var transformed = transform.Apply(TupleTransformType.Tuple, state.Tuple);
        yield return IndexUpdateCommand.Update(index.ReflectedType.MappingName, state.Key.Value, transformed);
      }
    }

    private static IEnumerable<Command> CreateRemove(EntityState state)
    {
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary))
        yield return IndexUpdateCommand.Remove(index.ReflectedType.MappingName, state.Key.Value);
    }

    #endregion

    #region IIndexResolver members

    /// <inheritdoc/>
    public virtual IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo)
    {
      if (StorageView == null)
        return null;
      return StorageView.GetIndex(indexInfo);
    }

    #endregion

    /// <summary>
    /// Translates thrown exception.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="exception">The exception.</param>
    /// <returns>Translated exception.</returns>
    protected virtual Exception TranslateException(object query, Exception exception)
    {
      return exception;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      storage = ((DomainHandler) Handlers.DomainHandler).Storage;
      isDebugLoggingEnabled = Session.IsDebugEventLoggingEnabled;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
    }
  }
}
