// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Transactions;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm
{
  public partial class Session
  {
    private bool disableAutoSaveChanges;
    private KeyRemapper remapper;
    
    internal ReferenceFieldsChangesRegistry ReferenceFieldsChangesRegistry { get; private set; }
    /// <summary>
    /// Saves all modified instances immediately to the database.
    /// Obsolete, use <see cref="SaveChanges"/> method instead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage.
    /// </para>
    /// <para>
    /// For non-disconnected (without <see cref="SessionOptions.Disconnected"/> option) session this method is called automatically when it's necessary,
    /// e.g. before beginning, committing and rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </para>
    /// <para>
    /// For disconnected session (with <see cref="SessionOptions.Disconnected"/> option) you should call this method manually.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    [Obsolete("Use Session.SaveChanges() method instead")]
    public void Persist()
    {
      SaveChanges();
    }

    /// <summary>
    /// Saves all modified instances immediately to the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage.
    /// </para>
    /// <para>
    /// For non-disconnected (without <see cref="SessionOptions.Disconnected"/> option) session this method is called automatically when it's necessary,
    /// e.g. before beginning, committing and rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </para>
    /// <para>
    /// For disconnected session (with <see cref="SessionOptions.Disconnected"/> option) you should call this method manually.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public void SaveChanges()
    {
      if (Configuration.Supports(SessionOptions.Disconnected))
        SaveLocalChanges();
      else
        Persist(PersistReason.Manual);
    }

    /// <summary>
    /// Cancels all changes and resets modified entities to their original state.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    /// <exception cref="NotSupportedException">Unable to cancel changes for non-disconnected session. Use transaction boundaries to control the state.</exception>
    public void CancelChanges()
    {
      if (Configuration.Supports(SessionOptions.Disconnected))
        CancelLocalChanges();
      else
        throw new NotSupportedException("Unable to cancel pending changes when session is not disconnected.");
    }

    private void SaveLocalChanges()
    {
      Validate();
      EndDisconnectedTransaction(true);
      try {
        DisconnectedState.ApplyChanges();
      }
      finally {
        BeginDisconnectedTransaction();
      }
    }

    private void CancelLocalChanges()
    {
      EndDisconnectedTransaction(false);
      try {
        DisconnectedState.CancelChanges();
      }
      finally {
        BeginDisconnectedTransaction();
      }
    }

    internal void Persist(PersistReason reason)
    {
      EnsureNotDisposed();
      if (IsPersisting || EntityChangeRegistry.Count==0)
        return;

      var performPinning = pinner.RootCount > 0;
      if (performPinning || disableAutoSaveChanges) 
        switch (reason) {
          case PersistReason.NestedTransaction:
          case PersistReason.Commit:
            throw new InvalidOperationException(Strings.ExCanNotPersistThereArePinnedEntities);
          }

      if (disableAutoSaveChanges && reason != PersistReason.Manual)
        return;

      using (var ts = OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified, false)) {
        IsPersisting = true;
        SystemEvents.NotifyPersisting();
        Events.NotifyPersisting();

        try {
          using (this.OpenSystemLogicOnlyRegion()) {
            DemandTransaction();
            OrmLog.Debug(Strings.LogSessionXPersistingReasonY, this, reason);

            EntityChangeRegistry itemsToPersist;
            if (performPinning) {
              pinner.Process(EntityChangeRegistry);
              itemsToPersist = pinner.PersistableItems;
            }
            else
              itemsToPersist = EntityChangeRegistry;

            if (LazyKeyGenerationIsEnabled) {
              RemapEntityKeys(remapper.Remap(itemsToPersist));
            }

            ApplyEntitySetsChanges();
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
                EntityChangeRegistry = pinner.PinnedItems;
                pinner.Reset();
              }
              else
                EntityChangeRegistry.Clear();

              OrmLog.Debug(Strings.LogSessionXPersistCompleted, this);
            }
          }
          SystemEvents.NotifyPersisted();
          Events.NotifyPersisted();
          if(Configuration.Supports(SessionOptions.NonTransactionalEntityStates))
            ts.Complete();
        }
        finally {
          IsPersisting = false;
        }
      }
    }

    /// <summary>
    /// Temporarily disables all save changes operations (both explicit ant automatic) 
    /// for specified <paramref name="target"/>.
    /// Such entity is prevented from being persisted to the database,
    /// when <see cref="SaveChanges"/> is called or query is executed.
    /// If persist is to be performed due to starting a nested transaction or committing a transaction,
    /// the presence of such an entity will lead to failure.
    /// If <paramref name="target"/> is not present in the database,
    /// all entities that reference <paramref name="target"/> are also pinned automatically.
    /// </summary>
    /// <param name="target">The entity to disable persisting.</param>
    /// <returns>A special object that controls lifetime of such behavior if <paramref name="target"/> was not previously processed by the method,
    /// otherwise <see langword="null"/>.</returns>
    public IDisposable DisableSaveChanges(IEntity target)
    {
      EnsureNotDisposed();
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var targetEntity = (Entity) target;
      targetEntity.EnsureNotRemoved();
      if (IsDisconnected)
        return new Disposable(b => {return;}); // No need to pin in this case
      return pinner.RegisterRoot(targetEntity.State);
    }

    /// <summary>
    /// Temporarily disables only automatic save changes operations before queries, etc.
    /// Explicit call of <see cref="SaveChanges"/> will lead to flush changes anyway.
    /// If save changes is to be performed due to starting a nested transaction or committing a transaction,
    /// active disabling save changes scope will lead to failure.
    /// <returns>A special object that controls lifetime of such behavior if there is no active scope,
    /// otherwise <see langword="null"/>.</returns>
    /// </summary>
    public IDisposable DisableSaveChanges()
    {
      if (IsDisconnected)
        return new Disposable(b => { return; }); // No need to pin in this case
      if (disableAutoSaveChanges)
        return null;

      disableAutoSaveChanges = true;
      return new Disposable(_ => {
        disableAutoSaveChanges = false;
        InvalidateEntitySetsWithInvalidState();
      });
    }

    private void ApplyEntitySetsChanges()
    {
      ProcessChangesOfEntitySets(entitySetState => entitySetState.ApplyChanges());
    }

    private void CancelEntitySetsChanges()
    {
      ProcessChangesOfEntitySets(entitySetState => entitySetState.CancelChanges());
    }

    private void ProcessChangesOfEntitySets(Action<EntitySetState> action)
    {
      var itemsToProcess = EntitySetChangeRegistry.GetItems();
      foreach (var entitySet in itemsToProcess)
        action.Invoke(entitySet);
      EntitySetChangeRegistry.Clear();
    }
  }
}