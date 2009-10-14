// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.06

using System;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Diagnostics
{
  /// <summary>
  /// Base class for logs producing textual output.
  /// </summary>
  [Serializable]
  public abstract class TextualLogImplementationBase : RealLogImplementationBase,
    IHasSyncRoot
  {
    private readonly static DateTime zeroTime = HighResolutionTime.Now;
    private LogFormat logFormat;
    private string customFormat;

    /// <inheritdoc/>
    public override LogEventTypes LoggedEventTypes {
      get {
        lock (this)
          return loggedEventTypes;
      }
      set {
        lock (this) {
          loggedEventTypes = value;
          UpdateCachedProperties();
        }
      }
    }
    
    /// <summary>
    /// Gets the log format.
    /// </summary>
    public LogFormat LogFormat {
      get {
        lock (this)
          return logFormat;
      }
      set {
        lock (this)
          logFormat = value;
      }
    }

    /// <summary>
    /// Gets or sets the custom format of logged messages.
    /// </summary>
    public string CustomFormat {
      get {
        lock (this)
          return customFormat;
      }
      set {
        ArgumentValidator.EnsureArgumentNotNull(customFormat, "customFormat");
        lock (this)
          customFormat = value;
      }
    }

    /// <inheritdoc/>
    public object SyncRoot {
      get { return this; }
    }

    /// <inheritdoc/>
    public sealed override void LogEvent(LogEventTypes eventType, object message, Exception exception, IRealLog sentTo, LogCaptureScope capturedBy)
    {
      double elapsed;
      string format;
      lock (this) {
        elapsed = (long) (HighResolutionTime.Now - zeroTime).TotalSeconds;
        format = GetLogFormat();
      }

      var thread = Thread.CurrentThread;
      string text = string.Format(
        format, 
        elapsed,
        thread.Name ?? thread.ManagedThreadId.ToString(),
        eventType.ToShortString(),
        Log.Name,
        LogIndentScope.CurrentIndentString,
        exception ?? message);

      LogEventText(text);
      base.LogEvent(eventType, message, exception, sentTo, capturedBy);
    }

    /// <summary>
    /// Logs the formatted event text.
    /// </summary>
    /// <param name="text">The text to log.</param>
    protected abstract void LogEventText(string text);

    private string GetLogFormat()
    {
      switch (logFormat) {
      case LogFormat.Simple:
        return Strings.SimpleLogFormat;
      case LogFormat.Custom:
        return customFormat;
      default:
        return Strings.ComprehensiveLogFormat;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Log name.</param>
    protected TextualLogImplementationBase(string name)
      : base(name)
    {
      loggedEventTypes = LogEventTypes.All;
    }
  }
}