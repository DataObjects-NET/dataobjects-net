// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Caching
{
  /// <summary>
  /// A set of weekly referenced items.
  /// Stores the references while the underlying items aren't collected by GC.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  public class WeakCache<TKey, TItem> :
    ICache<TKey, TItem>,
    IHasGarbage,
    IDisposable
    where TItem : class
  {
    /// <summary>
    /// Default <see cref="EfficiencyFactor"/> value.
    /// Value is <see langword="1"/>.
    /// </summary>
    public const int DefaultEfficiencyFactor = 1;

    /// <summary>
    /// Minimal <see cref="Count"/> value, until which <see cref="CollectGarbage"/> doesn't start at all.
    /// Value is <see langword="1024"/>.
    /// </summary>
    protected const int NoGcCount = 1024;

    /// <summary>
    /// Maximal <see cref="Count"/> value, at which <see cref="CollectGarbage"/> starts immediately.
    /// Value is <see langword="64*1024*1024"/>.
    /// </summary>
    protected const int ImmediateGcCount = 64*1024*1024;

    private const int GcOperationCost = 2;
    private readonly bool trackResurrection;
    private readonly int efficiencyFactor;
    private readonly Converter<TItem, TKey> keyExtractor;
    private Dictionary<TKey, GCHandle> items;
    private int time;
    private int timeShift = 1;

    #region Properites: KeyExtractor, ChainedCache, TrackResurrection, EfficiencyFactor, Count, Size

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor {
      [DebuggerStepThrough]
      get { return keyExtractor; }
    }

    /// <summary>
    /// Always <see langword="null" /> in this class.
    /// </summary>
    public ICache<TKey, TItem> ChainedCache {
      [DebuggerStepThrough]
      get { return null; }
    }

    /// <summary>
    /// Gets a value indicating whether this cache tracks resurrection.
    /// </summary>
    public bool TrackResurrection {
      [DebuggerStepThrough]
      get { return trackResurrection; }
    }

    /// <summary>
    /// Gets the time shift factor offset.
    /// </summary>
    public int EfficiencyFactor {
      [DebuggerStepThrough]
      get { return efficiencyFactor; }
    }

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get { return items.Count; }
    }

    /// <inheritdoc/>
    long ICountable.Count {
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
          return null;
      }
    }

    /// <inheritdoc/>
    public virtual bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      OnOperation();
      GCHandle cached;
      if (items.TryGetValue(key, out cached)) {
        item = (TItem) cached.Target;
        if (item!=null)
          return true;
        items.Remove(key);
        cached.Free();
        return false;
      }
      item = null;
      return false;
    }

    /// <inheritdoc/>
    public bool Contains(TItem item)
    {
      return ContainsKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      TItem item;
      return TryGetItem(key, false, out item);
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
      GCHandle cached;
      if (items.TryGetValue(key, out cached)) {
        if (!replaceIfExists) {
          var cachedItem = (TItem) cached.Target;
          if (cachedItem!=null)
            return cachedItem;
        }
        items.Remove(key);
        cached.Free();
      }
      items[key] = GCHandle.Alloc(item,
        trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
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
      GCHandle cached;
      if (items.TryGetValue(key, out cached)) {
        items.Remove(key);
        cached.Free();
      }
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      try {
        foreach (var pair in items)
          try {
            pair.Value.Free();
          }
          catch {}
      }
      finally {
        items = new Dictionary<TKey, GCHandle>();
        time = 0;
      }
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
      if (count<=NoGcCount)
        return;

      Exception error = null;
      int removedCount = 0;
      double effeciency = 0;
      try {
        // Filtering
        var newItems = new Dictionary<TKey, GCHandle>();
        foreach (var pair in items) {
          var cached = pair.Value;
          var item = cached.Target;
          if (item!=null)
            newItems.Add(pair.Key, cached);
          else
            cached.Free();
        }
        removedCount = count - newItems.Count;

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
        if (Log.IsLogged(LogEventTypes.Debug)) {
          Log.Debug("WeakCache.CollectGarbage: removed: {0} from {1}, efficiency: {2}, time shift: {3}",
            removedCount, count, effeciency, timeShift);
          if (error!=null)
            Log.Debug(error, "Caught at WeakCache.CollectGarbage");
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
      foreach (var pair in items) {
        var item = (TItem) pair.Value.Target;
        if (item!=null)
          yield return item;
      }
    }

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
      else if (count > ImmediateGcCount)
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
      else if (count > ImmediateGcCount)
        CollectGarbage();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="trackResurrection">The <see cref="TrackResurrection"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public WeakCache(bool trackResurrection, Converter<TItem, TKey> keyExtractor)
      : this(trackResurrection, DefaultEfficiencyFactor, keyExtractor)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="trackResurrection">The <see cref="TrackResurrection"/> property value.</param>
    /// <param name="efficiencyFactor">The <see cref="EfficiencyFactor"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public WeakCache(bool trackResurrection, int efficiencyFactor, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.trackResurrection = trackResurrection;
      this.efficiencyFactor = efficiencyFactor;
      if (efficiencyFactor<0)
        timeShift = -efficiencyFactor-1; // Constant timeShift is defined
      this.keyExtractor = keyExtractor;
      items = new Dictionary<TKey, GCHandle>(1024);
    }

    // Dispose pattern

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (items!=null) {
        try {
          Clear();
        }
        finally {
          items = null;
        }
      }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor" copy="true"/>
    /// /// </summary>
    ~WeakCache()
    {
      Dispose(false);
    }
  }
}
