// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.25

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Lock behavior.
  /// </summary>
  public enum LockBehavior
  {
    /// <summary>
    /// Default lock behavior. Equals to <see cref="Wait"/>.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Wait until a lock is released.
    /// </summary>
    Wait = 0,

    /// <summary>
    /// Throw exception if a lock is occupied.
    /// </summary>
    ThrowIfLocked,

    /// <summary>
    /// Skip locked records.
    /// </summary>
    Skip
  }
}