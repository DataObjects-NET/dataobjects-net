// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Services;
using System.Linq;

namespace Xtensive.Storage
{
  public partial class Session
  {
    /// <summary>
    /// Persists all modified instances immediately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. 
    /// </para>
    /// <para>
    /// Note, that this method is called automatically when it's necessary,
    /// e.g. before beginning, committing and rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public void Persist()
    {
      Persist(PersistReason.Manual);
    }

    internal void Persist(PersistReason reason)
    {
      EnsureNotDisposed();
      if (IsPersisting || EntityChangeRegistry.Count==0)
        return;

      using(var ts = OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified)){
        var performPinning = Pinner.RootCount > 0;
        if (performPinning)
          switch (reason) {
          case PersistReason.NestedTransaction:
          case PersistReason.Commit:
            throw new InvalidOperationException(Strings.ExCanNotPersistThereArePinnedEntities);
          }

        IsPersisting = true;
      
        SystemEvents.NotifyPersisting();
        Events.NotifyPersisting();
        try {
          using (this.OpenSystemLogicOnlyRegion()) {
            EnsureTransactionIsStarted();
            if (IsDebugEventLoggingEnabled)
              Log.Debug(Strings.LogSessionXPersistingReasonY, this, reason);

            EntityChangeRegistry itemsToPersist;
            if (performPinning) {
              Pinner.Process(EntityChangeRegistry);
              itemsToPersist = Pinner.PersistableItems;
            }
            else
              itemsToPersist = EntityChangeRegistry;

            try {
              Handler.Persist(itemsToPersist, reason == PersistReason.Query);
            }
            finally {
              foreach (var item in itemsToPersist.GetItems(PersistenceState.New))
                item.PersistenceState = PersistenceState.Synchronized;
              foreach (var item in itemsToPersist.GetItems(PersistenceState.Modified))
                item.PersistenceState = PersistenceState.Synchronized;
              foreach (var item in itemsToPersist.GetItems(PersistenceState.Removed))
                item.Update(null);

              if (performPinning) {
                EntityChangeRegistry = Pinner.PinnedItems;
                Pinner.Reset();
              }
              else
                EntityChangeRegistry.Clear();

              if (IsDebugEventLoggingEnabled)
                Log.Debug(Strings.LogSessionXPersistCompleted, this);
            }
          }
          SystemEvents.NotifyPersisted();
          Events.NotifyPersisted();
        }
        finally {
          IsPersisting = false;
        }
      }
    }

    /// <summary>
    /// Pins the specified <see cref="IEntity"/>.
    /// Pinned entity is prevented from being persisted,
    /// when <see cref="Persist"/> is called or query is executed.
    /// If persist is to be performed due to starting a nested transaction or committing a transaction,
    /// any pinned entity will lead to failure.
    /// If <paramref name="target"/> is not present in the database,
    /// all entities that reference <paramref name="target"/> are also pinned automatically.
    /// </summary>
    /// <param name="target">The entity to pin.</param>
    /// <returns>An entity pinning scope if <paramref name="target"/> was not previously pinned,
    /// otherwise <see langword="null"/>.</returns>
    public IDisposable Pin(IEntity target)
    {
      EnsureNotDisposed();
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var targetEntity = (Entity) target;
      targetEntity.EnsureNotRemoved();
      if (IsDisconnected)
        return new Disposable(b => {return;}); // No need to pin in this case
      else
        return Pinner.RegisterRoot(targetEntity.State);
    }
  }
}