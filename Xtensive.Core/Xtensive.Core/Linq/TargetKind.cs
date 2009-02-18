// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;

namespace Xtensive.Core.Linq {

  /// <summary>
  /// Flags for specifying kind of compilable methods.
  /// </summary>
  [Flags]
  public enum TargetKind
  {
    /// <summary>
    /// Compilable method is a property getter
    /// </summary>
    PropertyGet = 0x1,
    /// <summary>
    /// Compilable method is as property setter
    /// </summary>
    PropertySet = 0x2,
    /// <summary>
    /// Compilable method is a regular method
    /// </summary>
    Method = 0x4,
    /// <summary>
    /// Compilable method is static
    /// </summary>
    Static = 0x8
  }
}