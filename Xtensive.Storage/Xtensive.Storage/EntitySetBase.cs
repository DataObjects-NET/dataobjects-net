// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
using Xtensive.Storage;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using OperationType = Xtensive.Storage.PairIntegrity.OperationType;
using Tuple = Xtensive.Core.Tuples.Tuple;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for <see cref="EntitySet{TItem}"/>.
  /// </summary>
  [EntitySetAspect]
  [Initializable]
  public abstract class EntitySetBase : SessionBound,
    IFieldValueAdapter,
    INotifyPropertyChanged,
    INotifyCollectionChanged
  {
    private static readonly object entitySetCachingRegion = new object();
    private static readonly Parameter<Tuple> keyParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    internal static readonly Parameter<Entity> ownerParameter = new Parameter<Entity>("Owner");

    private readonly Entity owner;
    private readonly CombineTransform auxilaryTypeKeyTransform;
    private bool isInitialized;

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    [Infrastructure]
    public Entity Owner { get { return owner; } }

    /// <inheritdoc/>
    [Infrastructure] // Proxy
    Persistent IFieldValueAdapter.Owner { get { return Owner; } }

    /// <inheritdoc/>
    [Infrastructure]
    public FieldInfo Field { get; private set; }

    internal EntitySetState State { get; private set; }

    /// <summary>
    /// Gets the entities contained in this <see cref="EntitySetBase"/>.
    /// </summary>
    protected internal IEnumerable<IEntity> Entities {
      get {
        return InnerGetEntities().ToTransactional(Session);
      }
    }

    private IEnumerable<IEntity> InnerGetEntities()
    {
      Prefetch();
      foreach (var key in State)
        yield return Query.SingleOrDefault(Session, key);
    }

    /// <summary>
    /// Prefetches the entity set completely - i.e. ensures it is fully loaded.
    /// </summary>
    public void Prefetch()
    {
      Prefetch(null);
    }

    /// <summary>
    /// Prefetches the entity set - i.e. ensures it is either completely or partially loaded.
    /// </summary>
    /// <param name="maxItemCount">The maximal count of items to try to load.</param>
    public void Prefetch(int? maxItemCount)
    {
      EnsureOwnerIsNotRemoved();
      EnsureIsLoaded(maxItemCount);
    }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySetBase"/>.
    /// </summary>
    public long Count {
      get {
        EnsureOwnerIsNotRemoved();
        EnsureIsLoaded(WellKnown.EntitySetPreloadCount);
        EnsureCountIsLoaded();
        return (long) State.TotalItemCount;
      }
    }

    /// <summary>
    /// Clears this collection.
    /// </summary>
    public void Clear()
    {
      EnsureOwnerIsNotRemoved();
      var operations = Session.Operations;
      using (var scope = operations.BeginRegistration(Operations.OperationType.System)) {
        if (operations.CanRegisterOperation)
          operations.RegisterOperation(
            new EntitySetClearOperation(Owner.Key, Field));
        SystemBeforeClear();
        foreach (var entity in Entities.ToList())
          Remove(entity);
        SystemClear();
        scope.Complete();
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

      var entityState = Session.EntityStateCache[key, true];
      if (entityState != null) {
        return Contains(key, entityState.TryGetEntity());
      }
      return Contains(key, null);
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
    protected abstract Func<long> GetItemCountQueryDelegate(FieldInfo field);

    /// <summary>
    /// Ensures the owner is not removed.
    /// </summary>
    protected void EnsureOwnerIsNotRemoved()
    {
      if (Owner.IsRemoved)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }
    
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
      Session.NotifyEntitySetItemRemoved(Owner, this, item);
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
      if (subscriptionInfo.Second != null) {
        var handler = (NotifyCollectionChangedEventHandler) subscriptionInfo.Second;
        if (action==NotifyCollectionChangedAction.Reset)
          handler.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        else
          handler.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
      }
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

    #region Add/Remove/Contains methods

    [Transactional]
    internal bool Contains(Entity item)
    {
      EnsureOwnerIsNotRemoved();
      if (item==null || !Field.ItemType.IsAssignableFrom(item.TypeInfo.UnderlyingType))
        return false;
      return Contains(item.Key, item);
    }

    [Transactional]
    internal bool Add(Entity item)
    {
      return Add(item, null);
    }

    internal bool Add(Entity item, SyncContext syncContext)
    {
      if (Contains(item))
        return false;

      try {
        var operations = Session.Operations;
        using (var scope = operations.BeginRegistration(Operations.OperationType.System)) {
          if (operations.CanRegisterOperation)
            operations.RegisterOperation(new EntitySetItemAddOperation(Owner.Key, Field, item.Key));

          SystemBeforeAdd(item);

          Action finalizer = () => {
            var auxiliaryType = Field.Association.AuxiliaryType;
            if (auxiliaryType!=null && Field.Association.IsMaster) {
              var combinedTuple = auxilaryTypeKeyTransform.Apply(
                TupleTransformType.Tuple,
                Owner.Key.Value,
                item.Key.Value);

              var combinedKey = Key.Create(
                Session.Domain,
                auxiliaryType,
                TypeReferenceAccuracy.ExactType,
                combinedTuple);

              Session.CreateOrInitializeExistingEntity(auxiliaryType.UnderlyingType, combinedKey);
            }

            State.Add(item.Key);
            Owner.UpdateVersionInfo(Owner, Field);
          };
          
          if (Field.Association.IsPaired)
            Session.PairSyncManager.ProcessRecursively(
              syncContext, OperationType.Add, Field.Association, Owner, item, finalizer);
          else
            finalizer.Invoke();

          SystemAdd(item);
          SystemAddCompleted(item, null);
          scope.Complete();
          return true;
        }
      }
      catch(Exception e) {
        SystemAddCompleted(item, e);
        throw;
      }
    }

    [Transactional]
    internal bool Remove(Entity item)
    {
      return Remove(item, null);
    }

    internal bool Remove(Entity item, SyncContext syncContext)
    {
      if (!Contains(item))
        return false;

      try {
        var operations = Session.Operations;
        using (var scope = operations.BeginRegistration(Operations.OperationType.System)) {
          if (operations.CanRegisterOperation)
            operations.RegisterOperation(new EntitySetItemRemoveOperation(Owner.Key, Field, item.Key));

          SystemBeforeRemove(item);

          Action finalizer = () => {
            var auxiliaryType = Field.Association.AuxiliaryType;
            if (auxiliaryType != null && Field.Association.IsMaster) {
              var combinedTuple = auxilaryTypeKeyTransform.Apply(
                TupleTransformType.Tuple,
                Owner.Key.Value,
                item.Key.Value);

              var combinedKey = Key.Create(
                Session.Domain,
                auxiliaryType,
                TypeReferenceAccuracy.ExactType,
                combinedTuple);

              Session.RemoveOrCreateRemovedEntity(auxiliaryType.UnderlyingType, combinedKey);
            }

            State.Remove(item.Key);
            Owner.UpdateVersionInfo(Owner, Field);
          };

          if (Field.Association.IsPaired)
            Session.PairSyncManager.ProcessRecursively(
              syncContext, OperationType.Remove, Field.Association, Owner, item, finalizer);
          else
            finalizer.Invoke();

          SystemRemove(item);
          SystemRemoveCompleted(item, null);
          scope.Complete();
          return true;
        }
      }
      catch(Exception e) {
        SystemRemoveCompleted(item, e);
        throw;
      }
    }
    
    internal bool Add(IEntity item)
    {
      return Add((Entity) item);
    }

    internal bool Remove(IEntity item)
    {
      return Remove((Entity) item);
    }

    internal bool Contains(IEntity item)
    {
      return Contains((Entity) item);
    }

    #endregion

    #region Set operations

    internal void AddRange<TElement>(IEnumerable<TElement> items)
      where TElement: IEntity
    {
      EnsureOwnerIsNotRemoved();
      foreach (var item in items)
        Add(item);
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

    #endregion

    #region Private / internal members

    internal EntitySetState UpdateState(IEnumerable<Key> items, bool isFullyLoaded)
    {
      EnsureOwnerIsNotRemoved();
      var itemList = items.ToList();
      State.Update(itemList, isFullyLoaded ? (long?) itemList.Count : null);
      State.IsLoaded = true;
      return State;
    }

    internal bool CheckStateIsLoaded()
    {
      if (State.IsLoaded)
        return true;
      if (Owner.State.PersistenceState == PersistenceState.New) {
        State.TotalItemCount = State.CachedItemCount;
        State.IsLoaded = true;
        return true;
      }
      return false;
    }

    private void EnsureIsLoaded(int? maxItemCount)
    {
      if (CheckStateIsLoaded()) {
        if (State.IsFullyLoaded)
          return;
        var requestedItemCount = maxItemCount.HasValue
          ? (int) maxItemCount
          : int.MaxValue;
        if (State.CachedItemCount > requestedItemCount)
          return;
      }
      using (Session.Activate())
        Session.Handler.FetchEntitySet(Owner.Key, Field, maxItemCount);
    }

    private void EnsureCountIsLoaded()
    {
      if (State.TotalItemCount!=null)
        return;
      using (new ParameterContext().Activate()) {
        ownerParameter.Value = owner;
        var cachedState = GetEntitySetTypeState();
        State.TotalItemCount = Query.Execute(cachedState, cachedState.ItemCountQuery);
      }
    }

    private bool Contains(Key key, Entity item)
    {
      // state check
      var foundInCache = State.Contains(key);
      if (foundInCache)
        return true;
      var ownerState = Owner.PersistenceState;
      var itemState = item == null 
        ? PersistenceState.Synchronized 
        : item.PersistenceState;
      if (PersistenceState.New.In(ownerState, itemState) || State.IsFullyLoaded)
        return false;

      // association check
      if (item != null) {
        var association = Field.Association;
        if (association.IsPaired && association.Multiplicity.In(Multiplicity.ManyToOne, Multiplicity.OneToMany)) {
          var candidate = (IEntity)item.GetFieldValue(association.Reversed.OwnerField);
          return candidate == Owner;
        }
      }

      // load from storage
      EnsureIsLoaded(WellKnown.EntitySetPreloadCount);

      foundInCache = State.Contains(key);
      if (foundInCache)
        return true;
      if (State.IsFullyLoaded)
        return false;

      bool foundInDatabase;
      using (new ParameterContext().Activate()) {
        keyParameter.Value = GetEntitySetTypeState().SeekTransform
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
      object key = new Pair<object, FieldInfo>(entitySetCachingRegion, Field);
      Func<object, EntitySetBase, object> generator = BuildEntitySetTypeState;
      return (EntitySetTypeState) Session.Domain.Cache.GetValue(key, generator, this);
    }

    private static EntitySetTypeState BuildEntitySetTypeState(object pair, object entitySetObj)
    {
      var field = ((Pair<object, FieldInfo>) pair).Second;
      var entitySet = (EntitySetBase) entitySetObj;
      var seek = field.Association.UnderlyingIndex.ToRecordSet().Seek(() => keyParameter.Value);
      var ownerDescriptor = field.Association.OwnerType.Key.TupleDescriptor;
      var targetDescriptor = field.Association.TargetType.Key.TupleDescriptor;

      var itemColumnOffsets = field.Association.AuxiliaryType == null
        ? field.Association.UnderlyingIndex.ValueColumns
            .Where(ci => ci.IsPrimaryKey)
            .Select(ci => ci.Field.MappingInfo.Offset)
            .ToList()
        : Enumerable.Range(0, targetDescriptor.Count).ToList();

      var keyDescriptor = TupleDescriptor.Create(ownerDescriptor
        .Concat(itemColumnOffsets.Select(i => targetDescriptor[i]))
        .ToList());
      var map = Enumerable.Range(0, ownerDescriptor.Count)
        .Select(i => new Pair<int, int>(0, i))
        .Concat(itemColumnOffsets.Select(i => new Pair<int, int>(1, i)))
        .ToArray();
      var seekTransform = new MapTransform(true, keyDescriptor, map);
      Func<Tuple, Entity> itemCtor = null;
      if (field.Association.AuxiliaryType!=null)
        itemCtor = DelegateHelper.CreateDelegate<Func<Tuple, Entity>>(null,
          field.Association.AuxiliaryType.UnderlyingType, DelegateHelper.AspectedFactoryMethodName,
          ArrayUtils<Type>.EmptyArray);
      return new EntitySetTypeState(seek, seekTransform, itemCtor,entitySet.GetItemCountQueryDelegate(field));
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
      State = new EntitySetState(this);
      if (Field.Association.AuxiliaryType != null && Field.Association.IsMaster) {
        var itemType =  Field.ItemType.GetTypeInfo(Session.Domain);
        auxilaryTypeKeyTransform = new CombineTransform(
          false, 
          owner.TypeInfo.Key.TupleDescriptor, 
          itemType.Key.TupleDescriptor);
      }
      Initialize(typeof(EntitySetBase));
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
