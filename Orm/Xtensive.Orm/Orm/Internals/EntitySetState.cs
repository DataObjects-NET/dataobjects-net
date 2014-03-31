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
    /// Gets total count of elements which entity set contains.
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
    public long CachedItemCount { get { return FetchedItemsCount - RemovedItemsCount + AddedItemsCount; } }

    /// <summary>
    /// Gets the number of fetched keys.
    /// </summary>
    public long FetchedItemsCount
    {
      get { return FetchedKeys.Count; }
    }

    /// <summary>
    /// Gets count of keys which was added but changes are not applyed.
    /// </summary>
    public int AddedItemsCount
    {
      get { return addedKeys.Count; }
    }

    /// <summary>
    /// Gets count of keys which was removed but changes are not applied.
    /// </summary>
    public int RemovedItemsCount
    {
      get { return removedKeys.Count; }
    }

    /// <summary>
    /// Gets a value indicating whether state contains all keys which stored in database.
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
      get { return AddedItemsCount!=0 || RemovedItemsCount!=0; }
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
      if (removedKeys.ContainsKey(key))
        removedKeys.Remove(key);
      else
        addedKeys[key] = key;
      if (TotalItemCount!=null)
        TotalItemCount++;
      Rebind();
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(Key key)
    {
      EnsureFetchedKeysIsNotNull();
      if (addedKeys.ContainsKey(key))
        addedKeys.Remove(key);
      else
        removedKeys[key] = key;
      if (TotalItemCount!=null)
        TotalItemCount--;
      Rebind();
    }

    /// <summary>
    /// Applies all changes to state.
    /// </summary>
    public bool ApplyChanges()
    {
      if (HasChanges) {
        EnsureFetchedKeysIsNotNull();
        foreach (var removedKey in removedKeys)
          FetchedKeys.RemoveKey(removedKey.Value);
        foreach (var addedKey in addedKeys)
          FetchedKeys.Add(addedKey.Value);
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
      Rebind();
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
      InitializeFetchedKeysAndClearChanges();
    }

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      return FetchedKeys.Where(el => !removedKeys.ContainsKey(el)).Concat(addedKeys.Values).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    private void EnsureFetchedKeysIsNotNull()
    {
      if (FetchedKeys==null)
        InitializeFetchedKeys();
    }

    private void InitializeFetchedKeysAndClearChanges()
    {
      InitializeFetchedKeys();
      CancelChanges();
    }

    private void InitializeFetchedKeys()
    {
      FetchedKeys = new LruCache<Key, Key>(WellKnown.EntitySetCacheSize, cachedKey => cachedKey);
    }

    // Constructors

    internal EntitySetState(EntitySetBase entitySet)
      : base(entitySet.Session)
    {
      addedKeys = new Dictionary<Key, Key>();
      removedKeys = new Dictionary<Key, Key>();
    }
  }
}