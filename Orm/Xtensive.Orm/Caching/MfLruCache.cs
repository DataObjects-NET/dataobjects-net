// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Diagnostics;


namespace Xtensive.Caching
{
  /// <summary>
  /// A set of items limited by the maximal count of them.
  /// Stores as many most recently and frequently accessed items in memory as long as it is possible.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  public class MfLruCache<TKey, TItem> :
    ICache<TKey, TItem>,
    IHasGarbage
  {
    /// <summary>
    /// Default <see cref="EfficiencyFactor"/> value.
    /// Value is <see langword="4"/>.
    /// </summary>
    public const int DefaultEfficiencyFactor = 4;

    /// <summary>
    /// Minimal <see cref="Count"/> value, until which <see cref="CollectGarbage"/> doesn't start at all.
    /// Value is <see langword="1024"/>.
    /// </summary>
    protected const int NoGcCount = 1024;

    private const int GcOperationCost = 4;
    private readonly int lruCapacity;
    private readonly int mfuCapacity;
    private readonly int capacity;
    private readonly int efficiencyFactor;
    private readonly Converter<TItem, TKey> keyExtractor;
    private readonly ICache<TKey, TItem> chainedCache;
    private Dictionary<TKey, CachedItem> items;
    private int time;
    private int timeShift = 1;

    #region Nested type: CachedItem

    [DebuggerDisplay("Item = {Item}, HitCount = {HitCount}, HitTime = {HitTime}")]
    private class CachedItem
    {
      public TItem Item;
      public int HitTime;
      public int HitCount;

      public CachedItem(TItem item)
      {
        Item = item;
      }
    }

    #endregion

    #region Properites: KeyExtractor, ChainedCache, LruCapacity, MfuCapacity, Capacity, EfficiencyFactor, Count, Size

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor {
      [DebuggerStepThrough]
      get { return keyExtractor; }
    }

    /// <summary>
    /// Gets chained cache.
    /// </summary>
    public ICache<TKey, TItem> ChainedCache {
      [DebuggerStepThrough]
      get { return chainedCache; }
    }

    /// <summary>
    /// Gets the Least Recently Used capacity.
    /// </summary>
    public int LruCapacity {
      [DebuggerStepThrough]
      get { return lruCapacity; }
    }

    /// <summary>
    /// Gets the Most Frequently Used capacity.
    /// </summary>
    public int MfuCapacity {
      [DebuggerStepThrough]
      get { return mfuCapacity; }
    }

    /// <summary>
    /// Gets the total capacity (<see cref="LruCapacity"/> + <see cref="MfuCapacity"/>).
    /// </summary>
    public int Capacity {
      [DebuggerStepThrough]
      get { return capacity; }
    }

    /// <summary>
    /// Gets the time shift factor offset.
    /// </summary>
    public int EfficiencyFactor {
      get { return efficiencyFactor; }
    }

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get { return items.Count; }
    }

    #endregion

    /// <inheritdoc/>
    public TItem this[TKey key, bool markAsHit] {
      get {
        TItem item;
        if (TryGetItem(key, markAsHit, out item))
          return item;
        else
          return default(TItem);
      }
    }

    /// <inheritdoc/>
    public virtual bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      OnOperation();
      CachedItem cached;
      if (items.TryGetValue(key, out cached)) {
        if (markAsHit) {
          cached.HitTime = time;
          cached.HitCount++;
        }
        item = cached.Item;
        return true;
      }
      if (chainedCache==null) {
        item = default(TItem);
        return false;
      }
      if (chainedCache.TryGetItem(key, false, out item)) {
        chainedCache.Remove(item);
        Add(item, true);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public bool Contains(TItem item)
    {
      return ContainsKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public virtual bool ContainsKey(TKey key)
    {
      if (items.ContainsKey(key))
        return true;
      if (chainedCache==null)
        return false;
      return chainedCache.ContainsKey(key);
    }

    #region Modification methods: Add, Remove, Clear

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      Add(item, true);
    }

    /// <inheritdoc/>
    public virtual TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      OnOperation2();
      var key = KeyExtractor(item);
      CachedItem cached;
      if (items.TryGetValue(key, out cached)) {
        if (!replaceIfExists)
          return cached.Item;
        if (chainedCache!=null)
          chainedCache.Add(cached.Item, true);
        items.Remove(key);
        ItemRemoved(key);
      }
      cached = new CachedItem(item) {
        HitTime = time, 
        HitCount = 1
      };
      items[key] = cached;
      ItemAdded(key);
      return item;
    }

