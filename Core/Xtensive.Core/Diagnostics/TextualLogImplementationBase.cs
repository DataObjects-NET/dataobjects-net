// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.06

using System;
using System.Threading;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;


namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Base class for logs producing textual output.
  /// </summary>
  [Serializable]
  public abstract class TextualLogImplementationBase : RealLogImplementationBase,
    IHasSyncRoot
  {
    private readonly static DateTime zeroTime = HighResolutionTime.Now;
    private LogFormat format;
    private string formatString = Strings.SimpleLogFormat;

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
    public LogFormat Format {
      get {
        lock (this)
          return format;
      }
      set {
        lock (this)
          format = value;
      }
    }

    /// <summary>
    /// Gets or sets the custom format string of logged messages.
    /// Setting value of this property sets <see cref="Format"/>
    /// to <see cref="LogFormat.Custom"/> as well.
    /// </summary>
    public string FormatString {
      get {
        lock (this)
          return formatString;
      }
      set {
        ArgumentValidator.EnsureArgumentNotNull(formatString, "formatString");
        lock (this) {
          format = LogFormat.Custom;
          formatString = value;
        }
      }
    }

    /// <inheritdoc/>
    public object SyncRoot {
      get { return this; }
    }

    /// <inheritdoc/>
    public sealed override void LogEvent(LogEventTypes eventType, object message, Exception exception, IRealLog sentTo, LogCaptureScope capturedBy)
    {
      if (IsLogged(eventType)) {
        DateTime currentTime;
        double elapsed;
        string currentFormat;
        lock (this) {
          currentTime = HighResolutionTime.Now;
          elapsed = (currentTime - zeroTime).TotalSeconds;
          currentFormat = GetCurrentFormat();
        }

        var thread = Thread.CurrentThread;
        string text;
        if (currentFormat.EndsWith("{5}")) {
          string prefix = string.Format(
            currentFormat,
            elapsed,
            thread.Name ?? thread.ManagedThreadId.ToString(),
            eventType.ToShortString(),
            Log.Name,
            LogIndentScope.CurrentIndentString,
            string.Empty,
            currentTime);
          text = prefix + (exception ?? message).ToString().Indent(prefix.Length, false);
        }
        else 
          text = string.Format(
            currentFormat,
            elapsed,
            thread.Name ?? thread.ManagedThreadId.ToString(),
            eventType.ToShortString(),
            Log.Name,
            LogIndentScope.CurrentIndentString,
            (exception ?? message),
            currentTime);

        LogEventText(text);
      }
      base.LogEvent(eventType, message, exception, sentTo, capturedBy);
    }

    /// <summary>
    /// Logs the formatted event text.
    /// </summary>
    /// <param name="text">The text to log.</param>
    protected abstract void LogEventText(string text);

    private string GetCurrentFormat()
    {
      switch (format) {
      case LogFormat.Release:
        return Strings.ReleaseLogFormat;
      case LogFormat.Simple:
        return Strings.SimpleLogFormat;
      case LogFormat.Custom:
        return formatString;
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