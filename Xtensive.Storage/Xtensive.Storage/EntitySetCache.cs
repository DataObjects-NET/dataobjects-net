// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;

namespace Xtensive.Storage
{
  /// <summary>
  /// Public API to <see cref="EntitySet{TItem}"/> cache 
  /// (see <see cref="EntitySetBase.Cache">EntitySetBase.Cache</see>).
  /// </summary>
  [DebuggerDisplay("Count = {Count}")]
  public class EntitySetCache : SessionBound,
    ICountable<Key>
  {
    /// <summary>
    /// Gets the entity set this cache is bound to.
    /// </summary>
    [Infrastructure]
    public EntitySetBase EntitySet { get; private set; }

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    public long Count {
      get { return EntitySet.State.CachedItemCount; }
    }

    /// <summary>
    /// Gets a value indicating whether an attempt to read
    /// <see cref="EntitySetBase.Count"/> won't hit the database.
    /// </summary>
    public bool IsCountAvailable {
      get { return EntitySet.State.TotalItemCount.HasValue; }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="EntitySet"/> is fully loaded,
    /// so any read request to it won't hit the database.
    /// </summary>
    public bool IsFullyLoaded {
      get { return EntitySet.State.IsFullyLoaded; }
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
      return EntitySet.State.Contains(key);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return EntitySet.State.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      return EntitySet.State.GetEnumerator();
    }


    // Constructors

    [Infrastructure]
    internal EntitySetCache(EntitySetBase entitySet)
      : base(entitySet.Session)
    {
    }
  }
}