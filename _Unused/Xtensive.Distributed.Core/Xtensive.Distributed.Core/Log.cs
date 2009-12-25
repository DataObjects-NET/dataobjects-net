// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.18

using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Distributed.Core
{
  internal class Log : LogTemplate<Log>
  {
    public static readonly string Name;

    static Log()
    {
      string className = MethodInfo.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }

  }
}