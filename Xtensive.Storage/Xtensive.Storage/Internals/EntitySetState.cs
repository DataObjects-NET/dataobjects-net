// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Internals.DocTemplates;
using KeyCache = Xtensive.Core.Caching.ICache<Xtensive.Storage.Key, Xtensive.Storage.Key>;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Describes cached state of <see cref="EntitySetBase"/>
  /// </summary>
  [Serializable]
  public sealed class EntitySetState :
    IEnumerable<Key>,
    IHasVersion<long>
  {
    private readonly ICache<Key, Key> fetchedKeys;

    #region IHasVersion<...> methods

    /// <inheritdoc/>
    public long Version { get; private set; }

    /// <inheritdoc/>
    object IHasVersion.Version { get { return Version; } }

    #endregion

    /// <summary>
    /// Gets a value indicating whether state is fully loaded.
    /// </summary>
    public bool IsFullyLoaded { get { return TotalItemsCount==CachedItemsCount; } }

    /// <summary>
    /// Gets the total number of items.
    /// </summary>
    public long? TotalItemsCount { get; internal set; }

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    public long CachedItemsCount { get { return fetchedKeys.Count; } }

    /// <summary>
    /// Determines whether cached state contains specified item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Key key)
    {
      return fetchedKeys.ContainsKey(key);
    }

    /// <summary>
    /// Registers the specified fetched key in cached state.
    /// </summary>
    /// <param name="key">The key to register.</param>
    public void Register(Key key)
    {
      fetchedKeys.Add(key);
    }

    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <param name="key">The key to add.</param>
    public void Add(Key key)
    {
      Register(key);
      if (TotalItemsCount!=null)
        TotalItemsCount++;
      Version++;
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(Key key)
    {
      fetchedKeys.RemoveKey(key);
      if (TotalItemsCount!=null)
        TotalItemsCount--;
      Version++;
    }

    /// <summary>
    /// Clears fetched keys registry.
    /// </summary>
    public void Clear()
    {
      fetchedKeys.Clear();
      TotalItemsCount = null;
      Version++;
    }

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      return fetchedKeys.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxCacheSize">Maximal number of items to cache.</param>
    /// <inheritdoc/>
    public EntitySetState(long maxCacheSize)
    {
      fetchedKeys = new LruCache<Key, Key>(maxCacheSize, cachedKey => cachedKey);
    }
  }
}