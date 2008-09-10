// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;

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
    public EntityData Create(Key key, bool isNew, Transaction transaction)
    {
      return Create(key, key.Tuple, isNew, transaction);
    }

    [Infrastructure]
    public void Update(Key key, Tuple tuple, Transaction transaction)
    {
      EntityData data = this[key];
      if (data == null)
        Create(key, tuple, false, transaction);
      else {
        data.UpdateOrigin(tuple);
        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Merging: {1}", Session.Current, data);
      }
    }

    [Infrastructure]
    public void Clear()
    {
      cache.Clear();
    }

    [Infrastructure]
    private EntityData Create(Key key, Tuple tuple, bool isNew, Transaction transaction)
    {
      Tuple origin;
      if (isNew)
        origin = Session.Domain.Prototypes[key.Type].Clone();
      else
        origin = Tuple.Create(key.Type.TupleDescriptor);
      tuple.CopyTo(origin);
      EntityData result = new EntityData(key, new DifferentialTuple(origin), transaction);
      cache.Add(result);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Caching: {1}", Session.Current, result);

      return result;
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