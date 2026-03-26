// Copyright (C) 2013-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2013.12.13

using NLog;
using NLogLevel = NLog.LogLevel;
using NLogManager = NLog.LogManager;

namespace Xtensive.Orm.Logging.NLog
{
  /// <summary>
  /// Log implementation for NLog.
  /// </summary>
  public class Log : BaseLog
  {
    private readonly Logger target;

    /// <inheritdoc/>
    public override bool IsLogged(LogLevel level)
    {
      return target.IsEnabled(ConvertLevel(level));
    }

    /// <inheritdoc/>
    public override void Write(in LogEventInfo info)
    {
      if (info.Exception!=null)
        target.Log(ConvertLevel(info.Level), info.Exception, info.FormattedMessage);
      else
        target.Log(ConvertLevel(info.Level), info.FormattedMessage);
    }

    private static NLogLevel ConvertLevel(in LogLevel level)
    {
      switch (level) {
        case LogLevel.Debug:
          return NLogLevel.Debug;
        case LogLevel.Error:
          return NLogLevel.Error;
        case LogLevel.FatalError:
          return NLogLevel.Fatal;
        case LogLevel.Warning:
          return NLogLevel.Warn;
        default:
          return NLogLevel.Info;
      }
    }

    /// <summary>
    /// Creates instance of <see cref="Log"/> class.
    /// </summary>
    /// <param name="name">Log name.</param>
    public Log(string name)
    {
      target = NLogManager.GetLogger(name);
    }
  }
}
