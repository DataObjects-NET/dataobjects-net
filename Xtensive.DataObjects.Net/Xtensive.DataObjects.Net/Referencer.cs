// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.21

using System;

namespace Xtensive.Storage.Merged
{
  /// <summary>
  /// Does nothing, but references types from all Storage assemblies.
  /// </summary>
  public sealed class Referencer
  {
    private Type[] types = new [] {
      typeof (All.Referencer), // Referencing all storage assemblies
    };


    // Constructors

    // This is the only one. So you can't instantiate this type.
    private Referencer()
    {
    }
  }
}