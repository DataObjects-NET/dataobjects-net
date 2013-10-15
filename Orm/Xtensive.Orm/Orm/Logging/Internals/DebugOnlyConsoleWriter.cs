// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
using System.Diagnostics;
using System.Threading;

namespace Xtensive.Orm.Logging
{
  internal sealed class DebugOnlyConsoleWriter : ILogWriter
  {
    private bool debuggerIsAttached;
    private Timer debuggerAttachedRenewTimer;

    /// <inheritdoc/>
    public void Write(LogEventInfo logEvent)
    {
      if (debuggerIsAttached)
        Console.Out.WriteLine(logEvent);
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    public DebugOnlyConsoleWriter()
    {
      debuggerIsAttached = Debugger.IsAttached;
      debuggerAttachedRenewTimer = new Timer(
        state => debuggerIsAttached = Debugger.IsAttached,
        null, 1000, 1000);
    }
  }
}
