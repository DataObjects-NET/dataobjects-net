// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.10

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Provides access to <see cref="Session"/>-level events.
  /// </summary>
  public sealed class SessionEventAccessor
  {
    /// <summary>
    /// Gets the session this instance is bound to.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets indicates whether this accessor describes system events (<see cref="Orm.Session.SystemEvents"/>).
    /// </summary>
    public bool SystemEvents { get; private set;  }

    #region Events

    /// <summary>
    /// Occurs when <see cref="DbCommand"/> is about to execute.
    /// </summary>
    public event EventHandler<DbCommandEventArgs> DbCommandExecuting;

    /// <summary>
    /// Occurs when <see cref="DbCommand"/> is executed.
    /// </summary>
    public event EventHandler<DbCommandEventArgs> DbCommandExecuted;

    /// <summary>
    /// Occurs when LINQ query is about to execute.
    /// </summary>
    public event EventHandler<QueryEventArgs> QueryExecuting;

    /// <summary>
    /// Occures when LINQ query is executed, but before enumeration of result.
    /// </summary>
    public event EventHandler<QueryEventArgs> QueryExecuted;

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to be disposed.
    /// </summary>
    public event EventHandler Disposing;

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to <see cref="Orm.Session.SaveChanges"/> changes.
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
    public event EventHandler<EntityRemovingEventArgs> EntityRemoving;

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
    /// Occurs when <see cref="EntitySetBase"/> is about to be cleared.
    /// </summary>
    public event EventHandler<EntitySetEventArgs> EntitySetClearing;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> is cleared.
    /// </summary>
    public event EventHandler<EntitySetEventArgs> EntitySetClear;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> clearing is completed.
    /// </summary>
    public event EventHandler<EntitySetActionCompletedEventArgs> EntitySetClearCompleted;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to be opened.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionOpening;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is opened.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionOpened;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to be committed.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionPrecommitting;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to be committed.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionCommitting;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is committed.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionCommitted;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is about to be rolled back.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionRollbacking;

    /// <summary>
    /// Occurs when <see cref="Transaction"/> is rolled back.
    /// </summary>
    public event EventHandler<TransactionEventArgs> TransactionRollbacked;

    #endregion

    #region NotifyXxx methods

    internal void NotifyDbCommandExecuting(DbCommand command)
    {
      if (DbCommandExecuting!=null)
        DbCommandExecuting(this, new DbCommandEventArgs(command));
    }

    internal void NotifyDbCommandExecuted(DbCommand command, Exception exception=null)
    {
      if (DbCommandExecuted!=null)
        DbCommandExecuted(this, new DbCommandEventArgs(command, exception));
    }

    internal Expression NotifyQueryExecuting(Expression expression)
    {
      var args = new QueryEventArgs(expression);
      if (QueryExecuting!=null) {
        QueryExecuting(this, args);
      }
      return args.Expression;
    }

    internal void NotifyQueryExecuted(Expression expression, Exception exception=null)
    {
      if (QueryExecuted!=null)
        QueryExecuted(this, new QueryEventArgs(expression, exception));
    }

    internal void NotifyDisposing()
    {
      if (Disposing!=null)
        Disposing(this, EventArgs.Empty);
    }

    internal void NotifyPersisting()
    {
      if (Persisting!=null && AreNotificationsEnabled())
        Persisting(this, EventArgs.Empty);
    }

    internal void NotifyPersisted()
    {
      if (Persisted!=null && AreNotificationsEnabled())
        Persisted(this, EventArgs.Empty);
    }

    internal void NotifyKeyGenerated(Key key)
    {
      if (KeyGenerated!=null && AreNotificationsEnabled())
        KeyGenerated(this, new KeyEventArgs(key));
    }

    internal void NotifyEntityMaterialized(EntityState entityState)
    {
      if (EntityMaterialized!=null && AreNotificationsEnabled())
        EntityMaterialized(this, new EntityEventArgs(entityState.Entity));
    }

    internal void NotifyEntityCreated(Entity entity)
    {
      if (EntityCreated!=null && AreNotificationsEnabled())
        EntityCreated(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityChanging(Entity entity)
    {
      if (EntityChanging!=null && AreNotificationsEnabled())
        EntityChanging(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityVersionInfoChanging(Entity changedEntity, FieldInfo changedField, bool changed)
    {
      if (EntityVersionInfoChanging!=null && AreNotificationsEnabled())
        EntityVersionInfoChanging(this, new EntityVersionInfoChangedEventArgs(
          changedEntity, changedField, changed));
    }

    internal void NotifyEntityVersionInfoChanged(Entity changedEntity, FieldInfo changedField, bool changed)
    {
      if (EntityVersionInfoChanged!=null && AreNotificationsEnabled())
        EntityVersionInfoChanged(this, new EntityVersionInfoChangedEventArgs(
          changedEntity, changedField, changed));
    }

    internal void NotifyFieldValueGetting(Entity entity, FieldInfo field)
    {
      if (EntityFieldValueGetting!=null && AreNotificationsEnabled())
        EntityFieldValueGetting(this, new EntityFieldEventArgs(entity, field));
    }

    internal void NotifyFieldValueGet(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueGet!=null && AreNotificationsEnabled())
        EntityFieldValueGet(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueGetCompleted(Entity entity, FieldInfo field, object value, Exception exception)
    {
      if (EntityFieldValueGetCompleted!=null && AreNotificationsEnabled())
        EntityFieldValueGetCompleted(this, new EntityFieldValueGetCompletedEventArgs(entity, field, value, exception));
    }

    internal void NotifyFieldValueSettingAttempt(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueSettingAttempt!=null && AreNotificationsEnabled())
        EntityFieldValueSettingAttempt(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueSetting(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueSetting!=null && AreNotificationsEnabled())
        EntityFieldValueSetting(this, new EntityFieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueSet(Entity entity, FieldInfo field, object oldValue, object newValue)
    {
      if (EntityFieldValueSet!=null && AreNotificationsEnabled())
        EntityFieldValueSet(this, new EntityFieldValueSetEventArgs(entity, field, oldValue, newValue));
    }

    internal void NotifyFieldValueSetCompleted(Entity entity, FieldInfo field, object oldValue, object newValue, Exception exception)
    {
      if (EntityFieldValueSetCompleted!=null && AreNotificationsEnabled())
        EntityFieldValueSetCompleted(this, new EntityFieldValueSetCompletedEventArgs(entity, field, oldValue, newValue, exception));
    }

    internal void NotifyEntityRemoving(Entity entity, EntityRemoveReason reason)
    {
      if (EntityRemoving!=null && AreNotificationsEnabled())
        EntityRemoving(this, new EntityRemovingEventArgs(entity, reason));
    }

    internal void NotifyEntityRemove(Entity entity)
    {
      if (EntityRemove!=null && AreNotificationsEnabled())
        EntityRemove(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemoveCompleted(Entity entity, Exception exception)
    {
      if (EntityRemoveCompleted!=null && AreNotificationsEnabled())
        EntityRemoveCompleted(this, new EntityRemoveCompletedEventArgs(entity, exception));
    }

    internal void NotifyEntitySetItemRemoving(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemRemoving!=null && AreNotificationsEnabled())
        EntitySetItemRemoving(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemRemoved(Entity entity, EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemRemove!=null && AreNotificationsEnabled())
        EntitySetItemRemove(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemRemoveCompleted(EntitySetBase entitySet, Entity item, Exception exception)
    {
      if (EntitySetItemRemoveCompleted!=null && AreNotificationsEnabled())
        EntitySetItemRemoveCompleted(this, new EntitySetItemActionCompletedEventArgs(entitySet, item, exception));
    }

    internal void NotifyEntitySetItemAdding(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemAdding!=null && AreNotificationsEnabled())
        EntitySetItemAdding(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemAdd(EntitySetBase entitySet, Entity item)
    {
      if (EntitySetItemAdd!=null && AreNotificationsEnabled())
        EntitySetItemAdd(this, new EntitySetItemEventArgs(entitySet, item));
    }

    internal void NotifyEntitySetItemAddCompleted(EntitySetBase entitySet, Entity item, Exception exception)
    {
      if (EntitySetItemAddCompleted!=null && AreNotificationsEnabled())
        EntitySetItemAddCompleted(this, new EntitySetItemActionCompletedEventArgs(entitySet, item, exception));
    }

    internal void NotifyEntitySetClearing(EntitySetBase entitySet)
    {
      if (EntitySetClearing!=null && AreNotificationsEnabled())
        EntitySetClearing(this, new EntitySetEventArgs(entitySet));
    }

    internal void NotifyEntitySetClear(EntitySetBase entitySet)
    {
      if (EntitySetClear!=null && AreNotificationsEnabled())
        EntitySetClear(this, new EntitySetEventArgs(entitySet));
    }

    internal void NotifyEntitySetClearCompleted(EntitySetBase entitySet, Exception exception)
    {
      if (EntitySetClearCompleted!=null && AreNotificationsEnabled())
        EntitySetClearCompleted(this, new EntitySetActionCompletedEventArgs(entitySet, exception));
    }

    internal void NotifyTransactionOpening(Transaction transaction)
    {
      if (TransactionOpening!=null && AreNotificationsEnabled())
        TransactionOpening(this, new TransactionEventArgs(transaction));
    }

    internal void NotifyTransactionOpened(Transaction transaction)
    {
      if (TransactionOpened!=null && AreNotificationsEnabled())
        TransactionOpened(this, new TransactionEventArgs(transaction));
    }
    
    internal void NotifyTransactionPrecommitting(Transaction transaction)
    {
      if (TransactionPrecommitting!=null && AreNotificationsEnabled())
        TransactionPrecommitting(this, new TransactionEventArgs(transaction));
    }

    internal void NotifyTransactionCommitting(Transaction transaction)
    {
      if (TransactionCommitting!=null && AreNotificationsEnabled())
        TransactionCommitting(this, new TransactionEventArgs(transaction));
    }

    internal void NotifyTransactionCommitted(Transaction transaction)
    {
      if (TransactionCommitted!=null && AreNotificationsEnabled())
        TransactionCommitted(this, new TransactionEventArgs(transaction));
    }

    internal void NotifyTransactionRollbacking(Transaction transaction)
    {
      if (TransactionRollbacking!=null && AreNotificationsEnabled())
        TransactionRollbacking(this, new TransactionEventArgs(transaction));
    }

    internal void NotifyTransactionRollbacked(Transaction transaction)
    {
      if (TransactionRollbacked!=null && AreNotificationsEnabled())
        TransactionRollbacked(this, new TransactionEventArgs(transaction));
    }

    #endregion

    private bool AreNotificationsEnabled()
    {
      return SystemEvents || !Session.IsSystemLogicOnly;
    }


    // Constructors

    internal SessionEventAccessor(Session session, bool systemEvents)
    {
      Session = session;
      SystemEvents = systemEvents;
    }
  }
}