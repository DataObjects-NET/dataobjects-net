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
    public void NotifyChanged()
    {
      var subscribers = EntityEventBroker.GetSubscribers(EntityEventBroker.PropertyChangedEventKey);
      foreach (var triplet in subscribers) {
        if (triplet.Third != null)
          ((PropertyChangedEventHandler)triplet.Third)
            .Invoke(this, new PropertyChangedEventArgs(null));
      }
      subscribers = EntityEventBroker.GetSubscribers(EntityEventBroker.CollectionChangedEventKey);
      foreach (var triplet in subscribers) {
        if (triplet.Third != null)
          ((NotifyCollectionChangedEventHandler) triplet.Third)
            .Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemRemoved;

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
    /// Occurs when the <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationEventArgs> OperationCompleted;

    /// <summary>
    /// <see cref="OperationCompleted"/> event has subscribers.
    /// </summary>
    internal bool OperationCompletedHasSubscribers {
      get { return OperationCompleted!=null; }
    }

    private void NotifyPersisting()
    {
      if (!IsSystemLogicOnly && Persisting!=null)
        Persisting(this, EventArgs.Empty);
    }

    private void NotifyPersisted()
    {
      if (!IsSystemLogicOnly && Persisted!=null)
        Persisted(this, EventArgs.Empty);
    }

    private void NotifyDisposing()
    {
      if (Disposing!=null)
        Disposing(this, EventArgs.Empty);
    }

    internal void NotifyKeyGenerated(Key key)
    {
      if (KeyGenerated != null)
        KeyGenerated(this, new KeyEventArgs(key));
    }

    internal void NotifyEntityMaterialized(EntityState entityState)
    {
      if (EntityMaterialized!=null)
        EntityMaterialized(this, new EntityEventArgs(entityState.Entity));
    }

    internal void NotifyEntityCreated(Entity entity)
    {
      if (EntityCreated!=null)
        EntityCreated(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityChanging(Entity entity)
    {
      if (EntityChanging!=null)
        EntityChanging(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityVersionInfoChanging(Entity changedEntity, FieldInfo changedField, bool changed)
    {
      if (EntityVersionInfoChanging!=null)
        EntityVersionInfoChanging(this, new EntityVersionInfoChangedEventArgs(
          changedEntity, changedField, changed));
    }

    internal void NotifyEntityVersionInfoChanged(Entity changedEntity, FieldInfo changedField, bool changed)
    {
      if (EntityVersionInfoChanged!=null)
        EntityVersionInfoChanged(this, new EntityVersionInfoChangedEventArgs(
          changedEntity, changedField, changed));
    }

    internal void NotifyFieldValueGetting(Entity entity, FieldInfo field)
    {
      if (EntityFieldValueGetting!=null)
        EntityFieldValueGetting(this, new EntityFieldEventArgs(entity, field));
    }

    internal void NotifyFieldValueGet(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueGet!=null)
        EntityFieldValueGet(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueGetCompleted(Entity entity, FieldInfo field, object value, Exception exception)
    {
      if (EntityFieldValueGetCompleted != null)
        EntityFieldValueGetCompleted(this, new EntityFieldValueGetCompletedEventArgs(entity, field, value, exception));
    }

    internal void NotifyFieldValueSettingAttempt(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueSettingAttempt != null)
        EntityFieldValueSettingAttempt(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueSetting(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueSetting != null)
        EntityFieldValueSetting(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueSet(Entity entity, FieldInfo field, object oldValue, object newValue)
    {
      if (EntityFieldValueSet!=null)
        EntityFieldValueSet(this, new EntityFieldValueSetEventArgs(entity, field, oldValue, newValue));
    }

    internal void NotifyFieldValueSetCompleted(Entity entity, FieldInfo field, object oldValue, object newValue, Exception exception)
    {
      if (EntityFieldValueSetCompleted != null)
        EntityFieldValueSetCompleted(this, new EntityFieldValueSetCompletedEventArgs(entity, field, oldValue, newValue, exception));
    }

    internal void NotifyEntityRemoving(Entity entity)
    {
      if (EntityRemoving!=null)
        EntityRemoving(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemove(Entity entity)
    {
      if (EntityRemove!=null)
        EntityRemove(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemoveCompleted(Entity entity, Exception exception)
    {
      if (EntityRemoveCompleted != null)
        EntityRemoveCompleted(this, new EntityRemoveCompletedEventArgs(entity, exception));
    }

    internal void NotifyEntitySetItemRemoving(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemRemoving != null)
        EntitySetItemRemoving(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemRemoved(Entity entity, EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemRemoved != null)
        EntitySetItemRemoved(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemRemoveCompleted(EntitySetBase entitySet, Entity item, Exception exception)
    {
      if (EntitySetItemRemoveCompleted != null)
        EntitySetItemRemoveCompleted(this, new EntitySetItemActionCompletedEventArgs(entitySet, item, exception));
    }

    internal void NotifyEntitySetItemAdding(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemAdding != null)
        EntitySetItemAdding(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemAdd(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemAdd != null)
        EntitySetItemAdd(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemAddCompleted(EntitySetBase entitySet, Entity item, Exception exception)
    {
      if (EntitySetItemAddCompleted != null)
        EntitySetItemAddCompleted(this, new EntitySetItemActionCompletedEventArgs(entitySet, item, exception));
    }

    internal void NotifyOperationCompleted(IOperation operation)
    {
      if (OperationCompleted != null)
        OperationCompleted(this, new OperationEventArgs(operation));
    }
  }
}