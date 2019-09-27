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
  /// Base log.
  /// </summary>
  public abstract class BaseLog
  {
    /// <summary>
    /// Gets name of this log.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Checks if specified <see cref="LogLevel"/> is processed by current instance.
    /// </summary>
    /// <param name="level"><see cref="LogLevel"/> to check.</param>
    /// <returns>true if current instance supports specified <see cref="LogLevel"/>, otherwise false.</returns>
    public abstract bool IsLogged(LogLevel level);

    /// <summary>
    /// Creates region. Within the region, all messages are indented.
    /// </summary>
    /// <param name="message">Message, which writes to beginning of region and end of region.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <returns><see cref="IDisposable"/> object. Region will closed by disposing of this object.</returns>
    public IDisposable DebugRegion(string message, params object[] parameters)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      if (!IsLogged(LogLevel.Debug))
        return IndentManager.IncreaseIndent();
      var title = parameters!=null ? string.Format(message, parameters) : message;
      Debug(string.Format(Strings.LogRegionBegin, title));
      return IndentManager.IncreaseIndent(() => Debug(string.Format(Strings.LogRegionEnd, title)));
    }

    /// <summary>
    /// Creates region. Within the region, all messages are indented.
    /// </summary>
    /// <param name="message">Message, which writes to beginning of region and end of region.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <returns><see cref="IDisposable"/> object. Region will closed by disposing of this object.</returns>
    public IDisposable InfoRegion(string message, params object[] parameters)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      if (!IsLogged(LogLevel.Info))
        return IndentManager.IncreaseIndent();
      var title = parameters!=null ? string.Format(message, parameters) : message;
      Info(string.Format(Strings.LogRegionBegin, title));
      return IndentManager.IncreaseIndent(() => Info(string.Format(Strings.LogRegionEnd, title)));
    }

    /// <summary>
    /// Writes debug message.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Debug(string message, object[] parameters = null, Exception exception = null)
    {
      Write(new LogEventInfo(Name, LogLevel.Debug, message, parameters, exception));
    }

    /// <summary>
    /// Writes information message.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Info(string message, object[] parameters = null, Exception exception = null)
    {
      Write(new LogEventInfo(Name, LogLevel.Info, message, parameters, exception));
    }

    /// <summary>
    /// Writes warning message.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Warning(string message, object[] parameters = null, Exception exception = null)
    {
      Write(new LogEventInfo(Name, LogLevel.Warning, message, parameters, exception));
    }

    /// <summary>
    /// Writes error message.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Error(string message, object[] parameters = null, Exception exception = null)
    {
      Write(new LogEventInfo(Name, LogLevel.Error, message, parameters, exception));
    }

    /// <summary>
    /// Writes fatal error message.
    /// </summary>
    /// <param name="message">Message to write to.</param>
    /// <param name="parameters">Values of parameters in <paramref name="message"/>.</param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void FatalError(string message, object[] parameters = null, Exception exception = null)
    {
      Write(new LogEventInfo(Name, LogLevel.FatalError, message, parameters, exception));
    }

    /// <summary>
    /// Writes log message.
    /// </summary>
    /// <param name="info">Log event information.</param>
    public abstract void Write(LogEventInfo info);

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    protected BaseLog()
    {
      Name = string.Empty;
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    /// <param name="name">Log name.</param>
    protected BaseLog(string name)
    {
      Name = name;
    }
  }
}
