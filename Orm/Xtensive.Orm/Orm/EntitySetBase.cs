// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;
using Xtensive.Orm.PairIntegrity;
using Xtensive.Orm.ReferentialIntegrity;
using Xtensive.Orm.Rse;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Xtensive.Tuples.Transform;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using OperationType = Xtensive.Orm.PairIntegrity.OperationType;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// Abstract base for <see cref="EntitySet{TItem}"/>.
  /// </summary>
  public abstract class EntitySetBase : SessionBound,
    IFieldValueAdapter,
    INotifyPropertyChanged,
    INotifyCollectionChanged
  {
    private static readonly string presentationFrameworkAssemblyPrefix = "PresentationFramework,";
#if DEBUG
    private static readonly string storageTestsAssemblyPrefix = "Xtensive.Orm.Tests";
#endif
    private static readonly Parameter<Tuple> keyParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    internal static readonly Parameter<Entity> ownerParameter = new Parameter<Entity>("Owner");

    private static readonly Func<FieldInfo, EntitySetBase, EntitySetTypeState> EntitySetTypeStateFactory = BuildEntitySetTypeState;

    private readonly Entity owner;
    private readonly CombineTransform auxilaryTypeKeyTransform;
    private readonly bool skipOwnerVersionChange;
    private bool isInitialized;

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    public Entity Owner => owner;

    /// <inheritdoc/>
    Persistent IFieldValueAdapter.Owner => Owner;

    /// <inheritdoc/>
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySetBase"/>.
    /// </summary>
    public long Count
    {
      get {
        if (Owner.IsRemoved) {
          return 0; // WPF tries to use EntitySets of removed Entities
        }

        EnsureIsLoaded(WellKnown.EntitySetPreloadCount);
        EnsureCountIsLoaded();
        return (long) State.TotalItemCount;
      }
    }

    internal EntitySetState State { get; private set; }

    /// <summary>
    /// Gets the entities contained in this <see cref="EntitySetBase"/>.
    /// </summary>
    protected internal IEnumerable<IEntity> Entities
      => InnerGetEntities().ToTransactional(Session);

    private IEnumerable<IEntity> InnerGetEntities()
    {
      if (Owner.IsRemoved) {
        yield break; // WPF tries to enumerate EntitySets of removed Entities
      }

      PrefetchInternal();
      foreach (var key in State) {
        var entity = Session.Query.SingleOrDefault(key);
        if (entity == null) {
          if (!key.IsTemporary(Session.Domain)) {
            Session.RemoveOrCreateRemovedEntity(key.TypeReference.Type.UnderlyingType, key, EntityRemoveReason.Other);
            if (Session.LookupStateInCache(key, out var entityState)) {
              entity = entityState.Entity;
            }
          }
        }
        yield return entity;
      }
    }

    /// <summary>
    /// Prefetches the entity set completely - i.e. ensures it is fully loaded.
    /// </summary>
    public void Prefetch() => Prefetch(null);

    /// <summary>
    /// Prefetches the entity set - i.e. ensures it is either completely or partially loaded.
    /// </summary>
    /// <param name="maxItemCount">The maximal count of items to try to load.</param>
    public void Prefetch(int? maxItemCount)
    {
      EnsureOwnerIsNotRemoved();
      EnsureIsLoaded(maxItemCount, State.ShouldUseForcePrefetch(null));
    }

    /// <summary>
    /// Determines whether <see cref="EntitySetBase"/> contains the specified <see cref="Key"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if <see cref="EntitySetBase"/> contains the specified <see cref="Key"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Entity type is not supported.</exception>
    public bool Contains(Key key)
    {
      EnsureOwnerIsNotRemoved();
      if (key==null || !Field.ItemType.IsAssignableFrom(key.TypeInfo.UnderlyingType)) {
        return false;
      }

      return Session.LookupStateInCache(key, out var entityState)
        ? Contains(key, entityState.TryGetEntity())
        : Contains(key, null);
    }

    /// <summary>
    /// Gets a delegate which returns an <see cref="IQueryable{T}"/>
    /// returning count of items associated with this instance.
    /// </summary>
    /// <param name="field">The field containing <see cref="EntitySet{TItem}"/>.</param>
    /// <returns>
    /// The created delegate which returns an <see cref="IQueryable{T}"/>
    /// returning count of items associated with this instance.
    /// </returns>
    protected abstract Func<QueryEndpoint,long> GetItemCountQueryDelegate(FieldInfo field);

    /// <summary>
    /// Ensures the owner is not removed.
    /// </summary>
    protected void EnsureOwnerIsNotRemoved()
    {
      if (Owner.IsRemoved) {
        throw new InvalidOperationException(string.Format(Strings.ExEntityOfTypeXIsRemoved, Owner.TypeInfo.Name));
      }
    }

    #region System-level event-like members

    private void SystemInitialize()
    {
      if (Session.IsSystemLogicOnly) {
        return;
      }

      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializeEntitySetEventKey);
      if (subscriptionInfo.Second != null) {
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      }
      OnInitialize();
    }

    private void SystemBeforeAdd(Entity item)
    {
      Session.SystemEvents.NotifyEntitySetItemAdding(this, item);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetItemAdding(this, item);

        if (Session.IsSystemLogicOnly) {
          return;
        }

        var subscriptionInfo = GetSubscription(EntityEventBroker.AddingEntitySetItemEventKey);
        if (subscriptionInfo.Second != null) {
          ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, item);
        }
        OnAdding(item);
      }
    }

    private void SystemAdd(Entity item, int? index)
    {
      Session.SystemEvents.NotifyEntitySetItemAdd(this, item);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetItemAdd(this, item);

        if (Session.IsSystemLogicOnly) {
          return;
        }

        if (CanBeValidated) {
          Session.ValidationContext.RegisterForValidation(Owner);
        }

        var subscriptionInfo = GetSubscription(EntityEventBroker.AddEntitySetItemEventKey);
        if (subscriptionInfo.Second != null) {
          ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, item);
        }
        OnAdd(item);
        NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
      }
    }

    private void SystemAddCompleted(Entity item, Exception exception)
    {
      Session.SystemEvents.NotifyEntitySetItemAddCompleted(this, item, exception);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetItemAddCompleted(this, item, exception);
      }
    }

    private void SystemBeforeRemove(Entity item)
    {
      Session.SystemEvents.NotifyEntitySetItemRemoving(this, item);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetItemRemoving(this, item);

        if (Session.IsSystemLogicOnly) {
          return;
        }

        var subscriptionInfo = GetSubscription(EntityEventBroker.RemovingEntitySetItemEventKey);
        if (subscriptionInfo.Second != null) {
          ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second).Invoke(subscriptionInfo.First, Field, item);
        }
        OnRemoving(item);
      }
    }

    private void SystemRemove(Entity item, int? index)
    {
      Session.SystemEvents.NotifyEntitySetItemRemoved(Owner, this, item);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetItemRemoved(Owner, this, item);

        if (Session.IsSystemLogicOnly) {
          return;
        }

        if (CanBeValidated) {
          Session.ValidationContext.RegisterForValidation(Owner);
        }

        var subscriptionInfo = GetSubscription(EntityEventBroker.RemoveEntitySetItemEventKey);
        if (subscriptionInfo.Second != null) {
          ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, Field, item);
        }

        OnRemove(item);
        NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
      }
    }

    private void SystemRemoveCompleted(Entity item, Exception exception)
    {
      Session.SystemEvents.NotifyEntitySetItemRemoveCompleted(this, item, exception);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetItemRemoveCompleted(this, item, exception);
      }
    }

    private void SystemBeforeClear()
    {
      Session.SystemEvents.NotifyEntitySetClearing(this);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetClearing(this);

        if (Session.IsSystemLogicOnly) {
          return;
        }

        using (Session.Operations.EnableSystemOperationRegistration()) {
          var subscriptionInfo = GetSubscription(EntityEventBroker.ClearingEntitySetEventKey);
          if (subscriptionInfo.Second != null) {
            ((Action<Key, FieldInfo>) subscriptionInfo.Second)
              .Invoke(subscriptionInfo.First, Field);
          }

          OnClearing();
        }
      }
    }

    private void SystemClear()
    {
      Session.SystemEvents.NotifyEntitySetClear(this);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetClear(this);

        if (Session.IsSystemLogicOnly) {
          return;
        }

        if (CanBeValidated) {
          Session.ValidationContext.RegisterForValidation(Owner);
        }

        using (Session.Operations.EnableSystemOperationRegistration()) {
          var subscriptionInfo = GetSubscription(EntityEventBroker.ClearEntitySetEventKey);
          if (subscriptionInfo.Second != null) {
            ((Action<Key, FieldInfo>) subscriptionInfo.Second)
              .Invoke(subscriptionInfo.First, Field);
          }
          OnClear();
          NotifyCollectionChanged(NotifyCollectionChangedAction.Reset, null, null);
        }
      }
    }

    private void SystemClearCompleted(Exception exception)
    {
      Session.SystemEvents.NotifyEntitySetClearCompleted(this, exception);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntitySetClearCompleted(this, exception);
      }
    }

    #endregion

    #region INotifyXxxChanged & event support related methods

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        Session.EntityEvents.AddSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.PropertyChangedEventKey, value);
      }
    }

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler CollectionChanged {
      add {
        Session.EntityEvents.AddSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.CollectionChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.CollectionChangedEventKey, value);
      }
    }

    /// <summary>
    /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the changed property.</param>
    protected void NotifyPropertyChanged(string propertyName)
    {
      if (!Session.EntityEvents.HasSubscribers) {
        return;
      }
      var subscriptionInfo = GetSubscription(EntityEventBroker.PropertyChangedEventKey);
      if (subscriptionInfo.Second != null) {
        ((PropertyChangedEventHandler) subscriptionInfo.Second)
          .Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    /// <summary>
    /// Raises <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
    /// </summary>
    /// <param name="action">The actual action.</param>
    /// <param name="item">The item, that was participating in the specified action.</param>
    /// <param name="index">The index on the item, if available.</param>
    protected void NotifyCollectionChanged(NotifyCollectionChangedAction action, Entity item, int? index)
    {
      if (!Session.EntityEvents.HasSubscribers) {
        return;
      }

      var subscriptionInfo = GetSubscription(EntityEventBroker.CollectionChangedEventKey);
      if (subscriptionInfo.Second != null) {
        var handler = (NotifyCollectionChangedEventHandler) subscriptionInfo.Second;
        if (action == NotifyCollectionChangedAction.Reset) {
          handler.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }
        else if (!index.HasValue) {
          if (action == NotifyCollectionChangedAction.Remove) {
            // Workaround for WPF / non-WPF subscribers
            var invocationList = handler.GetInvocationList();
            foreach (var @delegate in invocationList) {
              var typedDelegate = (NotifyCollectionChangedEventHandler) @delegate;
              var subscriberAssemblyName = @delegate.Method.DeclaringType.Assembly.FullName;
              if (subscriberAssemblyName.StartsWith(presentationFrameworkAssemblyPrefix, StringComparison.Ordinal)) {
                // WPF can't handle "Remove" event w/o item index
                typedDelegate.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
              }
#if DEBUG
              else if (subscriberAssemblyName.StartsWith(storageTestsAssemblyPrefix, StringComparison.Ordinal)) {
                typedDelegate.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
              }
#endif
              else {
                typedDelegate.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
              }
            }
          }
          else {
            handler.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
          }
        }
        else {
          handler.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index.GetValueOrDefault()));
        }
      }
      NotifyPropertyChanged("Count");
    }

    /// <summary>
    /// Gets the subscription for the specified event key.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <returns>Event subscription (delegate) for the specified event key.</returns>
    protected Pair<Key, Delegate> GetSubscription(object eventKey)
    {
      var entityKey = GetOwnerKey(Owner);
      if (entityKey != null) {
        return new Pair<Key, Delegate>(entityKey,
          Session.EntityEvents.GetSubscriber(entityKey, Field, eventKey));
      }

      return new Pair<Key, Delegate>(null, null);
    }

    #endregion

    #region Event-like methods

    /// <summary>
    /// Called when entity set is initialized.
    /// </summary>
    protected virtual void OnInitialize()
    {
    }

    /// <summary>
    /// Called when item is adding to entity set.
    /// </summary>
    /// <param name="item">The item.</param>
    protected virtual void OnAdding(Entity item)
    {
    }

    /// <summary>
    /// Called when item is added to entity set.
    /// </summary>
    /// <param name="item">The item.</param>
    protected virtual void OnAdd(Entity item)
    {
    }

    /// <summary>
    /// Called when item is removing from entity set.
    /// </summary>
    /// <param name="item">The item.</param>
    protected virtual void OnRemoving(Entity item)
    {
    }

    /// <summary>
    /// Called when item is removed from entity set.
    /// </summary>
    /// <param name="item">The item.</param>
    protected virtual void OnRemove(Entity item)
    {
    }

    /// <summary>
    /// Called when entity set is clearing.
    /// </summary>
    protected virtual void OnClearing()
    {
    }

    /// <summary>
    /// Called when entity set is cleared.
    /// </summary>
    protected virtual void OnClear()
    {
    }

    /// <summary>
    /// Called when entity set should be validated.
    /// </summary>
    /// <remarks>
    /// Override this method to perform custom entity set validation.
    /// </remarks>
    protected virtual void OnValidate()
    {
    }

    /// <summary>
    /// Gets a value indicating whether validation can be performed for this entity.
    /// </summary>
    protected internal virtual bool CanBeValidated {
      get { return Owner!=null && !Owner.IsRemoved;  }
    }

    #endregion

    #region Add/Remove/Contains/Clear methods

    internal bool Contains(Entity item)
    {
      EnsureOwnerIsNotRemoved();
      return item != null && Field.ItemType.IsAssignableFrom(item.TypeInfo.UnderlyingType)
        && Contains(item.Key, item);
    }

    internal bool Add(Entity item) => Add(item, null, null);

    internal bool Add(Entity item, SyncContext syncContext, RemovalContext removalContext)
    {
      if (Contains(item)) {
        return false;
      }

      try {
        var operations = Session.Operations;
        using (var scope = operations.BeginRegistration(Operations.OperationType.System)) {
          var itemKey = item.Key;
          if (operations.CanRegisterOperation) {
            operations.RegisterOperation(new EntitySetItemAddOperation(Owner.Key, Field, itemKey));
          }

          SystemBeforeAdd(item);

          int? index = null;
          var association = Field.GetAssociation(item.TypeInfo);
          Action finalizer = () => {
            var auxiliaryType = association.AuxiliaryType;
            if (auxiliaryType!=null && association.IsMaster) {
              var combinedTuple = auxilaryTypeKeyTransform.Apply(
                TupleTransformType.Tuple,
                Owner.Key.Value,
                itemKey.Value);

              var combinedKey = Key.Create(
                Session.Domain,
                Session.StorageNodeId,
                auxiliaryType,
                TypeReferenceAccuracy.ExactType,
                combinedTuple);

              _ = Session.CreateOrInitializeExistingEntity(auxiliaryType.UnderlyingType, combinedKey);
              Session.ReferenceFieldsChangesRegistry.Register(Owner.Key, itemKey, combinedKey, Field);
            }

            var state = State;
            state.Add(itemKey);
            Session.EntitySetChangeRegistry.Register(state);
            index = GetItemIndex(state, itemKey);
          };

          operations.NotifyOperationStarting();
          if (association.IsPaired) {
            Session.PairSyncManager.ProcessRecursively(syncContext, removalContext,
              OperationType.Add, association, Owner, item, finalizer);
          }
          else {
            finalizer.Invoke();
          }

          // removalContext is unused here, since Add is never
          // invoked in reference cleanup process directly

          if (!skipOwnerVersionChange) {
            _ = Owner.UpdateVersionInfo(Owner, Field);
          }

          SystemAdd(item, index);
          SystemAddCompleted(item, null);
          scope.Complete();
          return true;
        }
      }
      catch (Exception e) {
        SystemAddCompleted(item, e);
        throw;
      }
    }

    internal bool Remove(Entity item) => Remove(item, null, null);

    internal bool Remove(Entity item, SyncContext syncContext, RemovalContext removalContext)
    {
      if (!Contains(item)) {
        return false;
      }

      try {
        var operations = Session.Operations;
        var scope = operations.BeginRegistration(Operations.OperationType.System);
        try {
          var itemKey = item.Key;
          if (operations.CanRegisterOperation) {
            operations.RegisterOperation(new EntitySetItemRemoveOperation(Owner.Key, Field, itemKey));
          }

          SystemBeforeRemove(item);

          int? index = null;
          var association = Field.GetAssociation(item.TypeInfo);
          Action finalizer = () => {
            var auxiliaryType = association.AuxiliaryType;
            if (auxiliaryType != null && association.IsMaster) {
              var combinedTuple = auxilaryTypeKeyTransform.Apply(
                TupleTransformType.Tuple,
                Owner.Key.Value,
                itemKey.Value);

              var combinedKey = Key.Create(
                Session.Domain,
                Session.StorageNodeId,
                auxiliaryType,
                TypeReferenceAccuracy.ExactType,
                combinedTuple);

              Session.RemoveOrCreateRemovedEntity(auxiliaryType.UnderlyingType, combinedKey, EntityRemoveReason.Association);
            }

            var state = State;
            index = GetItemIndex(state, itemKey);
            state.Remove(itemKey);
            Session.EntitySetChangeRegistry.Register(state);
          };

          operations.NotifyOperationStarting();
          if (association.IsPaired) {
            Session.PairSyncManager.ProcessRecursively(syncContext, removalContext,
              OperationType.Remove, association, Owner, item, finalizer);
          }
          else {
            finalizer.Invoke();
          }

          if (removalContext!=null) {
            // Postponing finalizers (events)
            removalContext.EnqueueFinalizer(() => {
              try {
                try {
                  index = GetItemIndex(State, itemKey); // Necessary, since index can be already changed
                  if (!skipOwnerVersionChange) {
                    _ = Owner.UpdateVersionInfo(Owner, Field);
                  }

                  SystemRemove(item, index);
                  SystemRemoveCompleted(item, null);
                  scope.Complete();
                }
                finally {
                  scope.DisposeSafely();
                }
              }
              catch (Exception e) {
                SystemRemoveCompleted(item, e);
                throw;
              }
            });
            return true;
          }

          if (!skipOwnerVersionChange) {
            _ = Owner.UpdateVersionInfo(Owner, Field);
          }

          SystemRemove(item, index);
          SystemRemoveCompleted(item, null);
          scope.Complete();
          return true;
        }
        finally {
          if (removalContext == null) {
            scope.DisposeSafely();
          }
        }
      }
      catch (Exception e) {
        SystemRemoveCompleted(item, e);
        throw;
      }
    }

    /// <summary>
    /// Clears this collection.
    /// </summary>
    public void Clear()
    {
      EnsureOwnerIsNotRemoved();
      try {
        var operations = Session.Operations;
        using (var scope = operations.BeginRegistration(Operations.OperationType.System)) {
          if (operations.CanRegisterOperation) {
            operations.RegisterOperation(new EntitySetClearOperation(Owner.Key, Field));
          }

          SystemBeforeClear();
          operations.NotifyOperationStarting();

          foreach (var entity in Entities.ToList()) {
            _ = Remove(entity);
          }

          SystemClear();
          SystemClearCompleted(null);
          scope.Complete();
        }
      }
      catch (Exception e) {
        SystemClearCompleted(e);
        throw;
      }
    }

    internal bool Add(IEntity item) => Add((Entity) item);

    internal bool Remove(IEntity item) => Remove((Entity) item);

    internal bool Contains(IEntity item) => Contains((Entity) item);

    #endregion

    #region Set operations

    internal void AddRange<TElement>(IEnumerable<TElement> items)
      where TElement: IEntity
    {
      EnsureOwnerIsNotRemoved();
      foreach (var item in items) {
        _ = Add(item);
      }
    }

    internal void IntersectWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      EnsureOwnerIsNotRemoved();
      if (this == other) {
        return;
      }

      var otherEntities = new HashSet<IEntity>(other.Cast<IEntity>());
      foreach (var item in Entities.ToList()) {
        if (!otherEntities.Contains(item)) {
          _ = Remove(item);
        }
      }
    }

    internal void UnionWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      EnsureOwnerIsNotRemoved();
      if (this == other) {
        return;
      }

      foreach (var item in other) {
        _ = Add(item);
      }
    }

    internal void ExceptWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      EnsureOwnerIsNotRemoved();
      if (this == other) {
        Clear();
        return;
      }
      foreach (var item in other) {
        _ = Remove(item);
      }
    }

    #endregion

    #region Private / internal members

    internal EntitySetState UpdateState(IEnumerable<Key> items, bool isFullyLoaded)
    {
      EnsureOwnerIsNotRemoved();
      var itemList = items.ToList();
      State.Update(itemList, isFullyLoaded ? (long?) itemList.Count : null);
      State.IsLoaded = true;
      Session.NotifyEntitySetCached(this);
      return State;
    }

    internal bool CheckStateIsLoaded()
    {
      if (State.IsLoaded) {
        return true;
      }

      if (Owner.State.PersistenceState == PersistenceState.New) {
        State.TotalItemCount = State.AddedItemsCount;
        State.IsLoaded = true;
        Session.NotifyEntitySetCached(this);
        return true;
      }
      return false;
    }

    private void PrefetchInternal()
    {
      EnsureOwnerIsNotRemoved();
      EnsureIsLoaded(null, false);
    }

    private void EnsureIsLoaded(int? maxItemCount, bool ignoreFullyLoaded = false)
    {
      if (CheckStateIsLoaded()) {
        if (State.IsFullyLoaded && !ignoreFullyLoaded) {
          return;
        }
        State.TotalItemCount = null;
        var requestedItemCount = maxItemCount.HasValue
          ? (int) maxItemCount
          : int.MaxValue;
        if (State.CachedItemCount > requestedItemCount) {
          return;
        }
      }
      using (Session.Activate()) {
        Session.Handler.FetchEntitySet(Owner.Key, Field, maxItemCount);
      }
    }

    private void EnsureCountIsLoaded()
    {
      if (State.TotalItemCount != null) {
        return;
      }

      var parameterContext = new ParameterContext();
      parameterContext.SetValue(ownerParameter, owner);

      var cachedState = GetEntitySetTypeState();
      State.TotalItemCount = Session.Query.Execute(cachedState, cachedState.ItemCountQuery, parameterContext);
    }

    private bool Contains(Key key, Entity item)
    {
      // state check
      var foundInCache = State.Contains(key);
      if (foundInCache) {
        return true;
      }

      var ownerState = Owner.PersistenceState;
      var itemState = item == null
        ? PersistenceState.Synchronized
        : item.PersistenceState;
      if (PersistenceState.New == ownerState || PersistenceState.New == itemState || State.IsFullyLoaded) {
        return false;
      }

      // association check
      if (item != null) {
        var association = Field.GetAssociation(item.TypeInfo);
        if (association.IsPaired && association.Multiplicity is Multiplicity.ManyToOne or Multiplicity.OneToMany) {
          var candidate = (IEntity)item.GetFieldValue(association.Reversed.OwnerField);
          return candidate == Owner;
        }
      }

      // load from storage
      EnsureIsLoaded(WellKnown.EntitySetPreloadCount);

      foundInCache = State.Contains(key);
      if (foundInCache) {
        return true;
      }
      if (State.IsFullyLoaded) {
        return false;
      }

      bool foundInDatabase;
      var entitySetTypeState = GetEntitySetTypeState();

      var parameterContext = new ParameterContext();
      parameterContext.SetValue(keyParameter, entitySetTypeState.SeekTransform
        .Apply(TupleTransformType.TransformedTuple, Owner.Key.Value, key.Value));
      using (var recordSetReader = entitySetTypeState.SeekProvider.GetRecordSetReader(Session, parameterContext)) {
        foundInDatabase = recordSetReader.MoveNext();
      }
      if (foundInDatabase) {
        State.Register(key);
      }
      return foundInDatabase;
    }

    private static Key GetOwnerKey(Persistent owner)
    {
      return owner is IFieldValueAdapter asFieldValueAdapter
        ? GetOwnerKey(asFieldValueAdapter.Owner)
        : ((Entity) owner).Key;
    }

    private EntitySetTypeState GetEntitySetTypeState()
    {
      EnsureOwnerIsNotRemoved();
      return Session.StorageNode.EntitySetTypeStateCache.GetOrAdd(Field, EntitySetTypeStateFactory, this);
    }

    private static EntitySetTypeState BuildEntitySetTypeState(FieldInfo field, EntitySetBase entitySet)
    {
      var association = field.Associations.Last();
      var query = association.UnderlyingIndex.GetQuery().Seek(context => context.GetValue(keyParameter));
      var seek = entitySet.Session.Compile(query);
      var ownerDescriptor = association.OwnerType.Key.TupleDescriptor;
      var targetDescriptor = association.TargetType.Key.TupleDescriptor;

      var itemColumnOffsets = association.AuxiliaryType == null
        ? association.UnderlyingIndex.ValueColumns
            .Where(ci => ci.IsPrimaryKey)
            .Select(ci => ci.Field.MappingInfo.Offset)
            .ToList()
        : CollectionUtils.RangeToList(0, targetDescriptor.Count);

      var keyFieldCount = ownerDescriptor.Count + itemColumnOffsets.Count;
      var keyFieldTypes = ownerDescriptor
        .Concat(itemColumnOffsets.Select(i => targetDescriptor[i]))
        .ToArray(keyFieldCount);
      var keyDescriptor = TupleDescriptor.Create(keyFieldTypes);

      var map = Enumerable.Range(0, ownerDescriptor.Count)
        .Select(i => new Pair<int, int>(0, i))
        .Concat(itemColumnOffsets.Select(i => new Pair<int, int>(1, i)))
        .ToArray(keyFieldCount);
      var seekTransform = new MapTransform(true, keyDescriptor, map);

      Func<Tuple, Entity> itemCtor = null;
      if (association.AuxiliaryType != null) {
        itemCtor = DelegateHelper.CreateDelegate<Func<Tuple, Entity>>(null,
          association.AuxiliaryType.UnderlyingType, DelegateHelper.AspectedFactoryMethodName,
          Array.Empty<Type>());
      }

      return new EntitySetTypeState(seek, seekTransform, itemCtor, entitySet.GetItemCountQueryDelegate(field));
    }

    private int? GetItemIndex(EntitySetState state, Key key)
    {
      if (!state.IsFullyLoaded) {
        return null;
      }
      if (!Session.EntityEvents.HasSubscribers) {
        return null;
      }
      var subscriptionInfo = GetSubscription(EntityEventBroker.CollectionChangedEventKey);
      if (subscriptionInfo.Second == null) {
        return null;
      }

      // Ok, it seems there is a reason
      // to waste linear time on calculating this...
      var i = 0;
      foreach (var cachedKey in state) {
        if (key == cachedKey) {
          return i;
        }
        i++;
      }
      return null;
    }

    internal static void ExecuteOnValidate(EntitySetBase target) => target.OnValidate();

    #endregion


    // Initialization

    /// <summary>
    /// Performs initialization (see <see cref="Initialize()"/>) of the <see cref="EntitySetBase"/>
    /// if type of <see langword="this" /> is the same as <paramref name="ctorType"/>.
    /// Automatically invoked in the epilogue of any constructor of this type and its ancestors.
    /// </summary>
    /// <param name="ctorType">The type, which constructor has invoked this method.</param>
    protected void Initialize(Type ctorType)
    {
      if (ctorType == GetType() && !isInitialized) {
        isInitialized = true;
        Initialize();
      }
    }

    /// <summary>
    /// Performs initialization of the <see cref="EntitySetBase"/>.
    /// </summary>
    protected virtual void Initialize() => SystemInitialize();


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySetBase(Entity owner, FieldInfo field)
      : base(owner.Session)
    {
      this.owner = owner;
      Field = field;
      State = new EntitySetState(this);
      var association = Field.Associations.Last();
      if (association.AuxiliaryType != null && association.IsMaster) {
        var domain = Session.Domain;
        var itemType = domain.Model.Types[Field.ItemType];
        auxilaryTypeKeyTransform = new CombineTransform(
          false,
          owner.TypeInfo.Key.TupleDescriptor,
          itemType.Key.TupleDescriptor);
      }

      skipOwnerVersionChange = association.Multiplicity != Multiplicity.ManyToOne && association.Multiplicity != Multiplicity.OneToMany
        ? false
        : Session.Domain.Configuration.VersioningConvention.DenyEntitySetOwnerVersionChange;

      Initialize(WellKnownOrmTypes.EntitySetBase);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected EntitySetBase(SerializationInfo info, StreamingContext context)
    {
      throw new NotImplementedException();
    }
  }
}
