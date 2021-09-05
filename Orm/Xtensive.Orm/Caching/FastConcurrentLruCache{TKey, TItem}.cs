// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BitFaster.Caching.Lru;
using Xtensive.Collections;
using Xtensive.Conversion;
using Xtensive.Core;


namespace Xtensive.Caching
{
  /// <summary>
  /// A set of items limited by the maximal amount of memory it can use, or by any other measure.
  /// Stores as many most frequently accessed items in memory as long as it is possible
  /// while maintaining the total size of cached items less or equal to <see cref="MaxSize"/>.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  public class FastConcurrentLruCache<TKey, TItem> :
    ICache<TKey, TItem>
  {
    private FastConcurrentLru<TKey, TItem> imp;

    public Converter<TItem, TKey> KeyExtractor { get; private set; }

    public int Count => imp.Count;

    public long MaxSize { get; private set; }

    //TODO: Change to imp.Clear() after updating BitFaster.Caching package to 1.0.4
    public void Clear() =>
      imp = new FastConcurrentLru<TKey, TItem>((int)MaxSize);

    public bool TryGetItem(TKey key, bool markAsHit, out TItem item) => imp.TryGet(key, out item);

    public bool ContainsKey(TKey key) => imp.TryGet(key, out var _);

    public TItem Add(TItem item, bool replaceIfExists)
    {
      var key = KeyExtractor(item);
      if (replaceIfExists) {
        imp.AddOrUpdate(key, item);
        return item;
      }
      else {
        return imp.GetOrAdd(key, _ => item);
      }
    }

    public void RemoveKey(TKey key) => imp.TryRemove(key);

    public void RemoveKey(TKey key, bool removeCompletely) => imp.TryRemove(key);

    public IEnumerator<TItem> GetEnumerator() => throw new NotImplementedException();

    public FastConcurrentLruCache(int maxSize, Converter<TItem, TKey> keyExtractor)
    {
      if (maxSize <= 0)
        ArgumentValidator.EnsureArgumentIsInRange(maxSize, 1, int.MaxValue, "maxSize");
      MaxSize = maxSize;
      KeyExtractor = keyExtractor;
      imp = new FastConcurrentLru<TKey, TItem>(maxSize);
    }
  }
}
