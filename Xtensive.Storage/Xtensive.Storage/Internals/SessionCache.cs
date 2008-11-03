// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  internal class SessionCache : SessionBound
  {
    private readonly ICache<Key, EntityState> cache;
    private readonly Dictionary<Key, EntityState> removed = new Dictionary<Key, EntityState>();
    // Cached properties
    private readonly Domain domain;
    private readonly PrototypeProvider prototypes;

    [Infrastructure]
    public EntityState this[Key key]
    {
      get { return cache[key, true]; }
    }

    [Infrastructure]
    public EntityState Add(Key key)
    {
      Tuple origin = prototypes[key.Type].Clone();
      key.CopyTo(origin);
      var result = new EntityState(key, new DifferentialTuple(origin), Session.Transaction);
      cache.Add(result);

      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Caching: {1}", Session, result);

      return result;
    }

    [Infrastructure]
    public EntityState Add(Key key, Tuple tuple)
    {
      EntityState result = this[key];
      if (result == null) {
        Tuple origin = Tuple.Create(key.Type.TupleDescriptor);
        tuple.CopyTo(origin);
        result = new EntityState(key, new DifferentialTuple(origin), Session.Transaction);
        cache.Add(result);

        if (Session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Caching: {1}", Session, result);
      }
      else {
        result.Update(tuple, Session.Transaction);
        if (Session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Updating cache: {1}", Session, result);
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
      state.Remove(Session.Transaction);
      Key key = state.Key;
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
      prototypes = domain.Prototypes;
    }
  }
}
