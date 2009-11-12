// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.29

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Kinds of <see cref="PersistRequest"/>.
  /// </summary>
  public enum PersistRequestKind
  {
    /// <summary>
    /// Insert request.
    /// </summary>
    Insert = 1,

    /// <summary>
    /// Remove request.
    /// </summary>
    Remove = -1,

    /// <summary>
    /// Update request.
    /// </summary>
    Update = 0,
  }
}