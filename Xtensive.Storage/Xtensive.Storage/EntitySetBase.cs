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
    private static readonly Parameter<Tuple> pKey = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private readonly Entity owner;
    private RecordSet rsItems;
    private RecordSet rsSeek;
    internal RecordSet rsCount;
    private Func<Tuple, Entity> itemCtor;
    private AssociationInfo association;
    private CombineTransform seekTransform;
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
      get { return State.Count; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        Session.EntityEvents.AddSubscriber(GetOwnerKey(Owner), Field,
        EntityEventManager.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(GetOwnerKey(Owner), Field,
        EntityEventManager.PropertyChangedEventKey, value);
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    public event NotifyCollectionChangedEventHandler CollectionChanged {
      add {
        Session.EntityEvents.AddSubscriber(GetOwnerKey(Owner), Field,
        EntityEventManager.CollectionChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(GetOwnerKey(Owner), Field,
        EntityEventManager.CollectionChangedEventKey, value);
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
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      var type = key.Type;
      if (!Field.ItemType.IsAssignableFrom(type.UnderlyingType))
        throw new InvalidOperationException(string.Format(
          Strings.ExEntityOfTypeXIsIncompatibleWithThisEntitySet, type.UnderlyingType.GetShortName()));

      if (State.IsFullyLoaded)
        return State.Contains(key);

      if (State.Contains(key))
        return true;
      
      bool result;
      using (new ParameterContext().Activate()) {
        pKey.Value = seekTransform.Apply(TupleTransformType.TransformedTuple, Owner.Key.Value, key.Value);
        result = rsSeek.FirstOrDefault() != null;
      }
      if (result)
        State.Register(key);
      return result;
    }

    /// <summary>
    /// Determines whether this collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for containment.</param>
    /// <returns>
    /// <see langword="true"/> if this collection contains the specified item; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [Infrastructure] // Proxy
    public bool Contains(Entity item)
    {
      return Contains(item.Key);
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
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (Contains(item))
        return false;

      NotifyAdding(item);

      if (association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Add, Owner, item, association);

      if (association.AuxiliaryType!=null && association.IsMaster)
        itemCtor(item.Key.Value.Combine(Owner.Key.Value));

      State.Add(item.Key);
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
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (!Contains(item))
        return false;

      NotifyRemoving(item);

      if (association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Remove, Owner, item, association);

      if (association.AuxiliaryType!=null && association.IsMaster) {
        var combinedKey = Key.Create(association.AuxiliaryType, item.Key.Value.Combine(Owner.Key.Value));
        Entity underlyingItem = Query.SingleOrDefault(Session, combinedKey);
        underlyingItem.Remove();
      }

      State.Remove(item.Key);
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

    #region NotifyXxx & GetSubscription members

    private void NotifyInitialize()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.InitializeEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnInitialize();
    }

    private void NotifyAdding(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.AddingEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field, item);
      OnAdding(item);
    }

    private void NotifyAdd(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.AddEntitySetItemEventKey);
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
      var subscriptionInfo = GetSubscription(EntityEventManager.RemovingEntitySetItemEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, Entity>) subscriptionInfo.Second).Invoke(subscriptionInfo.First, Field, item);
      OnRemoving(item);
    }

    private void NotifyRemove(Entity item)
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.RemoveEntitySetItemEventKey);
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
      var subscriptionInfo = GetSubscription(EntityEventManager.ClearingEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnClearing();
    }

    private void NotifyClear()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.ClearEntitySetEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, Field);
      OnClear();
      NotifyCollectionChanged(NotifyCollectionChangedAction.Reset, null);
    }

    protected void NotifyPropertyChanged(string name)
    {
      if (!Session.EntityEvents.HasSubscribers)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.PropertyChangedEventKey);
      if (subscriptionInfo.Second != null)
        ((PropertyChangedEventHandler) subscriptionInfo.Second)
          .Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void NotifyCollectionChanged(NotifyCollectionChangedAction action, Entity item)
    {
      if (!Session.EntityEvents.HasSubscribers)
        return;
      var subscriptionInfo = GetSubscription(EntityEventManager.PropertyChangedEventKey);
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
          Session.EntityEvents.GetSubscriber(entityKey, Field, eventKey));
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

    protected internal abstract IEnumerable<Entity> Entities { get; }

    internal void IntersectWith<TElement>(IEnumerable<TElement> other)
      where TElement : Entity
    {
      if (this == other)
        return;
      var otherEntities = other.Cast<Entity>().ToHashSet();
      foreach (var item in Entities.ToList())
        if (!otherEntities.Contains(item))
          Remove(item);
    }

    internal void UnionWith<TElement>(IEnumerable<TElement> other)
      where TElement : Entity
    {
      if (this == other)
        return;
      foreach (var item in other)
        Add(item);
    }

    internal void ExceptWith<TElement>(IEnumerable<TElement> other)
      where TElement : Entity
    {
      if (this == other) {
        Clear();
        return;
      }
      foreach (var item in other)
        Remove(item);
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
      association = Field.Association;
      if (association.AuxiliaryType!=null)
        itemCtor = DelegateHelper.CreateDelegate<Func<Tuple, Entity>>(null, association.AuxiliaryType.UnderlyingType, DelegateHelper.AspectedProtectedConstructorCallerName, ArrayUtils<Type>.EmptyArray);
      rsItems = association.UnderlyingIndex.ToRecordSet().Range(Owner.Key.Value, Owner.Key.Value);
      rsSeek = association.UnderlyingIndex.ToRecordSet().Seek(() => pKey.Value);
      rsCount = rsItems.Aggregate(null, new AggregateColumnDescriptor("$Count", 0, AggregateType.Count));

      seekTransform = new CombineTransform(true, association.OwnerType.Hierarchy.KeyInfo.TupleDescriptor, association.TargetType.Hierarchy.KeyInfo.TupleDescriptor);
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
  }
}