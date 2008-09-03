// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Core.Diagnostics;
using Xtensive.Integrity.Transactions;
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
      if (data!=null) {
        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Resolving key '{1}'. Key is already resolved", session, key);

        return GetEntity(data);
      }

      // Probing to get already resolved and cached key
      Key resolvedKey = session.Domain.KeyManager.GetCachedKey(key);

      // Key is not fully resolved yet (Type is unknown), so 1 fetch request is required
      if (resolvedKey.Type==null) {

        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is unknown. Fetch is required", session, key);

        FieldInfo field = key.Hierarchy.Root.Fields[NameBuilder.TypeIdFieldName];

        using (var transactionScope = session.OpenTransaction()) {
          Fetcher.Fetch(key, field);
          transactionScope.Complete();
        }

        // Resolving key again. If it was successfully fetched then it should contain Type
        resolvedKey = session.Domain.KeyManager.GetCachedKey(key);

        // Key is not found in storage
        if (resolvedKey.Type==null)
          return null;
      }
      else if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is known", session, key);

      // Type is known so we can create Entity instance.
      data = session.DataCache.Create(resolvedKey, PersistenceState.Persisted, session.Transaction);
      return GetEntity(data);
    }

    private static Entity GetEntity(EntityData data)
    {
      if (data.Entity != null)
        return data.Entity;
      Entity result = Entity.Activate(data.Type.UnderlyingType, data);
      data.Entity = result;
      return result;
    }
  }
}
