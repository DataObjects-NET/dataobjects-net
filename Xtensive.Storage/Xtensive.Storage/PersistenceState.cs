// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

namespace Xtensive.Storage
{
  /// <summary>
  /// Defines possible persistence states of the entities.
  /// </summary>
  public enum PersistenceState
  {
    /// <summary>
    /// The same as <see cref="Inconsistent"/>.
    /// </summary>
    Default = Inconsistent,
    /// <summary>
    /// Inconsistent state, e.g. created and rollbacked entity.
    /// </summary>
    Inconsistent = 0,
    /// <summary>
    /// The entity isn't persisted yet.
    /// </summary>
    New = 1,
    /// <summary>
    /// The entity is persisted, but has some unpersisted changes.
    /// </summary>
    Modified = 2,
    /// <summary>
    /// The entity was removed, but 'delete' sql-command was not excuted yet.
    /// </summary>
    Removing = 3,
    /// <summary>
    /// The entity is removed from the storage, i.e. 'delete' sql-command was excuted.
    /// </summary>
    Removed = 4,
    /// <summary>
    /// The entity is persisted.
    /// </summary>
    Persisted = 5,
  }
}