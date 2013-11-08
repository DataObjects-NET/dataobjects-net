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
    private readonly string logName;
    private readonly string indent;
    private readonly Exception exception;
    private string formattedMessage;

    /// <summary>
    /// Gets <see cref="LogLevel"/> of this instance.
    /// </summary>
    public LogLevel LogLevel { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.Append(indent);
      builder.AppendFormat("{0} | {1} | {2} ", CurrentTimeGetter.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture),
        LogLevel, logName);
      if (formattedMessage!=null)
        builder.AppendFormat("| {0} ", formattedMessage);
      if (exception!=null)
        builder.AppendFormat("| {0}", exception);
      return builder.ToString();
    }

    private void CreateFormattedMessage(string message, object[] parameters)
    {
      if (string.IsNullOrEmpty(message)) {
        formattedMessage = null;
        return;
      }
      if (parameters==null || parameters.Length==0) {
        formattedMessage = message;
        return;
      }
      try {
        formattedMessage = string.Format(message, parameters);
      }
      catch (Exception) {
        formattedMessage = message;
      }
    }

    internal LogEventInfo(BaseLog log, LogLevel level, string indent, string message=null, object[] parameters = null, Exception exception = null)
    {
      logName = log.Name;
      this.indent = indent;
      CreateFormattedMessage(message, parameters);
      this.exception = exception;
      LogLevel = level;
    }
  }
}
