// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Aspects;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;

namespace Xtensive.Storage
{
  /// <summary>
  /// Public API to <see cref="Session"/> cache 
  /// (see <see cref="Session.Cache">Session.Cache</see>).
  /// </summary>
  [DebuggerDisplay("Count = {Count}")]
  public class SessionCache : SessionBound, 
    ICountable<Entity>
  {
    private ICache<Key, EntityState> RealCache { 
      get { return Session.EntityStateCache; }}

    /// <summary>
    /// Gets the number of cached entities.
    /// Note that it can differ from the number of entities
    /// returned by <see cref="GetEnumerator"/> methods
    /// (cache can be week, etc.).
    /// </summary>
    [Infrastructure]
    public int Count {
      get { return RealCache.Count; }
    }

    /// <summary>
    /// Gets the number of cached entities.
    /// Note that it can differ from the number of entities
    /// returned by <see cref="GetEnumerator"/> methods
    /// (cache can be week, etc.).
    /// </summary>
    [Infrastructure]
    long ICountable.Count {
      get { return RealCache.Count; }
    }

    /// <summary>
    /// Gets cached <see cref="Entity"/> with the specified key.
    /// </summary>
    public Entity this[Key key] {
      get {
        var state = RealCache[key, true];
        var entity = state.Entity; // May create it!
        return entity;
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      var allCached = 
        from state in RealCache
        let entity = state.Entity
        where entity!=null
        select entity;
      return allCached.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Entity> GetEnumerator()
    {
      var allCached = 
        from state in RealCache
        let entity = state.Entity
        where entity!=null
        select entity;
      return allCached.GetEnumerator();
    }

    
    // Constructors

    [Infrastructure]
    internal SessionCache(Session session)
      : base(session)
    {
    }
  }
}