// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using Xtensive.Core;
using Xtensive.Orm.Logging;

namespace Xtensive.Caching
{
  /// <summary>
  /// A set of weekly referenced items.
  /// Stores the references while the underlying items aren't collected by GC.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  [SecuritySafeCritical]
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

    private readonly bool trackResurrection;
    private readonly Converter<TItem, TKey> keyExtractor;
    private Dictionary<TKey, GCHandle> items;
    private int time;

    #region Properites: KeyExtractor, ChainedCache, TrackResurrection, EfficiencyFactor, Count, Size

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor
    {
      [DebuggerStepThrough]
      get => keyExtractor;
    }

    /// <summary>
    /// Gets a value indicating whether this cache tracks resurrection.
    /// </summary>
    public bool TrackResurrection
    {
      [DebuggerStepThrough]
      get => trackResurrection;
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get => items?.Count ?? 0;
    }

    #endregion

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      RegisterOperation(1);
      if (items != null && items.TryGetValue(key, out var cached)) {
        item = ExtractTarget(cached);
        if (item != null) {
          return true;
        }

        items.Remove(key);
        cached.Free();
        return false;
      }
      item = null;
      return false;
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key) => TryGetItem(key, false, out var _);

    #region Modification methods: Add, Remove, Clear

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, nameof(item));
      RegisterOperation(2);
      var key = KeyExtractor(item);
      if (items == null) {
        items = CreateDictionary();
      }
      else if (replaceIfExists) {
        if (items.Remove(key, out var cached)) {
          cached.Free();
        }
      }
      else if (items.TryGetValue(key, out var cached)) {
        if (ExtractTarget(cached) is TItem cachedItem) {
          return cachedItem;
        }
        items.Remove(key);
        cached.Free();
      }
      items[key] = GCHandle.Alloc(item, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
      return item;
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual void RemoveKey(TKey key)
    {
      if (items != null && items.Remove(key, out var cached) == true) {
        cached.Free();
      }
    }

    /// <inheritdoc/>
    public void RemoveKey(TKey key, bool removeCompletely) => RemoveKey(key);

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual void Clear()
    {
      if (items == null) {
        return;
      }
      try {
        foreach (var pair in items) {
          try {
            pair.Value.Free();
          }
          catch { }
        }
      }
      finally {
        items = null;
        time = 0;
      }
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual void CollectGarbage()
    {
      var count = items?.Count ?? 0;
      if (count <= NoGcCount) {
        return;
      }

      Exception error = null;
      int removedCount = 0;
      try {
        // Filtering
        var newItems = CreateDictionary();
        foreach (var (key, cached) in items) {
          var item = cached.Target;
          if (item != null)
            newItems.Add(key, cached);
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
        if (CoreLog.IsLogged(LogLevel.Debug)) {
          CoreLog.Debug("WeakCache.CollectGarbage: removed: {0} from {1}", removedCount, count);
          if (error != null)
            CoreLog.Debug(error, "Caught at WeakCache.CollectGarbage");
        }
      }
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public virtual IEnumerator<TItem> GetEnumerator()
    {
      foreach (var pair in items ?? Enumerable.Empty<KeyValuePair<TKey, GCHandle>>()) {
        if (ExtractTarget(pair.Value) is TItem item)
          yield return item;
      }
    }

    [SecuritySafeCritical]
    private static TItem ExtractTarget(GCHandle handle) => (TItem) handle.Target;

    #endregion

    #region Private / internal methods

    private static Dictionary<TKey, GCHandle> CreateDictionary() => new Dictionary<TKey, GCHandle>();

    private void RegisterOperation(int weight)
    {
      time += weight;
      var count = items?.Count ?? 0;
      if (count > NoGcCount && time > (count << 1) + count) {
        CollectGarbage();
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="trackResurrection">The <see cref="TrackResurrection"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public WeakCache(bool trackResurrection, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.trackResurrection = trackResurrection;
      this.keyExtractor = keyExtractor;
    }

    // Dispose pattern

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    [SecuritySafeCritical]
    protected virtual void Dispose(bool disposing)
    {
      Clear();
    }

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources associated with this instance.
    /// /// </summary>
    ~WeakCache()
    {
      Dispose(false);
    }
  }
}