    /// <inheritdoc/>
    public void Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      RemoveKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public virtual void RemoveKey(TKey key)
    {
      CachedItem cached;
      if (items.TryGetValue(key, out cached)) {
        if (chainedCache!=null)
          chainedCache.Add(cached.Item, true);
        items.Remove(key);
        ItemRemoved(key);
      }
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      foreach (var pair in items) {
        var key = pair.Key;
        var cached = pair.Value;
        if (chainedCache!=null)
          chainedCache.Add(cached.Item, true);
        ItemRemoved(key);
      }
      items.Clear();
      time = 0;
      Cleared();
    }

    /// <inheritdoc/>
    public void Invalidate()
    {
      Clear();
    }

    /// <inheritdoc/>
    public virtual void CollectGarbage()
    {
      int count = items.Count;
      if (count<=capacity)
        return;

      Exception error = null;
      int removedCount = 0;
      double effeciency = 0;
      try {
        // Preparing arrays for selection
        var times = new int[count];
        var hits  = new int[count];
        int i = 0;
        foreach (var pair in items) {
          var cached = pair.Value;
          times[i] = cached.HitTime;
          hits[i]  = cached.HitCount;
          i++;
        }
        
        // Selection
        Func<int, int, int> reversedIntComparer = (l, r) => r - l;
        int minTime = times.Select(reversedIntComparer, lruCapacity);
        int minHits = hits.Select(reversedIntComparer, mfuCapacity);
        
        // Filtering
        var newItems = new Dictionary<TKey, CachedItem>();
        foreach (var pair in items) {
          var cached = pair.Value;
          if (cached.HitTime > minTime || cached.HitCount > minHits)
            newItems.Add(pair.Key, new CachedItem(cached.Item));
        }
        bool done = newItems.Count > capacity;
        foreach (var pair in items) {
          var cached = pair.Value;
          if (!done && (
              (cached.HitTime==minTime && cached.HitCount<=minHits) ||
              (cached.HitCount==minHits && cached.HitTime<=minTime))) {
            newItems.Add(pair.Key, new CachedItem(cached.Item));
            if (newItems.Count > capacity)
              done = true;
          }
          else if (cached.HitTime > minTime || cached.HitCount > minHits) {
            // Already added, so doing nothing ...
          }
          else {
            removedCount++;
            if (chainedCache!=null)
              chainedCache.Add(cached.Item, true);
            ItemRemoved(pair.Key);
          }
        }

        // Updating timeShift
        if (efficiencyFactor<0)
          timeShift = -efficiencyFactor; // Constant timeShift is defined
        else {
          // Relative effeciency factor is defined
          if (removedCount < 1)
            removedCount = 1;
          effeciency =
            ((double) GcOperationCost * removedCount + time) /
            ((double) GcOperationCost * count + time);
          timeShift = ((int) Math.Ceiling(Math.Log(1 / effeciency, 2)));
          timeShift += efficiencyFactor;
          if (timeShift > 7)
            timeShift = 7;
          if (timeShift < 1)
            timeShift = 1;
        }

        // Done
        items = newItems;
        time = 0;
      }
      catch (Exception e) {
        error = e;
        throw;
      }
      finally {
        // Logging
        if (CoreLog.IsLogged(LogEventTypes.Debug)) {
          CoreLog.Debug("MfLruCache.CollectGarbage: removed: {0} from {1}, efficiency: {2}, time shift: {3}",
            removedCount, count, effeciency, timeShift);
          if (error!=null)
            CoreLog.Debug(error, "Caught at MfLruCache.CollectGarbage");
        }
      }
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public virtual IEnumerator<TItem> GetEnumerator()
    {
      foreach (var pair in items)
        yield return pair.Value.Item;
    }

    #endregion

    #region Protected events (to override)

    protected virtual void ItemAdded(TKey key) { }
    protected virtual void ItemRemoved(TKey key) { }
    protected virtual void Cleared() { }

    #endregion

    #region Private \ internal methods

    private void OnOperation()
    {
      time++;
      var count = items.Count;
      if (count<=NoGcCount)
        return;
      var counted = time >> timeShift;
      if (counted > count)
        CollectGarbage();
      else if (counted > capacity)
        CollectGarbage();
    }

    private void OnOperation2()
    {
      time += 2;
      var count = items.Count;
      if (count<=NoGcCount)
        return;
      var counted = time >> timeShift;
      if (counted > count)
        CollectGarbage();
      else if (counted > capacity)
        CollectGarbage();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="lruCapacity">The <see cref="LruCapacity"/> property value.</param>
    /// <param name="mfuCapacity">The <see cref="MfuCapacity"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public MfLruCache(int lruCapacity, int mfuCapacity, Converter<TItem, TKey> keyExtractor)
      : this(lruCapacity, mfuCapacity, DefaultEfficiencyFactor, keyExtractor, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="lruCapacity">The <see cref="LruCapacity"/> property value.</param>
    /// <param name="mfuCapacity">The <see cref="MfuCapacity"/> property value.</param>
    /// <param name="efficiencyFactor">The <see cref="EfficiencyFactor"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public MfLruCache(int lruCapacity, int mfuCapacity, int efficiencyFactor,
      Converter<TItem, TKey> keyExtractor)
      : this(lruCapacity, mfuCapacity, efficiencyFactor, keyExtractor, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="lruCapacity">The <see cref="LruCapacity"/> property value.</param>
    /// <param name="mfuCapacity">The <see cref="MfuCapacity"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public MfLruCache(int lruCapacity, int mfuCapacity, 
      Converter<TItem, TKey> keyExtractor, ICache<TKey, TItem> chainedCache)
      : this(lruCapacity, mfuCapacity, DefaultEfficiencyFactor, keyExtractor, chainedCache)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="lruCapacity">The <see cref="LruCapacity"/> property value.</param>
    /// <param name="mfuCapacity">The <see cref="MfuCapacity"/> property value.</param>
    /// <param name="efficiencyFactor">The <see cref="EfficiencyFactor"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public MfLruCache(int lruCapacity, int mfuCapacity, int efficiencyFactor,
      Converter<TItem, TKey> keyExtractor, ICache<TKey, TItem> chainedCache)
    {
      if (lruCapacity <= 0)
        ArgumentValidator.EnsureArgumentIsInRange(lruCapacity , 1, int.MaxValue, "lruCapacity");
      if (mfuCapacity < 0)
        ArgumentValidator.EnsureArgumentIsInRange(lruCapacity , 0, int.MaxValue, "mfuCapacity");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.lruCapacity = lruCapacity;
      this.mfuCapacity = mfuCapacity;
      capacity = lruCapacity + mfuCapacity;
      this.efficiencyFactor = efficiencyFactor;
      if (efficiencyFactor<0)
        timeShift = -efficiencyFactor-1; // Constant timeShift is defined
      this.keyExtractor = keyExtractor;
      this.chainedCache = chainedCache;
      // items = new Dictionary<TKey, CachedItem>(1 + capacity);
      items = new Dictionary<TKey, CachedItem>();
    }
  }
}
