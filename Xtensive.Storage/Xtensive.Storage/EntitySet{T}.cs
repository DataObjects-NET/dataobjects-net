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
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public class EntitySet<TItem> : EntitySet,
    ICollection<TItem>,
    INotifyPropertyChanged,
    INotifyCollectionChanged
    where TItem : Entity
  {
    protected MapTransform KeyExtractTransform { get; private set; }
    protected CombineTransform KeyFilterTransform { get; private set; }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the <see cref="EntitySet{T}"/>.
    /// </returns>
    public long Count {
      [DebuggerStepThrough]
      get {
        return State.Count;
      }
    }

    /// <inheritdoc/>
    public virtual bool Contains(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (State.Contains(item.Key))
        return true;

      FieldInfo referencingField = Field.Association.Reversed.ReferencingField;
      if (item.GetKey(referencingField) == ConcreteOwner.Key) {
        State.Cache(item.Key);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public bool Contains(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      if (!typeof(TItem).IsAssignableFrom(key.Type.UnderlyingType))
        return false;

      if (State.Contains(key))
        return true;

      Tuple filterTuple = KeyFilterTransform.Apply(TupleTransformType.Tuple, ConcreteOwner.Key, key);
      if (RecordSet.Seek(filterTuple).Count() > 0) {
        State.Add(key);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public virtual bool Add(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Add, ConcreteOwner, item, association);

      OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
      return true;
    }

    /// <inheritdoc/>
    public virtual bool Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (!Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Remove, ConcreteOwner, item, association);

      OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
      return true;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      foreach (TItem item in this.ToList())
        Remove(item);
      OnCollectionChanged(NotifyCollectionChangedAction.Reset, null);
    }

    public int RemoveWhere(Predicate<TItem> criteria)
    {
      var items = this.Where(i => criteria(i)).ToList();
      foreach (TItem item in items)
        Remove(item);
      return items.Count;
    }

    #region Other ICollection<T> members

    /// <inheritdoc/>
    void ICollection<TItem>.Add(TItem item)
    {
      Add(item);
    }

    /// <inheritdoc/>
    int ICollection<TItem>.Count {
      [DebuggerStepThrough]
      get { return checked ((int) Count); }
    }

    /// <inheritdoc/>
    public bool IsReadOnly {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      foreach (Key key in GetKeys())
        yield return key.Resolve<TItem>();
    }

    /// <summary>
    /// Gets the keys.
    /// </summary>
    /// <returns>The <see cref="IEnumerable{Key}"/> collection of <see cref="Key"/> instances.</returns>
    public IEnumerable<Key> GetKeys()
    {
      long version = State.Version;
      if (State.IsFullyLoaded) {
        foreach (Key key in State) {
          EnsureVersionIs(version);
          yield return key;
        }
      }
      else {
        foreach (Tuple tuple in RecordSet) {
          EnsureVersionIs(version);
          var key = Key.Create<TItem>(KeyExtractTransform.Apply(TupleTransformType.TransformedTuple, tuple), true);
          State.Cache(key);
          yield return key;
        }
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public void CopyTo(TItem[] array, int arrayIndex)
    {
      foreach (TItem item in this)
        array[arrayIndex++] = item;
    }

    #endregion

    #region Protected and Internal

    internal override bool Add(Entity item)
    {
      return Add((TItem)item);
    }

    internal override bool Remove(Entity item)
    {
      return Remove((TItem) item);
    }

    protected void EnsureVersionIs(long expectedVersion)
    {
      if (expectedVersion!=State.Version)
        Exceptions.CollectionHasBeenChanged(null);
    }

    #endregion

    #region Initialization members

    protected internal override void Initialize()
    {
      base.Initialize();
      KeyExtractTransform = GetKeyExtractTransform();
      KeyFilterTransform = GetKeyFilterTransform();
    }

    protected virtual CombineTransform GetKeyFilterTransform()
    {
      HierarchyInfo hi = Session.Domain.Model.Types[typeof (TItem)].Hierarchy;
      return new CombineTransform(true, ConcreteOwner.Key.Value.Descriptor, hi.KeyTupleDescriptor);
    }

    protected virtual MapTransform GetKeyExtractTransform()
    {
      var keyTupleDescriptor = Owner.Session.Domain.Model.Types[typeof (TItem)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = Index.Columns.Where(columnInfo => columnInfo.IsPrimaryKey).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    protected override IndexInfo GetIndex()
    {
      FieldInfo referencingField = Field.Association.Reversed.ReferencingField;
      return referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
    }

    protected override RecordSet GetRecordSet()
    {
      return Index.ToRecordSet().Range(ConcreteOwner.Key.Value, ConcreteOwner.Key.Value);
    }

    #endregion

    #region INotifyPropertyChanged members

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null)
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
      switch (action) {
      case NotifyCollectionChangedAction.Add:
        State.Add(item.Key);
        break;
      case NotifyCollectionChangedAction.Remove:
        State.Remove(item.Key);
        break;
      case NotifyCollectionChangedAction.Reset:
        State.Clear();
        break;
      }

      if (CollectionChanged != null) {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged("Count");
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    public EntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}