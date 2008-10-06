// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.10.06

using System;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// Enumerates possible options of the <see cref="Session"/>.
  /// </summary>
  [Flags]
  public enum SessionOptions
  {
    /// <summary>
    /// Default options.
    /// Value is <see langword="0x0" />.
    /// </summary>
    Default = 0x0,
    /// <summary>
    /// Session works in UI mode.
    /// Value is <see langword="0x1" />.
    /// </summary>
    UI = 0x1
  }
}