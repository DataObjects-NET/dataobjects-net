// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.14

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Enumerates supported log providers.
  /// </summary>
  public enum LogProviderType
  {
    /// <summary>
    /// Refers to <see cref="NullLog"/> provider.
    /// </summary>
    Null = 0,
    /// <summary>
    /// Refers to <see cref="ConsoleLog"/> provider.
    /// </summary>
    Console = 1,
    /// <summary>
    /// Refers to <see cref="DebugLog"/> provider.
    /// </summary>
    Debug = 2,
    /// <summary>
    /// Refers to <see cref="DebugLog"/> provider.
    /// </summary>
    Error = 3,
    /// <summary>
    /// Refers to <see cref="FileLog"/> provider.
    /// </summary>
    File = 4,
    /// <summary>
    /// Refers to <see cref="DebugOnlyConsoleLog"/> provider.
    /// </summary>
    DebugOnlyConsole = 5,
  }
}