// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    private class BackupedState
    {
      public bool IsLoaded { get; private set; }
      public long? TotalItemCount { get; private set; }
      public IEnumerable<Key> AddedKeys { get; private set; }
      public IEnumerable<Key> RemovedKeys { get; private set; }

      public BackupedState(EntitySetState state)
      {
        IsLoaded = state.IsLoaded;
        TotalItemCount = state.TotalItemCount;
        AddedKeys = state.addedKeys.Values.ToList();
        RemovedKeys = state.removedKeys.Values.ToList();
      }
    }

    private readonly EntitySetBase owner;

    private bool isLoaded;
    private long? totalItemCount;
    private int version;
    private IDictionary<Key, Key> addedKeys;
    private IDictionary<Key, Key> removedKeys;

    private BackupedState previousState;

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
      unchecked { Interlocked.Add(ref version, 1); }
      Rebind();
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(Key key)
    {
      if (addedKeys.ContainsKey(key))
        addedKeys.Remove(key);
      else
        removedKeys[key] = key;
      if (TotalItemCount!=null)
        TotalItemCount--;
      unchecked { Interlocked.Add(ref version, 1); }
      Rebind();
    }

    /// <summary>
    /// Applies all changes to state.
    /// </summary>
    public bool ApplyChanges()
    {
      if (HasChanges) {
        EnsureFetchedKeysIsNotNull();
        BackupState();
        var currentFetchedKeys = FetchedKeys;
        InitializeFetchedKeys();

        foreach (var currentFetchedKey in currentFetchedKeys)
          if(!removedKeys.ContainsKey(currentFetchedKey))
            FetchedKeys.Add(currentFetchedKey);
        foreach (var addedKey in addedKeys)
          FetchedKeys.Add(addedKey.Value);
        InitializeDifferenceCollections();
        Rebind();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Clear all changes.
    /// </summary>
    public void CancelChanges()
    {
      InitializeDifferenceCollections();
      unchecked { Interlocked.Add(ref version, 1); }
      Rebind();
    }

    internal void RollbackState()
    {
      if (previousState!=null) {
        TotalItemCount = previousState.TotalItemCount;
        IsLoaded = previousState.IsLoaded;
        var fetchedKeys = FetchedKeys;

        InitializeFetchedKeys();
        InitializeDifferenceCollections();

        foreach (var fetchedKey in fetchedKeys)
          FetchedKeys.Add(fetchedKey);

        foreach (var addedKey in previousState.AddedKeys) {
          if (fetchedKeys.ContainsKey(addedKey))
            FetchedKeys.Remove(addedKey);
          addedKeys.Add(addedKey, addedKey);
        }
        foreach (var removedKey in previousState.RemovedKeys) {
          if (!FetchedKeys.ContainsKey(removedKey))
            FetchedKeys.Add(removedKey);
          removedKeys.Add(removedKey, removedKey);
        }
      }
    }

    internal void RemapKeys(KeyMapping mapping)
    {
      var oldAddedKeys = addedKeys;
      var oldRemovedKeys = removedKeys;
      InitializeDifferenceCollections();

      foreach (var oldAddedKey in oldAddedKeys) {
        var newKey = mapping.TryRemapKey(oldAddedKey.Key);
        addedKeys.Add(newKey, newKey);
      }
      foreach (var oldRemovedKey in oldRemovedKeys) {
        var newKey = mapping.TryRemapKey(oldRemovedKey.Key);
        removedKeys.Add(newKey, newKey);
      }
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
      InitializeFetchedKeys();
    }

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      var versionSnapshot = version;
      var fetchedKeysBeforePersist = FetchedKeys;
      var addedKeysBeforePersist = addedKeys;
      var removedKeysBeforePersist = removedKeys;
      foreach (var fetchedKey in fetchedKeysBeforePersist) {
        if (versionSnapshot!=version)
          throw new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
        if (!removedKeysBeforePersist.ContainsKey(fetchedKey))
          yield return fetchedKey;
      }
      foreach (var addedKey in addedKeysBeforePersist) {
        if (versionSnapshot!=version)
          throw new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
        yield return addedKey.Value;
      }
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

    private void BackupState()
    {
      previousState = new BackupedState(this);
    }

    //private void InitializeFetchedKeysAndClearChanges()
    //{
    //  InitializeFetchedKeys();
    //  InitializeDifferenceCollections();
    //}

    private void InitializeFetchedKeys()
    {
      FetchedKeys = new LruCache<Key, Key>(WellKnown.EntitySetCacheSize, cachedKey => cachedKey);
    }

    private void InitializeDifferenceCollections()
    {
      addedKeys = new Dictionary<Key, Key>();
      removedKeys = new Dictionary<Key, Key>();
    }

    // Constructors

    internal EntitySetState(EntitySetBase entitySet)
      : base(entitySet.Session)
    {
      InitializeFetchedKeys();
      InitializeDifferenceCollections();
      owner = entitySet;
      version = int.MinValue;
    }
  }
}