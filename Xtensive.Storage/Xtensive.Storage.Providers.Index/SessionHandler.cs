// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Providers.Index.Resources;
using Xtensive.Storage.Indexing;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// <see cref="Session"/>-level handler for index storage.
  /// </summary>
  public class SessionHandler : Providers.SessionHandler
  {
    private IndexStorage storage;
    
    /// <summary>
    /// Gets the storage view.
    /// </summary>
    public IStorageView StorageView { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Transaction is already open.</exception>
    public override void BeginTransaction()
    {
      lock (ConnectionSyncRoot) {
        if (StorageView!=null)
          throw new InvalidOperationException(Strings.ExTransactionIsAlreadyOpened);
        StorageView = storage.CreateView(Session.Transaction.IsolationLevel);
        // TODO: Implement transactions
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Transaction is not open.</exception>
    public override void CommitTransaction()
    {
      base.CommitTransaction();
      lock (ConnectionSyncRoot) {
        if (StorageView==null)
          throw new InvalidOperationException(Strings.ExTransactionIsNotOpened);
        StorageView.Transaction.Commit();
        StorageView = null;
        // TODO: Implement transactions
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Transaction is not open.</exception>
    public override void RollbackTransaction()
    {
      base.RollbackTransaction();
      lock (ConnectionSyncRoot) {
        if (StorageView==null)
          throw new InvalidOperationException(Strings.ExTransactionIsNotOpened);
        StorageView.Transaction.Rollback();
        StorageView = null;
        // TODO: Implement transactions
      }
    }

    /// <inheritdoc/>
    public override void Persist(IEnumerable<PersistAction> persistActions, bool dirty)
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

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      storage = ((DomainHandler) Handlers.DomainHandler).GetIndexStorage();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
    }
  }
}
