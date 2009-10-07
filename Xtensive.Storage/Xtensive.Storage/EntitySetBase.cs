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
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

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
    private static readonly object entitySetCachingRegion = new object();

    internal static readonly Parameter<Tuple> pKey = new Parameter<Tuple>(WellKnown.KeyFieldName);
    internal static readonly Parameter<Entity> parameterOwner = new Parameter<Entity>("Owner");

    private readonly Entity owner;
    private bool isInitialized;

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    public Entity Owner {
      get { return owner; }
    }

    /// <inheritdoc/>
    [Infrastructure] // Proxy
    Persistent IFieldValueAdapter.Owner {
      get { return Owner; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySetBase"/>.
    /// </summary>
    public long Count
    {
      [DebuggerStepThrough]
      get {
        if (State.Count == null || !State.IsFullyLoaded)
          LoadItemsCount();
        return (long) State.Count;
      }
    }

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
    /// Determines whether <see cref="EntitySetBase"/> contains the specified <see cref="Key"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if <see cref="EntitySetBase"/> contains the specified <see cref="Key"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Entity type is not supported.</exception>
    public bool Contains(Key key)
    {
      if (key == null || !Field.ItemType.IsAssignableFrom(key.Type.UnderlyingType))
        return false;

      var state = Session.EntityStateCache[key, true];
      return state != null ? Contains(key, state.PersistenceState) : Contains(key, null);
    }

    /// <summary>
    /// Determines whether this collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for containment.</param>
    /// <returns>
    /// <see langword="true"/> if this collection contains the specified item; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected bool Contains(Entity item)
    {
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

      NotifyAdding(item);

      if (Field.Association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Add, Owner, item, Field.Association);

      if (Field.Association.AuxiliaryType!=null && Field.Association.IsMaster)
        GetEntitySetTypeState().ItemCtor.Invoke(Owner.Key.Value.Combine(item.Key.Value));

      State.Add(item.Key);
      Owner.OnFieldChanged();
      NotifyAdd(item);

      return true;
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

      NotifyRemoving(item);

      if (Field.Association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Remove, Owner, item, Field.Association);

      if (Field.Association.AuxiliaryType!=null && Field.Association.IsMaster) {
        var combinedKey = Key.Create(Field.Association.AuxiliaryType, Owner.Key.Value.Combine(item.Key.Value));
        Entity underlyingItem = Query.SingleOrDefault(Session, combinedKey);
        underlyingItem.Remove();
      }

      State.Remove(item.Key);
      Owner.OnFieldChanged();
      NotifyRemove(item);

      return true;
    }

    /// <summary>
    /// Clears this collection.
    /// </summary>
    public void Clear()
    {
      NotifyClearing();
      foreach (var entity in Entities.ToList())
        Remove(entity);
      NotifyClear();
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
    
    #region NotifyXxx & GetSubscription members

    private void NotifyInitialize()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializeEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnInitialize();
    }

    private void NotifyAdding(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.AddingEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, item);
      OnAdding(item);
    }

    private void NotifyAdd(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.AddEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, item);
      OnAdd(item);
      NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item);
    }

    private void NotifyRemoving(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.RemovingEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second).Invoke(subscriptionInfo.First, Field, item);
      OnRemoving(item);
    }

    private void NotifyRemove(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.RemoveEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, item);
      OnRemove(item);
      NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item);
    }

    private void NotifyClearing()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.ClearingEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnClearing();
    }

    private void NotifyClear()
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

    protected void NotifyPropertyChanged(string name)
    {
      if (!Session.EntityEventBroker.HasSubscribers)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.PropertyChangedEventKey);
      if (subscriptionInfo.Second != null)
        ((PropertyChangedEventHandler) subscriptionInfo.Second)
          .Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void NotifyCollectionChanged(NotifyCollectionChangedAction action, Entity item)
    {
      if (!Session.EntityEventBroker.HasSubscribers)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.CollectionChangedEventKey);
      if (subscriptionInfo.Second != null)
        ((NotifyCollectionChangedEventHandler) subscriptionInfo.Second)
          .Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      NotifyPropertyChanged("Count");
    }

    protected Pair<Key, Delegate> GetSubscription(object eventKey)
    {
      var entityKey = GetOwnerKey(Owner);
      if (entityKey!=null)
        return new Pair<Key, Delegate>(entityKey,
          Session.EntityEventBroker.GetSubscriber(entityKey, Field, eventKey));
      else
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

    private bool Contains(Key key, PersistenceState? state)
    {
      bool containsKey = State.Contains(key);

      // Valid result
      if ((state.HasValue && state == PersistenceState.New) || State.IsFullyLoaded || containsKey)
        return containsKey;

      bool result;
      using (new ParameterContext().Activate()) {
        pKey.Value = GetEntitySetTypeState().SeekTransform
          .Apply(TupleTransformType.TransformedTuple, Owner.Key.Value, key.Value);
        result = GetEntitySetTypeState().SeekRecordSet.FirstOrDefault()!=null;
      }
      if (result)
        State.Register(key);
      return result;
    }

    protected void ValidateVersion(long expectedVersion)
    {
      if (expectedVersion!=State.Version)
        throw Exceptions.CollectionHasBeenChanged(null);
    }

    private static Key GetOwnerKey(Persistent owner)
    {
      var asFieldValueAdapter = owner as IFieldValueAdapter;
      if (asFieldValueAdapter != null)
        return GetOwnerKey(asFieldValueAdapter.Owner);
      return ((Entity) owner).Key;
    }

    protected internal abstract IEnumerable<IEntity> Entities { get; }

    internal EntitySetState UpdateState(IEnumerable<Key> items, bool isFullyLoaded)
    {
      var state = new EntitySetState(1024);
      state.Clear();
      long count = 0;
      foreach (var item in items) {
        state.Register(item);
        count++;
      }
      if (isFullyLoaded)
        state.count = count;
      State = state;
      return State;
    }

    internal EntitySetState GetState()
    {
      if (IsStateLoaded)
        return State;
      return null;
    }

    internal void IntersectWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      if (this == other)
        return;
      var otherEntities = other.Cast<IEntity>().ToHashSet();
      foreach (var item in Entities.ToList())
        if (!otherEntities.Contains(item))
          Remove(item);
    }

    internal void UnionWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      if (this == other)
        return;
      foreach (var item in other)
        Add(item);
    }

    internal void ExceptWith<TElement>(IEnumerable<TElement> other)
      where TElement : IEntity
    {
      if (this == other) {
        Clear();
        return;
      }
      foreach (var item in other)
        Remove(item);
    }

    internal EntitySetTypeState GetEntitySetTypeState()
    {
      return (EntitySetTypeState) Session.Domain.GetCachedItem(
        new Pair<object, FieldInfo>(entitySetCachingRegion, Field), BuildEntitySetTypeState, this);
    }

    private static EntitySetTypeState BuildEntitySetTypeState(object pair, object entitySetObj)
    {
      var field = ((Pair<object, FieldInfo>) pair).Second;
      var entitySet = (EntitySetBase) entitySetObj;
      var seek = field.Association.UnderlyingIndex.ToRecordSet().Seek(() => pKey.Value);
      var seekTransform = new CombineTransform(true,
        field.Association.OwnerType.KeyInfo.TupleDescriptor,
        field.Association.TargetType.KeyInfo.TupleDescriptor);
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
        parameterOwner.Value = owner;
        var cachedState = GetEntitySetTypeState();
        State.count = Query.Execute(cachedState, (Func<int>) cachedState.ItemCountQuery);
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
      if (ctorType == GetType() && !isInitialized) {
        isInitialized = true;
        Initialize();
      }
    }

    /// <summary>
    /// Performs initialization of the <see cref="EntitySetBase"/>.
    /// </summary>
    protected virtual void Initialize()
    {
      NotifyInitialize();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySetBase(Entity owner, FieldInfo field)
    {
      Field = field;
      this.owner = owner;
      Initialize(GetType());
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

    protected internal bool Add(IEntity item)
    {
      return Add((Entity)item);
    }

    protected internal bool Remove(IEntity item)
    {
      return Remove((Entity) item);
    }

    protected bool Contains(IEntity item)
    {
      return Contains((Entity)item);
    }
  }
}