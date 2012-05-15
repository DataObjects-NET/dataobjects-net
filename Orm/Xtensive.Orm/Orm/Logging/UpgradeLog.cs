// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.13

using Xtensive.Diagnostics;

namespace Xtensive.Orm
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  internal sealed class UpgradeLog: LogTemplate<UpgradeLog>
  {
    // Copy-paste this code!
    /// <summary>
    /// Gets the name of this log.
    /// </summary>
    public static readonly string Name;

    static UpgradeLog()
    {
      Name = "Xtensive.Orm.Upgrade";
    }
  }
}