// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.29

namespace Xtensive.Orm.Providers
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