// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;

namespace Xtensive.Orm.Logging
{
  public sealed class ConsoleWriter : ILogWriter
  {
    /// <inheritdoc/>
    public void Write(LogEventInfo logEvent)
    {
      Console.Out.WriteLine(logEvent);
    }
  }
}
