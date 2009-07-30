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
    internal long count;
    internal ICache<Key, Key> keys;

    #region IHasVersion<...> methods

    /// <inheritdoc/>
    public long Version { get; private set; }

    /// <inheritdoc/>
    object IHasVersion.Version
    {
      get { return Version; }
    }

    #endregion

    /// <summary>
    /// Gets a value indicating whether state is fully loaded.
    /// </summary>
    public bool IsFullyLoaded {
      get {
        return count == keys.Count;
      }
    }

    /// <summary>
    /// Gets the count of cached items.
    /// </summary>
    public long Count {
      get { return count;}
    }

    /// <summary>
    /// Determines whether cached state contains specified item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Key key)
    {
      return keys.ContainsKey(key);
    }

    /// <summary>
    /// Registers the specified fetched key in cached state.
    /// </summary>
    /// <param name="key">The key to register.</param>
    public void Register(Key key)
    {
      keys.Add(key);
    }

    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <param name="key">The key to add.</param>
    public void Add(Key key)
    {
      Register(key);
      count++;
      Version++;
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(Key key)
    {
      keys.RemoveKey(key);
      count--;
      Version++;
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
      keys.Clear();
      count = 0;
      Version++;
    }

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      return keys.GetEnumerator();
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
    /// <param name="maxCacheSize">Maximal count of items to cache.</param>
    /// <inheritdoc/>
    public EntitySetState(long maxCacheSize)
    {
      keys = new LruCache<Key, Key>(maxCacheSize, cachedKey => cachedKey);
    }
  }
}