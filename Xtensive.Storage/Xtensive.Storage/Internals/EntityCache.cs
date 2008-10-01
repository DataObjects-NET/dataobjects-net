// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Caching;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class EntityCache : SessionBound,
    IEnumerable<EntityState>
  {
    private readonly ICache<Key, EntityState> cache;
    private readonly Dictionary<Key, EntityState> removed = new Dictionary<Key, EntityState>();
    // Cached properties
    private readonly Domain domain;
    private readonly Dictionary<TypeInfo, Tuple> prototypes;

    [Infrastructure]
    public EntityState this[Key key]
    {
      get { return cache[key, true]; }
    }

    [Infrastructure]
    public EntityState Create(Key key, Transaction transaction)
    {
      return Create(key, key, true, transaction);
    }

    [Infrastructure]
    private EntityState Create(Key key, Tuple tuple, bool isNew, Transaction transaction)
    {
      Tuple origin;
      if (isNew)
        origin = prototypes[key.Type].Clone();
      else
        origin = prototypes[key.Type].CreateNew();
      tuple.CopyTo(origin);
      var result = new EntityState(key, new DifferentialTuple(origin), transaction);
      cache.Add(result);

      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Caching: {1}", Session, result);

      return result;
    }

    [Infrastructure]
    public void Update(Key key, Tuple tuple, Transaction transaction)
    {
      EntityState state = this[key];
      if (state == null)
        Create(key, tuple, false, transaction);
      else {
        state.Import(tuple, transaction);
        if (Session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Merging: {1}", Session, state);
      }
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
      state.IsRemoved = true;
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

    [Infrastructure]
    public IEnumerator<EntityState> GetEnumerator()
    {
      return cache.GetEnumerator();
    }

    [Infrastructure]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    public EntityCache(Session session, int cacheSize) 
      : base(session)
    {
      cache = new LruCache<Key, EntityState>(cacheSize, i => i.Key,
        new WeakCache<Key, EntityState>(false, i => i.Key));
      domain = session.Domain;
      prototypes = domain.Prototypes;
    }
  }
}
