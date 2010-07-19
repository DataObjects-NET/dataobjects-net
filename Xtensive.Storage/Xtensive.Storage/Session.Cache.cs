// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public partial class Session
  {
    internal ICache<Key, EntityState> EntityStateCache { get; private set; }
    internal EntityChangeRegistry EntityChangeRegistry { get; private set; }

    internal void Invalidate()
    {
      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXInvalidate, this);
      ClearChangeRegistry();
      foreach (var invalidable in EntityStateCache.Cast<IInvalidatable>())
        invalidable.Invalidate();
    }

    internal void RemapEntityKeys(KeyMapping keyMapping)
    {
      if (keyMapping.Map.Count==0)
        return;
      Persist();
      Invalidate();
      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXRemappingEntityKeys, this);
      var oldCacheContent = EntityStateCache.ToDictionary(entityState => entityState.Key);
      EntityStateCache.Clear();
      foreach (var pair in oldCacheContent) {
        var key = pair.Key;
        var entityState = pair.Value;
        var remappedKey = keyMapping.TryRemapKey(key);
        if (remappedKey!=key)
          entityState.RemapKey(remappedKey);
        EntityStateCache.Add(entityState);
      }
    }

    internal void EnforceChangeRegistrySizeLimit()
    {
      if (EntityChangeRegistry.Count>=Configuration.EntityChangeRegistrySize)
        Persist(PersistReason.ChangeRegistrySizeLimit);
    }

    internal EntityState CreateEntityState(Key key)
    {
      // Checking for deleted entity with the same key
      var cachedState = EntityStateCache[key, false];
      if (cachedState != null && cachedState.PersistenceState==PersistenceState.Removed)
        Persist();
      else
        EnforceChangeRegistrySizeLimit(); // Must be done before new entity registration

      // If type is unknown, we consider tuple is null, 
      // so its Entity is considered as non-existing
      Tuple tuple = null;
      if (key.HasExactType)
        // A tuple with all the fields set to default values rather then N/A
        tuple = key.Type.CreateEntityTuple(key.Value);

      var result = new EntityState(this, key, tuple) {
        PersistenceState = PersistenceState.New
      };
      EntityStateCache.Add(result);

      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.SessionXCachingY, this, result);
      return result;
    }

    /// <exception cref="InvalidOperationException">
    /// Attempt to associate non-null <paramref name="tuple"/> with <paramref name="key"/> of unknown type.
    /// </exception>
    internal EntityState UpdateEntityState(Key key, Tuple tuple, bool isStale)
    {
      var result = EntityStateCache[key, true];
      if (result == null) {
        if (!key.HasExactType && tuple!=null)
          throw Exceptions.InternalError(Strings.ExCannotAssociateNonEmptyEntityStateWithKeyOfUnknownType,
            Log.Instance);
        result = AddEntityStateToCache(key, tuple, isStale);
      }
      else {
        if (!result.Key.HasExactType && key.HasExactType) {
          EntityStateCache.RemoveKey(result.Key);
          result = AddEntityStateToCache(key, tuple, result.IsStale);
        }
        result.Update(tuple);
        result.IsStale = isStale;
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.SessionXUpdatingCacheY, this, result);
      }
      return result;
    }

    internal EntityState UpdateEntityState(Key key, Tuple tuple)
    {
      return UpdateEntityState(key, tuple, false);
    }

    internal EntitySetState UpdateEntitySetState(Key key, FieldInfo fieldInfo, IEnumerable<Key> items, bool isFullyLoaded)
    {
      var entityState = EntityStateCache[key, true];
      if (entityState==null)
        return null;
      var entity = entityState.Entity;
      if (entity==null)
        return null;
      var entitySet = (EntitySetBase) entity.GetFieldValue(fieldInfo);
      return entitySet.UpdateState(items, isFullyLoaded);
    }

    internal void UpdateCache(RecordSet source)
    {
      var reader = Domain.RecordSetReader;
      foreach (var record in reader.Read(source, source.Header)) {
        for (int i = 0; i < record.Count; i++) {
          var key = record.GetKey(i);
          if (key==null)
            continue;
          var tuple = record.GetTuple(i);
          if (tuple==null)
            continue;
          UpdateEntityState(key, tuple);
        }
      }
    }

    private EntityState AddEntityStateToCache(Key key, Tuple tuple, bool isStale)
    {
      var result = new EntityState(this, key, tuple, isStale) {
        PersistenceState = PersistenceState.Synchronized
      };
      EntityStateCache.Add(result);
      if (IsDebugEventLoggingEnabled)
        Log.Debug(Strings.SessionXCachingY, this, result);
      return result;
    }

    private static ICache<Key,EntityState> CreateSessionCache(SessionConfiguration configuration)
    {
      switch (configuration.CacheType) {
      case SessionCacheType.Infinite:
        return new InfiniteCache<Key, EntityState>(configuration.CacheSize, i => i.Key);
      default:
        return new LruCache<Key, EntityState>(configuration.CacheSize, i => i.Key,
          new WeakCache<Key, EntityState>(false, i => i.Key));
      }
    }
  }
}