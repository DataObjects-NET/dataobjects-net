// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    /// <param name="messageId">
    /// Identifier of the message in resources that should be written at the begging
    /// and at the end of the region or message itself.
    /// </param>
    /// <param name="parameters">Values of parameters in <paramref name="messageId"/> (resource string or message itself).</param>
    /// <returns><see cref="IDisposable"/> object. Region will closed by disposing of this object.</returns>
    public IndentManager.IndentScope DebugRegion(string messageId, params object[] parameters)
    {
      ArgumentNullException.ThrowIfNull(messageId, "message");
      if (!IsLogged(LogLevel.Debug))
        return IndentManager.IncreaseIndent();
      var message = Strings.ResourceManager.GetString(messageId, Strings.Culture) ?? messageId;
      var title = parameters!=null ? string.Format(message, parameters) : message;
      var titleParams = new object[] { title };
      Debug(nameof(Strings.LogRegionBegin), titleParams);
      return IndentManager.IncreaseIndent(() => Debug(nameof(Strings.LogRegionEnd), titleParams));
    }

    /// <summary>
    /// Creates region. Within the region, all messages are indented.
    /// </summary>
    /// <param name="messageId">
    /// Identifier of the message in resources that should be written at the begging
    /// and at the end of the region or message itself.
    /// </param>
    /// <param name="parameters">
    /// Values of parameters in <paramref name="messageId"/> (resource string or message itself).
    /// </param>
    /// <returns><see cref="IDisposable"/> object. Region will closed by disposing of this object.</returns>
    public IndentManager.IndentScope InfoRegion(string messageId, params object[] parameters)
    {
      ArgumentNullException.ThrowIfNull(messageId, "message");
      if (!IsLogged(LogLevel.Info))
        return IndentManager.IncreaseIndent();
      var message = Strings.ResourceManager.GetString(messageId, Strings.Culture) ?? messageId;
      var title = parameters!=null ? string.Format(message, parameters) : message;
      Info(string.Format(Strings.LogRegionBegin, title));
      return IndentManager.IncreaseIndent(() => Info(string.Format(Strings.LogRegionEnd, title)));
    }

    /// <summary>
    /// Writes debug message.
    /// </summary>
    /// <param name="messageId">
    /// Identifier of message in resources that should be written to log or message itself.
    /// </param>
    /// <param name="parameters">
    /// Values of parameters in <paramref name="messageId"/> (resource string or message itself).
    /// </param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Debug(string messageId, object[] parameters = null, Exception exception = null) =>
      Write(LogLevel.Debug, messageId, parameters, exception);

    /// <summary>
    /// Writes information message.
    /// </summary>
    /// <param name="messageId">
    /// Identifier of message in resources that should be written to log or message itself.</param>
    /// <param name="parameters">
    /// Values of parameters in <paramref name="messageId"/> (resource string or message itself).
    /// </param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Info(string messageId, object[] parameters = null, Exception exception = null) =>
      Write(LogLevel.Info, messageId, parameters, exception);

    /// <summary>
    /// Writes warning message.
    /// </summary>
    /// <param name="messageId">
    /// Identifier of message in resources that should be written to log or message itself.
    /// </param>
    /// <param name="parameters">
    /// Values of parameters in <paramref name="messageId"/> (resource string or message itself).
    /// </param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Warning(string messageId, object[] parameters = null, Exception exception = null) =>
      Write(LogLevel.Warning, messageId, parameters, exception);

    /// <summary>
    /// Writes error message.
    /// </summary>
    /// <param name="messageId">
    /// Identifier of message in resources that should be written to log or message itself.
    /// </param>
    /// <param name="parameters">
    /// Values of parameters in <paramref name="messageId"/> (resource string or message itself).
    /// </param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void Error(string messageId, object[] parameters = null, Exception exception = null) =>
      Write(LogLevel.Error, messageId, parameters, exception);

    /// <summary>
    /// Writes fatal error message.
    /// </summary>
    /// <param name="messageId">
    /// Identifier of message in resources that should be written to log or message itself.
    /// </param>
    /// <param name="parameters">
    /// Values of parameters in <paramref name="messageId"/> (resource string or message itself).
    /// </param>
    /// <param name="exception">Exception, which must be written.</param>
    public virtual void FatalError(string messageId, object[] parameters = null, Exception exception = null) =>
      Write(LogLevel.FatalError, messageId, parameters, exception);

    private void Write(LogLevel logLevel, string messageId, object[] parameters, Exception exception)
    {
      var message = string.IsNullOrEmpty(messageId)
        ? null
        : Strings.ResourceManager.GetString(messageId, Strings.Culture) ?? messageId;
      Write(new LogEventInfo(Name, logLevel, message, parameters, exception));
    }

    /// <summary>
    /// Writes log message.
    /// </summary>
    /// <param name="info">Log event information.</param>
    public abstract void Write(in LogEventInfo info);

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
