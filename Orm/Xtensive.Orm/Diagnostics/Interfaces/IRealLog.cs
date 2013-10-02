// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Real log implementation.
  /// </summary>
  public interface IRealLog: ILogBase
  {
    /// <summary>
    /// Gets the <see cref="ILog"/> object wrapping this <see cref="IRealLog"/>.
    /// </summary>
    ILog Log { get; set; }

    /// <summary>
    /// Logs the event.
    /// </summary>
    /// <param name="eventType">The type of event to log.</param>
    /// <param name="message">Message to log.</param>
    /// <param name="exception">Exception to log.</param>
    /// <param name="sentTo">The original log, to which message was sent 
    /// (useful e.g. when <see cref="LogCaptureScope"/> is used to capture the events sent to another log).</param>
    /// <param name="capturedBy">The scope that captured the event, if any.</param>
    void LogEvent(LogEventTypes eventType, object message, Exception exception, IRealLog sentTo, LogCaptureScope capturedBy);

    /// <summary>
    /// Raised on any <see cref="LogEvent"/> call.
    /// </summary>
    event LogEventHandler OnLogEvent;
  }
}