// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Aspects;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Registers <see cref="EntityState"/> changes.
  /// </summary>
  public class EntityStateRegistry : SessionBound
  {
    private readonly List<EntityState> @new = new List<EntityState>();
    private readonly List<EntityState> modified = new List<EntityState>();
    private readonly List<EntityState> removed = new List<EntityState>();
    private readonly int sizeLimit;
    private int count;

    /// <summary>
    /// Gets the count of registered entities.
    /// </summary>
    [Infrastructure]
    public int Count {
      get { return count; }
    }

    /// <summary>
    /// Gets the maximal allowed count of registered entities for this registry.
    /// </summary>
    [Infrastructure]
    public int SizeLimit {
      get { return sizeLimit; }
    }

    /// <summary>
    /// Enforces the size limit by flushing the changes if it is reached.
    /// </summary>
    [Infrastructure]
    public void EnforceSizeLimit()
    {
      if (count>=sizeLimit)
        Session.Persist();
    }

    /// <summary>
    /// Registers the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    [Infrastructure]
    internal void Register(EntityState item)
    {
      var container = GetContainer(item.PersistenceState);
      container.Add(item);
      count++;
    }

    /// <summary>
    /// Gets the items with specified <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The state of items to get.</param>
    /// <returns>The sequence of items with specified state.</returns>
    [Infrastructure]
    public IEnumerable<EntityState> GetItems(PersistenceState state)
    {
      foreach (var item in GetContainer(state))
        yield return item;
    }

    /// <summary>
    /// Clears the registry.
    /// </summary>
    [Infrastructure]
    public void Clear()
    {
      count = 0;
      @new.Clear();
      modified.Clear();
      removed.Clear();
    }

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="state"/> is out of range.</exception>
    [Infrastructure]
    private List<EntityState> GetContainer(PersistenceState state)
    {
      switch (state) {
      case PersistenceState.New:
        return @new;
      case PersistenceState.Modified:
        return modified;
      case PersistenceState.Removed:
        return removed;
      default:
        throw new ArgumentOutOfRangeException("state");
      }
    }


    // Constructors

    /// <inheritdoc/>
    public EntityStateRegistry(Session session, int sizeLimit)
      : base(session)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(sizeLimit, 0, "maxCount");
      this.sizeLimit = sizeLimit;
    }
  }
}