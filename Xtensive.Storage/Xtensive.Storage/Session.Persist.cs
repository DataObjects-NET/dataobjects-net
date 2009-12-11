// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;

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

    private void Persist(PersistReason reason)
    {
      EnsureNotDisposed();

      if (IsPersisting || EntityChangeRegistry.Count==0)
        return;

      bool performPinning = pinner.RootCount > 0;

      if (performPinning)
        switch (reason) {
        case PersistReason.NestedTransaction:
        case PersistReason.Commit:
          throw new InvalidOperationException(Strings.ExCanNotPersistThereArePinnedEntities);
        }

      IsPersisting = true;
      NotifyPersisting();

      try {
        using (CoreServices.OpenSystemLogicOnlyRegion()) {
          EnsureTransactionIsStarted();
          if (IsDebugEventLoggingEnabled)
            Log.Debug(Strings.LogSessionXPersistingReasonY, this, reason);

          EntityChangeRegistry itemsToPersist;
          if (performPinning) {
            pinner.Process(EntityChangeRegistry);
            itemsToPersist = pinner.PersistableItems;
          }
          else
            itemsToPersist = EntityChangeRegistry;

          try {
            Handler.Persist(itemsToPersist, reason==PersistReason.Query);
          }
          finally {
            foreach (var item in itemsToPersist.GetItems(PersistenceState.New))
              item.PersistenceState = PersistenceState.Synchronized;
            foreach (var item in itemsToPersist.GetItems(PersistenceState.Modified))
              item.PersistenceState = PersistenceState.Synchronized;
            foreach (var item in itemsToPersist.GetItems(PersistenceState.Removed))
              item.Update(null);

            if (performPinning) {
              EntityChangeRegistry = pinner.PinnedItems;
              pinner.Reset();
            }
            else
              EntityChangeRegistry.Clear();

            if (IsDebugEventLoggingEnabled)
              Log.Debug(Strings.LogSessionXPersistCompleted, this);
          }
        }
        NotifyPersisted();
      }
      finally {
        IsPersisting = false;
      }
    }
  }
}