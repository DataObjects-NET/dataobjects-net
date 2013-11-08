// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Parent class for providers of logs. 
  /// </summary>
  public abstract class LogProvider
  {
    /// <summary>
    /// Gets log by name.
    /// </summary>
    /// <param name="logName">Name of log.</param>
    /// <returns>Founded log or default.</returns>
    public abstract BaseLog GetLog(string logName);
  }
}