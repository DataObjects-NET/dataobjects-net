// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.10.29

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Diagnostics;

namespace Xtensive.Storage.Providers.Indexing
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
      string className = MethodInfo.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}