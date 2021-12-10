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
using Xtensive.Core;
using Xtensive.Orm.Logging;

namespace Xtensive.Caching
{
  /// <summary>
  /// A set of weekly referenced items identified by weekly referenced keys.
  /// Stores the references while the underlying keys or items aren't collected by GC.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  [SecuritySafeCritical]
  public class WeakestCache<TKey, TItem> :
    CacheBase<TKey, TItem>,
    IHasGarbage,
    IDisposable
    where TKey  : class
    where TItem : class
  {
    /// <summary>
    /// Minimal <see cref="Count"/> value, until which <see cref="CollectGarbage"/> doesn't start at all.
    /// Value is <see langword="1024"/>.
    /// </summary>
    protected const int NoGcCount = 1024;

    private const int GcOperationCost = 2;
    private readonly bool trackKeyResurrection;
    private readonly bool trackItemResurrection;
    private Dictionary<object, WeakEntry> items;
    private int time;

    #region Nested type: WeakEntry

    [DebuggerDisplay("Key = {Key}, Item = {Item}, HashCode = {hashCode}")]
    [SecuritySafeCritical]
    internal sealed class WeakEntry : 
      IEquatable<WeakEntry>,
      IDisposable
    {
      private GCHandle keyHandle;
      private GCHandle itemHandle;
      private readonly int hashCode;

      public TKey Key {
        [DebuggerStepThrough]
        get {
          return keyHandle.Target as TKey;
        }
      }

      public TItem Item {
        [DebuggerStepThrough]
        get {
          return itemHandle.Target as TItem;
        }
      }

      public KeyValuePair<TKey, TItem> Value {
        [DebuggerStepThrough]
        get {
          object key = keyHandle.Target;
          if (key==null)
            return default(KeyValuePair<TKey, TItem>);
          object item = itemHandle.Target;
          if (item ==null)
            return default(KeyValuePair<TKey, TItem>);
          return new KeyValuePair<TKey, TItem>(key as TKey, item as TItem);
        }
      }

      [SecuritySafeCritical]
      public void Dispose()
      {
        keyHandle.Free();
        itemHandle.Free();
      }

      #region Equality members

      public bool Equals(WeakEntry obj)
      {
        return 
          obj.hashCode==hashCode &&
          obj.Value.Key==Value.Key;
      }

      public override bool Equals(object obj)
      {
        return Equals((WeakEntry) obj);
      }

      public override int GetHashCode()
      {
        return hashCode;
      }

      #endregion

      
      // Constructors

      public WeakEntry(TKey key, TItem item, bool trackKeyResurrection, bool trackItemResurrection)
      {
        keyHandle = GCHandle.Alloc(key, 
          trackKeyResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
        itemHandle = GCHandle.Alloc(item, 
          trackItemResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
        hashCode = key.GetHashCode();
      }
    }

    #endregion

    #region Nested type: WeakEntryEqualityComparer

    internal sealed class WeakEntryEqualityComparer : IEqualityComparer<object>
    {
      private readonly IEqualityComparer<TKey> keyComparer;

      public new bool Equals(object x, object y)
      {
        if (x is WeakEntry we) {
          // x is WeakEntry
          if (y is TKey key)
            // x is WeakEntry, y is TKey
            return keyComparer.Equals(we.Key, key);
          else {
            return y is WeakEntry we2 && keyComparer.Equals(we.Key, we2.Key); // x is WeakEntry, y is WeakEntry
          }
        }
        if (x is TKey keyX) {
          if (y is WeakEntry weY)
            // x is TKey, y is WeakEntry
            return keyComparer.Equals(keyX, weY.Key);
          else
            // x is TKey, y must be TKey
            return keyComparer.Equals(keyX, y as TKey);
        }
        return false;
      }

      public int GetHashCode(object obj)
      {
        return obj.GetHashCode();
      }

      public WeakEntryEqualityComparer()
      {
        keyComparer = EqualityComparer<TKey>.Default;
      }
    }

    #endregion

    #region Properites: KeyExtractor, TrackKeyResurrection, TrackItemResurrection, EfficiencyFactor, Count, Size

    /// <summary>
    /// Gets a value indicating whether this cache tracks key resurrection.
    /// </summary>
    public bool TrackKeyResurrection {
      [DebuggerStepThrough]
      get { return trackKeyResurrection; }
    }

    /// <summary>
    /// Gets a value indicating whether this cache tracks item resurrection.
    /// </summary>
    public bool TrackItemResurrection {
      [DebuggerStepThrough]
      get { return trackItemResurrection; }
    }

    /// <inheritdoc/>
    public override int Count {
      [DebuggerStepThrough]
      get { return items.Count; }
    }

    #endregion

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      item = GetItemByKeyInternal(key, markAsHit);
      return item!=null;
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      return GetItemByKeyInternal(key, false)!=null;
    }

    #region Modification methods: Add, Remove, Clear

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var key = KeyExtractor(item);
      ArgumentValidator.EnsureArgumentNotNull(key, "KeyExtractor.Invoke(item)");
      RegisterOperation(4);
      WeakEntry entry;
      if (items.TryGetValue(key, out entry)) {
        var pair = entry.Value;
        if (!replaceIfExists) {
          if (pair.Key!=null && pair.Value!=null)
            return pair.Value;
        }
        if (pair.Key==null) {
          items.Remove(key);
          entry.Dispose();
        }
      }
      entry = new WeakEntry(key, item, trackKeyResurrection, trackItemResurrection);
      items[entry] = entry;
      return item;
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void RemoveKey(TKey key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      if (items.Remove(key, out var entry)) {
        entry.Dispose();
      }
    }

    /// <inheritdoc/>
    public override void RemoveKey(TKey key, bool removeCompletely)
    {
      RemoveKey(key);
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void Clear()
    {
      try {
        foreach (var pair in items)
          try {
            pair.Value.Dispose();
          }
          catch {}
      }
      finally {
        items = new Dictionary<object, WeakEntry>(new WeakEntryEqualityComparer());;
        time = 0;
      }
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual void CollectGarbage()
    {
      int count = items.Count;
      Exception error = null;
      int removedCount = 0;
      try {
        // Filtering
        var newItems = new Dictionary<object, WeakEntry>(new WeakEntryEqualityComparer());;
        foreach (var pair in items) {
          var entry = pair.Value;
          var value = entry.Value;
          if (value.Key!=null)
            newItems.Add(entry, entry);
          else
            entry.Dispose();
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
          CoreLog.Debug("WeakestCache.CollectGarbage: removed: {0} from {1}", removedCount, count);
          if (error!=null)
            CoreLog.Debug(error, "Caught at WeakestCache.CollectGarbage");
        }
      }
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override IEnumerator<TItem> GetEnumerator()
    {
      foreach (var pair in items) {
        var item = pair.Value.Value.Value;
        if (item!=null)
          yield return item;
      }
    }

    #endregion

    #region Private / internal methods

    private void RegisterOperation(int weight)
    {
      time+=weight;
      var count = items.Count;
      if (count<=NoGcCount)
        return;
      if (time > ((count << 1) + count))
        CollectGarbage();
    }

    [SecuritySafeCritical]
    private TItem GetItemByKeyInternal(TKey key, bool markAsHit)
    {
      RegisterOperation(1);
      WeakEntry entry;
      if (items.TryGetValue(key, out entry)) {
        var pair = entry.Value;
        if (pair.Key != null) {
          return pair.Value;
        }
        items.Remove(key);
        entry.Dispose();
      }
      return null;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="trackKeyResurrection">The <see cref="TrackKeyResurrection"/> property value.</param>
    /// <param name="trackItemResurrection">The <see cref="TrackItemResurrection"/> property value.</param>
    /// <param name="keyExtractor"><see cref="ICache{TKey, TItem}.KeyExtractor"/> property value.</param>
    public WeakestCache(bool trackKeyResurrection, bool trackItemResurrection, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.trackKeyResurrection = trackKeyResurrection;
      this.trackItemResurrection = trackItemResurrection;
      this.KeyExtractor = keyExtractor;
      items = new Dictionary<object, WeakEntry>(1024, new WeakEntryEqualityComparer());
    }

    // Dispose pattern

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    [SecuritySafeCritical]
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
    ~WeakestCache()
    {
      Dispose(false);
    }
  }
}
