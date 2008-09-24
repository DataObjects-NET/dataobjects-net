// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public class EntitySet<T> : EntitySet,
    ICollection<T>
    where T : Entity
  {
    /// <summary>
    /// Maximum items in <see cref="EntitySet{T}"/> cache.
    /// </summary>
    internal static int MaximumCacheSize = 1000;

    private Transaction transaction;
    private long? count;
    private long version;
    private IndexInfo index;
    private MapTransform keyTransform;

    /// <inheritdoc/>
    public long Count
    {
      get
      {
        EnsureInitialized();
        return count.Value;
      }
    }

    /// <inheritdoc/>
    int ICollection<T>.Count
    {
      get { return checked((int)Count); }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return false; }
    }

    /// <inheritdoc/>
    public virtual bool Contains(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureInitialized();
      if (Cache.Contains(item.Key))
        return true;
      if (Cache.Count==Count)
        return false;
      FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
      var accessor = referencingField.GetAccessor<Entity>();
      bool result = ReferenceEquals(accessor.GetValue(item, referencingField), Owner);
      if (result)
        Cache.Add(new CachedKey(item.Key));
      return result;
    }

    /// <inheritdoc/>
    public bool Contains(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      EnsureInitialized();
      if (key.Type.UnderlyingType!=typeof (T)) {
        return false;
      }
      if (!Cache.Contains(key)) {
        if (Cache.Count==Count)
          return false;
        // Request from database
        Tuple filterTuple = new CombineTransform(true, ((Entity) Owner).Key.Descriptor, key.Descriptor).Apply(TupleTransformType.Tuple, ((Entity) Owner).Key, key);
        if (GetRecordSet().Range(filterTuple, filterTuple).Count() > 0) {
          Cache.Add(new CachedKey(key));
        }
        else {
          return false;
        }
      }
      return true;
    }

    /// <inheritdoc/>
    public virtual bool Add(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureInitialized();
      FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
      var accessor = referencingField.GetAccessor<Entity>();
      long previouseCount = count.Value;
      accessor.SetValue(item, referencingField, (Entity) Owner);
      return previouseCount!=count;
    }

    /// <inheritdoc/>
    void ICollection<T>.Add(T item)
    {
      Add(item);
    }

    /// <inheritdoc/>
    public virtual bool Remove(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureInitialized();
      if (!Contains(item))
        return false;
      RemoveInternal(item);
      return true;
    }

    /// <inheritdoc/>
    public int RemoveWhere(Predicate<T> match)
    {
      EnsureInitialized();
      var itemsToRemove = new List<T>();
      foreach (T item in this) {
        if (match(item))
          itemsToRemove.Add(item);
      }
      foreach (T itemToRemove in itemsToRemove)
        RemoveInternal(itemToRemove);
      return itemsToRemove.Count;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      EnsureInitialized();
      foreach (T itemToRemove in this.ToList()) {
        RemoveInternal(itemToRemove);
      }
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      EnsureInitialized();
      long startVersion = version;
      if (count==Cache.Count) {
        foreach (CachedKey key in Cache) {
          EnsureVersion(startVersion);
          yield return key.Key.Resolve<T>();
        }
      }
      else {
        RecordSet recordSet = GetRecordSet();
        foreach (Tuple tuple in recordSet) {
          EnsureVersion(startVersion);
          Tuple keyTuple = KeyTransform.Apply(TupleTransformType.Tuple, tuple);
          var key = Key.Get(typeof (T), keyTuple);
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

    #region Protected and Internal

    internal Cache<Key, CachedKey, CachedKey> Cache { get; private set; }

    internal override sealed void ClearCache()
    {
      count = null;
      Cache.Clear();
      IncreaseVersion();
    }

    internal override void AddToCache(Key key, bool recount)
    {
      bool reInitialized = EnsureInitialized();
      if (recount || reInitialized) {
        count++;
        Cache.Add(new CachedKey(key));
      }
      IncreaseVersion();
    }

    internal override void RemoveFromCache(Key key, bool recount)
    {
      bool reInitialized = EnsureInitialized();
      if(recount || reInitialized) {
        count--;
        Cache.Remove(key);
      }
      IncreaseVersion();
    }

    protected IndexInfo Index
    {
      get
      {
        index = index ?? GetIndex();
        return index;
      }
    }

    protected virtual IndexInfo GetIndex()
    {
      FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
      return referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
    }

    protected virtual MapTransform GetKeyTransform()
    {
      var keyTupleDescriptor = Owner.Session.Domain.Model.Types[typeof (T)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = Index.Columns.Where(columnInfo => columnInfo.IsPrimaryKey).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    protected RecordSet GetRecordSet()
    {
      Tuple key = ((Entity) Owner).Key;
      RecordSet recordSet = Index.ToRecordSet();
      var rsResult = recordSet.Range(key, key);
      return rsResult;
    }

    protected bool EnsureInitialized()
    {
      EnusureTransaction();
      if (!count.HasValue) {
        count = GetRecordSet().Count();
        IncreaseVersion();
        return false;
      }
      return true;
    }

    protected void EnsureVersion(long currentVersion)
    {
      if (version!=currentVersion)
        Exceptions.CollectionHasBeenChanged(null);
    }

    #endregion

    #region Private methods

    private void EnusureTransaction()
    {
      if (Transaction.Current!=transaction) {
        ClearCache();
        transaction = Transaction.Current;
        IncreaseVersion();
      }
      if (transaction==null || Transaction.Current.State!=TransactionState.Active)
        throw new InvalidOperationException(Strings.ExEntitySetInvalidBecauseTransactionIsNotActive);
    }

    private void IncreaseVersion()
    {
      unchecked {
        Interlocked.Increment(ref version);
      }
    }

    private MapTransform KeyTransform
    {
      get
      {
        keyTransform = keyTransform ?? GetKeyTransform();
        return keyTransform;
      }
    }

    private void RemoveInternal(T item)
    {
      FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
      var accessor = referencingField.GetAccessor<Entity>();
      accessor.SetValue(item, referencingField, null);
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
      Cache = new Cache<Key, CachedKey, CachedKey>(MaximumCacheSize, cachedKey => cachedKey.Key);
    }
  }
}