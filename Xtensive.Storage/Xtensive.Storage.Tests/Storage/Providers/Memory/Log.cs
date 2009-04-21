// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.20

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Storage.Tests.Storage.Providers.Memory
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