// Copyright (C) 2003-2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2013.12.13

namespace Xtensive.Orm.Logging.NLog
{
  /// <summary>
  /// Provides NLog specific <see cref="BaseLog"/> descendant instances.
  /// </summary>
  public class LogProvider : Logging.LogProvider
  {
    /// <inheritdoc />
    public override BaseLog GetLog(string logName)
    {
      return new Log(logName);
    }
  }
}