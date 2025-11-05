// Copyright (C) 2013-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
