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

    public bool IsFullyLoaded
    {
      get
      {
        return count == keys.Count;
      }
    }

    public long Count
    {
      get { return count;}
    }

    public bool Contains(Key key)
    {
      return keys.ContainsKey(key);
    }

    public void Register(Key key)
    {
      keys.Add(key);
    }

    public void Add(Key key)
    {
      Register(key);
      count++;
      Version++;
    }

    public void Remove(Key key)
    {
      keys.RemoveKey(key);
      count--;
      Version++;
    }

    public void Clear()
    {
      keys.Clear();
      count = 0;
      Version++;
    }

    #region GetEnumerator<...> methods

    public IEnumerator<Key> GetEnumerator()
    {
      return keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="cacheSize">Size of the cache.</param>
    /// <inheritdoc/>
    public EntitySetState(long cacheSize)
    {
      keys = new LruCache<Key, Key>(cacheSize, cachedKey => cachedKey);
    }
  }
}