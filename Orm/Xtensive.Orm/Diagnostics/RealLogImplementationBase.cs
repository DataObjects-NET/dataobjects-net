// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;
using Xtensive.Core;


namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Base class for any <see cref="IRealLog"/> implementation.
  /// </summary>
  public abstract class RealLogImplementationBase: IRealLog
  {
    private readonly string name;
    private ILog log;
    private LogEventHandler onLogEvent;
    protected LogEventTypes loggedEventTypes;

    /// <inheritdoc/>
    public string Name {
      get { return name; }
    }

    /// <inheritdoc/>
    public virtual string Text {
      get { 
        throw new NotImplementedException();
      }
    }

    /// <inheritdoc/>
    public ILog Log {
      get { return log; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        if (log!=null)
          throw Exceptions.AlreadyInitialized("Log");
        if (value.RealLog!=this)
          throw Exceptions.InternalError("RealLogImplementationBase.Log: value.RealLog!=this", CoreLog.Instance);
        log = value;
        Log.UpdateCachedProperties();
      }
    }

    /// <inheritdoc/>
    public virtual LogEventTypes LoggedEventTypes {
      get { return loggedEventTypes; }
      set {
        throw Exceptions.AlreadyInitialized("LoggedEventTypes");
      }
    }

    /// <inheritdoc/>
    public bool IsLogged(LogEventTypes eventType)
    {
      LogCaptureScope currentScope = LogCaptureScope.CurrentScope;
      return ((loggedEventTypes | 
        (currentScope==null ? 0 : currentScope.CaptureEventTypes)) & eventType)!=0;
    }

    /// <inheritdoc/>
    public virtual void LogEvent(LogEventTypes eventType, object message, Exception exception, IRealLog sentTo, LogCaptureScope capturedBy)
    {
      if (capturedBy==null) {
        LogCaptureScope current = LogCaptureScope.CurrentScope;
        if (current!=null)
          current.OnLogEvent(this, eventType, message, exception);
      }
      if (onLogEvent!=null)
        onLogEvent(this, eventType, message, exception, capturedBy);
    }

    /// <inheritdoc/>
    public event LogEventHandler OnLogEvent {
      add    { onLogEvent += value; }
      remove { onLogEvent -= value; }
    }

    /// <inheritdoc/>
    public virtual void UpdateCachedProperties()
    {
      if (Log!=null)
        Log.UpdateCachedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="name">Log name.</param>
    protected RealLogImplementationBase(string name)
    {
      this.name = name;
      UpdateCachedProperties();
    }
  }
}