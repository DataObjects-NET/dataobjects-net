// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.03.27

using System.Collections.Generic;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Contains <see cref="EntitySetState"/>s which modified during the bounded session.
  /// </summary>
  public sealed class EntitySetChangeRegistry : SessionBound
  {
    private HashSet<EntitySetState> modifiedEntitySets = new HashSet<EntitySetState>();

    /// <summary>
    /// Count of registered <see cref="EntitySetState"/>.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Register the specified <see cref="EntitySetState"/>.
    /// </summary>
    /// <param name="entitySetState"><see cref="EntitySetState"/> to bound.</param>
    public void Register(EntitySetState entitySetState)
    {
      if (!modifiedEntitySets.Contains(entitySetState)) {
        modifiedEntitySets.Add(entitySetState);
        Count++;
      }
    }

    /// <summary>
    /// Gets all registered items.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<EntitySetState> GetItems()
    {
      return modifiedEntitySets;
    }

    public void Clear()
    {
      modifiedEntitySets.Clear();
      Count = 0;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session"><see cref="Session"/>, to which current instance 
    /// is bound.</param>
    public EntitySetChangeRegistry(Session session)
      : base(session)
    {
      Count = 0;
    }
  }
}
