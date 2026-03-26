// Copyright (C) 2014-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2014.02.20

using System.Reflection;
using log4net.Core;
using log4netLogManager = log4net.LogManager;
using log4netLog = log4net.ILog;

namespace Xtensive.Orm.Logging.log4net
{
  /// <summary>
  /// Log implementation for log4net.
  /// </summary>
  public class Log : BaseLog
  {
    private readonly log4netLog target;

    /// <inheritdoc />
    public override bool IsLogged(LogLevel level)
    {
      return target.Logger.IsEnabledFor(ConvertLevel(level));
    }

    /// <inheritdoc />
    public override void Write(in LogEventInfo info)
    {
      target.Logger.Log(target.Logger.GetType(), ConvertLevel(info.Level), info.FormattedMessage, info.Exception);
    }

    private Level ConvertLevel(in LogLevel level)
    {
      switch (level) {
        case LogLevel.Debug:
          return Level.Debug;
        case LogLevel.Info:
          return Level.Info;
        case LogLevel.Warning:
          return Level.Warn;
        case LogLevel.Error:
          return Level.Error;
        case LogLevel.FatalError:
          return Level.Fatal;
        default:
          return Level.Info;
      }
    }

    /// <summary>
    /// Creates instance of <see cref="Log"/> class.
    /// </summary>
    /// <param name="repositoryAssembly">An <see cref="Assembly"/> instance this <see cref="Log"/> instance
    /// is related to.</param>
    /// <param name="name">Log name.</param>
    public Log(Assembly repositoryAssembly, string name)
    {
      target = log4netLogManager.GetLogger(repositoryAssembly, name);

    }
  }
}
