// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System.Reflection;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Aspects.Tests
{
  /// <summary>
  /// Null log - for testing only.
  /// </summary>
  public sealed class NullLog: LogTemplate<NullLog>
  {
    public static readonly string Name;
    
    static NullLog()
    {
      Name = "Null";
    }
  }
}