// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class SessionCache : SessionBound
  {
    private readonly ICache<Key, EntityState> cache;
    private readonly Dictionary<Key, EntityState> removed = new Dictionary<Key, EntityState>();
    // Cached properties
    private readonly Domain domain;
    private readonly Dictionary<TypeInfo, Tuple> persistentTuplePrototypes;

    [Infrastructure]
    public EntityState this[Key key]
    {
      get { return cache[key, true]; }
    }

    [Infrastructure]
    public EntityState Add(Key key)
    {
      var session = Session;
      EntityState result;
      if (key.IsTypeCached) {
        // New instance contains a tuple with all fields set with default values.
        var origin = persistentTuplePrototypes[key.Type].Clone();
        key.CopyTo(origin);
        result = new EntityState(session, key, origin);
      }
      else {
        // Key belongs to non-existing Entity
        result = new EntityState(session, key, null);
      }
      result.PersistenceState = PersistenceState.New;
      cache.Add(result);

      if (session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Caching: {1}", session, result);
      return result;
    }

    [Infrastructure]
    public EntityState Add(Key key, Tuple tuple)
    {
      var session = Session;
      EntityState result = this[key];
      if (result == null) {
        if (key.IsTypeCached) {
          // Fetched instance contains a tuple with some fields set with fetched values.
          // Other fields MUST be not available.
          // That is why Tuple.Create() is used instead of prototype.Clone();
          var origin = Tuple.Create(key.Type.TupleDescriptor);
          tuple.CopyTo(origin);
          result = new EntityState(session, key, origin);
        }
        else {
          // Key belongs to non-existing Entity
          result = new EntityState(session, key, null);
        }
        result.PersistenceState = PersistenceState.Synchronized;
        cache.Add(result);
        if (session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Caching: {1}", session, result);
      }
      else {
        result.Update(tuple);
        if (session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Updating cache: {1}", session, result);
      }
      return result;
    }

    [Infrastructure]
    public void Remove(Key key)
    {
      EntityState state = cache[key, false];
      if (state!=null)
        Remove(state);
    }

    [Infrastructure]
    public void Remove(EntityState state)
    {
      state.Update(null);
      var key = state.Key;
      if (!removed.ContainsKey(key))
        removed[key] = cache[key, false];
      cache.RemoveKey(key);
    }

    [Infrastructure]
    public void ClearRemoved()
    {
      removed.Clear();
    }

    [Infrastructure]
    public void RestoreRemoved()
    {
      foreach (EntityState data in removed.Values) {
        if (cache.ContainsKey(data.Key))
          cache.Remove(data);
        cache.Add(data);
      }
      ClearRemoved();
    }

    [Infrastructure]
    public void Clear()
    {
      cache.Clear();
    }


    // Constructors

    public SessionCache(Session session, int cacheSize) 
      : base(session)
    {
      cache = new LruCache<Key, EntityState>(cacheSize, i => i.Key,
        new WeakCache<Key, EntityState>(false, i => i.Key));
      domain = session.Domain;
      persistentTuplePrototypes = domain.PersistentTuplePrototypes;
    }
  }
}
