// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
using System.Diagnostics;
using System.Threading;

namespace Xtensive.Orm.Logging
{
  internal sealed class DebugOnlyConsoleWriter : LogWriter
  {
    private bool debuggerIsAttached;
    private Timer debuggerAttachedRenewTimer;

    /// <inheritdoc/>
    public override void Write(in LogEventInfo logEvent)
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
