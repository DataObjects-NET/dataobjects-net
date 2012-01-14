// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.27

using System;
using System.Diagnostics;

namespace Xtensive.Orm
{
  /// <summary>
  /// Enumerates <see cref="DisconnectedState.Versions"/> usage options.
  /// </summary>
  [Flags]
  public enum VersionsUsageOptions
  {
    /// <summary>
    /// Default value.
    /// The same as <see cref="All"/>.
    /// </summary>
    Default = All,
    /// <summary>
    /// Validate versions.
    /// Value is <see langword="0x1" />.
    /// </summary>
    Validate = 0x1,
    /// <summary>
    /// Update versions.
    /// Value is <see langword="0x2" />.
    /// </summary>
    Update = 0x2,
    /// <summary>
    /// All options (<see cref="Validate"/> and <see cref="Update"/>).
    /// Value is <see langword="0x3" />.
    /// </summary>
    All = 0x3,
  }
}