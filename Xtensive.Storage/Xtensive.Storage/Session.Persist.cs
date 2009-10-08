// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;

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
      Persist(false);
    }

    internal void Persist(bool dirty)
    {
      if (IsPersisting)
        return;
      IsPersisting = true;
      NotifyPersisting();

      try {
        using (CoreServices.OpenSystemLogicOnlyRegion()) {
          EnsureNotDisposed();

          if (EntityChangeRegistry.Count==0)
            return;

          if (IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Persisting...", this);

          try {
            Handler.Persist(EntityChangeRegistry, dirty);
          }
          finally {
            foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.New))
              item.PersistenceState = PersistenceState.Synchronized;
            foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Modified))
              item.PersistenceState = PersistenceState.Synchronized;
            foreach (var item in EntityChangeRegistry.GetItems(PersistenceState.Removed))
              item.Update(null);
            EntityChangeRegistry.Clear();

            if (IsDebugEventLoggingEnabled)
              Log.Debug("Session '{0}'. Persisted.", this);
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