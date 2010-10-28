// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.13

using System.Reflection;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  public sealed class Log : LogTemplate<Log>
  {
    // Copy-paste this code!
    /// <summary>
    /// Gets the name of this log.
    /// </summary>
    public static readonly string Name;

    static Log()
    {
      string className = MethodBase.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}