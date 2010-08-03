// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.29

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Storage
{
  public partial class Session
  {
    /// <summary>
    /// The manager of <see cref="Entity"/>'s events.
    /// </summary>
    public EntityEventBroker EntityEventBroker { get; private set; }

    /// <summary>
    /// Raises events on all <see cref="INotifyPropertyChanged"/> and
    /// <see cref="INotifyCollectionChanged"/> subscribers stating that
    /// all entities and collections are changed.
    /// </summary>
    /// <param name="options">The options.</param>
    public void NotifyChanged()
    {
      NotifyChanged(
        NotifyChangedOptions.SkipRemovedEntities | 
        NotifyChangedOptions.Prefetch);
    }

    /// <summary>
    /// Raises events on all <see cref="INotifyPropertyChanged"/> and
    /// <see cref="INotifyCollectionChanged"/> subscribers stating that
    /// all entities and collections are changed.
    /// </summary>
    /// <param name="options">The options.</param>
    public void NotifyChanged(NotifyChangedOptions options)
    {
      using (Activate()) 
      using (var transactionScope = Transaction.Open(this)) {
        var entitySubscribers    = EntityEventBroker.GetSubscribers(EntityEventBroker.PropertyChangedEventKey).ToList();
        var entitySetSubscribers = EntityEventBroker.GetSubscribers(EntityEventBroker.CollectionChangedEventKey).ToList();

        if ((options & NotifyChangedOptions.Prefetch)==NotifyChangedOptions.Prefetch) {
          var keys =
            from triplet in entitySubscribers
            select triplet.First;
          keys.Prefetch().Run();
        }

        var skipRemovedEntities = 
          (options & NotifyChangedOptions.SkipRemovedEntities)==NotifyChangedOptions.SkipRemovedEntities;
        foreach (var triplet in entitySubscribers) {
          if (triplet.Third!=null) {
            var handler = (PropertyChangedEventHandler) triplet.Third;
            var key = triplet.First;
            var entityState = EntityStateCache[key, false];
            var sender = entityState!=null ? entityState.Entity : Query.SingleOrDefault(key);
            if (skipRemovedEntities && sender.IsRemoved())
              continue;
            handler.Invoke(sender, new PropertyChangedEventArgs(null));
          }
        }

        foreach (var triplet in entitySetSubscribers) {
          if (triplet.Third!=null) {
            var handler = (NotifyCollectionChangedEventHandler) triplet.Third;
            var key = triplet.First;
            var entityState = EntityStateCache[key, false];
            var owner = entityState!=null ? entityState.Entity : Query.SingleOrDefault(key);
            var ownerIsRemoved = owner.IsRemoved();
            if (skipRemovedEntities && ownerIsRemoved)
              continue;
            var sender = ownerIsRemoved ? null : owner.GetFieldValue(triplet.Second);
            handler.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
          }
        }
      }
    }

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to be disposed.
    /// </summary>
    public event EventHandler Disposing;

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to <see cref="Persist"/> changes.
    /// </summary>
    public event EventHandler Persisting;

    /// <summary>
    /// Occurs when <see cref="Session"/> persisted.
    /// </summary>
    public event EventHandler Persisted;

    /// <summary>
    /// Occurs when local <see cref="Key"/> created.
    /// </summary>
    public event EventHandler<KeyEventArgs> KeyGenerated;

    /// <summary>
    /// Occurs when <see cref="Entity"/> is materialized.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityMaterialized;

    /// <summary>
    /// Occurs when <see cref="Entity"/> is created.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityCreated;

    /// <summary>
    /// Occurs when <see cref="Entity"/> is about to change.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityChanging;

    /// <summary>
    /// Occurs when <see cref="Entity"/>.<see cref="Entity.VersionInfo"/> is about to change.
    /// </summary>
    public event EventHandler<EntityVersionInfoChangedEventArgs> EntityVersionInfoChanging;

    /// <summary>
    /// Occurs when <see cref="Entity"/>.<see cref="Entity.VersionInfo"/> is changed.
    /// </summary>
    public event EventHandler<EntityVersionInfoChangedEventArgs> EntityVersionInfoChanged;

    /// <summary>
    /// Occurs when field value is about to be read.
    /// </summary>
    public event EventHandler<EntityFieldEventArgs> EntityFieldValueGetting;

    /// <summary>
    /// Occurs when field value was read successfully.
    /// </summary>
    public event EventHandler<EntityFieldValueEventArgs> EntityFieldValueGet;

    /// <summary>
    /// Occurs when field value reading is completed.
    /// </summary>
    public event EventHandler<EntityFieldValueGetCompletedEventArgs> EntityFieldValueGetCompleted;

    /// <summary>
    /// Occurs when is field value is about to be set.
    /// This event is raised on any set attempt (even if new value is the same as the current one).
    /// </summary>
    public event EventHandler<EntityFieldValueEventArgs> EntityFieldValueSettingAttempt;

    /// <summary>
    /// Occurs when is field value is about to be changed.
    /// This event is raised only on actual change attempt (i.e. when new value differs from the current one).
    /// </summary>
    public event EventHandler<EntityFieldValueEventArgs> EntityFieldValueSetting;

    /// <summary>
    /// Occurs when field value was changed successfully.
    /// </summary>
    public event EventHandler<EntityFieldValueSetEventArgs> EntityFieldValueSet;

    /// <summary>
    /// Occurs when field value changing is completed.
    /// </summary>
    public event EventHandler<EntityFieldValueSetCompletedEventArgs> EntityFieldValueSetCompleted;

    /// <summary>
    /// Occurs when <see cref="Entity"/> is about to remove.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityRemoving;

    /// <summary>
    /// Occurs when <see cref="Entity"/> removed.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityRemove;

    /// <summary>
    /// Occurs when <see cref="Entity"/> removing is completed.
    /// </summary>
    public event EventHandler<EntityRemoveCompletedEventArgs> EntityRemoveCompleted;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> is about to change.
    /// </summary>
    public event EventHandler<EntitySetEventArgs> EntitySetChanging;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item is about to remove.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemRemoving;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removed.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemRemove;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removing is completed.
    /// </summary>
    public event EventHandler<EntitySetItemActionCompletedEventArgs> EntitySetItemRemoveCompleted;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item is about to remove.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemAdding;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removed.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemAdd;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removing is completed.
    /// </summary>
    public event EventHandler<EntitySetItemActionCompletedEventArgs> EntitySetItemAddCompleted;

    /// <summary>
    /// Occurs when outermost <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationEventArgs> OutermostOperationCompleted;

    /// <summary>
    /// Occurs when nested <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationEventArgs> NestedOperationCompleted;

    private void NotifyPersisting()
    {
      if (Persisting!=null && !IsSystemLogicOnly)
        Persisting(this, EventArgs.Empty);
    }

    private void NotifyPersisted()
    {
      if (Persisted!=null && !IsSystemLogicOnly)
        Persisted(this, EventArgs.Empty);
    }

    private void NotifyDisposing()
    {
      if (Disposing!=null)
        Disposing(this, EventArgs.Empty);
    }

    internal void NotifyKeyGenerated(Key key)
    {
      if (KeyGenerated!=null && !IsSystemLogicOnly)
        KeyGenerated(this, new KeyEventArgs(key));
    }

    internal void NotifyEntityMaterialized(EntityState entityState)
    {
      if (EntityMaterialized!=null && !IsSystemLogicOnly)
        EntityMaterialized(this, new EntityEventArgs(entityState.Entity));
    }

    internal void NotifyEntityCreated(Entity entity)
    {
      if (EntityCreated!=null && !IsSystemLogicOnly)
        EntityCreated(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityChanging(Entity entity)
    {
      if (EntityChanging!=null && !IsSystemLogicOnly)
        EntityChanging(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityVersionInfoChanging(Entity changedEntity, FieldInfo changedField, bool changed)
    {
      if (EntityVersionInfoChanging!=null && !IsSystemLogicOnly)
        EntityVersionInfoChanging(this, new EntityVersionInfoChangedEventArgs(
          changedEntity, changedField, changed));
    }

    internal void NotifyEntityVersionInfoChanged(Entity changedEntity, FieldInfo changedField, bool changed)
    {
      if (EntityVersionInfoChanged!=null && !IsSystemLogicOnly)
        EntityVersionInfoChanged(this, new EntityVersionInfoChangedEventArgs(
          changedEntity, changedField, changed));
    }

    internal void NotifyFieldValueGetting(Entity entity, FieldInfo field)
    {
      if (EntityFieldValueGetting!=null && !IsSystemLogicOnly)
        EntityFieldValueGetting(this, new EntityFieldEventArgs(entity, field));
    }

    internal void NotifyFieldValueGet(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueGet!=null && !IsSystemLogicOnly)
        EntityFieldValueGet(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueGetCompleted(Entity entity, FieldInfo field, object value, Exception exception)
    {
      if (EntityFieldValueGetCompleted!=null && !IsSystemLogicOnly)
        EntityFieldValueGetCompleted(this, new EntityFieldValueGetCompletedEventArgs(entity, field, value, exception));
    }

    internal void NotifyFieldValueSettingAttempt(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueSettingAttempt!=null && !IsSystemLogicOnly)
        EntityFieldValueSettingAttempt(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueSetting(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueSetting!=null && !IsSystemLogicOnly)
        EntityFieldValueSetting(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueSet(Entity entity, FieldInfo field, object oldValue, object newValue)
    {
      if (EntityFieldValueSet!=null && !IsSystemLogicOnly)
        EntityFieldValueSet(this, new EntityFieldValueSetEventArgs(entity, field, oldValue, newValue));
    }

    internal void NotifyFieldValueSetCompleted(Entity entity, FieldInfo field, object oldValue, object newValue, Exception exception)
    {
      if (EntityFieldValueSetCompleted!=null && !IsSystemLogicOnly)
        EntityFieldValueSetCompleted(this, new EntityFieldValueSetCompletedEventArgs(entity, field, oldValue, newValue, exception));
    }

    internal void NotifyEntityRemoving(Entity entity)
    {
      if (EntityRemoving!=null && !IsSystemLogicOnly)
        EntityRemoving(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemove(Entity entity)
    {
      if (EntityRemove!=null && !IsSystemLogicOnly)
        EntityRemove(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemoveCompleted(Entity entity, Exception exception)
    {
      if (EntityRemoveCompleted != null)
        EntityRemoveCompleted(this, new EntityRemoveCompletedEventArgs(entity, exception));
    }

    internal void NotifyEntitySetItemRemoving(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemRemoving!=null && !IsSystemLogicOnly)
        EntitySetItemRemoving(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemRemoved(Entity entity, EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemRemove!=null && !IsSystemLogicOnly)
        EntitySetItemRemove(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemRemoveCompleted(EntitySetBase entitySet, Entity item, Exception exception)
    {
      if (EntitySetItemRemoveCompleted!=null && !IsSystemLogicOnly)
        EntitySetItemRemoveCompleted(this, new EntitySetItemActionCompletedEventArgs(entitySet, item, exception));
    }

    internal void NotifyEntitySetItemAdding(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemAdding!=null && !IsSystemLogicOnly)
        EntitySetItemAdding(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemAdd(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemAdd!=null && !IsSystemLogicOnly)
        EntitySetItemAdd(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemAddCompleted(EntitySetBase entitySet, Entity item, Exception exception)
    {
      if (EntitySetItemAddCompleted!=null && !IsSystemLogicOnly)
        EntitySetItemAddCompleted(this, new EntitySetItemActionCompletedEventArgs(entitySet, item, exception));
    }

    internal void NotifyOutermostOperationCompleted(IOperation operation)
    {
      if (IsOutermostOperationLoggingEnabled)
        OutermostOperationCompleted(this, new OperationEventArgs(operation));
    }

    internal void NotifyNestedOperationCompleted(IOperation operation)
    {
      if (IsNestedOperationLoggingEnabled)
        NestedOperationCompleted(this, new OperationEventArgs(operation));
    }
  }
}