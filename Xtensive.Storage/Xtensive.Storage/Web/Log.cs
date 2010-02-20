// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.21

using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Storage.Web
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  internal sealed class Log: LogTemplate<Log>
  {
    // Copy-paste this code!
    /// <summary>
    /// Gets the name of this log.
    /// </summary>
    public static readonly string Name;

    static Log()
    {
      string className = MethodInfo.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}