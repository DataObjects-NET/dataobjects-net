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
  public sealed class LogEventInfo
  {
    private readonly string source;
    private readonly string indent;
    private readonly Exception exception;
    private readonly LogLevel level;
    private readonly string formattedMessage;

    /// <inheritdoc/>
    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.Append(indent);
      builder.AppendFormat("{0} | {1} | {2} ",
        SystemClock.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture), level, source);
      if (formattedMessage!=null)
        builder.AppendFormat("| {0} ", formattedMessage);
      if (exception!=null)
        builder.AppendFormat("| {0}", exception);
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

    internal LogEventInfo(string source, LogLevel level, string message = null, object[] parameters = null, Exception exception = null)
    {
      this.source  = source;
      this.level = level;
      formattedMessage = FormatMessage(message, parameters);
      this.exception = exception;
      indent = IndentManager.CurrentIdent;
    }
  }
}
