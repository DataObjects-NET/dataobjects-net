// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.20

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// Enumerates possible types of the <see cref="Session"/>.
  /// </summary>
  public enum SessionType
  {
    /// <summary>
    /// Default session type - a regular user session.
    /// Value is <see langword="0x0" />.
    /// </summary>
    Default = User,
    /// <summary>
    /// A regular user session.
    /// Value is <see langword="0x0" />.
    /// </summary>
    User = 0,
    /// <summary>
    /// A system session.
    /// Value is <see langword="0x1" />.
    /// </summary>
    System = 1,
  }
}