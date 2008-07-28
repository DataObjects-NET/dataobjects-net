// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.28

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  internal class EntityDataCache
  {
    private readonly WeakCache<Key, EntityData> cache;

    public EntityData Create(Key key, Tuple tuple)
    {
      key.Tuple.CopyTo(tuple, 0);
      EntityData result = new EntityData(key, new DifferentialTuple(tuple));
      cache.Add(result);
      return result;
    }

    public EntityData Create(Key key)
    {
      Tuple origin = Tuple.Create(key.Type.TupleDescriptor);
      return Create(key, origin);
    }

    public void Update(Key key, Tuple tuple)
    {
      EntityData data;
      if (!TryGetValue(key, out data))
        Create(key, tuple);
      else
        data.Tuple.Origin.MergeWith(tuple);
    }

    public EntityData this[Key key]
    {
      get
      {
        EntityData result;
        if (!TryGetValue(key, out result))
          throw new ArgumentException(String.Format(String.Format("Item '{0}' not found.", key)));
        return result;
      }
    }

    public bool TryGetValue(Key key, out EntityData data)
    {
      data = cache[key, true];
      return data!=null;
    }


    // Constructors

    public EntityDataCache(int cacheSize)
    {
      cache = new WeakCache<Key, EntityData>(cacheSize, d => d.Key);
    }
  }
}