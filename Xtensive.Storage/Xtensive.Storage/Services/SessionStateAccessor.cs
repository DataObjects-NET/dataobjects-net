// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Aspects;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Services
{
  /// <summary>
  /// Public API to cached state of the <see cref="Session"/>
  /// (see <see cref="DirectStateAccessor"/>).
  /// </summary>
  [DebuggerDisplay("Count = {Count}")]
  public struct SessionStateAccessor : ICountable<Entity>
  {
    private readonly Session session;

    /// <summary>
    /// Gets the <see cref="Session"/> instance this accessor is bound to.
    /// </summary>
    public Session Session {
      get { return session; }
    }

    /// <summary>
    /// Gets the number of cached entities.
    /// Note that it can differ from the number of entities
    /// returned by <see cref="GetEnumerator"/> methods
    /// (cache can be week, etc.).
    /// </summary>
    [Infrastructure]
    public int Count {
      get { return EntityStateCache.Count; }
    }

    /// <summary>
    /// Gets the number of cached entities.
    /// Note that it can differ from the number of entities
    /// returned by <see cref="GetEnumerator"/> methods
    /// (cache can be week, etc.).
    /// </summary>
    [Infrastructure]
    long ICountable.Count {
      get { return EntityStateCache.Count; }
    }

    /// <summary>
    /// Gets cached <see cref="Entity"/> with the specified key.
    /// </summary>
    public Entity this[Key key] {
      get {
        var state = EntityStateCache[key, true];
        var entity = state.Entity; // May create it!
        return entity;
      }
    }

    /// <summary>
    /// Invalidates (forgets) all the pending changes 
    /// and the state of all cached entities.
    /// </summary>
    public void Invalidate()
    {
      Session.Invalidate();
    }

    #region IEnuemrable<...> members

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      var allCached = 
        from state in EntityStateCache
        let entity = state.Entity
        where entity!=null
        select entity;
      return allCached.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Entity> GetEnumerator()
    {
      var allCached = 
        from state in EntityStateCache
        let entity = state.Entity
        where entity!=null
        select entity;
      return allCached.GetEnumerator();
    }

    #endregion

    #region Private \ internal members

    private ICache<Key, EntityState> EntityStateCache { 
      get { return session.EntityStateCache; }}

    #endregion

    
    // Constructors

    internal SessionStateAccessor(Session session)
    {
      this.session = session;
    }
  }
}