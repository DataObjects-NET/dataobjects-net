// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.16

namespace Xtensive.Orm.Logging
{
  internal abstract class LogWriter
  {
    public abstract void Write(in LogEventInfo logEvent);
  }
}
