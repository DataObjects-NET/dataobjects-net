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

    public EntityData Add(Key key, Tuple tuple, PersistenceState state)
    {
      EntityData result = new EntityData(key, new DifferentialTuple(tuple), state);
      cache.Add(result);
      return result;
    }

    public EntityData Add(Key key, PersistenceState state)
    {
      Tuple tuple = Tuple.Create(key.Type.TupleDescriptor);
      key.Tuple.CopyTo(tuple, 0);
      return Add(key, tuple, state);
    }

    public void Update(Key key, Tuple tuple)
    {
      EntityData data = this[key];
      if (data == null)
        Add(key, tuple, PersistenceState.Persisted);
      else
        data.Tuple.Origin.MergeWith(tuple);
    }

    public void Remove(Key key)
    {
      cache.Remove(key);
    }


    // Constructors

    public EntityDataCache(int cacheSize)
    {
      cache = new WeakCache<Key, EntityData>(cacheSize, d => d.Key);
    }
  }
}