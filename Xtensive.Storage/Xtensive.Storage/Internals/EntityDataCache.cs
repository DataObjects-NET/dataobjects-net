// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Internals
{
  internal class EntityDataCache : SessionBound,
    IEnumerable<EntityData>
  {
    private readonly WeakCache<Key, EntityData> cache;
    
    [Infrastructure]
    public EntityData this[Key key]
    {
      get { return cache[key, true]; }
    }

    [Infrastructure]
    public EntityData Create(Key key, PersistenceState state)
    {
      return Create(key, key.Tuple, state);
    }

    [Infrastructure]
    public void Update(Key key, Tuple tuple)
    {
      EntityData data = this[key];
      if (data == null)
        Create(key, tuple, PersistenceState.Persisted);
      else {
        data.DifferentialData.Origin.MergeWith(tuple);
        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Merging: {1}", Session.Current, data);
      }
    }

    [Infrastructure]
    public void Remove(Key key)
    {
      cache.Remove(key);
    }

    [Infrastructure]
    public void Clear()
    {
      cache.Clear();
    }

    [Infrastructure]
    private EntityData Create(Key key, Tuple tuple, PersistenceState state)
    {
      Tuple origin;
      if (state == PersistenceState.New)
        origin = Session.Domain.Prototypes[key.Type].Clone();
      else
        origin = Tuple.Create(key.Type.TupleDescriptor);
      tuple.CopyTo(origin);
      EntityData result = new EntityData(key, new DifferentialTuple(origin), state);
      cache.Add(result);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Caching: {1}", Session.Current, result);

      return result;
    }

    [Infrastructure]
    public void Reset()
    {
      foreach (EntityData data in this)
        data.Reset();
    }

    [Infrastructure]
    public IEnumerator<EntityData> GetEnumerator()
    {
      return cache.GetEnumerator();
    }

    [Infrastructure]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    public EntityDataCache(Session session, int cacheSize) : base(session)
    {
      cache = new WeakCache<Key, EntityData>(cacheSize, d => d.Key);
    }
  }
}