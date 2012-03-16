// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// "Debug only" console log implementation.
  /// This log writes out to console only when debugger is attached to the current process;
  /// otherwise is does nothing.
  /// </summary>
  public sealed class DebugOnlyConsoleLog : TextualLogImplementationBase
  {
    private static volatile bool isDebuggerAttached = false;
    private static Timer timer;

    /// <inheritdoc/>
    protected override void LogEventText(string text)
    {
      if (isDebuggerAttached)
        Console.Out.WriteLine(text);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Log name.</param>
    public DebugOnlyConsoleLog(string name)
      : base(name)
    {
    }

    static DebugOnlyConsoleLog()
    {
      isDebuggerAttached = Debugger.IsAttached;
      timer = new Timer(
        state => isDebuggerAttached = Debugger.IsAttached,
        null, 0, 1000); // Every second
    }
  }
}