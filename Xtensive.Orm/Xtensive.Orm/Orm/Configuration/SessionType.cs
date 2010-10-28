// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.20

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Enumerates possible types of the <see cref="Session"/>.
  /// </summary>
  public enum SessionType
  {
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

    /// <summary>
    /// A generator session.
    /// Value is <see langword="0x2"/>
    /// </summary>
    KeyGenerator = 2,

    /// <summary>
    /// A service session.
    /// Value is <see langword="0x3"/>
    /// </summary>
    Service = 3,

    /// <summary>
    /// Default session type - <see cref="User"/>.
    /// Value is <see langword="0x0" />.
    /// </summary>
    Default = User,
  }
}