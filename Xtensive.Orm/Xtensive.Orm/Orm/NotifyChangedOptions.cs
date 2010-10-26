// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.03

using System;
using System.Diagnostics;

namespace Xtensive.Orm
{
  /// <summary>
  /// Enumerates possible options for <see cref="Session.NotifyChanged(NotifyChangedOptions)"/> method.
  /// </summary>
  [Flags]
  public enum NotifyChangedOptions
  {
    /// <summary>
    /// Indicates whether all entities must be prefetched.
    /// </summary>
    Prefetch = 0x1,
    /// <summary>
    /// Indicates whether removed entities must not be notified.
    /// Implies <see cref="Prefetch"/>.
    /// </summary>
    SkipRemovedEntities = 0x3,
  }
}