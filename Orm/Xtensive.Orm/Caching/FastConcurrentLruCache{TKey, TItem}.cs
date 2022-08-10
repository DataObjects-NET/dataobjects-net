// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using BitFaster.Caching.Lru;
using Xtensive.Core;


namespace Xtensive.Caching
{
  /// <summary>
  /// An adapter for <see cref="BitFaster.Caching.Lru.FastConcurrentLru{K, V}"/> type.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  public class FastConcurrentLruCache<TKey, TItem> :
    CacheBase<TKey, TItem>
  {
    private readonly FastConcurrentLru<TKey, TItem> realCache;

    /// <inheritdoc/>
    public override int Count => realCache.Count;

    /// <inheritdoc/>
    public long MaxSize { get; private set; }

    public IEnumerable<TKey> Keys => realCache.Keys;

    /// <inheritdoc/>
    public override void Clear() => realCache.Clear();

    /// <inheritdoc/>
    public override bool TryGetItem(TKey key, bool markAsHit, out TItem item) => realCache.TryGet(key, out item);

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key) => realCache.TryGet(key, out var _);

    /// <inheritdoc/>
    public override TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var key = KeyExtractor(item);
      if (replaceIfExists) {
        realCache.AddOrUpdate(key, item);
        return item;
      }
      else {
        return realCache.GetOrAdd(key, _ => item);
      }
    }

    /// <inheritdoc/>
    public override void RemoveKey(TKey key) => realCache.TryRemove(key);

    /// <inheritdoc/>
    public override void RemoveKey(TKey key, bool removeCompletely) => realCache.TryRemove(key);

    /// <exception cref="NotImplementedException"/>
    public override IEnumerator<TItem> GetEnumerator() => throw new NotImplementedException();


    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="maxSize">Max size of the original cache. Ideally it should be devisible by 3</param>
    /// <param name="keyExtractor">Extracts key value from caching item.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxSize"/> is less than 3.</exception>
    public FastConcurrentLruCache(int maxSize, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(maxSize, 3, nameof(maxSize));
      MaxSize = maxSize;
      KeyExtractor = keyExtractor;
      realCache = new FastConcurrentLru<TKey, TItem>(maxSize);
    }
  }
}
