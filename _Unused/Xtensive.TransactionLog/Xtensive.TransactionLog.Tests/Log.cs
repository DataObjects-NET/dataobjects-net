// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.03

using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.TransactionLog.Tests
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