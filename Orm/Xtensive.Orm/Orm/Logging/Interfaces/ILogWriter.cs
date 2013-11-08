// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.16

namespace Xtensive.Orm.Logging
{
  public interface ILogWriter
  {
    /// <summary>
    /// Writes <see cref="LogEventInfo"/> instance directly to target of log.
    /// </summary>
    /// <param name="logEvent"><see cref="LogEventInfo"/> instance to write to.</param>
    void Write(LogEventInfo logEvent);
  }
}
