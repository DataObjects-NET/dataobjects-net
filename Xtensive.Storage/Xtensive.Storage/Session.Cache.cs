// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public partial class Session
  {
    internal ICache<Key, EntityState> EntityStateCache { get; private set; }
    internal EntityChangeRegistry EntityChangeRegistry { get; private set; }

    internal void EnforceChangeRegistrySizeLimit()
    {
      if (EntityChangeRegistry.Count>=EntityChangeRegistrySizeLimit)
        Persist();
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
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.SessionXUpdatingCacheY, this, result);
      }
      return result;
    }

    internal EntityState UpdateEntityState(Key key, Tuple tuple)
    {
      return UpdateEntityState(key, tuple, false);
    }

    internal void UpdateCacheFrom(RecordSet source)
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
  }
}