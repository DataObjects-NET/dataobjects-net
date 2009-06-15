// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
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
      if (StorageView!=null)
        throw new InvalidOperationException(Strings.ExTransactionIsAlreadyOpened);
      StorageView = storage.CreateView(Session.Transaction.IsolationLevel);
      // TODO: Implement transactions
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Transaction is not open.</exception>
    public override void CommitTransaction()
    {
      if (StorageView==null)
        throw new InvalidOperationException(Strings.ExTransactionIsNotOpened);
      StorageView.Transaction.Commit();
      StorageView = null;
      // TODO: Implement transactions
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Transaction is not open.</exception>
    public override void RollbackTransaction()
    {
      if (StorageView==null)
        throw new InvalidOperationException(Strings.ExTransactionIsNotOpened);
      StorageView.Transaction.Rollback();
      StorageView = null;
      // TODO: Implement transactions
    }

    /// <inheritdoc/>
    protected override void Insert(EntityState state)
    {
      var batch = new List<Command>();
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var transform = ((DomainHandler) Handlers.DomainHandler).GetTransform(index, state.Type);
        var transformed = transform.Apply(TupleTransformType.Tuple, state.Tuple).ToFastReadOnly();
        batch.Add(IndexUpdateCommand.Insert(index.MappingName, state.Key.Value, transformed));
      }

      StorageView.Execute(batch);
    }

    /// <inheritdoc/>
    protected override void Update(EntityState state)
    {
      var batch = new List<Command>();
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary)) {
        var transform = ((DomainHandler) Handlers.DomainHandler).GetTransform(index, state.Type);
        var transformed = transform.Apply(TupleTransformType.Tuple, state.Tuple).ToFastReadOnly();
        batch.Add(IndexUpdateCommand.Update(index.MappingName, state.Key.Value, transformed));
      }

      StorageView.Execute(batch);
    }

    /// <inheritdoc/>
    protected override void Remove(EntityState state)
    {
      var batch = new List<Command>();
      foreach (var index in state.Type.AffectedIndexes.Where(i => i.IsPrimary))
        batch.Add(IndexUpdateCommand.Remove(index.MappingName, state.Key.Value));

      StorageView.Execute(batch);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      storage = ((DomainHandler) Handlers.DomainHandler).GetIndexStorage();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
    }
  }
}
