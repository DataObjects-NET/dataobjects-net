// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.11

using System.Reflection;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  internal sealed class Log : LogTemplate<Log>
  {
    // Copy-paste this code!
    /// <summary>
    /// Gets the name of this log.
    /// </summary>
    public static readonly string Name;

    static Log()
    {
      Name = "Xtensive.Storage.Providers.Sql";
    }
  }
}