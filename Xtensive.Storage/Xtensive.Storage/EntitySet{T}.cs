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
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public class EntitySet<T> : EntitySet,
    ICollection<T>,
    INotifyPropertyChanged,
    INotifyCollectionChanged
    where T : Entity
  {
    private long? count;
    private long version;
    protected RecordSet RecordSet { get; private set; }
    protected IndexInfo Index { get; private set; }
    protected MapTransform KeyExtractTransform { get; private set; }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the <see cref="EntitySet{T}"/>.
    /// </returns>
    public long Count {
      [DebuggerStepThrough]
      get {
        EnsureStateIsConsistent();
        return count.Value;
      }
    }

    /// <inheritdoc/>
    public virtual bool Contains(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureStateIsConsistent();
      if (State.Contains(item.Key))
        return true;
      if (Xxx())
        return false;
      FieldInfo referencingField = Field.Association.Reversed.ReferencingField;

      if (item.GetReference(referencingField) == Entity.Key) {
        State.Add(item.Key);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public bool Contains(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      EnsureStateIsConsistent();
      if (!typeof(T).IsAssignableFrom(key.Type.UnderlyingType))
        return false;
      if (State.Contains(key))
        return true;
      if (Xxx())
        return false;
      // Request from database
      Tuple filterTuple = new CombineTransform(true, Entity.Key.Descriptor, key.Descriptor).Apply(TupleTransformType.Tuple, Entity.Key, key);
      if (RecordSet.Range(filterTuple, filterTuple).Count() > 0)
        State.Add(key);
      else
        return false;
      return true;
    }

    /// <inheritdoc/>
    public virtual bool Add(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureStateIsConsistent();

      if (Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Add, Entity, item, association);

      OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
      return true;
    }

    /// <inheritdoc/>
    public virtual bool Remove(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureStateIsConsistent();

      if (!Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Remove, Entity, item, association);

      OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
      return true;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      EnsureStateIsConsistent();
      foreach (T item in this.ToList())
        Remove(item);
      OnCollectionChanged(NotifyCollectionChangedAction.Reset, null);
    }

    /// <inheritdoc/>
    public int RemoveWhere(Predicate<T> match)
    {
      EnsureStateIsConsistent();
      var itemsToRemove = new List<T>();
      foreach (T item in this)
        if (match(item))
          itemsToRemove.Add(item);
      foreach (T itemToRemove in itemsToRemove)
        Remove(itemToRemove);
      return itemsToRemove.Count;
    }

    #region Other ICollection<T> members

    /// <inheritdoc/>
    void ICollection<T>.Add(T item)
    {
      Add(item);
    }

    /// <inheritdoc/>
    int ICollection<T>.Count {
      [DebuggerStepThrough]
      get { return checked ((int) Count); }
    }

    /// <inheritdoc/>
    public bool IsReadOnly {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      EnsureStateIsConsistent();
      long initialVersion = version;
      if (Xxx()) {
        foreach (Key key in State) {
          CheckVersion(initialVersion);
          yield return key.Resolve<T>();
        }
      }
      else {
        foreach (Tuple tuple in RecordSet) {
          CheckVersion(initialVersion);
          var key = Key.Get(typeof (T), KeyExtractTransform.Apply(TupleTransformType.TransformedTuple, tuple));
          yield return key.Resolve<T>();
        }
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
      foreach (T item in this)
        array[arrayIndex++] = item;
    }

    #endregion

    #region Protected and Internal

    internal override bool Add(Entity item)
    {
      return Add((T)item);
    }

    internal override bool Remove(Entity item)
    {
      return Remove((T) item);
    }

    internal protected void EnsureStateIsConsistent()
    {
      Transaction current = Transaction.Current;
      if (current==null || current.State!=TransactionState.Active)
        throw new InvalidOperationException(Strings.ExEntitySetInvalidBecauseTransactionIsNotActive);

      if (!State.IsConsistent(current)) {
        State.Reset(current);
        count = RecordSet.Count();
        IncreaseVersion();
      }
    }

    protected void CheckVersion(long current)
    {
      if (version!=current)
        Exceptions.CollectionHasBeenChanged(null);
    }

    #endregion

    #region Private methods

    private void IncreaseVersion()
    {
      unchecked {
        Interlocked.Increment(ref version);
      }
    }

    private bool Xxx()
    {
      return count==State.Count;
    }

    #endregion

    #region Initialization members

    protected internal override void Initialize()
    {
      Index = GetIndex();
      RecordSet = GetRecordSet();
      KeyExtractTransform = GetKeyExtractTransform();
    }

    protected virtual MapTransform GetKeyExtractTransform()
    {
      var keyTupleDescriptor = Owner.Session.Domain.Model.Types[typeof (T)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = Index.Columns.Where(columnInfo => columnInfo.IsPrimaryKey).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    protected virtual IndexInfo GetIndex()
    {
      FieldInfo referencingField = Field.Association.Reversed.ReferencingField;
      return referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
    }

    protected virtual RecordSet GetRecordSet()
    {
      return Index.ToRecordSet().Range(Entity.Key, Entity.Key);
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

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, Entity item)
    {
      switch (action) {
      case NotifyCollectionChangedAction.Add:
        count++;
        State.Add(item.Key);
        break;
      case NotifyCollectionChangedAction.Remove:
        count--;
        State.Remove(item.Key);
        break;
      case NotifyCollectionChangedAction.Reset:
        count = null;
        State.Clear();
        break;
      }
      IncreaseVersion();

      if (CollectionChanged != null) {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
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