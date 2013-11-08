// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Log event types.
  /// </summary>
  [Flags]
  public enum LogEventTypes
  {
    /// <summary>
    /// None of the events.
    /// </summary>
    None = 0,
    /// <summary>
    /// Debug-only event.
    /// </summary>
    Debug = 0x1,
    /// <summary>
    /// Information.
    /// </summary>
    Info = 0x2,
    /// <summary>
    /// Warning.
    /// </summary>
    Warning = 0x4,
    /// <summary>
    /// Error.
    /// </summary>
    Error = 0x8,
    /// <summary>
    /// Fatal error.
    /// </summary>
    FatalError = 0x18,
    /// <summary>
    /// All events.
    /// </summary>
    All = 0x1F,
  }
}