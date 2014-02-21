﻿// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;

namespace Xtensive.Orm.Logging
{
  internal sealed class ConsoleWriter : LogWriter
  {
    /// <inheritdoc/>
    public override void Write(LogEventInfo logEvent)
    {
      Console.Out.WriteLine(logEvent);
    }
  }
}
