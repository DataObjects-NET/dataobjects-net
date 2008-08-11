// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  internal class EntityDataCache
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
      else
        data.Tuple.Origin.MergeWith(tuple);
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
      Tuple origin = Tuple.Create(key.Type.TupleDescriptor);
      tuple.CopyTo(origin);
      EntityData result = new EntityData(key, new DifferentialTuple(origin), state);
      cache.Add(result);
      return result;
    }


    // Constructors

    public EntityDataCache(int cacheSize)
    {
      cache = new WeakCache<Key, EntityData>(cacheSize, d => d.Key);
    }
  }
}