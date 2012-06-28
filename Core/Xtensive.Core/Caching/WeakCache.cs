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
using System.Security;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Caching
{
  /// <summary>
  /// A set of weekly referenced items.
  /// Stores the references while the underlying items aren't collected by GC.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
#if NET40
  [SecuritySafeCritical]
#endif
  public class WeakCache<TKey, TItem> :
    ICache<TKey, TItem>,
    IHasGarbage,
    IDisposable
    where TItem : class
  {
    /// <summary>
    /// Minimal <see cref="Count"/> value, until which <see cref="CollectGarbage"/> doesn't start at all.
    /// Value is <see langword="1024"/>.
    /// </summary>
    protected const int NoGcCount = 1024;

    private const int GcOperationCost = 2;
    private readonly bool trackResurrection;
    private readonly Converter<TItem, TKey> keyExtractor;
    private Dictionary<TKey, GCHandle> items;
    private int time;

    #region Properites: KeyExtractor, ChainedCache, TrackResurrection, EfficiencyFactor, Count, Size

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor {
      [DebuggerStepThrough]
      get { return keyExtractor; }
    }

    /// <summary>
    /// Gets a value indicating whether this cache tracks resurrection.
    /// </summary>
    public bool TrackResurrection {
      [DebuggerStepThrough]
      get { return trackResurrection; }
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
          return null;
      }
    }

    /// <inheritdoc/>
  #if NET40
    [SecuritySafeCritical]
  #endif
    public virtual bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      RegisterOperation(1);
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
  #if NET40
    [SecuritySafeCritical]
  #endif
    public virtual TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      RegisterOperation(2);
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
  #if NET40
    [SecuritySafeCritical]
  #endif
    public virtual void RemoveKey(TKey key)
    {
      GCHandle cached;
      if (items.TryGetValue(key, out cached)) {
        items.Remove(key);
        cached.Free();
      }
    }

    /// <inheritdoc/>
  #if NET40
    [SecuritySafeCritical]
  #endif
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
  #if NET40
    [SecuritySafeCritical]
  #endif
    public virtual void CollectGarbage()
    {
      int count = items.Count;
      if (count<=NoGcCount)
        return;

      Exception error = null;
      int removedCount = 0;
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
          Log.Debug("WeakCache.CollectGarbage: removed: {0} from {1}", removedCount, count);
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
        var item = ExtractTarget(pair.Value);
        if (item!=null)
          yield return item;
      }
    }

  #if NET40
    [SecuritySafeCritical]
  #endif
    private static TItem ExtractTarget(GCHandle handle)
    {
      return (TItem) handle.Target;
    }

    #endregion

    #region Private \ internal methods

    private void RegisterOperation(int weight)
    {
      time += weight;
      var count = items.Count;
      if (count <= NoGcCount)
        return;
      if (time > ((count << 1) + count))
        CollectGarbage();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="trackResurrection">The <see cref="TrackResurrection"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public WeakCache(bool trackResurrection, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.trackResurrection = trackResurrection;
      this.keyExtractor = keyExtractor;
      items = new Dictionary<TKey, GCHandle>(1024);
    }

    // Dispose pattern

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    /// </summary>
  #if NET40
    [SecuritySafeCritical]
  #endif
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
