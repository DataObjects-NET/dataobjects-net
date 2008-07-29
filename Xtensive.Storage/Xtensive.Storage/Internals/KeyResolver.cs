// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class KeyResolver
  {
    public static Entity Resolve(Key key)
    {
      Session session = Session.Current;

      // Key is already resolved
      EntityData data;
      if (session.DataCache.TryGetValue(key, out data))
        return GetEntity(data);

      // Probing to get already resolved and cached key
      Key resolvedKey = session.Domain.KeyManager[key];

      // Key is not resolved yet or TypeId is unknown, so 1 fetch request required
      if (resolvedKey == null || key.Type==null) {
        FieldInfo field = key.Hierarchy.Root.Fields[session.HandlerAccessor.NameBuilder.TypeIdFieldName];
        Tuple tuple = Fetcher.Fetch(key, field);

        // Key is not found in storage
        if (tuple==null)
          return null;

        resolvedKey = session.Domain.KeyManager.Get(key.Hierarchy, tuple);
        data = session.DataCache.Create(resolvedKey, tuple, PersistenceState.Persisted);
      }
      else
        // Creating empty Entity
        data = session.DataCache.Create(key, PersistenceState.Persisted);

      return GetEntity(data);
    }

    private static Entity GetEntity(EntityData data)
    {
      return data.Entity ?? Entity.Activate(data.Type.UnderlyingType, data);
    }
  }
}
