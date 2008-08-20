// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  internal class EntityDataCache : SessionBound
  {
    private readonly WeakCache<Key, EntityData> cache;

    public EntityData this[Key key]
    {
      get { return cache[key, true]; }
    }

    public EntityData Create(Key key, PersistenceState state)
    {
      return Create(key, key.Tuple, state);
    }

    public void Update(Key key, Tuple tuple)
    {
      EntityData data = this[key];
      if (data == null)
        Create(key, tuple, PersistenceState.Persisted);
      else {
        try {
          data.Tuple.Origin.MergeWith(tuple);
          if (Log.IsLogged(LogEventTypes.Debug))
            Log.Debug("Session '{0}'. Merging: {1}", Session.Current, data);
        }
        catch(Exception e) {
          throw;
        }
      }
    }

    public void Remove(Key key)
    {
      cache.Remove(key);
    }

    public void Clear()
    {
      cache.Clear();
    }

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


    // Constructors

    public EntityDataCache(Session session, int cacheSize) : base(session)
    {
      cache = new WeakCache<Key, EntityData>(cacheSize, d => d.Key);
    }
  }
}