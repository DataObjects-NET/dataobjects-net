// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.09

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Action to be executed during a persisting.
  /// </summary>
  public enum PersistActionKind
  {
    /// <summary>
    /// 'Insert' action.
    /// </summary>
    Insert = 0,
    /// <summary>
    /// 'Update' action.
    /// </summary>
    Update = 1,
    /// <summary>
    /// 'Remove' action.
    /// </summary>
    Remove = 2
  }
}