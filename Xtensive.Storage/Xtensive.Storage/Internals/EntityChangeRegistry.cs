// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Registers <see cref="EntityState"/> changes.
  /// </summary>
  public class EntityChangeRegistry
  {
    private readonly List<EntityState> @new = new List<EntityState>();
    private readonly List<EntityState> modified = new List<EntityState>();
    private readonly List<EntityState> removed = new List<EntityState>();
    private readonly int sizeLimit;
    private readonly Session session;
    private int count;

    /// <summary>
    /// Gets the count of registered entities.
    /// </summary>
    public int Count {
      get { return count; }
    }

    /// <summary>
    /// Gets the maximal allowed count of registered entities for this registry.
    /// </summary>
    public int SizeLimit {
      get { return sizeLimit; }
    }

    /// <summary>
    /// Enforces the size limit by flushing the changes if it is reached.
    /// </summary>
    public void EnforceSizeLimit()
    {
      if (count>=sizeLimit)
        session.Persist();
    }

    /// <summary>
    /// Registers the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
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
    public IEnumerable<EntityState> GetItems(PersistenceState state)
    {
      foreach (var item in GetContainer(state))
        yield return item;
    }

    /// <summary>
    /// Clears the registry.
    /// </summary>
    public void Clear()
    {
      count = 0;
      @new.Clear();
      modified.Clear();
      removed.Clear();
    }

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="state"/> is out of range.</exception>
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
    public EntityChangeRegistry(Session session, int sizeLimit)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(sizeLimit, 0, "maxCount");
      this.session = session;
      this.sizeLimit = sizeLimit;
    }
  }
}