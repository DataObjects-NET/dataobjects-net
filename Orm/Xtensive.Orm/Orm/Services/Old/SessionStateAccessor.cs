// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Aspects;
using Xtensive.Caching;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Public API to cached state of the <see cref="Session"/>
  /// (see <see cref="DirectStateAccessor"/>).
  /// </summary>
  [DebuggerDisplay("Count = {Count}")]
  public struct SessionStateAccessor : IEnumerable<Entity>
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
    /// Very similar to what happens on rollback, but
    /// without rollback itself.
    /// </summary>
    public void Invalidate()
    {
      if (Session.EntityChangeRegistry.GetItems(PersistenceState.New).Any())
        throw new InvalidOperationException(Strings.UnableToInvalidateSessionStateNewlyCreatedEntitiesAreAttachedToSession);
      Session.Invalidate();
    }

    /// <summary>
    /// Remaps the keys of cached entities
    /// accordingly with the specified <paramref name="keyMapping"/>.
    /// </summary>
    /// <param name="keyMapping">The key mapping.</param>
    public void RemapEntityKeys(KeyMapping keyMapping)
    {
      Session.RemapEntityKeys(keyMapping);
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