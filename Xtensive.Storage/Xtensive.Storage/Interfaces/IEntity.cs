// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using Xtensive.Core;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage
{
  /// <summary>
  /// Should be implemented by any persistent entity.
  /// </summary>
  [SystemType]
  public interface IEntity: IIdentified<Key>
  {
    /// <summary>
    /// Gets the <see cref="Key"/> of the <see cref="Entity"/>.
    /// </summary>
    Key Key { get; }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    PersistenceState PersistenceState { get; }

    /// <summary>
    /// Removes the instance.
    /// </summary>
    void Remove();
  }
}