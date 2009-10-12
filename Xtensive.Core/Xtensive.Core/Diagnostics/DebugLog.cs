// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.06

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Diagnostics
{
  [Serializable]
  public class DebugLog : RealLogImplementationBase
  {
    private static readonly Stopwatch Clock;

    public override void LogEvent(LogEventTypes eventType, object message, Exception exception, IRealLog sentTo, LogCaptureScope capturedBy)
    {
      long elapsed;
      lock (Clock) {
        elapsed = Clock.ElapsedMilliseconds;
      }
      var thread = Thread.CurrentThread;
      Debug.WriteLine(string.Format(
        Resources.Strings.DebugLogFormat, 
        elapsed,
        thread.Name ?? thread.ManagedThreadId.ToString(),
        eventType.ToShortString(),
        Log.Name,
        LogIndentScope.CurrentIndentString,
        exception ?? message));
      base.LogEvent(eventType, message, exception, sentTo, capturedBy);
    }

    /// <summary>
    /// Creates a new <see cref="StringLog"/> object.
    /// </summary>
    /// <returns>Newly created <see cref="StringLog"/> object.</returns>
    public static ILog Create()
    {
      return Create(LogProvider.Debug);
    }

    /// <summary>
    /// Creates a new <see cref="StringLog"/> object.
    /// </summary>
    /// <param name="name">Log name.</param>
    /// <returns>Newly created <see cref="StringLog"/> object.</returns>
    public static ILog Create(string name)
    {
      return new LogImplementation(new DebugLog(name));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Log name.</param>
    public DebugLog(string name)
      : base(name)
    {
    }

    // Type initializer

    static DebugLog()
    {
      Clock = new Stopwatch();
      Clock.Start();
    }
  }
}