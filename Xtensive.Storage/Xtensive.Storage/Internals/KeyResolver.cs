// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class KeyResolver
  {
    public static Entity Resolve(Key key)
    {
      Session session = Session.Current;
      EntityState state = session.Cache[key];

      // Key is already resolved
      if (state!=null) {
        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Resolving key '{1}'. Key is already resolved.", session, key);
        
        state.EnsureIsActual();
        return state.IsRemoved ? null : GetEntity(state);
      }

      // Key is not fully resolved yet (Type is unknown), so 1 fetch request is required
      if (key.Type==null) {

        if (Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is unknown. Fetch is required.", session, key);

        FieldInfo field = key.Hierarchy.Root.Fields[session.Domain.NameBuilder.TypeIdFieldName];
        Fetcher.Fetch(key, field);
      }
      else {
        if(Log.IsLogged(LogEventTypes.Debug))
          Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is known.", session, key);

        // Type is known so we can create Entity instance.
        Fetcher.Fetch(key);
      }
      state = session.Cache[key];
      return state==null ? null : GetEntity(state);
    }

    private static Entity GetEntity(EntityState state)
    {
      if (state.Entity != null)
        return state.Entity;
      Entity result = Entity.Activate(state.Type.UnderlyingType, state);
      state.Entity = result;
      return result;
    }
  }
}
