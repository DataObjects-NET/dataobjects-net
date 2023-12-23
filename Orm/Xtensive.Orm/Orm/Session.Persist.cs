// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Transactions;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm
{
  public partial class Session
  {
    private bool disableAutoSaveChanges;
    private KeyRemapper remapper;
    private bool persistingIsFailed;

    internal bool DisableAutoSaveChanges { get { return disableAutoSaveChanges; } }
    internal NonPairedReferenceChangesRegistry NonPairedReferencesRegistry { get; private set; }
    internal ReferenceFieldsChangesRegistry ReferenceFieldsChangesRegistry { get; private set; }

    /// <summary>
    /// Saves all modified instances immediately to the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage.
    /// </para>
    /// <para>
    /// For session with auto saving (with <see cref="SessionOptions.AutoSaveChanges"/> this method is called automatically when it's necessary,
    /// e.g. before beginning, committing and rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </para>
    /// <para>
    /// For session without auto saving (without <see cref="SessionOptions.AutoSaveChanges"/> option) you should call this method manually.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public void SaveChanges()
    {
      if (Configuration.Supports(SessionOptions.NonTransactionalEntityStates))
        SaveLocalChanges();
      else
        Persist(PersistReason.Manual);
    }

    /// <summary>
    /// Asynchronously saves all modified instances immediately to the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.
    /// </para>
    /// <para>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage.
    /// </para>
    /// <para>
    /// For session with auto saving (with <see cref="SessionOptions.AutoSaveChanges"/> this method is called automatically when it's necessary,
    /// e.g. before beginning, committing and rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </para>
    /// <para>
    /// For session without auto saving (without <see cref="SessionOptions.AutoSaveChanges"/> option) you should call this method manually.
    /// </para>
    /// </remarks>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
      if (Configuration.Supports(SessionOptions.NonTransactionalEntityStates)) {
        await SaveLocalChangesAsync(token).ConfigureAwait(false);
      }
      else {
        await PersistAsync(PersistReason.Manual, token).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Cancels all changes and resets modified entities to their original state.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    /// <exception cref="NotSupportedException">Unable to cancel changes for non-disconnected session. Use transaction boundaries to control the state.</exception>
    public void CancelChanges()
    {
      SystemEvents.NotifyChangesCanceling();
      Events.NotifyChangesCanceling();

      CancelEntitySetsChanges();
      CancelEntitiesChanges();
      NonPairedReferencesRegistry.Clear();

      SystemEvents.NotifyChangesCanceled();
      Events.NotifyChangesCanceled();
    }

    internal void Persist(PersistReason reason) => Persist(reason, false).GetAwaiter().GetResult();

    internal async Task PersistAsync(PersistReason reason, CancellationToken token = default) =>
      await Persist(reason, true, token).ConfigureAwait(false);

    private async ValueTask Persist(PersistReason reason, bool isAsync, CancellationToken token = default)
    {
      EnsureNotDisposed();
      if (IsPersisting || EntityChangeRegistry.Count == 0) {
        return;
      }

      var performPinning = pinner.RootCount > 0;
      if (performPinning
        || (disableAutoSaveChanges && !Configuration.Supports(SessionOptions.NonTransactionalEntityStates))) {
        switch (reason) {
          case PersistReason.NestedTransaction:
          case PersistReason.Commit:
            throw new InvalidOperationException(Strings.ExCanNotPersistThereArePinnedEntities);
        }
      }

      if (disableAutoSaveChanges && reason != PersistReason.Manual) {
        return;
      }

      var ts = await InnerOpenTransaction(
        TransactionOpenMode.Default, IsolationLevel.Unspecified, false, isAsync, token);
      try {
        IsPersisting = true;
        persistingIsFailed = false;
        SystemEvents.NotifyPersisting();
        Events.NotifyPersisting();
        using (OpenSystemLogicOnlyRegion()) {
          DemandTransaction();
          if (IsDebugEventLoggingEnabled) {
            OrmLog.Debug(nameof(Strings.LogSessionXPersistingReasonY), this, reason);
          }

          EntityChangeRegistry itemsToPersist;
          if (performPinning) {
            pinner.Process(EntityChangeRegistry);
            itemsToPersist = pinner.PersistableItems;
          }
          else {
            itemsToPersist = EntityChangeRegistry;
          }

          if (LazyKeyGenerationIsEnabled) {
            await RemapEntityKeys(remapper.Remap(itemsToPersist), isAsync, token).ConfigureAwait(false);
          }

          ApplyEntitySetsChanges();
          var persistIsSuccessful = false;
          try {
            if (isAsync) {
              await Handler.PersistAsync(itemsToPersist, reason == PersistReason.Query, token).ConfigureAwait(false);
            }
            else {
              Handler.Persist(itemsToPersist, reason == PersistReason.Query);
            }

            persistIsSuccessful = true;
          }
          catch (Exception) {
            persistingIsFailed = true;
            RollbackChangesOfEntitySets();
            RestoreEntityChangesAfterPersistFailed();
            throw;
          }
          finally {
            if (persistIsSuccessful || !Configuration.Supports(SessionOptions.NonTransactionalEntityStates)) {
              DropDifferenceBackup();
              foreach (var item in itemsToPersist.GetItems(PersistenceState.New)) {
                item.PersistenceState = PersistenceState.Synchronized;
              }

              foreach (var item in itemsToPersist.GetItems(PersistenceState.Modified)) {
                item.PersistenceState = PersistenceState.Synchronized;
              }

              foreach (var item in itemsToPersist.GetItems(PersistenceState.Removed)) {
                item.Update(null);
              }

              if (performPinning) {
                EntityChangeRegistry = pinner.PinnedItems;
                pinner.Reset();
              }
              else {
                EntityChangeRegistry.Clear();
              }

              EntitySetChangeRegistry.Clear();
              NonPairedReferencesRegistry.Clear();
            }

            if (IsDebugEventLoggingEnabled) {
              OrmLog.Debug(nameof(Strings.LogSessionXPersistCompleted), this);
            }
          }
        }

        SystemEvents.NotifyPersisted();
        Events.NotifyPersisted();
      }
      finally {
        IsPersisting = false;
        if (isAsync) {
          await ts.DisposeAsync().ConfigureAwait(false);
        }
        else {
          ts.Dispose();
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
    /// <returns>
    /// A special object that controls lifetime of such behavior if <paramref name="target"/> was not previously processed by the method
    /// and automatic saving of changes is enabled (<see cref="SessionOptions.AutoSaveChanges"/>),
    /// otherwise <see langword="null"/>.
    /// </returns>
    public IDisposable DisableSaveChanges(IEntity target)
    {
      EnsureNotDisposed();
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      if (!Configuration.Supports(SessionOptions.AutoSaveChanges))
        return null; // No need to pin in this case

      var targetEntity = (Entity) target;
      targetEntity.EnsureNotRemoved();
      return pinner.RegisterRoot(targetEntity.State);
    }

    /// <summary>
    /// Temporarily disables only automatic save changes operations before queries, etc.
    /// Explicit call of <see cref="SaveChanges"/> will lead to flush changes anyway.
    /// If save changes is to be performed due to starting a nested transaction or committing a transaction,
    /// active disabling save changes scope will lead to failure.
    /// <returns>A special object that controls lifetime of such behavior if there is no active scope
    /// and automatic saving of changes is enabled (<see cref="SessionOptions.AutoSaveChanges"/>),
    /// otherwise <see langword="null"/>.</returns>
    /// </summary>
    public IDisposable DisableSaveChanges()
    {
      if (!Configuration.Supports(SessionOptions.AutoSaveChanges) || disableAutoSaveChanges) {
        return null; // No need to pin in these cases
      }

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

    private void RollbackChangesOfEntitySets()
    {
      ProcessChangesOfEntitySets(entitySetState => entitySetState.RollbackState());
    }

    private void SaveLocalChanges()
    {
      Validate();
      using (var transaction = OpenTransaction(TransactionOpenMode.New)) {
        try {
          Persist(PersistReason.Manual);
        }
        finally {
          transaction.Complete();
        }
      }
    }

    private async Task SaveLocalChangesAsync(CancellationToken token = default)
    {
      Validate();
      var transaction = OpenTransaction(TransactionOpenMode.New);
      await using (transaction.ConfigureAwait(false)) {
        try {
          await PersistAsync(PersistReason.Manual, token).ConfigureAwait(false);
        }
        finally {
          transaction.Complete();
        }
      }
    }

    private void CancelEntitiesChanges()
    {
      foreach (var newEntity in EntityChangeRegistry.GetItems(PersistenceState.New).ToList()) {
        newEntity.Update(null);
        newEntity.PersistenceState = PersistenceState.Removed;
      }

      foreach (var modifiedEntity in EntityChangeRegistry.GetItems(PersistenceState.Modified)) {
        modifiedEntity.RollbackDifference();
        modifiedEntity.PersistenceState = PersistenceState.Synchronized;
      }

      foreach (var removedEntity in EntityChangeRegistry.GetItems(PersistenceState.Removed)) {
        removedEntity.RollbackDifference();
        removedEntity.PersistenceState = PersistenceState.Synchronized;
      }
      EntityChangeRegistry.Clear();
      NonPairedReferencesRegistry.Clear();
    }

    private void RestoreEntityChangesAfterPersistFailed()
    {
      foreach (var entityState in EntityChangeRegistry.GetItems(PersistenceState.New))
        entityState.RestoreDifference();

      foreach (var entityState in EntityChangeRegistry.GetItems(PersistenceState.Modified))
        entityState.RestoreDifference();

      foreach (var entityState in EntityChangeRegistry.GetItems(PersistenceState.Removed))
        entityState.RestoreDifference();
    }

    private void DropDifferenceBackup()
    {
      foreach (var entityState in EntityChangeRegistry.GetItems(PersistenceState.New))
        entityState.DropBackedUpDifference();

      foreach (var entityState in EntityChangeRegistry.GetItems(PersistenceState.Modified))
        entityState.DropBackedUpDifference();

      foreach (var entityState in EntityChangeRegistry.GetItems(PersistenceState.Removed))
        entityState.DropBackedUpDifference();
    }

    private void ProcessChangesOfEntitySets(Action<EntitySetState> action)
    {
      var itemsToProcess = EntitySetChangeRegistry.GetItems();
      foreach (var entitySet in itemsToProcess)
        action.Invoke(entitySet);
    }
  }
}
