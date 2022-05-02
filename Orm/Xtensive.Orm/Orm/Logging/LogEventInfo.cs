// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
using System.Globalization;
using System.Text;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Represent information to write to target of log.
  /// </summary>
  public readonly struct LogEventInfo
  {
    /// <summary>
    /// Gets source of this event.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets log level for this event.
    /// </summary>
    public LogLevel Level { get; }

    /// <summary>
    /// Gets log message for this event.
    /// </summary>
    public string FormattedMessage { get; }

    /// <summary>
    /// Gets exception for this event.
    /// </summary>
    public Exception Exception { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.AppendFormat("{0} | {1} | {2} ",
        SystemClock.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture), Level, Source);
      if (FormattedMessage!=null)
        builder.AppendFormat("| {0} ", FormattedMessage);
      if (Exception!=null)
        builder.AppendFormat("| {0}", Exception);
      return builder.ToString();
    }

    private static string FormatMessage(string message, object[] parameters)
    {
      if (string.IsNullOrEmpty(message))
        return null;
      if (parameters==null || parameters.Length==0) {
        return message;
      }
      try {
        return string.Format(message, parameters);
      }
      catch (Exception) {
        return message;
      }
    }

    private static string AppendIndent(string message)
    {
      if (string.IsNullOrEmpty(message))
        return message;
      var indent = IndentManager.CurrentIdent;
      return indent > 0 ? new string(' ', indent) + message : message;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="source">Event source.</param>
    /// <param name="level">Event level.</param>
    /// <param name="message">Log message.</param>
    /// <param name="parameters">Format parameters for log message.</param>
    /// <param name="exception">Exception.</param>
    public LogEventInfo(string source, LogLevel level, string message = null, object[] parameters = null, Exception exception = null)
    {
      Source = source;
      Level = level;
      FormattedMessage = AppendIndent(FormatMessage(message, parameters));
      Exception = exception;
    }
  }
}
