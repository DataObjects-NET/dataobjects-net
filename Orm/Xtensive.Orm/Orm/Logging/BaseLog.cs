// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.10.11

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Base class for logs.
  /// </summary>
  public abstract class BaseLog
  {
    [ThreadStatic]
    protected static string indent;
    private readonly ILogWriter logWriter;
    public string Name;

    /// <summary>
    /// Creates region. Within the region, all messages are indented.
    /// </summary>
    /// <param name="message">Message, which writes to beginning of region and end of region.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <returns><see cref="IDisposable"/> object. Region will closed by disposing of this object.</returns>
    public virtual IDisposable DebugRegion(string message, params object[] parameters)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      string title = (parameters!=null) ? string.Format(message, parameters) : message;
      Debug(string.Format(Strings.LogRegionBegin, title), null);
      if (indent==null)
        indent = "";
      var oldIndent = indent;
      indent += "  ";
      return new Disposable(
        delegate(bool disposing) {
          indent = oldIndent;
          Debug(string.Format(Strings.LogRegionEnd, title), null);
        });
    }

    /// <summary>
    /// Writes debug message to log.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Debug(string message, object[] parameters, Exception exception = null)
    {
      var eventInfo = new LogEventInfo(this, LogLevel.Debug, indent, message, parameters, exception);
      logWriter.Write(eventInfo);
    }

    /// <summary>
    /// Creates region. Within the region, all messages are indented.
    /// </summary>
    /// <param name="message">Message, which writes to beginning of region and end of region.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <returns><see cref="IDisposable"/> object. Region will closed by disposing of this object.</returns>
    public virtual IDisposable InfoRegion(string message, params object[] parameters)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      string title = (parameters!=null) ? string.Format(message, parameters) : message;
      Info(string.Format(Strings.LogRegionBegin, title), null);
      if (indent==null)
        indent = "";
      var oldIndent = indent;
      indent += "  ";
      return new Disposable(
        delegate(bool disposing) {
          indent = oldIndent;
          Info(string.Format(Strings.LogRegionEnd, title), null);
        });
    }

    /// <summary>
    /// Writes information message to log.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Info(string message, object[] parameters, Exception exception = null)
    {
      var eventInfo = new LogEventInfo(this, LogLevel.Info, indent, message, parameters, exception);
      logWriter.Write(eventInfo);
    }

    /// <summary>
    /// Writes warning message to log.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Warn(string message, object[] parameters, Exception exception = null)
    {
      var eventInfo = new LogEventInfo(this, LogLevel.Warning, indent, message, parameters, exception);
      logWriter.Write(eventInfo);
    }

    /// <summary>
    /// Writes error message to log.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Error(string message, object[] parameters, Exception exception = null)
    {
      var eventInfo = new LogEventInfo(this, LogLevel.Error, indent, message, parameters, exception);
      logWriter.Write(eventInfo);
    }

    /// <summary>
    /// Writes error message to log.
    /// </summary>
    /// <param name="exception">Exception to write to.</param>
    /// <returns>The same exception that was pass in <paramref name="exception"/> parameter.</returns>
    public virtual Exception Error(Exception exception)
    {
      Debug(null, null, exception);
      return exception;
    }

    /// <summary>
    /// Writes fatal error message to log.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Fatal(string message, object[] parameters, Exception exception = null)
    {
      var eventInfo = new LogEventInfo(this, LogLevel.Fatal, indent, message, parameters, exception);
      logWriter.Write(eventInfo);
    }

    public BaseLog()
    {
      Name = "";
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    /// <param name="name">Name of instance of log.</param>
    /// <param name="logWriter">One of realizations of <see cref="ILogWriter"/> i.e target, in which message will be written.</param>
    public BaseLog(string name, ILogWriter logWriter)
    {
      Name = name;
      this.logWriter = logWriter; 
    }
  }
}
