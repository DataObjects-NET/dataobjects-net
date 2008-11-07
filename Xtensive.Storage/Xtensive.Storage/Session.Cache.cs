// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage
{
  public partial class Session
  {
    internal ICache<Key, EntityState> EntityStateCache { get; private set; }
    internal EntityStateRegistry EntityStateRegistry { get; private set; }

    internal EntityState CreateNewEntityState(Key key)
    {
      EntityState result;
      if (key.IsTypeCached) {
        // New instance contains a tuple with all fields set with default values.
        var origin = Domain.PersistentTuplePrototypes[key.Type].Clone();
        key.Value.CopyTo(origin);
        result = new EntityState(this, key, origin);
      }
      else {
        // Key belongs to non-existing Entity
        result = new EntityState(this, key, null);
      }
      result.PersistenceState = PersistenceState.New;
      EntityStateCache.Add(result);

      if (IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Caching: {1}", this, result);
      return result;
    }

    internal EntityState UpdateEntityState(Key key, Tuple tuple)
    {
      EntityState result = EntityStateCache[key, true];
      if (result == null) {
        if (key.IsTypeCached) {
          // Fetched instance contains a tuple with some fields set with fetched values.
          // Other fields MUST be not available.
          // That is why Tuple.Create() is used instead of prototype.Clone();
          var origin = Tuple.Create(key.Type.TupleDescriptor);
          tuple.CopyTo(origin);
          result = new EntityState(this, key, origin);
        }
        else {
          // Key belongs to non-existing Entity
          result = new EntityState(this, key, null);
        }
        result.PersistenceState = PersistenceState.Synchronized;
        EntityStateCache.Add(result);
        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Caching: {1}", this, result);
      }
      else {
        result.Update(tuple);
        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Updating cache: {1}", this, result);
      }
      return result;
    }
  }
}