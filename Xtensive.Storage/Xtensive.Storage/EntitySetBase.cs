// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
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
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for <see cref="EntitySet{TItem}"/>.
  /// </summary>
  public abstract class EntitySetBase : TransactionalStateContainer<EntitySetState>,
    IFieldValueAdapter,
    INotifyPropertyChanged,
    INotifyCollectionChanged
  {
    private static readonly Parameter<Tuple> pKey = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private RecordSet items;
    internal RecordSet count;
    private RecordSet seek;
    private Func<Tuple, Entity> itemCtor;
    private AssociationInfo association;
    private CombineTransform seekTransform;
    private bool isInitialized;


    #region Public Count, Contains members

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySetBase"/>.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the <see cref="EntitySetBase"/>.
    /// </returns>
    [Infrastructure]
    public long Count
    {
      [DebuggerStepThrough]
      get { return State.Count; }
    }

    /// <summary>
    /// Determines whether <see cref="EntitySetBase"/> contains the specified <see cref="Key"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/> if <see cref="EntitySetBase"/> contains the specified <see cref="Key"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Entity type is not supported.</exception>
    [Infrastructure]
    public bool Contains(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      if (!Field.ItemType.IsAssignableFrom(key.Type.UnderlyingType))
        throw new InvalidOperationException(string.Format("Entity type {0} is not supported by this instance.", key.Type.Name));

      if (State.IsFullyLoaded)
        return State.Contains(key);

      if (State.Contains(key))
        return true;
      
      bool result;
      using (new ParameterContext().Activate()) {
        pKey.Value = seekTransform.Apply(TupleTransformType.TransformedTuple, ConcreteOwner.Key.Value, key.Value);
        result = seek.FirstOrDefault() != null;
      }
      if (result)
        State.Register(key);
      return result;
    }
    #endregion

    #region Initialization members

    /// <summary>
    /// Performs initialization (see <see cref="Initialize()"/>) of the <see cref="EntitySetBase"/> 
    /// if type of <see langword="this" /> is the same as <paramref name="ctorType"/>.
    /// Invoked by <see cref="InitializableAttribute"/> aspect in the epilogue of any 
    /// constructor of this type and its ancestors.
    /// </summary>
    /// <param name="ctorType">The type, which constructor has invoked this method.</param>
    [Infrastructure]
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
    [Infrastructure]
    protected virtual void Initialize()
    {
      association = Field.Association;
      if (association.AuxiliaryType!=null)
        itemCtor = DelegateHelper.CreateDelegate<Func<Tuple, Entity>>(null, association.AuxiliaryType.UnderlyingType, DelegateHelper.AspectedProtectedConstructorCallerName, ArrayUtils<Type>.EmptyArray);
      items = association.UnderlyingIndex.ToRecordSet().Range(ConcreteOwner.Key.Value, ConcreteOwner.Key.Value);
      seek = association.UnderlyingIndex.ToRecordSet().Seek(() => pKey.Value);
      count = items.Aggregate(null, new AggregateColumnDescriptor("$Count", 0, AggregateType.Count));

      seekTransform = new CombineTransform(true, association.OwnerType.Hierarchy.KeyInfo.TupleDescriptor, association.TargetType.Hierarchy.KeyInfo.TupleDescriptor);
      NotifyInitialize();
    }

    #endregion

    #region Internal Contains, Add, Remove, Clear, IntersectWith, ExceptWith, UnionWith members

    internal bool Contains(Entity item)
    {
      return Contains(item.Key);
    }

    internal bool Add(Entity item)
    {
      if (Contains(item))
        return false;

      NotifyAdding(item);

      if (association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Add, ConcreteOwner, item, association);

      if (association.AuxiliaryType!=null && association.IsMaster)
        itemCtor(item.Key.Value.Combine(ConcreteOwner.Key.Value));

      State.Add(item.Key);
      NotifyAdd(item);

      return true;
    }

    internal bool Remove(Entity item)
    {
      if (!Contains(item))
        return false;

      NotifyRemoving(item);

      if (association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Remove, ConcreteOwner, item, association);

      if (association.AuxiliaryType!=null && association.IsMaster) {
        var combinedKey = Key.Create(association.AuxiliaryType, item.Key.Value.Combine(ConcreteOwner.Key.Value));
        Entity underlyingItem = Query.SingleOrDefault(combinedKey);
        underlyingItem.Remove();
      }

      State.Remove(item.Key);
      NotifyRemove(item);

      return true;
    }

    internal void Clear()
    {
      NotifyClearing();
      foreach (var entity in GetEntities().ToList())
        Remove(entity);
      NotifyClear();
    }

    internal void IntersectWith<TElement>(IEnumerable<TElement> other)
      where TElement : Entity
    {
      if (this == other)
        return;
      var otherEntities = other.Cast<Entity>().ToHashSet();
      foreach (var item in GetEntities().ToList())
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

    #region Private members

    private Entity ConcreteOwner
    {
      get { return (Entity) Owner; }
    }

    internal void EnsureVersionIs(long expectedVersion)
    {
      if (expectedVersion!=State.Version)
        throw Exceptions.CollectionHasBeenChanged(null);
    }

    protected internal abstract IEnumerable<Entity> GetEntities();

    #endregion

    #region System-level events

    private void NotifyInitialize()
    {
      if (Session.SystemLogicOnly)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.InitializeEntitySetEventKey, out entityKey, out subscriber);
      if (subscriber!=null)
        ((Action<Key, FieldInfo>) subscriber).Invoke(entityKey, Field);
      OnInitialize();
    }

    private void NotifyAdding(Entity item)
    {
      if (Session.SystemLogicOnly)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.AddingEntitySetItemEventKey, out entityKey, out subscriber);
      if (subscriber!=null)
        ((Action<Key, FieldInfo, Entity>) subscriber).Invoke(entityKey, Field, item);
      OnAdding(item);
    }

    private void NotifyAdd(Entity item)
    {
      if (Session.SystemLogicOnly)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.AddEntitySetItemEventKey, out entityKey, out subscriber);
      if (subscriber!=null)
        ((Action<Key, FieldInfo, Entity>) subscriber).Invoke(entityKey, Field, item);
      OnAdd(item);
      NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item);
    }

    private void NotifyRemoving(Entity item)
    {
      if (Session.SystemLogicOnly)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.RemovingEntitySetItemEventKey, out entityKey, out subscriber);
      if (subscriber!=null)
        ((Action<Key, FieldInfo, Entity>) subscriber).Invoke(entityKey, Field, item);
      OnRemoving(item);
    }

    private void NotifyRemove(Entity item)
    {
      if (Session.SystemLogicOnly)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.RemoveEntitySetItemEventKey, out entityKey, out subscriber);
      if (subscriber!=null)
        ((Action<Key, FieldInfo, Entity>) subscriber).Invoke(entityKey, Field, item);
      OnRemove(item);
      NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item);
    }

    private void NotifyClearing()
    {
      if (Session.SystemLogicOnly)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.ClearingEntitySetEventKey, out entityKey, out subscriber);
      if (subscriber!=null)
        ((Action<Key, FieldInfo>) subscriber).Invoke(entityKey, Field);
      OnClearing();
    }

    private void NotifyClear()
    {
      if (Session.SystemLogicOnly)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.ClearEntitySetEventKey, out entityKey, out subscriber);
      if (subscriber!=null)
        ((Action<Key, FieldInfo>) subscriber).Invoke(entityKey, Field);
      OnClear();
      NotifyCollectionChanged(NotifyCollectionChangedAction.Reset, null);
    }

    [Infrastructure]
    private void GetSubscription(object eventKey, out Key entityKey,
      out Delegate subscriber)
    {
      entityKey = FindOwnerEntityKey(Owner);
      subscriber = Session.EntityEvents.GetSubscriber(entityKey, Field, eventKey);
    }

    private static Key FindOwnerEntityKey(Persistent owner)
    {
      var asFieldValueAdapter = owner as IFieldValueAdapter;
      if(asFieldValueAdapter != null)
        return FindOwnerEntityKey(asFieldValueAdapter.Owner);
      return ((Entity) owner).Key;
    }

    #endregion

    #region User-level events

    [Infrastructure]
    protected virtual void OnInitialize()
    {
    }

    [Infrastructure]
    protected virtual void OnAdding(Entity item)
    {
    }

    [Infrastructure]
    protected virtual void OnAdd(Entity item)
    {
    }

    [Infrastructure]
    protected virtual void OnRemoving(Entity item)
    {
    }

    [Infrastructure]
    protected virtual void OnRemove(Entity item)
    {
    }

    [Infrastructure]
    protected virtual void OnClearing()
    {
    }

    [Infrastructure]
    protected virtual void OnClear()
    {
    }

    #endregion

    #region IFieldValueAdapter members

    /// <inheritdoc/>
    [Infrastructure]
    public Persistent Owner { get; private set; }

    /// <inheritdoc/>
    [Infrastructure]
    public FieldInfo Field { get; private set; }

    #endregion

    #region INotifyPropertyChanged members

    /// <inheritdoc/>
    [Infrastructure]
    public event PropertyChangedEventHandler PropertyChanged {
      add {
        Session.EntityEvents.AddSubscriber(FindOwnerEntityKey(Owner), Field,
        EntityEventManager.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(FindOwnerEntityKey(Owner), Field,
        EntityEventManager.PropertyChangedEventKey, value);
      }
    }

    protected void NotifyPropertyChanged(string name)
    {
      if(!Session.EntityEvents.HasSubscribers)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.PropertyChangedEventKey, out entityKey, out subscriber);
      if(subscriber != null)
        ((PropertyChangedEventHandler)subscriber).Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion

    #region INotifyCollectionChanged members

    /// <summary>
    /// Occurs when the collection changes.
    /// </summary>
    [Infrastructure]
    public event NotifyCollectionChangedEventHandler CollectionChanged{
      add {
        Session.EntityEvents.AddSubscriber(FindOwnerEntityKey(Owner), Field,
        EntityEventManager.CollectionChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(FindOwnerEntityKey(Owner), Field,
        EntityEventManager.CollectionChangedEventKey, value);
      }
    }

    private void NotifyCollectionChanged(NotifyCollectionChangedAction action, Entity item)
    {
      if(!Session.EntityEvents.HasSubscribers)
        return;
      Key entityKey;
      Delegate subscriber;
      GetSubscription(EntityEventManager.PropertyChangedEventKey, out entityKey, out subscriber);
      if(subscriber != null)
        ((NotifyCollectionChangedEventHandler)subscriber)
          .Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      NotifyPropertyChanged("Count");
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySetBase(Entity owner, FieldInfo field)
    {
      Field = field;
      Owner = owner;
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