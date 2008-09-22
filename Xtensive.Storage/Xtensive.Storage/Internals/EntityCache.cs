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
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class EntityCache : SessionBound,
    IEnumerable<EntityData>
  {
    private readonly WeakCache<Key, EntityData> cache;
    private readonly Dictionary<Key, EntityData> removed = new Dictionary<Key, EntityData>();
    // Cached properties
    private readonly Domain domain;
    private readonly Dictionary<TypeInfo, Tuple> prototypes;
    
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
    private EntityData Create(Key key, Tuple tuple, bool isNew, Transaction transaction)
    {
      Tuple origin;
      if (isNew)
        origin = prototypes[key.Type].Clone();
      else {
        if (tuple is RegularTuple)
          origin = tuple.Clone();
        else
          origin = tuple.ToRegular();
      }
      var result = new EntityData(key, new DifferentialTuple(origin), transaction);
      cache.Add(result);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Caching: {1}", Session, result);

      return result;
    }

    [Infrastructure]
    public void Update(Key key, Tuple tuple, Transaction transaction)
    {
      EntityData data = this[key];
      if (data == null)
        Create(key, tuple, false, transaction);
      else {
        data.Import(tuple, transaction);
        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Merging: {1}", Session, data);
      }
    }

    [Infrastructure]
    public void Remove(Key key)
    {
      EntityData data = cache[key, false];
      if (data!=null)
        Remove(data);      
    }

    [Infrastructure]
    public void Remove(EntityData data)
    {      
      data.IsRemoved = true;
      Key key = data.Key;
      if (!removed.ContainsKey(key))
        removed[key] = cache[key, false];
      cache.Remove(key);
    }

    [Infrastructure]
    public void ClearRemoved()
    {
      removed.Clear();
    }

    [Infrastructure]
    public void RestoreRemoved()
    {
      foreach (EntityData data in removed.Values) {
        if (cache.Contains(data.Key))
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

    public EntityCache(Session session, int cacheSize) : base(session)
    {
      cache = new WeakCache<Key, EntityData>(cacheSize, d => d.Key);
      domain = session.Domain;
      prototypes = domain.Prototypes;
    }
  }
}