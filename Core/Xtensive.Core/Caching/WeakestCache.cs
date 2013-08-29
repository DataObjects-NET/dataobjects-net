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
  /// A set of weekly referenced items identified by weekly referenced keys.
  /// Stores the references while the underlying keys or items aren't collected by GC.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  [SecuritySafeCritical]
  public class WeakestCache<TKey, TItem> :
    ICache<TKey, TItem>,
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
    private readonly Converter<TItem, TKey> keyExtractor;
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
        TKey key;
        var we = x as WeakEntry;
        if (we!=null) {
          // x is WeakEntry
          key = y as TKey;
          if (key!=null)
            // x is WeakEntry, y is TKey
            return keyComparer.Equals(we.Key, key);
          else {
            var we2 = y as WeakEntry;
            if (we2==null)
              return false;
            // x is WeakEntry, y is WeakEntry
            return keyComparer.Equals(we.Key, we2.Key);
          }
        }
        key = x as TKey;
        if (key==null)
          return false;
        // x is TKey
        we = y as WeakEntry;
        if (we!=null)
          // x is TKey, y is WeakEntry
          return keyComparer.Equals(key, we.Key);
        else
          // x is TKey, y must be TKey
          return keyComparer.Equals(key, y as TKey);
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

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor {
      [DebuggerStepThrough]
      get { return keyExtractor; }
    }

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
    [SecuritySafeCritical]
    public virtual bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      RegisterOperation(1);
      WeakEntry entry;
      if (items.TryGetValue(key, out entry)) {
        var pair = entry.Value;
        if (pair.Key!=null) {
          item = pair.Value;
          return true;
        }
        items.Remove(key);
        entry.Dispose();
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
    [SecuritySafeCritical]
    public virtual TItem Add(TItem item, bool replaceIfExists)
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
    public void Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      RemoveKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual void RemoveKey(TKey key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      WeakEntry entry;
      if (items.TryGetValue(key, out entry)) {
        items.Remove(key);
        entry.Dispose();
      }
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public virtual void Clear()
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
    public void Invalidate()
    {
      Clear();
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
        if (Log.IsLogged(LogEventTypes.Debug)) {
          Log.Debug("WeakestCache.CollectGarbage: removed: {0} from {1}", removedCount, count);
          if (error!=null)
            Log.Debug(error, "Caught at WeakestCache.CollectGarbage");
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
    [SecuritySafeCritical]
    public virtual IEnumerator<TItem> GetEnumerator()
    {
      foreach (var pair in items) {
        var item = pair.Value.Value.Value;
        if (item!=null)
          yield return item;
      }
    }

    #endregion

    #region Private \ internal methods

    private void RegisterOperation(int weight)
    {
      time+=weight;
      var count = items.Count;
      if (count<=NoGcCount)
        return;
      if (time > ((count << 1) + count))
        CollectGarbage();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="trackKeyResurrection">The <see cref="TrackKeyResurrection"/> property value.</param>
    /// <param name="trackItemResurrection">The <see cref="TrackItemResurrection"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public WeakestCache(bool trackKeyResurrection, bool trackItemResurrection, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.trackKeyResurrection = trackKeyResurrection;
      this.trackItemResurrection = trackItemResurrection;
      this.keyExtractor = keyExtractor;
      items = new Dictionary<object, WeakEntry>(1024, new WeakEntryEqualityComparer());
    }

    // Dispose pattern

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
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
    ~WeakestCache()
    {
      Dispose(false);
    }
  }
}
