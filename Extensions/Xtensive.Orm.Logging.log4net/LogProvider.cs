// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.20

using System.Reflection;
using log4netManager = log4net.LogManager;

namespace Xtensive.Orm.Logging.log4net
{
  /// <summary>
  /// Provides log4net specific <see cref="BaseLog"/> descendant instances.
  /// </summary>
  public class LogProvider : Logging.LogProvider
  {
    /// <inheritdoc />
    public override BaseLog GetLog(string logName)
    {
      return new Log(Assembly.GetCallingAssembly(), logName);
    }
  }
}
