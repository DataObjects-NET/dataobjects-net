// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Logging
{
  /// <summary>
  /// Standarn log, which creates form <see cref="LogConfiguration"/>. 
  /// </summary>
  public sealed class Log: BaseLog
  {
    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    /// <param name="name">Name of instance of log.</param>
    /// <param name="logWriter">One of realizations of <see cref="ILogWriter"/> i.e target, in which message will be written.</param>
    public Log(string name, ILogWriter logWriter)
      :base(name, logWriter)
    {
    }
  }
}
