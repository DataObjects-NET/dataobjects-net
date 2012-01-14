// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.08.17

namespace Xtensive.Core
{
  /// <summary>
  /// Enumerates possible locks types.
  /// </summary>
  public enum LockType
  {
    /// <summary>
    /// No lock.
    /// Value is <see langword="0" />.
    /// </summary>
    None = 0,
    /// <summary>
    /// Read lock.
    /// Value is <see langword="1" />.
    /// </summary>
    Read = 1,
    /// <summary>
    /// Shared lock. The same as <see cref="Read"/> lock.
    /// Value is <see langword="0" />.
    /// </summary>
    Shared = 1,
    /// <summary>
    /// Write lock.
    /// Value is <see langword="2" />.
    /// </summary>
    Write = 2,
    /// <summary>
    /// Exclusive lock. The same as <see cref="Write"/> lock.
    /// Value is <see langword="2" />.
    /// </summary>
    Exclusive = 2,
    /// <summary>
    /// Suspend lock.
    /// Value is <see langword="4" />.
    /// </summary>
    Suspend = 4,
  }
}