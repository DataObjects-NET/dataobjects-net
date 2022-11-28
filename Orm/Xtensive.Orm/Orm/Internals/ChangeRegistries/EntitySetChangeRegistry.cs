// Copyright (C) 2014-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private readonly HashSet<EntitySetState> modifiedEntitySets = new();

    /// <summary>
    /// Count of registered <see cref="EntitySetState"/>.
    /// </summary>
    public int Count { get { return modifiedEntitySets.Count; } }

    /// <summary>
    /// Register the specified <see cref="EntitySetState"/>.
    /// </summary>
    /// <param name="entitySetState"><see cref="EntitySetState"/> to bound.</param>
    public void Register(EntitySetState entitySetState) => modifiedEntitySets.Add(entitySetState);

    /// <summary>
    /// Gets all registered items.
    /// </summary>
    /// <returns></returns>
    public RegistryItems<EntitySetState> GetItems() => new(modifiedEntitySets);

    /// <summary>
    /// Clears the registry.
    /// </summary>
    public void Clear() => modifiedEntitySets.Clear();

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session"><see cref="Session"/>, to which current instance 
    /// is bound.</param>
    public EntitySetChangeRegistry(Session session)
      : base(session)
    {
    }
  }
}
