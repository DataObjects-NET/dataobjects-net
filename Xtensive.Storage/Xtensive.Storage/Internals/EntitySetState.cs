// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Caching;
using KeyCache = Xtensive.Core.Caching.ICache<Xtensive.Storage.Key, Xtensive.Storage.Key>;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Describes cached state of <see cref="EntitySetBase"/>
  /// </summary>
  public sealed class EntitySetState : TransactionalStateContainer<KeyCache>,
    IEnumerable<Key>
  {
    private bool isLoaded;
    private long? totalItemsCount;

    private KeyCache FetchedKeys {
      get { return State; }
      set { State = value; }
    }

    /// <summary>
    /// Gets the total number of items.
    /// </summary>
    [Infrastructure]
    public long? TotalItemsCount {
      get {
        EnsureStateIsActual();
        return totalItemsCount;
      }
      internal set {
        totalItemsCount = value;
      }
    }

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    [Infrastructure]
    public long CachedItemsCount { get { return FetchedKeys.Count; } }

    /// <summary>
    /// Gets a value indicating whether state is fully loaded.
    /// </summary>
    [Infrastructure]
    public bool IsFullyLoaded { get { return TotalItemsCount==CachedItemsCount; } }
    
    /// <summary>
    /// Gets or sets a value indicating whether this instance is loaded.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is preloaded; otherwise, <see langword="false"/>.
    /// </value>
    [Infrastructure]
    public bool IsLoaded {
      get {
        EnsureStateIsActual();
        return isLoaded;
      }
      internal set {
        isLoaded = value;
      }
    }

    /// <summary>
    /// Sets cached keys to <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The keys.</param>
    [Infrastructure]
    public void Update(IEnumerable<Key> keys, long? count)
    {
      FetchedKeys.Clear();
      TotalItemsCount = count;
      foreach (var key in keys)
        FetchedKeys.Add(key);
      MarkStateAsModified();
    }

    /// <summary>
    /// Determines whether cached state contains specified item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Check result.</returns>
    [Infrastructure]
    public bool Contains(Key key)
    {
      return FetchedKeys.ContainsKey(key);
    }

    /// <summary>
    /// Registers the specified fetched key in cached state.
    /// </summary>
    /// <param name="key">The key to register.</param>
    [Infrastructure]
    public void Register(Key key)
    {
      FetchedKeys.Add(key);
    }

    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <param name="key">The key to add.</param>
    [Infrastructure]
    public void Add(Key key)
    {
      FetchedKeys.Add(key);
      if (TotalItemsCount!=null)
        TotalItemsCount++;
      MarkStateAsModified();
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    [Infrastructure]
    public void Remove(Key key)
    {
      FetchedKeys.RemoveKey(key);
      if (TotalItemsCount!=null)
        TotalItemsCount--;
      MarkStateAsModified();
    }

    /// <inheritdoc/>
    protected override void ResetState()
    {
      TotalItemsCount = null;
      IsLoaded = false;
      base.ResetState();
    }

    /// <inheritdoc/>
    protected override void LoadState()
    {
      FetchedKeys = new LruCache<Key, Key>(WellKnown.EntitySetCacheSize, cachedKey => cachedKey);
    }

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    [Infrastructure]
    public IEnumerator<Key> GetEnumerator()
    {
      return FetchedKeys.GetEnumerator();
    }

    /// <inheritdoc/>
    [Infrastructure]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    internal EntitySetState(EntitySetBase entitySet)
      : base(entitySet.Session)
    {
    }
  }
}