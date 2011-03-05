// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System.Reflection;
using Xtensive.Diagnostics;

namespace Xtensive.Indexing.Tests
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  internal sealed class Log : LogTemplate<Log>
  {
    // Copy-paste this code!
    public static readonly string Name;

    static Log()
    {
      string className = MethodBase.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}