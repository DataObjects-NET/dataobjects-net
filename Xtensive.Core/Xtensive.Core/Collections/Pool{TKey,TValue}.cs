// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.09

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Collections
{
  /// <summary>
  /// Keyed pool implementation.
  /// <see langword="Thread-safe." />
  /// </summary>
  /// <typeparam name="TKey">The type of key to retrieve the items by.</typeparam>
  /// <typeparam name="TItem">The type of pooled item.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}, AvailableCount = {AvailableCount}, Capacity = {Capacity}")]
  public class Pool<TKey, TItem>: IPool<TKey, TItem>, 
    IExpiringItemCollection<TItem>,
    IDisposable
  {
    private const int DefaultCapacity = 64;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly int capacity;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int availableCount;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int count;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TimeSpan itemExpirationPeriod;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TimeSpan garbageCollectionPeriod;
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private Dictionary<TKey, PoolEntry<TItem>> pool = new Dictionary<TKey, PoolEntry<TItem>>();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Dictionary<TItem, TKey> keys = new Dictionary<TItem, TKey>();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private object _lock = new object();
    private int  version = 0;
    private Thread cleanupThread;
    private bool   stopCleanup = false;


    /// <inheritdoc/>
    public int Capacity
    {
      [DebuggerStepThrough]
      get { return capacity; }
    }

    /// <inheritdoc/>
    public int AvailableCount
    {
      [DebuggerStepThrough]
      get { return availableCount; }
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return count; }
    }

    /// <inheritdoc/>
    public TimeSpan ItemExpirationPeriod
    {
      [DebuggerStepThrough]
      get { return itemExpirationPeriod; }
    }

    /// <inheritdoc/>
    public TimeSpan GarbageCollectionPeriod
    {
      [DebuggerStepThrough]
      get { return garbageCollectionPeriod; }
    }

    #region IPool<TKey,TItem> Members

    /// <inheritdoc/>
    public bool Add(TKey key, TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      ArgumentValidator.EnsureArgumentNotNull(key, "key");

      lock (_lock) {
        if (Count>=Capacity || keys.ContainsKey(item))
          return false;

        keys.Add(item, key);

        PoolEntry<TItem> poolEntry;
        if (!pool.TryGetValue(key, out poolEntry)) {
          poolEntry = new PoolEntry<TItem>();
          pool.Add(key, poolEntry);
        }
        poolEntry.FreeObjects.Enqueue(item, DateTime.Now);
        count++;
        availableCount++;
        return true;
      }
    }

    /// <inheritdoc/>
    public TItem Consume(TKey key)
    {
      lock (_lock) {
        if (AvailableCount==0)
          throw new InvalidOperationException(Strings.ExNoAvailableItems);
        PoolEntry<TItem> poolEntry;
        if (!pool.TryGetValue(key, out poolEntry) || poolEntry.FreeObjects.Count==0)
          throw new InvalidOperationException(Strings.ExNoAvailableItems);
        TItem item = poolEntry.FreeObjects.Dequeue();
        poolEntry.BusyObjects.Add(item);
        availableCount--;
        return item;
      }
    }

    /// <inheritdoc/>
    public TItem Consume(TKey key, Func<TItem> itemGenerator)
    {
      lock (_lock) {
        PoolEntry<TItem> poolEntry;
        if (!pool.TryGetValue(key, out poolEntry)) {
          TItem item = itemGenerator();
          Consume(key, item);
          return item;
        }
        if (poolEntry.FreeObjects.Count>0) {
          TItem item = poolEntry.FreeObjects.Dequeue();
          poolEntry.BusyObjects.Add(item);
          availableCount--;
          return item;
        }
        TItem newItem = itemGenerator();
        if (count<capacity)
          Consume(key, newItem);
        return newItem;
      }
    }

    /// <inheritdoc/>
    public void Consume(TKey key, TItem item)
    {
      lock (_lock) {
        if (availableCount>=capacity)
          return;
        // Check if item not consumed under another key.
        TKey existingKey;
        if (keys.TryGetValue(item, out existingKey)) {
          if (!existingKey.Equals(key))
            throw new ArgumentOutOfRangeException("key", Strings.ExPoolWrongKey);
        }
        PoolEntry<TItem> poolEntry;
        if (!pool.TryGetValue(key, out poolEntry)) {
          poolEntry = new PoolEntry<TItem>();
          pool.Add(key, poolEntry);
        }
        if (poolEntry.BusyObjects.Contains(item))
          throw new InvalidOperationException(Strings.ExItemIsInUse);
        if (poolEntry.FreeObjects.Contains(item)) {
          poolEntry.FreeObjects.Remove(item);
          availableCount--;
        }
        else
          count++;
        poolEntry.BusyObjects.Add(item);
        if (keys.ContainsKey(item))
          keys[item] = key;
        else
          keys.Add(item, key);
      }
    }

    /// <inheritdoc/>
    public void ExecuteConsumer(TKey key, Func<TItem> itemGenerator, Action<TKey, TItem> consumer)
    {
      TItem item = Consume(key, itemGenerator);
      try {
        consumer(key, item);
      }
      finally {
        Release(item);
      }
    }

    /// <inheritdoc/>
    public TItem[] RemoveKey(TKey key)
    {
      lock (_lock) {
        PoolEntry<TItem> poolEntry;
        if (pool.TryGetValue(key, out poolEntry)) {
          List<TItem> result = new List<TItem>();
          result.AddRange(poolEntry.FreeObjects);
          result.AddRange(poolEntry.BusyObjects);
          availableCount -= (int)poolEntry.FreeObjects.Count;
          count -= result.Count;
          pool.Remove(key);
          foreach (var item in result)
            OnItemRemoved(new ItemRemovedEventArgs<TItem>(item));
          return result.ToArray();
        }
        else
          return new TItem[0];
      }
    }

    #endregion

    #region IPoolBase<TItem> Members

    /// <inheritdoc/>
    public bool Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      lock (_lock) {
        TKey key;
        if (!keys.TryGetValue(item, out key))
          throw new InvalidOperationException(Strings.ExItemIsNotPooled);
        PoolEntry<TItem> poolEntry = pool[key];
        if (poolEntry.BusyObjects.Contains(item)) {
          return false;
          // throw new InvalidOperationException(Strings.ExItemIsInUse);
        }
        poolEntry.FreeObjects.Remove(item);
        keys.Remove(item);
        count--;
        availableCount--;
        if (((ICollection<TItem>)poolEntry.BusyObjects).Count==0 && poolEntry.FreeObjects.Count==0)
          pool.Remove(key);
        OnItemRemoved(new ItemRemovedEventArgs<TItem>(item));
        return true;
      }
    }

    /// <inheritdoc/>
    public void Release(TItem item)
    {
      lock (_lock) {
        TKey key;
        if (!keys.TryGetValue(item, out key)) {
          return;
          // throw new InvalidOperationException(Strings.ExItemIsNotPooled);
        }
        PoolEntry<TItem> poolEntry = pool[key];
        if (!poolEntry.BusyObjects.Contains(item)) {
          return;
          // throw new InvalidOperationException(Strings.ExItemIsNotInUse);
        }
        poolEntry.BusyObjects.Remove(item);
        if (Count<Capacity && Capacity>0) {
          poolEntry.FreeObjects.Enqueue(item, DateTime.Now);
          availableCount++;
        }
        else {
          count--;
          keys.Remove(item);
          if (poolEntry.Count==0)
            pool.Remove(key);
        }
      }
    }

    #endregion

    #region IsXxx methods

    /// <inheritdoc/>
    public bool IsPooled(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      lock (_lock) {
        return keys.ContainsKey(item);
      }
    }

    /// <inheritdoc/>
    public bool IsAvailable(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      lock (_lock) {
        TKey key;
        if (!keys.TryGetValue(item, out key))
          return false;
        return pool[key].FreeObjects.Contains(item);
      }
    }

    /// <inheritdoc/>
    public bool IsConsumed(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      lock (_lock) {
        TKey key;
        if (!keys.TryGetValue(item, out key))
          return false;
        return pool[key].BusyObjects.Contains(item);
      }
    }

    #endregion

    #region IEnumerable<...> Members

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      int oldVersion = version;
      lock (_lock) {
        foreach (KeyValuePair<TKey, PoolEntry<TItem>> poolItem in pool) {
          foreach (TItem busyItem in poolItem.Value.BusyObjects) {
            if (oldVersion!=version)
              throw new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
            yield return busyItem;
          }
          foreach (TItem freeItem in poolItem.Value.FreeObjects) {
            if (oldVersion!=version)
              throw new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
            yield return freeItem;
          }
        }
      }
    }

    #endregion

    #region ICleanupable Members

    /// <summary>
    /// Removes expired items from pool.
    /// </summary>
    public void CollectGarbage()
    {
      lock (_lock) {
        DateTime expireTime = DateTime.Now-itemExpirationPeriod;
        List<TKey> freeEntryPools = new List<TKey>();
        foreach (KeyValuePair<TKey, PoolEntry<TItem>> pair in pool) {
          PoolEntry<TItem> poolEntry = pair.Value;
          TItem[] expiredItems = poolEntry.FreeObjects.DequeueRange(expireTime);
          foreach (TItem item in expiredItems) {
            bool backToPool = false;
            ItemExpiresEventArgs<TItem> expiresEventArgs = new ItemExpiresEventArgs<TItem>(item);
            OnItemExpires(expiresEventArgs);
            if (expiresEventArgs.Cancel)
              backToPool = true;
            if (backToPool)
              poolEntry.FreeObjects.Enqueue(item, DateTime.Now);
            else {
              keys.Remove(item);
              count--;
              availableCount--;
              OnItemRemoved(new ItemRemovedEventArgs<TItem>(item));
            }
          }
          if (poolEntry.FreeObjects.Count==0 && ((ICollection<TItem>)poolEntry.BusyObjects).Count==0)
            freeEntryPools.Add(pair.Key);
        }

        // Remove excess keys from pool
        foreach (TKey key in freeEntryPools)
          pool.Remove(key);
      }
    }

    #endregion

    #region Events

    /// <inheritdoc/>
    public event EventHandler<ItemExpiresEventArgs<TItem>> ItemExpires;

    /// <inheritdoc/>
    public event EventHandler<ItemRemovedEventArgs<TItem>> ItemRemoved;

    /// <summary>
    /// Invokes <see cref="ItemExpires"/> event.
    /// </summary>
    /// <param name="eventArgs">Event arguments.</param>
    protected virtual void OnItemExpires(ItemExpiresEventArgs<TItem> eventArgs)
    {
      if (ItemExpires!=null)
        ItemExpires(this, eventArgs);
    }

    /// <summary>
    /// Invokes <see cref="ItemRemoved"/> event.
    /// </summary>
    /// <param name="eventArgs">Event arguments.</param>
    protected virtual void OnItemRemoved(ItemRemovedEventArgs<TItem> eventArgs)
    {
      if (ItemRemoved!=null)
        ItemRemoved(this, eventArgs);
    }

    #endregion

    #region Private \ internal methods

    private void CleanupThread()
    {
      // TODO: Refactor to weak reference usage!!!
      while (!stopCleanup) {
        CollectGarbage();
        Thread.Sleep(GarbageCollectionPeriod);
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Pool()
      : this(DefaultCapacity)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="initialCapacity">Initial <see cref="Capacity"/> property value.</param>
    public Pool(int initialCapacity)
      : this(initialCapacity, TimeSpan.FromSeconds(10))
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="initialCapacity">Initial <see cref="Capacity"/> property value.</param>
    /// <param name="itemExpirationPeriod"><see cref="ItemExpirationPeriod"/> property value.</param>
    public Pool(int initialCapacity, TimeSpan itemExpirationPeriod)
      : this(initialCapacity, itemExpirationPeriod, TimeSpan.FromSeconds(1))
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="initialCapacity">Initial <see cref="Capacity"/> property value.</param>
    /// <param name="itemExpirationPeriod"><see cref="ItemExpirationPeriod"/> property value.</param>
    /// <param name="garbageCollectionPeriod"><see cref="GarbageCollectionPeriod"/> property value.</param>
    public Pool(int initialCapacity, TimeSpan itemExpirationPeriod, TimeSpan garbageCollectionPeriod)
    {
      if (initialCapacity<=0)
        throw new ArgumentOutOfRangeException("initialCapacity", Strings.ExArgumentValueMustBeGreaterThanZero);
      if (itemExpirationPeriod<TimeSpan.Zero)
        throw new ArgumentOutOfRangeException("itemExpirationPeriod", Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);
      capacity = initialCapacity;
      this.itemExpirationPeriod = itemExpirationPeriod;
      this.garbageCollectionPeriod = garbageCollectionPeriod;
      if (garbageCollectionPeriod>TimeSpan.Zero) {
        cleanupThread = new Thread(CleanupThread);
        cleanupThread.IsBackground = true;
        cleanupThread.Start();
      }
    }

    #region IDisposable Members

    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <see cref="DisposableDocTemplate.Dtor" copy="true"/>
    ~Pool()
    {
      Dispose(false);
    }

    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    protected void Dispose(bool disposing)
    {
      if (pool==null)
        return; // Already disposed
      try {
        if (cleanupThread!=null)
          stopCleanup = true;
        // Expire all free items
        lock (_lock) {
          foreach (KeyValuePair<TKey, PoolEntry<TItem>> pair in pool) {
            PoolEntry<TItem> poolEntry = pair.Value;
            foreach (TItem item in poolEntry.FreeObjects)
              OnItemRemoved(new ItemRemovedEventArgs<TItem>(item));
          }
        }
      }
      finally {
        pool = null;
        keys = null;
      }
    }

    #endregion
  }
}