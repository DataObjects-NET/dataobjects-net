// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using Xtensive.Core;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// Should be implemented by any persistent entity.
  /// </summary>
  [SystemType]
  public interface IEntity: 
    IIdentified<Key>, 
    IHasVersion<VersionInfo>
  {
    /// <summary>
    /// Gets the <see cref="Key"/> of the <see cref="Entity"/>.
    /// </summary>
    Key Key { get; }

    /// <summary>
    /// Gets <see cref="VersionInfo"/> object describing 
    /// current version of the <see cref="Entity"/>.
    /// </summary>
    VersionInfo VersionInfo { get; }

    /// <summary>
    /// Gets <see cref="TypeInfo"/> object describing <see cref="Entity"/> structure.
    /// </summary>
    TypeInfo Type { get; }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    PersistenceState PersistenceState { get; }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    /// <seealso cref="Remove"/>
    bool IsRemoved { get; }

      /// <summary>
    /// Removes the instance.
    /// </summary>
    void Remove();

    /// <summary>
    /// Locks this instance in the storage.
    /// </summary>
    /// <param name="lockMode">The lock mode.</param>
    /// <param name="lockBehavior">The lock behavior.</param>
    void Lock(LockMode lockMode, LockBehavior lockBehavior);

  }
}