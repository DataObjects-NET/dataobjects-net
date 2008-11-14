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
using Xtensive.Core;
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
  public abstract class EntitySetBase : TransactionalStateContainer<EntitySetState>,
    IFieldHandler,
    INotifyPropertyChanged,
    INotifyCollectionChanged
  {
    private const int                                 CacheSize = 10240;
    private static readonly Parameter<Tuple>          pKey = new Parameter<Tuple>("Key");

    private AssociationInfo                  association;
    private TypeInfo                                  underlyingType;
    private Func<Tuple, Entity>                       itemConstructor;
    private RecordSet                                 count;
    private RecordSet                                 seek;
    private Func<Tuple, Tuple>                        extractKey;
    private SegmentTransform                          extractKeyTransform;
    private Func<Tuple, Tuple>                        combineKey;
    private CombineTransform                          combineKeyTransform;

    internal RecordSet Items { get; private set; }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySetBase"/>.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the <see cref="EntitySetBase"/>.
    /// </returns>
    public long Count
    {
      [DebuggerStepThrough]
      get { return State.Count; }
    }

    /// <summary>
    /// Determines whether <see cref="EntitySetBase"/> contains the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    /// <see langword="true"/> if <see cref="EntitySetBase"/> contains the specified item; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Entity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Contains(item.Key);
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
      if (!Field.ValueType.IsAssignableFrom(key.Type.UnderlyingType))
        throw new InvalidOperationException(string.Format("Entity type {0} is not supported by this instance.", key.Type.Name));

      if (State.Contains(key))
        return true;

      bool result;
      using (new ParameterScope()) {
        pKey.Value = combineKey(key.Value);
        result = seek.FirstOrDefault() != null;
      }
      if (result)
        State.Register(key);
      return result;
    }

    /// <summary>
    /// Gets the keys.
    /// </summary>
    /// <returns>The <see cref="IEnumerable{Key}"/> collection of <see cref="Key"/> instances.</returns>
    public IEnumerable<Key> GetKeys()
    {
      long version = State.Version;
      bool isCached = State.IsFullyLoaded;
      IEnumerable<Key> keys = isCached ? State : FetchKeys();

      foreach (Key key in keys) {
        EnsureVersionIs(version);
        if (!isCached)
          State.Register(key);
        yield return key;
      }
    }

    #region Initialization members

    internal void Initialize(bool notify)
    {
      association = Field.Association;
      if (association.UnderlyingType != null) {
        underlyingType = association.UnderlyingType;
        itemConstructor = DelegateHelper.CreateDelegate<Func<Tuple, Entity>>(null, underlyingType.UnderlyingType, DelegateHelper.AspectedProtectedConstructorCallerName, ArrayUtils<Type>.EmptyArray);
      }
      IndexInfo index = GetIndex();
      Items = index.ToRecordSet().Range(ConcreteOwner.Key.Value, ConcreteOwner.Key.Value);
      seek = index.ToRecordSet().Seek(() => pKey.Value);
      count = Items.Aggregate(null, new AggregateColumnDescriptor("$Count", 0, AggregateType.Count));
      extractKey = BuildExtractKey(index);
      combineKey = BuildCombineKey(index);
      OnInitialize(notify);
    }

    private IndexInfo GetIndex()
    {
      if (underlyingType == null) {
        FieldInfo referencingField = association.Reversed.ReferencingField;
        return referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
      }
      if (association.IsMaster)
        return underlyingType.Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
      return underlyingType.Indexes.Where(indexInfo => indexInfo.IsSecondary).First();
    }

    private Func<Tuple, Tuple> BuildExtractKey(IndexInfo index)
    {
      Segment<int> foreignKeySegment = new Segment<int>(association.ReferencingType.Hierarchy.MappingInfo.Length, association.ReferencedType.Hierarchy.MappingInfo.Length);
      extractKeyTransform = new SegmentTransform(true, index.TupleDescriptor, foreignKeySegment);
      return tuple => extractKeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
    }

    private Func<Tuple, Tuple> BuildCombineKey(IndexInfo index)
    {
      if (underlyingType == null) {
        combineKeyTransform = new CombineTransform(true, association.ReferencedType.Hierarchy.KeyTupleDescriptor, association.ReferencingType.Hierarchy.KeyTupleDescriptor);
        return tuple => combineKeyTransform.Apply(TupleTransformType.TransformedTuple, ConcreteOwner.Key.Value, tuple);
      }
      combineKeyTransform = new CombineTransform(true, association.ReferencingType.Hierarchy.KeyTupleDescriptor, association.ReferencedType.Hierarchy.KeyTupleDescriptor);
      return tuple => combineKeyTransform.Apply(TupleTransformType.TransformedTuple, ConcreteOwner.Key.Value, tuple);
    }

    #endregion

    #region Internal Add, Remove, Clear members

    internal bool Add(Entity item, bool notify)
    {
      OnAdding(item, notify);

      if (Contains(item))
        return false;

      if (association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Add, ConcreteOwner, item, association);

      if (underlyingType!=null && association.IsMaster)
        itemConstructor(item.Key.Value.Combine(ConcreteOwner.Key.Value));

      State.Add(item.Key);
      OnAdd(item, notify);

      return true;
    }

    internal bool Remove(Entity item, bool notify)
    {
      OnRemoving(item, notify);

      if (!Contains(item))
        return false;

      if (association.IsPaired)
        Session.PairSyncManager.Enlist(OperationType.Remove, ConcreteOwner, item, association);

      if (underlyingType!=null && association.IsMaster) {
        var combinedKey = Key.Create(underlyingType, item.Key.Value.Combine(ConcreteOwner.Key.Value));
        Entity underlyingItem = combinedKey.Resolve();
        underlyingItem.Remove();
      }

      State.Remove(item.Key);
      OnRemove(item, notify);

      return true;
    }

    internal void Clear(bool notify)
    {
      OnClearing(notify);
      foreach (Key key in GetKeys())
        Remove(key.Resolve(), notify);
      OnClear(notify);
    }

    #endregion

    /// <inheritdoc/>
    protected sealed override EntitySetState LoadState()
    {
      return new EntitySetState(CacheSize, () => count.First().GetValue<long>(0));
    }

    private Entity ConcreteOwner
    {
      get { return (Entity) Owner; }
    }

    private void EnsureVersionIs(long expectedVersion)
    {
      if (expectedVersion!=State.Version)
        Exceptions.CollectionHasBeenChanged(null);
    }

    private IEnumerable<Key> FetchKeys()
    {
      foreach (Tuple tuple in Items)
        yield return Key.Create(Field.ValueType, extractKey(tuple));
    }

    #region System-level events

    private void OnInitialize(bool notify)
    {
      if (!notify)
        return;
      OnInitialize();
    }

    private void OnAdding(Entity item, bool notify)
    {
      if (!notify)
        return;
      OnAdding(item);
    }

    private void OnAdd(Entity item, bool notify)
    {
      if (!notify)
        return;
      OnAdd(item);
      OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
    }

    private void OnRemoving(Entity item, bool notify)
    {
      if (!notify)
        return;
      OnRemoving(item);
    }

    private void OnRemove(Entity item, bool notify)
    {
      if (!notify)
        return;
      OnRemove(item);
      OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
    }

    private void OnClearing(bool notify)
    {
      if (!notify)
        return;
      OnClearing();
    }

    private void OnClear(bool notify)
    {
      if (!notify)
        return;
      OnClear();
      OnCollectionChanged(NotifyCollectionChangedAction.Reset, null);
    }

    #endregion

    #region User-level events

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

    #region IFieldHandler members

    /// <inheritdoc/>
    public Persistent Owner { get; private set; }

    /// <inheritdoc/>
    public FieldInfo Field { get; private set; }

    #endregion

    #region INotifyPropertyChanged members

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      if (PropertyChanged!=null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    #endregion

    #region INotifyCollectionChanged members

    /// <summary>
    /// Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, Entity item)
    {
      if (CollectionChanged==null)
        return;

      CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      OnPropertyChanged("Count");
    }

    #endregion


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    /// <param name="notify">If set to <see langword="true"/>, 
    /// initialization related events will be raised.</param>
    protected EntitySetBase(Persistent owner, FieldInfo field, bool notify)
    {
      Field = field;
      Owner = owner;
      Initialize(notify);
    }
  }
}