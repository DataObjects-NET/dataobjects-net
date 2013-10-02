﻿// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.10.02

using Xtensive.Diagnostics;

namespace Xtensive.Core
{
  public class CoreLog : LogTemplate<CoreLog>
  {
    /// <summary>
    /// Gets the name of this log.
    /// </summary>
    public static readonly string Name;

    static CoreLog()
    {
      Name = "Xtensive.Core";
    }
  }
}