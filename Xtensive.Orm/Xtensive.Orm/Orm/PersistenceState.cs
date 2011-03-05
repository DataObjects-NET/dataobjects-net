// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

namespace Xtensive.Orm
{
  /// <summary>
  /// Defines possible persistence states of the entities.
  /// </summary>
  public enum PersistenceState
  {
    /// <summary>
    /// The entity is synchronized with the database (there are no unsaved changes).
    /// </summary>
    Synchronized = 0,
    /// <summary>
    /// The entity is created, but not persisted yet.
    /// </summary>
    New = 1,
    /// <summary>
    /// The entity presents in database, but has some unpersisted changes.
    /// </summary>
    Modified = 2,
    /// <summary>
    /// The entity is marked as removed, but is not removed from database yet.
    /// </summary>
    Removed = 3,
  }
}