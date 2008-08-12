// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.20

using System.Diagnostics;
using System.IO;
using System.Text;
using Xtensive.Core.Diagnostics.Helpers;
using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Diagnostics
{
  /// <summary>
  /// Log writing all events to <see cref="StringBuilder"/> <see cref="Output"/>.
  /// </summary>
  public class StringLog: RealLogImplementationBase,
    IHasSyncRoot
  {
    private readonly StringBuilder output  = new StringBuilder();
    private DateTime zeroTime;
    private string format = "{0,-8:F4} {1,-10} {2,-20} {3}\r\n";

    /// <inheritdoc/>
    public override LogEventTypes LoggedEventTypes {
      get {
        using (LockType.Exclusive.LockRegion(SyncRoot))
          return loggedEventTypes;
      }
      set {
        using (LockType.Exclusive.LockRegion(SyncRoot)) {
          loggedEventTypes = value;
          UpdateCachedProperties();
        }
      }
    }
    
    /// <summary>
    /// Gets or sets the format of logged messages.
    /// </summary>
    public string Format {
      get {
        using (LockType.Exclusive.LockRegion(SyncRoot))
          return format;
      }
      set {
        using (LockType.Exclusive.LockRegion(SyncRoot))
          format = value;
      }
    }

    /// <summary>
    /// Gets or sets the zero time.
    /// </summary>
    public DateTime ZeroTime
    {
      get
      {
        using (LockType.Exclusive.LockRegion(SyncRoot))
          return zeroTime;
      }
      set
      {
        using (LockType.Exclusive.LockRegion(SyncRoot))
          zeroTime = value;
      }
    }

    /// <summary>
    /// Gets output of this log (as <see cref="StringBuilder"/>).
    /// </summary>
    public StringBuilder Output
    {
      [DebuggerStepThrough]
      get { return output; }
    }

    /// <summary>
    /// Gets output of this log (as <see cref="String"/>).
    /// </summary>
    public override string Text {
      get {
        using (LockType.Exclusive.LockRegion(SyncRoot))
          return output.ToString();
      }
    }

    /// <inheritdoc/>
    public override void LogEvent(LogEventTypes eventType, object message, Exception exception, IRealLog sentTo, LogCaptureScope capturedBy)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot)) {
        TimeSpan time = HighResolutionTime.Now - zeroTime;
        output.AppendFormat(format, time.TotalSeconds, eventType, sentTo.Name, message);
        base.LogEvent(eventType, message, exception, sentTo, capturedBy);
      }
    }

    /// <inheritdoc/>
    public object SyncRoot
    {
      [DebuggerStepThrough]
      get { return this; }
    }

    
    // LogImplementation constructors

    /// <summary>
    /// Creates a new <see cref="StringLog"/> object.
    /// </summary>
    /// <returns>Newly created <see cref="StringLog"/> object.</returns>
    public static LogImplementation Create()
    {
      return Create("Unnamed");
    }

    /// <summary>
    /// Creates a new <see cref="StringLog"/> object.
    /// </summary>
    /// <param name="name">Log name.</param>
    /// <returns>Newly created <see cref="StringLog"/> object.</returns>
    public static LogImplementation Create(string name)
    {
      return new LogImplementation(new StringLog(name));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Log name.</param>
    private StringLog(string name) 
      : base(name)
    {
      loggedEventTypes = LogEventTypes.All;
      zeroTime = HighResolutionTime.Now;
    }
  }
}