// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Resources;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Actual <see cref="ILog"/> implementation
  /// forwarding all the events to its <see cref="RealLog"/>.
  /// </summary>
  public abstract class LogImplementationBase: ILog
  {
    private readonly string name;
    private readonly IRealLog realLog;
    private LogEventTypes loggedEventTypes;

    /// <inheritdoc/>
    public string Name {
      get { return name; }
    }

    /// <inheritdoc/>
    public string Text {
      get { return realLog.Text; }
    }

    /// <inheritdoc/>
    public IRealLog RealLog {
      get { return realLog; }
    }

    /// <inheritdoc/>
    public LogEventTypes LoggedEventTypes {
      get { return loggedEventTypes; }
    }

    /// <inheritdoc/>
    public bool IsLogged(LogEventTypes eventType)
    {
      LogCaptureScope currentScope = LogCaptureScope.CurrentScope;
      return ((loggedEventTypes | 
        (currentScope==null ? 0 : currentScope.CaptureEventTypes)) & eventType)!=0;
    }

    /// <inheritdoc/>
    public virtual void UpdateCachedProperties()
    {
      loggedEventTypes = realLog.LoggedEventTypes;
    }

    /// <summary>
    /// Gets the formatted message.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    protected virtual object GetFormattedMessage(string format, object[] args)
    {
      return string.Format(format, args);
    }

    #region ILog logging methods

    /// <inheritdoc/>
    public void Debug(string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Debug, stringFormat, null, RealLog, null);
    }

    /// <inheritdoc/>
    public Exception Debug(Exception exception, string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Debug, stringFormat, exception, RealLog, null);
      return exception;
    }

    /// <inheritdoc/>
    public Exception Debug(Exception exception)
    {
      return Debug(exception, Strings.LogException);
    }

    /// <inheritdoc/>
    public IDisposable DebugRegion(string format, params object[] args)
    {
      string title = string.Format(format, args);
      Debug(string.Format(Strings.LogRegionBegin, title));
      return new Disposable<IDisposable>(
        new LogIndentScope(),
        delegate(bool disposing, IDisposable disposable) {
          disposable.DisposeSafely(true);
          Debug(string.Format(Strings.LogRegionEnd, title));
        });
    }

    /// <inheritdoc/>
    public void Info(string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Info, stringFormat, null, RealLog, null);
    }

    /// <inheritdoc/>
    public Exception Info(Exception exception, string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Info, stringFormat, exception, RealLog, null);
      return exception;
    }

    /// <inheritdoc/>
    public Exception Info(Exception exception)
    {
      return Info(exception, Strings.LogException);
    }

    /// <inheritdoc/>
    public IDisposable InfoRegion(string format, params object[] args)
    {
      string title = string.Format(format, args);
      Info(string.Format(Strings.LogRegionBegin, title));
      return new Disposable<IDisposable>(
        new LogIndentScope(),
        delegate(bool disposing, IDisposable disposable) {
          disposable.DisposeSafely(true);
          Info(string.Format(Strings.LogRegionEnd, title));
        });
    }

    /// <inheritdoc/>
    public void Warning(string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Warning, stringFormat, null, RealLog, null);
    }

    public Exception Warning(Exception exception, string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Warning, stringFormat, exception, RealLog, null);
      return exception;
    }

    /// <inheritdoc/>
    public Exception Warning(Exception exception)
    {
      return Warning(exception, Strings.LogException);
    }

    /// <inheritdoc/>
    public void Error(string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Error, stringFormat, null, RealLog, null);
    }

    /// <inheritdoc/>
    public Exception Error(Exception exception, string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.Error, stringFormat, exception, RealLog, null);
      return exception;
    }

    /// <inheritdoc/>
    public Exception Error(Exception exception)
    {
      return Error(exception, Strings.LogException);
    }

    /// <inheritdoc/>
    public void FatalError(string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.FatalError, stringFormat, null, RealLog, null);
    }

    public Exception FatalError(Exception exception, string format, params object[] args)
    {
      object stringFormat = GetFormattedMessage(format, args);
      RealLog.LogEvent(LogEventTypes.FatalError, stringFormat, exception, RealLog, null);
      return exception;
    }

    /// <inheritdoc/>
    public Exception FatalError(Exception exception)
    {
      return FatalError(exception, Strings.LogException);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return realLog.ToString();
    }

    #region IContext<LogCaptureScope> Members

    /// <inheritdoc/>
    bool IContext.IsActive {
      get {
        return false;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    IDisposable IContext.Activate()
    {
      return (this as IContext<LogCaptureScope>).Activate();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    LogCaptureScope IContext<LogCaptureScope>.Activate()
    {
      throw new NotSupportedException(Strings.ExUseLogCaptureScopeConstructorInstead);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="realLog">Real log to wrap.</param>
    protected LogImplementationBase(IRealLog realLog)
    {
      ArgumentValidator.EnsureArgumentNotNull(realLog, "realLog");
      this.realLog = realLog;
      this.name = realLog.Name;
      realLog.Log = this;
      UpdateCachedProperties();
    }
  }
}