// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.22

using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Distributed.Test.TaskActivator
{
  public sealed class Log : LogTemplate<Log>
  {
    public static readonly string Name;

    static Log()
    {
      string className = MethodBase.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}