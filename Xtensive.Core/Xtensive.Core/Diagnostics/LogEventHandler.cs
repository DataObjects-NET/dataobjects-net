// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// <see cref="IRealLog"/> event handler (see <see cref="IRealLog.OnLogEvent"/>).
  /// </summary>
  /// <param name="source">Event source.</param>
  /// <param name="eventType">Event type.</param>
  /// <param name="message">Message to log.</param>
  /// <param name="exception">Exception to log.</param>
  /// <param name="capturedBy">The scope which captured this event, if any.</param>
  public delegate void LogEventHandler(IRealLog source, LogEventTypes eventType, object message, Exception exception, LogCaptureScope capturedBy);
}