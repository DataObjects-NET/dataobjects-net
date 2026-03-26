// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Collections.Generic;


namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Registers <see cref="EntityState"/> changes.
  /// </summary>
  public sealed class EntityChangeRegistry : SessionBound
  {
    private readonly HashSet<EntityState> @new = new();
    private readonly HashSet<EntityState> modified = new();
    private readonly HashSet<EntityState> removed = new();

    /// <summary>
    /// Gets the number of registered entities.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Registers the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    internal void Register(EntityState item)
    {
      // Remove-create sequences fix for Issue 690
      if (item.PersistenceState == PersistenceState.New && removed.Contains(item)) {
        _ = removed.Remove(item);
        Count--;
        if (item.DifferentialTuple.Difference == null) {
          item.SetPersistenceState(PersistenceState.Synchronized);
          return;
        }
        item.SetPersistenceState(PersistenceState.Modified);
      }
      else if (item.PersistenceState == PersistenceState.Removed && @new.Contains(item)) {
        _ = @new.Remove(item);
        Count--;
        return;
      }
      else if (item.PersistenceState == PersistenceState.Removed && modified.Contains(item)) {
        _ = modified.Remove(item);
        Count--;
      }

      var container = GetContainer(item.PersistenceState);
      if (container.Add(item)) {
        Count++;
      }
    }

    /// <summary>
    /// Gets the items with specified <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The state of items to get.</param>
    /// <returns>The sequence of items with specified state.</returns>
    public RegistryItems<EntityState> GetItems(in PersistenceState state) =>
      new(GetContainer(state));

    /// <summary>
    /// Clears the registry.
    /// </summary>
    public void Clear()
    {
      Count = 0;
      @new.Clear();
      modified.Clear();
      removed.Clear();
    }

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="state"/> is out of range.</exception>
    private HashSet<EntityState> GetContainer(in PersistenceState state)
    {
      return state switch {
        PersistenceState.New => @new,
        PersistenceState.Modified => modified,
        PersistenceState.Removed => removed,
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
      };
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