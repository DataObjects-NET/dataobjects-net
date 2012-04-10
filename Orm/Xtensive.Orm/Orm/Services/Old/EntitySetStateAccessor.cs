// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Public API to cached state of <see cref="Orm.EntitySet{TItem}"/> 
  /// (see <see cref="DirectStateAccessor"/>).
  /// </summary>
  [DebuggerDisplay("Count = {Count}")]
  public struct EntitySetStateAccessor : ICountable<Key>
  {
    private readonly EntitySetBase entitySet;

    /// <summary>
    /// Gets the entity set this accessor is bound to.
    /// </summary>
    public EntitySetBase EntitySet
    {
      get { return entitySet; }
    }

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    public long Count {
      get {
        var state = EntitySet.State;
        return state==null ? 0 : state.CachedItemCount;
      }
    }

    /// <summary>
    /// Gets a value indicating whether an attempt to read
    /// <see cref="EntitySetBase.Count"/> won't hit the database.
    /// </summary>
    public bool IsCountAvailable {
      get {
        var state = EntitySet.State;
        return state==null ? false : state.TotalItemCount.HasValue;
      }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="EntitySet"/> is fully loaded,
    /// so any read request to it won't hit the database.
    /// </summary>
    public bool IsFullyLoaded {
      get {
        var state = EntitySet.State;
        return state==null ? false : state.IsFullyLoaded;
      }
    }

    /// <summary>
    /// Indicates whether a specified <paramref name="key"/> is cached or not.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified key is cached; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Key key)
    {
      var state = EntitySet.State;
      return state==null ? false : state.Contains(key);
    }

    
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    
    public IEnumerator<Key> GetEnumerator()
    {
      var state = EntitySet.State;
      return state==null ? EnumerableUtils<Key>.EmptyEnumerator : state.GetEnumerator();
    }


    // Constructors

    internal EntitySetStateAccessor(EntitySetBase entitySet)
    {
      this.entitySet = entitySet;
    }
  }
}