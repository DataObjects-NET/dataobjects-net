// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.27

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Access point to a single <see cref="Key"/> resolving.
  /// </summary>
  public static class Query
  {
    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Entity with the specified key is not found.</exception>
    public static Entity Single(Key key)
    {
      var session = Session.Demand();
      var result = SingleOrDefault(session, key);
      if (result == null)
        throw new KeyNotFoundException(string.Format(
          Strings.EntityWithKeyXDoesNotExist, key));
      return result;
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null" />, if there is no such entity.
    /// </returns>
    public static Entity SingleOrDefault(Key key)
    {
      var session = Session.Demand();
      return SingleOrDefault(session, key);
    }

    /// <summary>
    /// Resolves the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <param name="session">The session to resolve the <paramref name="key"/> in.</param>
    /// <returns>
    /// The <see cref="Entity"/> the specified <paramref name="key"/> identifies or <see langword="null" />.
    /// </returns>
    internal static Entity SingleOrDefault(Session session, Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      using (Transaction.Open(session)) {
        var cache = session.EntityStateCache;
        var state = cache[key, true];
        bool hasBeenFetched = false;

        if (state==null) {
          if (session.IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is {0}.", session, key,
              key.IsTypeCached ? "known" : "unknown");
          session.Handler.FetchInstance(key);
          state = cache[key, true];
          hasBeenFetched = true;
        }

        if (!hasBeenFetched && session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Resolving key '{1}'. Key is already resolved.", session, key);

        if (state == null || state.IsRemoved)
          return null;

        return state.Entity;
      }
    }
  }
}