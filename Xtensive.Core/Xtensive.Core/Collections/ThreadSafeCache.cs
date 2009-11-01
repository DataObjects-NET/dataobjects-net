// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.06.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <para>Represents a set of items
  /// that has a limit by the maximal amount of memory it can use.</para>
  /// <para><see cref="ThreadSafeCache{TKey,TValue}"/> holds most frequently 
  /// accessed items in memory as long as possible while the remaining items 
  /// can be removed if the maximum cache size exceeded.
  /// </para>
  /// </summary>
  /// <typeparam name="TKey">Identifier type of the item.</typeparam>
  /// <typeparam name="TValue">Type of the item to cache.</typeparam>
  public class ThreadSafeCache<TKey, TValue>: ICache<TKey, TValue>,
                                    ISynchronizable
    where TValue : class
  {
    private const int minMaxSize = 1*10*1024;

    protected readonly ReaderWriterLockSlim rwLock;
    private readonly TopDeque<TKey, TValue> storage;
    private readonly int maxSize = 1*1024*1024;
    private int size;
    private readonly ICacheConverter<TValue> cacheConverter;
    private Converter<TValue, TKey> keyExtractor;

    // Overrides
    protected virtual void ItemAdded(TKey key){}
    protected virtual void ItemRemoved(TKey key) { }
    protected virtual void Cleared() { }


    /// <inheritdoc/>
    public bool IsSynchronized
    {
      get { return rwLock!=null; }
    }

    /// <summary>
    /// Gets or sets the synchronization root of the object.
    /// </summary>
    public object SyncRoot
    {
      get { return IsSynchronized ? (object)rwLock : this; }
    }

    /// <inheritdoc/>
    public int CurrentCount
    {
      get {
        return rwLock.ExecuteReader(() => storage.Count);
      }
    }

    /// <inheritdoc/>
    public int CurrentSize
    {
      get {
        return rwLock.ExecuteReader(() => size);
      }
    }

    /// <summary>
    /// Gets the key extractor.
    /// </summary>
    /// <value>The key extractor.</value>
    public Converter<TValue, TKey> KeyExtractor
    {
      get { return keyExtractor; }
    }

    /// <inheritdoc/>
    object ICache.this[object identity]
    {
      get {
        return identity is TKey ? this[(TKey)identity] : default(TValue);
      }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// While adding a new <paramref name="item"/>
    /// in the case the maximal cache size is exceeded
    /// the <see cref="ThreadSafeCache{TKey,TValue}"/> can decide to remove some elements 
    /// which were last time accessed earlier than remaining ones.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The specified 
    /// <paramref name="item"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The specified 
    /// <paramref name="item"/> cannot be cached because its type 
    /// is incompatible with underlying storage format.</exception>
    public void Add(object item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      TValue cacheableItem;
      try {
        cacheableItem = (TValue)item;
      }
      catch(InvalidCastException) {
        throw new ArgumentException(Strings.ExItemCantBeCachedIncompatibleType);
      }
      Add(cacheableItem);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">The specified 
    /// <paramref name="item"/> is <see langword="null"/>.</exception>
    public void Remove(object item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      TValue cacheableItem;
      try {
        cacheableItem = (TValue)item;
      }
      catch (InvalidCastException) {
        throw new ArgumentException(Strings.ExItemCantBeCachedIncompatibleType);
      }
      Remove(cacheableItem);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      rwLock.ExecuteWriter(delegate {
        storage.Clear();
        Cleared();
      });
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    /// <remarks>The <see cref="ThreadSafeCache{TKey,TValue}"/> marks all items accessed via
    /// that dictionary as newer ones. So most frequently accessed items
    /// remains cached whereas another ones could be removed in the
    /// case when maximal cache size is exceeded.</remarks>
    public TValue this[TKey identity] {
      get {
        return
          rwLock.ExecuteReader(delegate {
          TValue item = storage[identity];
            if (item != null) {
              rwLock.ExecuteWriter(delegate {
                if (storage.Contains(identity))
                  storage.MoveToTop(identity);
                else
                  storage.AddToTop(identity, item);
              });
              return cacheConverter.FromCached(item);
            }
            return null;
          });
      }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// While adding a new <paramref name="item"/>
    /// in the case the maximal cache size is exceeded
    /// the <see cref="ThreadSafeCache{TKey,TValue}"/> can decide to remove some elements 
    /// which were last time accessed earlier than remaining ones.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The specified 
    /// <paramref name="item"/> is <see langword="null"/>.</exception>
    public void Add(TValue item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      TKey identity = KeyExtractor(item);
      rwLock.ExecuteWriter(delegate {
        TValue oldItemClone = storage.Contains(identity) ? storage[identity] : null;
        if (oldItemClone!=null) {
          size -= cacheConverter.GetSize(oldItemClone);
          storage.Remove(identity);
        }
        else
          ItemAdded(identity);
        TValue itemClone = cacheConverter.ToCached(item);
        size += cacheConverter.GetSize(itemClone);
        storage.AddToTop(identity, itemClone);
        while (size > maxSize) {
          oldItemClone = storage.PeekBottom();
          size -= cacheConverter.GetSize(oldItemClone);
          ItemRemoved(KeyExtractor(oldItemClone));
        }
      });
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">The specified 
    /// <paramref name="item"/> is <see langword="null"/>.</exception>
    public void Remove(TValue item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      TKey identity = KeyExtractor(item);
      rwLock.ExecuteWriter(delegate {
        TValue oldItemClone = storage.Contains(identity) ? storage[identity] : null;
        if (oldItemClone != null) {
          size -= cacheConverter.GetSize(oldItemClone);
          storage.Remove(identity);
          ItemRemoved(KeyExtractor(oldItemClone));
        }
      });
    }

    /// <inheritdoc/>
    public IEnumerator<TValue> GetEnumerator()
    {
      using (rwLock.ReadRegion()) {
        foreach (TValue itemClone in storage)
          yield return cacheConverter.FromCached(itemClone);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="cacheConverter">A cache converter.</param>
    /// <param name="keyExtractor"><typeparamref name="TValue"/> to 
    /// <typeparamref name="TKey"/> converter.</param>
    public ThreadSafeCache(ICacheConverter<TValue> cacheConverter,
      Converter<TValue, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "valueToKeyConverter");

      rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
      storage = new TopDeque<TKey, TValue>();
      this.cacheConverter = cacheConverter;
      this.keyExtractor = keyExtractor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize">A maximum amount of memory in bytes that can be used
    /// for the caching purposes.</param>
    /// <param name="cacheConverter">A cache converter.</param>
    /// <param name="keyExtractor"><typeparamref name="TValue"/> to 
    /// <typeparamref name="TKey"/> converter.</param>
    /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="maxSize"/>
    /// value must be greater then zero.</exception>
    public ThreadSafeCache(int maxSize, ICacheConverter<TValue> cacheConverter,
      Converter<TValue, TKey> keyExtractor)
      : this(cacheConverter, keyExtractor)
    {
      if (maxSize<=0)
        throw new ArgumentOutOfRangeException("maxSize", Strings.ExArgumentValueMustBeGreaterThanZero);
      this.maxSize = maxSize < minMaxSize ? minMaxSize : maxSize;
    }
  }
}
