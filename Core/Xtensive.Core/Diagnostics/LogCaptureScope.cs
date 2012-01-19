using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.IoC;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Log capture scope.
  /// Forward all the log messages sent to any <see cref="LogTemplate{T}"/> (or to the specified set of them) 
  /// to specified <see cref="Log"/> as well.
  /// </summary>
  public sealed class LogCaptureScope: Scope<ILog>
  {
    private readonly bool logEnterLeave;
    private int initialIndent;
    private LogEventTypes capturedEventTypes;
    private LogEventTypes captureEventTypes;
    private readonly ReadOnlySet<ILog> captureFilter;

    internal new static LogCaptureScope CurrentScope {
      get {
        return (LogCaptureScope)Scope<ILog>.CurrentScope;
      }
    }

    private new LogCaptureScope OuterScope
    {
      get { return (LogCaptureScope)base.OuterScope; }
    }

    /// <summary>
    /// Gets enabled event types for this and outer <see cref="LogCaptureScope"/>s.
    /// </summary>
    public LogEventTypes CaptureEventTypes
    {
      get { return captureEventTypes; }
    }

    /// <summary>
    /// Gets capture filter for this scope.
    /// <see langword="Null"/> implies events from all <see cref="ILog"/>s are captured.
    /// </summary>
    public ReadOnlySet<ILog> CaptureFilter
    {
      get { return captureFilter; }
    }

    /// <summary>
    /// Gets <see cref="LogIndentScope.Indent"/> value on the moment
    /// when this <see cref="LogCaptureScope"/> was created.
    /// </summary>
    public int InitialIndent
    {
      get { return initialIndent; }
    }

    /// <summary>
    /// Clears captured event types state.
    /// </summary>
    public void Clear()
    {
      capturedEventTypes = LogEventTypes.None;
    }

    /// <summary>
    /// Returns <see langword="true"/> if at least one event with specified event type are captured by current instance of <see cref="LogCaptureScope"/>.
    /// </summary>
    /// <param name="eventType">Event type to check.</param>
    /// <returns><see langword="true"/> if this scope captured events with specified 
    /// event type; otherwise <see langword="false"/>.</returns>
    public bool IsCaptured(LogEventTypes eventType)
    {
      return (capturedEventTypes & eventType) != 0;
    }


    /// <summary>
    /// Returns <see langword="true"/> if this or outer scope should capture specified 
    /// event type.
    /// </summary>
    /// <param name="eventType">Event type to check.</param>
    /// <returns><see langword="True"/> if this or outer scope should capture specified 
    /// event type.</returns>
    public bool IsCapturing(LogEventTypes eventType)
    {
      return (captureEventTypes & eventType)!=0;
    }

    /// <summary>
    /// Returns <see langword="true"/> if this scope should capture events from 
    /// specified <see cref="ILog"/>.
    /// </summary>
    /// <param name="log">Log to check.</param>
    /// <returns><see langword="True"/> if this scope should capture events from 
    /// specified <see cref="ILog"/>.</returns>
    public bool IsCapturing(ILog log)
    {
      return (captureFilter == null || captureFilter.Contains(log));
    }

    internal void OnLogEvent(IRealLog realLog, LogEventTypes eventType, object message, Exception exception)
    {
      if (!IsCapturing(eventType))
        return;
      capturedEventTypes |= eventType;
      if (IsCapturing(realLog.Log))
        Context.RealLog.LogEvent(eventType, message, exception, realLog, this);
      LogCaptureScope outerScope = OuterScope;
      if (outerScope!=null)
        outerScope.OnLogEvent(realLog, eventType, message, exception);
    }

    /// <inheritdoc/>
    public override void Activate(ILog newContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(newContext, "newContext");
      base.Activate(newContext); // To ensure Context is already set here
      LogCaptureScope outerOrThisScope = OuterScope;
      outerOrThisScope = outerOrThisScope ?? this;
      captureEventTypes = Context.LoggedEventTypes | outerOrThisScope.CaptureEventTypes;
      initialIndent = LogIndentScope.CurrentIndent;
    }

    
    // Constructors

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="log">New log to use.</param>
    /// <param name="captureFilter">A set of logs which events should be captured;
    /// <see langword="null"/> means events from all logs should be captured.</param>
    public LogCaptureScope(ILog log, params ILog[] captureFilter)
      : this(log, false, captureFilter)
    {}

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="log">New log to use.</param>
    /// <param name="logEnterLeave">Logs entering and leaving this scope.</param>
    /// <param name="captureFilter">A set of logs which events should be captured;
    /// <see langword="null"/> means events from all logs should be captured.</param>
    public LogCaptureScope(ILog log, bool logEnterLeave, params ILog[] captureFilter)
      : base(log)
    {
      this.logEnterLeave = logEnterLeave;
      if (captureFilter!=null && captureFilter.Length!=0)
        this.captureFilter = new ReadOnlySet<ILog>(new SetSlim<ILog>(captureFilter));
      if (logEnterLeave)
        Log.Debug("LogCaptureScope: Enter \"{0}\"", log);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (logEnterLeave)
        Log.Debug("LogCaptureScope: Leave \"{0}\"", Context);
      base.Dispose(disposing);
    }
  }
}
