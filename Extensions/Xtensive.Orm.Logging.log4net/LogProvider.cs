// Copyright (C) 2014-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
