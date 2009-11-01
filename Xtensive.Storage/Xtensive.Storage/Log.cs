// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.13

using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Storage
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  public sealed class Log: LogTemplate<Log>
  {
    // Copy-paste this code!
    public static readonly string Name;

    static Log()
    {
      string className = MethodInfo.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}