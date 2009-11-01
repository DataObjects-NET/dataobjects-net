// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Arithmetic
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  public sealed class Log: LogTemplate<Log>
  {
    // Copy-paste this code!
    /// <summary>
    /// Log name.
    /// </summary>
    public static readonly string Name;
    
    static Log()
    {
      string className = MethodInfo.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}