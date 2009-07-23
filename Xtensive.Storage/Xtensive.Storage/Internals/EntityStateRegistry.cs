// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is empty; otherwise, <see langword="false"/>.
    /// </value>
    [Infrastructure]
    public bool IsEmpty
    {
      get { return @new.Count == 0 && modified.Count == 0 && removed.Count == 0; }
    }

    [Infrastructure]
    internal void Register(EntityState item)
    {
      List<EntityState> container = GetContainer(item.PersistenceState);
      container.Add(item);
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
    public EntityStateRegistry(Session session)
      : base(session)
    {
    }
  }
}