// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Collections.Generic;
using Xtensive.Aspects;
using Xtensive.Collections;


namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Registers <see cref="EntityState"/> changes.
  /// </summary>
  [Infrastructure]
  public sealed class EntityChangeRegistry : SessionBound
  {
    private readonly HashSet<EntityState> @new = new HashSet<EntityState>();
    private readonly HashSet<EntityState> modified = new HashSet<EntityState>();
    private readonly HashSet<EntityState> removed = new HashSet<EntityState>();
    private int count;

    /// <summary>
    /// Gets the number of registered entities.
    /// </summary>
    public int Count { get { return count; } }

    /// <summary>
    /// Registers the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    internal void Register(EntityState item)
    {
      // Remove-create sequences fix for Issue 690
      if (item.PersistenceState == PersistenceState.New && removed.Contains(item)) {
        removed.Remove(item);
        count--;
        if (item.DifferentialTuple.Difference == null) {
          item.SetPersistenceState(PersistenceState.Synchronized);
          return;
        }
        item.SetPersistenceState(PersistenceState.Modified);
      }
      else if (item.PersistenceState == PersistenceState.Removed && @new.Contains(item)) {
        @new.Remove(item);
        count--;
        return;
      }
      else if (item.PersistenceState == PersistenceState.Removed && modified.Contains(item)) {
        modified.Remove(item);
      }

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
    private HashSet<EntityState> GetContainer(PersistenceState state)
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

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session"><see cref="Session"/>, to which current instance 
    /// is bound.</param>
    public EntityChangeRegistry(Session session)
      : base(session)
    {
    }
  }
}