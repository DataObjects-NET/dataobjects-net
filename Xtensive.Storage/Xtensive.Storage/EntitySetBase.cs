// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using OperationType=Xtensive.Storage.PairIntegrity.OperationType;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for <see cref="EntitySet{TItem}"/>.
  /// </summary>
  public abstract class EntitySetBase : TransactionalStateContainer<EntitySetState>,
    ICountable,
    IFieldValueAdapter,
    INotifyPropertyChanged,
    INotifyCollectionChanged
  {
    private static readonly object EntitySetCachingRegion = new object();

    private static readonly Parameter<Tuple> KeyParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    internal static readonly Parameter<Entity> OwnerParameter = new Parameter<Entity>("Owner");

    private readonly Entity owner;
    private bool isInitialized;

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    public Entity Owner { get { return owner; } }

    /// <inheritdoc/>
    [Infrastructure] // Proxy
    Persistent IFieldValueAdapter.Owner { get { return Owner; } }

    /// <inheritdoc/>
    [Infrastructure]
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySetBase"/>.
    /// </summary>
    public long Count {
      [DebuggerStepThrough]
      get {
        EnsureOwnerIsNotRemoved();
        if (State.TotalItemsCount==null)
          LoadItemsCount();
        return (long) State.TotalItemsCount;
      }
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
      if (key==null || !Field.ItemType.IsAssignableFrom(key.Type.UnderlyingType))
        return false;

      var state = Session.EntityStateCache[key, true];
      return state!=null ? Contains(key, state.PersistenceState) : Contains(key, null);
    }

    /// <summary>
    /// Determines whether this collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for containment.</param>
    /// <returns>
    /// <see langword="true"/> if this collection contains the specified item; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Entity item)
    {
      EnsureOwnerIsNotRemoved();
      if (item==null || !Field.ItemType.IsAssignableFrom(item.Type.UnderlyingType))
        return false;
      return Contains(item.Key, item.PersistenceState);
    }

    /// <summary>
    /// Adds the specified item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>
    /// <see langword="True"/>, if the item is added to the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Add(Entity item)
    {
      if (Contains(item))
        return false;

      try {
        using (var context = OpenOperationContext(true)) {
          if (context.IsEnabled())
            context.Add(new EntitySetItemOperation(
              Owner.Key, 
              Field, 
              Operations.OperationType.AddEntitySetItem, 
              item.Key));

          SystemBeforeAdd(item);

          if (Field.Association.IsPaired)
            Session.PairSyncManager.Enlist(OperationType.Add, Owner, item, Field.Association);
          if (Field.Association.AuxiliaryType != null && Field.Association.IsMaster)
            GetEntitySetTypeState().ItemCtor.Invoke(Owner.Key.Value.Combine(item.Key.Value));

          State.Add(item.Key);
          MarkStateAsModified();
          Owner.UpdateVersionInternal();

          SystemAdd(item);
          SystemAddCompleted(item, null);
          context.Complete();
          return true;
        }
      }
      catch(Exception e) {
        SystemAddCompleted(item, e);
        throw;
      }
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>
    /// <see langword="True"/>, if the item is removed from the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(Entity item)
    {
      if (!Contains(item))
        return false;

      try {
        using (var context = OpenOperationContext(true)) {
          if (context.IsEnabled())
            context.Add(new EntitySetItemOperation(
              Owner.Key, 
              Field, 
              Operations.OperationType.RemoveEntitySetItem,
              item.Key));
          SystemBeforeRemove(item);

          if (Field.Association.IsPaired)
            Session.PairSyncManager.Enlist(OperationType.Remove, Owner, item, Field.Association);

          if (Field.Association.AuxiliaryType != null && Field.Association.IsMaster) {
            var combinedKey = Key.Create(Session.Domain, Field.Association.AuxiliaryType,
                                         TypeReferenceAccuracy.ExactType,
                                         Owner.Key.Value.Combine(item.Key.Value));
            Entity underlyingItem = Query.SingleOrDefault(Session, combinedKey);
            underlyingItem.Remove();
          }

          State.Remove(item.Key);
          MarkStateAsModified();
          Owner.UpdateVersionInternal();
          SystemRemove(item);
          SystemRemoveCompleted(item, null);
          context.Complete();
          return true;
        }
      }
      catch(Exception e) {
        SystemRemoveCompleted(item, e);
        throw;
      }
    }

    /// <summary>
    /// Adds the specified item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>
    /// <see langword="True"/>, if the item is added to the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected bool Add(IEntity item)
    {
      return Add((Entity) item);
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>
    /// <see langword="True"/>, if the item is removed from the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected bool Remove(IEntity item)
    {
      return Remove((Entity) item);
    }

    /// <summary>
    /// Determines whether this collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for containment.</param>
    /// <returns>
    /// <see langword="true"/> if this collection contains the specified item; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected bool Contains(IEntity item)
    {
      return Contains((Entity) item);
    }

    /// <summary>
    /// Clears this collection.
    /// </summary>
    public void Clear()
    {
      EnsureOwnerIsNotRemoved();
      using (var context = OpenOperationContext(true)) {
        if (context.IsEnabled())
          context.Add(new EntitySetOperation(Owner.Key, Operations.OperationType.ClearEntitySet, Field));
        SystemBeforeClear();
        foreach (var entity in Entities.ToList())
          Remove(entity);
        SystemClear();
        context.Complete();
      }
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
    protected abstract Delegate GetItemCountQueryDelegate(FieldInfo field);

    /// <summary>
    /// Validates current version.
    /// </summary>
    /// <param name="expectedVersion">The expected version.</param>
    protected void ValidateVersion(long expectedVersion)
    {
      if (expectedVersion!=State.Version)
        throw Exceptions.CollectionHasBeenChanged(null);
    }

    /// <summary>
    /// Ensures the owner is not removed.
    /// </summary>
    protected void EnsureOwnerIsNotRemoved()
    {
      if (Owner.IsRemoved)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }

    /// <inheritdoc/>
    protected sealed override EntitySetState LoadState()
    {
      if (Owner.State.PersistenceState!=PersistenceState.New) {
        Session.Handler.Prefetch(Owner.Key, Owner.Type,
          new FieldDescriptorCollection(new PrefetchFieldDescriptor(Field, WellKnown.EntitySetPreloadCount)));
        Session.Handler.ExecutePrefetchTasks();
        return State;
      }
      var state = new EntitySetState(WellKnown.EntitySetStateMaxSize);
      state.TotalItemsCount = 0;
      return state;
    }

    protected internal abstract IEnumerable<IEntity> Entities { get; }

    #region System-level event-like members

    private void SystemInitialize()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializeEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnInitialize();
    }

    private void SystemBeforeAdd(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntitySetItemAdding(this, item);
      var subscriptionInfo = GetSubscription(EntityEventBroker.AddingEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, item);
      OnAdding(item);
    }

    private void SystemAdd(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntitySetItemAdd(this, item);
      var subscriptionInfo = GetSubscription(EntityEventBroker.AddEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, item);
      OnAdd(item);
      NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item);
    }

    private void SystemAddCompleted(Entity item, Exception exception)
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntitySetItemAddCompleted(this, item, exception);
    }

    private void SystemBeforeRemove(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntitySetItemRemoving(this, item);
      var subscriptionInfo = GetSubscription(EntityEventBroker.RemovingEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second).Invoke(subscriptionInfo.First, Field, item);
      OnRemoving(item);
    }

    private void SystemRemove(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntitySetItemRemoved(this, item);
      var subscriptionInfo = GetSubscription(EntityEventBroker.RemoveEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, item);
      OnRemove(item);
      NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item);
    }

    private void SystemRemoveCompleted(Entity item, Exception exception)
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntitySetItemRemoveCompleted(this, item, exception);
    }

    private void SystemBeforeClear()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.ClearingEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnClearing();
    }

    private void SystemClear()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.ClearEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnClear();
      NotifyCollectionChanged(NotifyCollectionChangedAction.Reset, null);
    }

    #endregion

    #region INotifyXxxChanged & event support related methods

    /// <inheritdoc/>
    [Infrastructure]
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        Session.EntityEventBroker.AddSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEventBroker.RemoveSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.PropertyChangedEventKey, value);
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public event NotifyCollectionChangedEventHandler CollectionChanged {
      add {
        Session.EntityEventBroker.AddSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.CollectionChangedEventKey, value);
      }
      remove {
        Session.EntityEventBroker.RemoveSubscriber(GetOwnerKey(Owner), Field,
          EntityEventBroker.CollectionChangedEventKey, value);
      }
    }

    /// <summary>
    /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the changed property.</param>
    [Infrastructure]
    protected void NotifyPropertyChanged(string propertyName)
    {
      if (!Session.EntityEventBroker.HasSubscribers)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.PropertyChangedEventKey);
      if (subscriptionInfo.Second != null)
        ((PropertyChangedEventHandler) subscriptionInfo.Second)
          .Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
    /// </summary>
    /// <param name="action">The actual action.</param>
    /// <param name="item">The item, that was participating in the specified action.</param>
    [Infrastructure]
    protected void NotifyCollectionChanged(NotifyCollectionChangedAction action, Entity item)
    {
      if (!Session.EntityEventBroker.HasSubscribers)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.CollectionChangedEventKey);
      if (subscriptionInfo.Second != null)
        ((NotifyCollectionChangedEventHandler) subscriptionInfo.Second)
          .Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      NotifyPropertyChanged("Count");
    }

    /// <summary>
    /// Gets the subscription for the specified event key.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <returns>Event subscription (delegate) for the specified event key.</returns>
    [Infrastructure]
    protected Pair<Key, Delegate> GetSubscription(object eventKey)
    {
      var entityKey = GetOwnerKey(Owner);
      if (entityKey!=null)
        return new Pair<Key, Delegate>(entityKey,
          Session.EntityEventBroker.GetSubscriber(entityKey, Field, eventKey));
      return new Pair<Key, Delegate>(null, null);
    }

    #endregion

    #region Event-like methods

    protected virtual void OnInitialize()
    {
    }

    protected virtual void OnAdding(Entity item)
    {
    }

    protected virtual void OnAdd(Entity item)
    {
    }

    protected virtual void OnRemoving(Entity item)
    {
    }

    protected virtual void OnRemove(Entity item)
    {
    }

    protected virtual void OnClearing()
    {
    }

    protected virtual void OnClear()
    {
    }

    #endregion

    #region IEnumerable members

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
      return Entities.GetEnumerator();
    }

    #endregion

    #region Private / internal members
    
    internal EntitySetState UpdateState(IEnumerable<Key> items, bool isFullyLoaded)
    {
      EnsureOwnerIsNotRemoved();
      var newState = new EntitySetState(WellKnown.EntitySetStateMaxSize);
      newState.Clear();
      long count = 0;
      foreach (var item in items) {
        newState.Register(item);
        count++;
      }
      if (isFullyLoaded)
        newState.TotalItemsCount = count;
      State = newState;
      return State;
    }

    internal EntitySetState GetState()
    {
      EnsureOwnerIsNotRemoved();
      return StateIsLoaded ? State : null;
    }

    internal void IntersectWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      EnsureOwnerIsNotRemoved();
      if (this==other)
        return;
      var otherEntities = other.Cast<IEntity>().ToHashSet();
      foreach (var item in Entities.ToList())
        if (!otherEntities.Contains(item))
          Remove(item);
    }

    internal void UnionWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      EnsureOwnerIsNotRemoved();
      if (this == other)
        return;
      foreach (var item in other)
        Add(item);
    }

    internal void ExceptWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      EnsureOwnerIsNotRemoved();
      if (this == other) {
        Clear();
        return;
      }
      foreach (var item in other)
        Remove(item);
    }

    private bool Contains(Key key, PersistenceState? peristentState)
    {
      EnsureOwnerIsNotRemoved();
      bool foundInCache = State.Contains(key);

      if (peristentState==PersistenceState.New || State.IsFullyLoaded)
        return foundInCache;

      bool foundInDatabase;
      using (new ParameterContext().Activate()) {
        KeyParameter.Value = GetEntitySetTypeState().SeekTransform
          .Apply(TupleTransformType.TransformedTuple, Owner.Key.Value, key.Value);
        foundInDatabase = GetEntitySetTypeState().SeekRecordSet.FirstOrDefault()!=null;
      }
      if (foundInDatabase)
        State.Register(key);
      return foundInDatabase;
    }

    private static Key GetOwnerKey(Persistent owner)
    {
      var asFieldValueAdapter = owner as IFieldValueAdapter;
      if (asFieldValueAdapter != null)
        return GetOwnerKey(asFieldValueAdapter.Owner);
      return ((Entity) owner).Key;
    }

    private EntitySetTypeState GetEntitySetTypeState()
    {
      EnsureOwnerIsNotRemoved();
      return (EntitySetTypeState) Session.Domain.GetCachedItem(
        new Pair<object, FieldInfo>(EntitySetCachingRegion, Field), BuildEntitySetTypeState, this);
    }

    private static EntitySetTypeState BuildEntitySetTypeState(object pair, object entitySetObj)
    {
      var field = ((Pair<object, FieldInfo>) pair).Second;
      var entitySet = (EntitySetBase) entitySetObj;
      var seek = field.Association.UnderlyingIndex.ToRecordSet().Seek(() => KeyParameter.Value);
      var seekTransform = new CombineTransform(true,
        field.Association.OwnerType.KeyProviderInfo.TupleDescriptor,
        field.Association.TargetType.KeyProviderInfo.TupleDescriptor);
      Func<Tuple, Entity> itemCtor = null;
      if (field.Association.AuxiliaryType!=null)
        itemCtor = DelegateHelper.CreateDelegate<Func<Tuple, Entity>>(null,
          field.Association.AuxiliaryType.UnderlyingType, DelegateHelper.AspectedProtectedConstructorCallerName,
          ArrayUtils<Type>.EmptyArray);
      return new EntitySetTypeState(seek, seekTransform, itemCtor,entitySet.GetItemCountQueryDelegate(field));
    }

    private void LoadItemsCount()
    {
      using (new ParameterContext().Activate()) {
        OwnerParameter.Value = owner;
        var cachedState = GetEntitySetTypeState();
        State.TotalItemsCount = Query.Execute(cachedState, (Func<int>) cachedState.ItemCountQuery);
      }
    }
    
    #endregion


    // Initialization

    /// <summary>
    /// Performs initialization (see <see cref="Initialize()"/>) of the <see cref="EntitySetBase"/> 
    /// if type of <see langword="this" /> is the same as <paramref name="ctorType"/>.
    /// Invoked by <see cref="InitializableAttribute"/> aspect in the epilogue of any 
    /// constructor of this type and its ancestors.
    /// </summary>
    /// <param name="ctorType">The type, which constructor has invoked this method.</param>
    protected void Initialize(Type ctorType)
    {
      if (ctorType==GetType() && !isInitialized) {
        isInitialized = true;
        Initialize();
      }
    }

    /// <summary>
    /// Performs initialization of the <see cref="EntitySetBase"/>.
    /// </summary>
    protected virtual void Initialize()
    {
      SystemInitialize();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySetBase(Entity owner, FieldInfo field)
      : base(owner.Session)
    {
      this.owner = owner;
      Field = field;
      Initialize(typeof (EntitySetBase));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected EntitySetBase(SerializationInfo info, StreamingContext context)
    {
      throw new NotImplementedException();
    }
  }
}