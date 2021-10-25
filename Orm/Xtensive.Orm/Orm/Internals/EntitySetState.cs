// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xtensive.Caching;
using Xtensive.Core;
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
    private readonly bool isDisconnected;

    private Guid lastManualPrefetchId;
    private bool isLoaded;
    private long? totalItemCount;
    private int version;
    private IDictionary<Key, Key> addedKeys;
    private IDictionary<Key, Key> removedKeys;

    private BackupedState previousState;

    public KeyCache FetchedKeys
    {
      get => State;
      set => State = value;
    }

    /// <summary>
    /// Gets total count of elements which entity set contains.
    /// </summary>
    public long? TotalItemCount
    {
      get {
        EnsureIsActual();
        return totalItemCount;
      }
      internal set => totalItemCount = value;
    }

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    public long CachedItemCount
      => FetchedItemsCount - RemovedItemsCount + AddedItemsCount;

    /// <summary>
    /// Gets the number of fetched keys.
    /// </summary>
    public long FetchedItemsCount => FetchedKeys.Count;

    /// <summary>
    /// Gets count of keys which was added but changes are not applyed.
    /// </summary>
    public int AddedItemsCount => addedKeys.Count;

    /// <summary>
    /// Gets count of keys which was removed but changes are not applied.
    /// </summary>
    public int RemovedItemsCount => removedKeys.Count;

    /// <summary>
    /// Gets a value indicating whether state contains all keys which stored in database.
    /// </summary>
    public bool IsFullyLoaded => TotalItemCount == CachedItemCount;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is loaded.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is preloaded; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsLoaded
    {
      get {
        EnsureIsActual();
        return isLoaded;
      }
      internal set => isLoaded = value;
    }

    /// <summary>
    /// Get value indicating whether state has changes.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this state has changes; otherwise, <see langword="false"/>.
    /// </value>
    public bool HasChanges
      => AddedItemsCount != 0 || RemovedItemsCount != 0;

    /// <summary>
    /// Sets cached keys to <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The keys.</param>
    /// <param name="count">Total item count.</param>
    public void Update(IEnumerable<Key> keys, long? count)
    {
      if (HasChanges) {
        UpdateCachedState(keys, count);
      }
      else {
        UpdateSyncedState(keys, count);
      }
    }

    /// <summary>
    /// Determines whether cached state contains specified item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Key key)
    {
      if (removedKeys.ContainsKey(key)) {
        return false;
      }
      if (addedKeys.ContainsKey(key)) {
        return true;
      }
      return FetchedKeys.ContainsKey(key);
    }

    /// <summary>
    /// Registers the specified fetched key in cached state.
    /// </summary>
    /// <param name="key">The key to register.</param>
    public void Register(Key key) => FetchedKeys.Add(key);

    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <param name="key">The key to add.</param>
    public void Add(Key key)
    {
      if (!removedKeys.Remove(key)) {
        addedKeys[key] = key;
      }
      if (TotalItemCount != null) {
        TotalItemCount++;
      }

      unchecked {
        _ = Interlocked.Add(ref version, 1);
      }
      Rebind();
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void Remove(Key key)
    {
      if (!addedKeys.Remove(key)) {
        removedKeys[key] = key;
      }
      if (TotalItemCount!=null) {
        TotalItemCount--;
      }

      unchecked {
        _ = Interlocked.Add(ref version, 1);
      }
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

        foreach (var currentFetchedKey in currentFetchedKeys) {
          if (!removedKeys.ContainsKey(currentFetchedKey)) {
            FetchedKeys.Add(currentFetchedKey);
          }
        }

        foreach (var addedKey in addedKeys) {
          FetchedKeys.Add(addedKey.Value);
        }
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
      unchecked {
        _ =  Interlocked.Add(ref version, 1);
      }
      Rebind();
    }

    internal void RollbackState()
    {
      if (previousState != null) {
        TotalItemCount = previousState.TotalItemCount;
        IsLoaded = previousState.IsLoaded;
        var fetchedKeys = FetchedKeys;

        InitializeFetchedKeys();
        InitializeDifferenceCollections();

        foreach (var fetchedKey in fetchedKeys) {
          FetchedKeys.Add(fetchedKey);
        }

        foreach (var addedKey in previousState.AddedKeys) {
          if (fetchedKeys.ContainsKey(addedKey)) {
            FetchedKeys.Remove(addedKey);
          }
          addedKeys.Add(addedKey, addedKey);
        }
        foreach (var removedKey in previousState.RemovedKeys) {
          if (!FetchedKeys.ContainsKey(removedKey)) {
            FetchedKeys.Add(removedKey);
          }
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

    internal bool ShouldUseForcePrefetch(Guid? currentPrefetchOperation)
    {
      if (currentPrefetchOperation.HasValue) {
        if (currentPrefetchOperation.Value == lastManualPrefetchId) {
          return false;
        }

        lastManualPrefetchId = currentPrefetchOperation.Value;
      }

      if (Session.Transaction != null) {
        switch (Session.Transaction.Outermost.IsolationLevel) {
          case System.Transactions.IsolationLevel.ReadCommitted:
          case System.Transactions.IsolationLevel.ReadUncommitted:
            return true;
          case System.Transactions.IsolationLevel.RepeatableRead:
            return string.Equals(Session.Handlers.ProviderInfo.ProviderName, WellKnown.Provider.SqlServer, StringComparison.Ordinal);
          default:
            return false;
        }
      }

      if (isDisconnected) {
        return true;
      }

      return false;
    }

    internal void SetLastManualPrefetchId(Guid? prefetchOperationId)
    {
      if (prefetchOperationId.HasValue) {
        lastManualPrefetchId = prefetchOperationId.Value;
      }
    }

    /// <inheritdoc/>
    protected override void Invalidate()
    {
      TotalItemCount = null;
      IsLoaded = false;
      base.Invalidate();
    }

    void IInvalidatable.Invalidate() => Invalidate();

    /// <inheritdoc/>
    protected override void Refresh() => InitializeFetchedKeys();

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      var versionSnapshot = version;
      using (var fetchedKeysEnumerator = FetchedKeys.GetEnumerator()) {
        while (true) {
          if (versionSnapshot != version) {
            throw new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
          }

          if (!fetchedKeysEnumerator.MoveNext()) {
            break;
          }

          var fetchedKey = fetchedKeysEnumerator.Current;
          if (!removedKeys.ContainsKey(fetchedKey)) {
            yield return fetchedKey;
          }
        }
      }

      using (var addedKeysEnumerator = addedKeys.GetEnumerator()) {
        while (true) {
          if (versionSnapshot != version) {
            throw new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
          }

          if (!addedKeysEnumerator.MoveNext()) {
            break;
          }

          var addedKey = addedKeysEnumerator.Current;
          yield return addedKey.Value;
        }
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    private void UpdateSyncedState(IEnumerable<Key> keys, long? count)
    {
      FetchedKeys.Clear();
      TotalItemCount = count;
      foreach (var key in keys) {
        FetchedKeys.Add(key);
      }
      Rebind();
    }

    public void UpdateCachedState(IEnumerable<Key> syncronizedKeys, long? count)
    {
      FetchedKeys.Clear();
      var becameRemovedOnSever = new HashSet<Key>(removedKeys.Keys);
      foreach (var key in syncronizedKeys) {
        if (!addedKeys.Remove(key)) {
          _ = becameRemovedOnSever.Remove(key);
        }
        FetchedKeys.Add(key);
      }
      foreach (var removedOnServer in becameRemovedOnSever) {
        _ = removedKeys.Remove(removedOnServer);
      }

      TotalItemCount = count.HasValue
        ? FetchedKeys.Count - removedKeys.Count + AddedItemsCount
        : count;
    }

    private void EnsureFetchedKeysIsNotNull()
    {
      if (FetchedKeys == null) {
        InitializeFetchedKeys();
      }
    }

    private void BackupState() => previousState = new BackupedState(this);

    private void InitializeFetchedKeys()
      => FetchedKeys = new LruCache<Key, Key>(WellKnown.EntitySetCacheSize, cachedKey => cachedKey);

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
      isDisconnected = entitySet.Session.IsDisconnected;
      lastManualPrefetchId = Guid.Empty;
    }
  }
}
