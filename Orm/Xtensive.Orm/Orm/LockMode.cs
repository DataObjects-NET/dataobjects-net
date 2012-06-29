// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.24

namespace Xtensive.Orm
{
  /// <summary>
  /// Lock mode.
  /// </summary>
  public enum LockMode
  {
    /// <summary>
    /// Shared lock.
    /// </summary>
    Shared = 0,

    /// <summary>
    /// Lock for the following update.
    /// </summary>
    Update,

    /// <summary>
    /// Exclusive lock.
    /// </summary>
    Exclusive,

    /// <summary>
    /// Default lock mode. Equals to <see cref="Shared"/>.
    /// </summary>
    Default = Shared,
  }
}