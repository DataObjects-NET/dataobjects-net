// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Internals
{
  internal static class KeyResolver
  {
    public static Entity Resolve(Key key)
    {
      Session session = Session.Current;
      EntityData data = session.DataCache[key];

      // Key is already resolved
      if (data != null)
        return GetEntity(data);

      // Probing to get already resolved and cached key
      Key resolvedKey = session.Domain.KeyManager.GetCached(key);

        // Key is not fully resolved yet (Type is unknown), so 1 fetch request is required
      if (resolvedKey.Type==null) {
        FieldInfo field = key.Hierarchy.Root.Fields[NameBuilder.TypeIdFieldName];
        Fetcher.Fetch(key, field);

        // Resolving key again. If it was successfully fetched then it should contain Type
        resolvedKey = session.Domain.KeyManager.GetCached(key);

        // Key is not found in storage
        if (resolvedKey.Type==null)
          return null;
      }

      // Type is known so we can create Entity instance.
      data = session.DataCache.Create(resolvedKey, PersistenceState.Persisted);
      return GetEntity(data);
    }

    private static Entity GetEntity(EntityData data)
    {
      return data.Entity ?? Entity.Activate(data.Type.UnderlyingType, data);
    }
  }
}
