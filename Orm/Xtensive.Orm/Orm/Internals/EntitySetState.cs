// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Caching;
using KeyCache = Xtensive.Caching.ICache<Xtensive.Orm.Key, Xtensive.Orm.Key>;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Describes cached state of <see cref="EntitySetBase"/>
  /// </summary>
  public sealed class EntitySetState : TransactionalStateContainer<KeyCache>,
    IEnumerable<Key>,
    IInvalidatable
  {
    private readonly IDictionary<Key, Key> addedKeys;
    private readonly IDictionary<Key, Key> removedKeys;
    private bool isLoaded;
    private long? totalItemCount;

    public KeyCache FetchedKeys {
      get { return State; }
      set { State = value; }
    }

    /// <summary>
    /// Gets the total number of items.
    /// </summary>
    public long? TotalItemCount {
      get {
        EnsureIsActual();
        return totalItemCount;
      }
      internal set {
        totalItemCount = value;
      }
    }

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    public long CachedItemCount { get { return FetchedKeys.Except(removedKeys.Values).Concat(addedKeys.Values).Count(); } }

    /// <summary>
    /// Gets a value indicating whether state is fully loaded.
    /// </summary>
    public bool IsFullyLoaded { get { return TotalItemCount==CachedItemCount; } }
    
    /// <summary>
    /// Gets or sets a value indicating whether this instance is loaded.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is preloaded; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsLoaded {
      get {
        EnsureIsActual();
        return isLoaded;
      }
      internal set {
        isLoaded = value;
      }
    }

    /// <summary>
    /// Get value indicating whether state has changes.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this state has changes; otherwise, <see langword="false"/>.
    /// </value>
    public bool HasChanges
    {
      get { return addedKeys.Count!=0 || removedKeys.Count!=0; }
    }

    /// <summary>
    /// Sets cached keys to <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The keys.</param>
    public void Update(IEnumerable<Key> keys, long? count)
    {
      FetchedKeys.Clear();
      TotalItemCount = count;
      foreach (var key in keys)
        FetchedKeys.Add(key);
      Rebind();
    }

    /// <summary>
    /// Determines whether cached state contains specified item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Key key)
    {
      if (removedKeys.ContainsKey(key))
        return false;
      if (addedKeys.ContainsKey(key))
        return true;
      if (FetchedKeys.ContainsKey(key))
        return true;
      return false;
    }

    /// <summary>
    /// Registers the specified fetched key in cached state.
    /// </summary>
    /// <param name="key">The key to register.</param>
    public void Register(Key key)
    {
      FetchedKeys.Add(key);
    }

    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <param name="key">The key to add.</param>
    public void Add(Key key)
    {
      if (!addedKeys.ContainsKey(key)) {
        addedKeys.Add(key, key);
        removedKeys.Remove(key);
        if (TotalItemCount != null)
          TotalItemCount++;
      }
      Rebind();
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(Key key)
    {
      if (!removedKeys.ContainsKey(key)) {
        removedKeys.Add(key, key);
        addedKeys.Remove(key);
        if (TotalItemCount!=null)
          TotalItemCount--;
      }
      Rebind();
    }

    /// <summary>
    /// Applies all changes to state.
    /// </summary>
    public bool ApplyChanges()
    {
      if (HasChanges) {
        for (var index = removedKeys.Count - 1; index > -1; index--) {
          var removedKey = removedKeys.ElementAt(index).Key;
          FetchedKeys.Remove(removedKey);
        }
        for (var index = addedKeys.Count - 1; index > -1; index--) {
          var addedKey = addedKeys.ElementAt(index).Key;
          FetchedKeys.Add(addedKey);
        }
        CancelChanges();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Clear all changes.
    /// </summary>
    public void CancelChanges()
    {
      addedKeys.Clear();
      removedKeys.Clear();
    }

    /// <inheritdoc/>
    protected override void Invalidate()
    {
      TotalItemCount = null;
      IsLoaded = false;
      base.Invalidate();
    }

    void IInvalidatable.Invalidate()
    {
      Invalidate();
    }

    /// <inheritdoc/>
    protected override void Refresh()
    {
      FetchedKeys = new LruCache<Key, Key>(WellKnown.EntitySetCacheSize, cachedKey => cachedKey);
      CancelChanges();
    }

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      return FetchedKeys.Except(removedKeys.Values).Concat(addedKeys.Values).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    internal EntitySetState(EntitySetBase entitySet)
      : base(entitySet.Session)
    {
      addedKeys = new Dictionary<Key, Key>();
      removedKeys = new Dictionary<Key, Key>();
    }
  }
}