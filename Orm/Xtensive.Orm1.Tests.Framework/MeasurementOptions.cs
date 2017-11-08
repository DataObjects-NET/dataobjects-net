// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// <see cref="Measurement"/> options.
  /// </summary>
  [Flags]
  public enum MeasurementOptions
  {
    /// <summary>
    /// Don't collect garbage and don't write to log.
    /// </summary>
    None = 0,
    /// <summary>
    /// Default measurement options: <see cref="Log"/>, <see cref="CollectGarbageOnEnter"/>, <see cref="CollectGarbageOnLeave"/>.
    /// </summary>
    Default = 0x103,
    /// <summary>
    /// Indicates whether garbage must be collected both on enter and leave.
    /// </summary>
    CollectGarbage = 0x3,
    /// <summary>
    /// Indicates whether garbage must be collected on enter.
    /// </summary>
    CollectGarbageOnEnter = 0x1,
    /// <summary>
    /// Indicates whether garbage must be collected on leave.
    /// </summary>
    CollectGarbageOnLeave = 0x2,
    /// <summary>
    /// Indicates whether measurement results must be logged.
    /// </summary>
    Log      = 0x100,
    /// <summary>
    /// Indicates whether measurement start must be logged.
    /// </summary>
    LogEnter = 0x200,
  }
}